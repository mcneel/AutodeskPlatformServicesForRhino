using System;
using System.Linq;
using System.Collections.Generic;

using Eto.Drawing;
using Eto.Forms;

using Rhino;
using Rhino.DocObjects;

using AutodeskPlatformServices;
using APSRHPlugin.Controls;

namespace APSRHPlugin.Dialogs
{
    public class APSParamsAddDialog : BaseDialog
    {
        #region Fields
        readonly IEnumerable<RhinoObject> _objects = Enumerable.Empty<RhinoObject>();

        readonly APSSpinner _spinner = new APSSpinner();

        readonly DropDown _collectionsSelector = new DropDown { Style = "apsrhplugin.dialogs.dropDown" };

        readonly TextBox _searchTerm = new TextBox { Style = "apsrhplugin.dialogs.textBox" };

        readonly Button _searchButton = new Button { Text = "Find", Style = "apsrhplugin.dialogs.button" };

        readonly GridColumn _paramName = new GridColumn
        {
            HeaderText = "Name",
            MinWidth = 150,
            DataCell = new TextBoxCell(0)
            {
                TextAlignment = TextAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            }
        };

        readonly GridColumn _propSpec = new GridColumn
        {
            HeaderText = "Specification",
            MinWidth = 100,
            DataCell = new TextBoxCell(1)
            {
                TextAlignment = TextAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            }
        };

        readonly GridColumn _propGroup = new GridColumn
        {
            HeaderText = "Group",
            MinWidth = 100,
            DataCell = new TextBoxCell(2)
            {
                TextAlignment = TextAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };

        readonly GridColumn _propCategories = new GridColumn
        {
            HeaderText = "Categories",
            MinWidth = 100,
            DataCell = new TextBoxCell(3)
            {
                TextAlignment = TextAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };

        readonly TreeGridItem _root = new TreeGridItem();

        readonly TreeGridView _paramTree = new TreeGridView
        {

        };

        readonly Button _cancelButton = new Button { Text = "Cancel", Style = "apsrhplugin.dialogs.button" };

        readonly Button _addParamButton = new Button { Text = "Add Parameter(s)", Style = "apsrhplugin.dialogs.button", Width = 120 };
        #endregion

        class APSParamItem : TreeGridItem
        {
            public Parameter Parameter { get; }

            public object Value => Values[1];

            public APSParamItem(APSParamsAddDialog dlg, Parameter param)
            {
                Parameter = param;

                Values = new object[] { param.Name, null, null, null };
                UpdateAsync(dlg, param);
            }

            async void UpdateAsync(APSParamsAddDialog dlg, Parameter param)
            {
                dlg._spinner.Spin();

                Spec spec = await param.GetSpecAsync();
                ClassificationGroup group = await param.GetGroupAsync();
                IEnumerable<ClassificationCategory> categories = await param.GetCategoriesAsync();

                string specstr = spec is null ? "Unknown" : spec.Name;
                string groupstr = group is null ? "Unknown" : group.Name;
                string catstr = string.Join(",", categories);

                Values = new object[] { param.Name, specstr, groupstr, catstr };

                dlg._spinner.UnSpin();
                dlg._paramTree.ReloadItem(this);
            }
        }

        public APSParamsAddDialog(IEnumerable<RhinoObject> objects)
        {
            Title = "Add APS Parameter";

            _objects = objects ?? _objects;

            _spinner.SpinStopped += (s, e) =>
            {
                bool hasCollections = _collectionsSelector.Items.Any();
                _collectionsSelector.Enabled = _collectionsSelector.Items.Count() > 1;

                _searchTerm.Enabled = hasCollections;
                _searchButton.Enabled = hasCollections;

            };

            _collectionsSelector.Enabled = false;

            _searchButton.Click += (s, e) => UpdateParameters();

            _paramTree.Columns.Add(_paramName);
            _paramTree.Columns.Add(_propSpec);
            _paramTree.Columns.Add(_propGroup);
            _paramTree.Columns.Add(_propCategories);

            _paramTree.Enabled = false;
            _paramTree.AllowMultipleSelection = true;
            _paramTree.DataStore = _root;

            _paramTree.SelectionChanged += (s, e) =>
            {
                if (_paramTree.SelectedItems.Any())
                    _addParamButton.Enabled = true;
            };

            _addParamButton.Enabled = false;
            _addParamButton.Click += (s, e) => Submit();

            _cancelButton.Click += (s, e) => Cancel();

            SetSize(new Size(600, 700), new Size(400, 400));

            Content = new TableLayout
            {
                Rows =
                {
                    _spinner,
                    new TableLayout
                    {
                        Style = "apsrhplugin.dialogs.tableContent",
                        Rows = {
                            _collectionsSelector,
                            new TableLayout
                            {
                                Style = "apsrhplugin.dialogs.tableLayout",
                                Rows =
                                {
                                    new TableRow {
                                        Cells = {
                                            new Label { Text = "Search" },
                                            new TableCell { Control = _searchTerm, ScaleWidth = true },
                                            _searchButton
                                        }
                                    }
                                }
                            },
                            new TableRow { ScaleHeight = true, Cells = { _paramTree } },
                            new TableLayout
                            {
                                Style = "apsrhplugin.dialogs.tableLayout",
                                Rows = { new TableRow { Cells = {
                                    null,
                                    _cancelButton,
                                    _addParamButton,
                                } } }
                            },
                        }
                    }
                }
            };

            DefaultButton = _addParamButton;

            UpdateCollections();
        }

        protected override void Submit()
        {
            base.Submit();

            foreach (RhinoObject obj in _objects)
            {
                foreach (var param in _paramTree.SelectedItems.OfType<APSParamItem>())
                {
                    try
                    {
                        APSRhino.Parameters.SetParameter(obj, param.Parameter);
                    }
                    catch (Exception ex)
                    {
                        Rhino.UI.Dialogs.ShowMessage($"Error Adding Parameter: {ex}", "APS Parameters");
                    }
                }
            }
        }

        async void UpdateCollections()
        {
            _spinner.Spin();
            _searchTerm.Enabled = false;
            _searchButton.Enabled = false;

            try
            {
                _collectionsSelector.Items.Clear();

                ListCollectionsResult res = await APSRhino.Parameters.ListCollectionsAsync();

                if (res.Collections.Any())
                {
                    foreach (Collection collection in res.Collections)
                        _collectionsSelector.Items.Add(collection.Name, collection.Id);

                    _collectionsSelector.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Rhino.UI.Dialogs.ShowMessage($"API Error: {ex}", "APS Parameters");
            }
            finally
            {
                _spinner.UnSpin();
            }
        }

        async void UpdateParameters()
        {
            if (_collectionsSelector.SelectedIndex >= 0
                 && !string.IsNullOrWhiteSpace(_searchTerm.Text))
            {
                string collectionId = _collectionsSelector.SelectedKey;

                _spinner.Spin();
                _paramTree.Enabled = false;

                try
                {
                    ListParametersResult res = await APSRhino.Parameters.ListParametersAsync(collectionId);

                    _root.Children.Clear();
                    foreach (Parameter param in res.Parameters)
                        _root.Children.Add(new APSParamItem(this, param));
                    _paramTree.ReloadData();
                }
                catch (Exception ex)
                {
                    Rhino.UI.Dialogs.ShowMessage($"API Error: {ex}", "APS Parameters");
                }
                finally
                {
                    _paramTree.Enabled = true;
                    _spinner.UnSpin();
                }
            }
        }
    }
}