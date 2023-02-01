using System;

using Eto.Drawing;
using Eto.Forms;
using Rhino.UI;

namespace APSRHPlugin.Dialogs
{
    public abstract class BaseDialog : Dialog
    {
        public bool Cancelled { get; private set; } = true;

        public BaseDialog()
        {
            Topmost = true;
            Owner = RhinoEtoApp.MainWindow;

            Styles.Add<TableLayout>(
              "apsrhplugin.dialogs.tableContent", ct =>
              {
                  ct.Padding = new Padding(20, 20);
                  ct.Spacing = new Size(10, 10);
              });

            Styles.Add<StackLayout>(
              "apsrhplugin.dialogs.stackContent", cs =>
              {
                  cs.Padding = new Padding(20, 20);
                  cs.Spacing = 10;
              });

            Styles.Add<TableLayout>(
              "apsrhplugin.dialogs.tableLayout", t =>
              {
                  t.Spacing = new Size(10, 10);
              });

            Styles.Add<StackLayout>(
              "apsrhplugin.dialogs.stackLayout", s =>
              {
                  s.Padding = new Padding(10, 10);
                  s.Spacing = 10;
              });

            Styles.Add<StackLayout>(
              "apsrhplugin.dialogs.horizStackLayout", s =>
              {
                  s.Orientation = Orientation.Horizontal;
                  s.HorizontalContentAlignment = HorizontalAlignment.Left;
                  s.VerticalContentAlignment = VerticalAlignment.Center;
                  s.AlignLabels = true;
                  s.Spacing = 10;
              });

            Styles.Add<StackLayout>(
              "apsrhplugin.dialogs.horizStackLayoutRight", s =>
              {
                  s.Orientation = Orientation.Horizontal;
                  s.HorizontalContentAlignment = HorizontalAlignment.Right;
                  s.VerticalContentAlignment = VerticalAlignment.Center;
                  s.AlignLabels = true;
                  s.Spacing = 10;
              });

            Styles.Add<TextBox>(
              "apsrhplugin.dialogs.textBox", ct =>
              {
                  ct.Height = 24;
              });

            Styles.Add<DropDown>(
              "apsrhplugin.dialogs.dropDown", ct =>
              {
                  ct.Height = 24;
              });

            Styles.Add<Button>(
              "apsrhplugin.dialogs.button", ct =>
              {
                  ct.MinimumSize = new Size(100, 24);
              });
        }

        public void SetSize(Size size, Size minSize)
        {
            var screenWidth = Convert.ToInt16(Screen.WorkingArea.Width);
            var screenHeight = Convert.ToInt16(Screen.WorkingArea.Height);

            ClientSize = new Size(
              Math.Min(size.Width, screenWidth),
              Math.Min(size.Height, screenHeight)
            );

            MinimumSize = minSize;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (Cancelled)
                OnCancelled();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.Key)
            {
                case Keys.Escape:
                    e.Handled = true;
                    Cancelled = true;
                    Close();
                    break;
            }
        }

        protected virtual void Submit()
        {
            Cancelled = false;
            Close();
        }

        protected void Cancel()
        {
            Cancelled = true;
            Close();
        }

        protected virtual void OnCancelled() { }
    }
}
