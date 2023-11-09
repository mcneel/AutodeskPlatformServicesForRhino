using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Eto.Drawing;
using Eto.Forms;

using Rhino;
using Rhino.DocObjects;
using Rhino.UI;

using AutodeskPlatformServices;

using APSRHPlugin.Dialogs;
using APSRHPlugin.Controls;

namespace APSRHPlugin.Panels
{
    [Guid("FAD64797-CD32-486F-B050-85B502E60808")]
    public class APSParamsEditor : BasePanel, IPanel
    {
        public static Guid PanelId => typeof(APSParamsEditor).GUID;

        #region Fields
        readonly uint _document_sn = 0;

        readonly APSSpinner _spinner = new APSSpinner();

        readonly Button _connectButton = new Button { Text = "Connect", Enabled = true, Style = "apsrhplugin.panels.button" };

        readonly Button _addParamButton = new Button { Text = "Add", Enabled = false, Style = "apsrhplugin.panels.button" };

        readonly Button _removeParamButton = new Button { Text = "Remove", Enabled = false, Style = "apsrhplugin.panels.button" };

        readonly StackLayout _buttonsTray = new StackLayout
        {
            Style = "apsrhplugin.panels.horizStackLayout",
            Padding = new Padding(10, 10)
        };

        readonly GridColumn _propName = new GridColumn
        {
            HeaderText = "Name",
            MinWidth = 150,
            DataCell = new TextBoxCell(0)
            {
                TextAlignment = TextAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            }
        };

        readonly GridColumn _propValue = new GridColumn
        {
            HeaderText = "Value",
            MinWidth = 100,
            DataCell = new TextBoxCell(1)
            {
                TextAlignment = TextAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            },
            Editable = true,
        };

        readonly GridColumn _propSpec = new GridColumn
        {
            HeaderText = "Specification",
            MinWidth = 100,
            DataCell = new TextBoxCell(2)
            {
                TextAlignment = TextAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            }
        };

        readonly TreeGridView _paramTree = new TreeGridView
        {

        };

        readonly TreeGridItem _root = new TreeGridItem();

        RhinoObject _selectedObj;
        #endregion

        class APSParamValueItem : TreeGridItem
        {
            public Parameter Parameter { get; }

            public object Value => Values[1];

            public APSParamValueItem(APSParamsEditor parent, Parameter param, object value)
            {
                Parameter = param;

                Values = new object[] { param.Name, value, null };
                UpdateAsync(parent, param);
            }

            async void UpdateAsync(APSParamsEditor parent, Parameter param)
            {
                parent._spinner.Spin();
                ClassificationSpec spec = await param.GetSpecAsync();
                Values[2] = spec is null ? "Unknown" : spec.Name;

                parent._spinner.UnSpin();
                parent._paramTree.ReloadItem(this);
            }
        }

        public APSParamsEditor(uint documentSerialNumber)
        {
            _document_sn = documentSerialNumber;

            _connectButton.Text = State.Connected != APSAPI.State ? "Connect" : "Reconnect";
            _connectButton.Click += async (s, e) =>
            {
                if (!await APSRhino.CreateConnectionAsync())
                    return;

                _spinner.Spin();

                try
                {
                    if (State.Connected == APSAPI.State)
                        await APSRhino.ReConnectAsync();
                    else
                        await APSRhino.ConnectAsync();

                    if (State.Errored == APSAPI.State)
                        APSRhino.ReportAPIError();
                }
                catch (Exception ex)
                {
                    APSRhino.ReportAPIError(ex);
                }
                finally
                {
                    _paramTree.Enabled = true;
                    _spinner.UnSpin();
                }
            };

            _addParamButton.Click += (s, e) => AddSelectedParameters();
            _removeParamButton.Click += (s, e) => RemoveSelectedParameters();

            _buttonsTray.Items.Add(_connectButton);
            _buttonsTray.Items.Add(_addParamButton);
            _buttonsTray.Items.Add(_removeParamButton);

            _paramTree.Columns.Add(_propName);
            _paramTree.Columns.Add(_propValue);
            _paramTree.Columns.Add(_propSpec);

            _paramTree.DataStore = _root;
            _paramTree.SelectionChanged += (s, e) =>
            {
                _removeParamButton.Enabled = _paramTree.SelectedItems.Any();
            };
            _paramTree.CellEdited += (s, e) =>
            {
                if (e.Item is APSParamValueItem item
                        && _selectedObj is RhinoObject obj)
                {
                    APSRhino.Parameters.SetParameter(obj, item.Parameter, item.Value);
                }
            };

            _spinner.SpinStopped += (s, e) =>
            {
                _paramTree.Enabled = true;
                _buttonsTray.Enabled = true;
            };

            Content = new TableLayout
            {
                Rows = {
                    _spinner,
                    _buttonsTray,
                    new TableRow {
                        ScaleHeight = true,
                        Cells = { _paramTree }
                    }
                }
            };

            RhinoDoc.SelectObjects += (s, e) =>
            {
                _addParamButton.Enabled = true;
                RefreshData(e.Document, e.RhinoObjects);
            };

            RhinoDoc.DeselectAllObjects += (s, e) =>
            {
                _addParamButton.Enabled = false;
                ClearData(e.Document);
            };

            APSAPI.StateChanged += OnStateChanged;
        }

        void AddSelectedParameters()
        {
            var objects = new RhinoObject[] { _selectedObj };

            var pdlg = new APSParamsAddDialog(objects);
            pdlg.ShowModal();

            if (pdlg.Cancelled)
                return;

            RefreshData(_selectedObj.Document, objects);
        }

        void RemoveSelectedParameters()
        {
            var objects = new RhinoObject[] { _selectedObj };

            foreach (RhinoObject obj in objects)
                foreach (APSParamValueItem item in _paramTree.SelectedItems.OfType<APSParamValueItem>())
                    APSRhino.Parameters.UnsetParameter(obj, item.Parameter);

            RefreshData(_selectedObj.Document, objects);
        }

        void OnStateChanged(State from, State to)
        {
            Application.Instance.Invoke(() =>
            {
                if (State.Connecting < to)
                    _connectButton.Text = "Reconnect";
            });
        }

        async void RefreshData(RhinoDoc doc, IEnumerable<RhinoObject> objects)
        {
            if (State.Connected != APSAPI.State)
            {
                ClearData(doc);
                return;
            }

            if (doc.RuntimeSerialNumber != _document_sn)
                return;

            _spinner.Spin();
            _paramTree.Enabled = false;
            _buttonsTray.Enabled = false;

            try
            {
                _root.Children.Clear();
                if (objects.FirstOrDefault() is RhinoObject obj)
                {
                    _selectedObj = obj;
                    foreach (var param in await APSRhino.Parameters.GetParametersAsync(obj))
                    {
                        if (APSRhino.Parameters.GetParameter(obj, param, out object value))
                            _root.Children.Add(new APSParamValueItem(this, param, value));
                    }

                    _paramTree.ReloadData();
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

        void ClearData(RhinoDoc doc)
        {
            if (doc.RuntimeSerialNumber != _document_sn)
                return;

            _selectedObj = null;
            _root.Children.Clear();
            _paramTree.ReloadData();
        }

        #region IPanel methods
        public void PanelShown(uint documentSerialNumber, ShowPanelReason reason) { }
        public void PanelHidden(uint documentSerialNumber, ShowPanelReason reason) { }
        public void PanelClosing(uint documentSerialNumber, bool onCloseDocument) { }
        #endregion IPanel methods
    }
}