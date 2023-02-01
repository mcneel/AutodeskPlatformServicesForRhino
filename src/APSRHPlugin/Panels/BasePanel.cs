using System;
using System.Linq;

using Eto.Drawing;
using Eto.Forms;

namespace APSRHPlugin.Dialogs
{
    public abstract class BasePanel : Panel
    {
        public BasePanel()
        {
            Styles.Add<TableLayout>(
              "apsrhplugin.panels.tableContent", ct =>
              {
                  ct.Padding = new Padding(20, 20);
                  ct.Spacing = new Size(10, 10);
              });

            Styles.Add<StackLayout>(
              "apsrhplugin.panels.stackContent", cs =>
              {
                  cs.Padding = new Padding(20, 20);
                  cs.Spacing = 10;
              });

            Styles.Add<TableLayout>(
              "apsrhplugin.panels.tableLayout", t =>
              {
                  t.Spacing = new Size(10, 10);
              });

            Styles.Add<StackLayout>(
              "apsrhplugin.panels.stackLayout", s =>
              {
                  s.Padding = new Padding(10, 10);
                  s.Spacing = 10;
              });

            Styles.Add<StackLayout>(
              "apsrhplugin.panels.horizStackLayout", s =>
              {
                  s.Orientation = Orientation.Horizontal;
                  s.HorizontalContentAlignment = HorizontalAlignment.Left;
                  s.VerticalContentAlignment = VerticalAlignment.Center;
                  s.AlignLabels = true;
                  s.Spacing = 10;
              });

            Styles.Add<StackLayout>(
              "apsrhplugin.panels.horizStackLayoutRight", s =>
              {
                  s.Orientation = Orientation.Horizontal;
                  s.HorizontalContentAlignment = HorizontalAlignment.Right;
                  s.VerticalContentAlignment = VerticalAlignment.Center;
                  s.AlignLabels = true;
                  s.Spacing = 10;
              });

            Styles.Add<TextBox>(
              "apsrhplugin.panels.textBox", ct =>
              {
                  ct.Height = 24;
              });

            Styles.Add<DropDown>(
              "apsrhplugin.panels.dropDown", ct =>
              {
                  ct.Height = 24;
              });

            Styles.Add<Button>(
              "apsrhplugin.panels.button", ct =>
              {
                  ct.MinimumSize = new Size(100, 24);
              });
        }
    }
}
