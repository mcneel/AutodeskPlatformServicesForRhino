using System;

using Rhino;
using Rhino.Commands;

using APSRHPlugin.Panels;

namespace APSRHPlugin.Commands
{
    public class APSParamsEditorCommand : Command
    {
        public APSParamsEditorCommand()
        {
            Instance = this;
        }

        public static APSParamsEditorCommand Instance { get; private set; }

        public override string EnglishName => "EditAPSParameters";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var panelId = APSParamsEditor.PanelId;

            if (!Rhino.UI.Panels.IsPanelVisible(panelId))
                Rhino.UI.Panels.OpenPanel(panelId);

            return Result.Success;
        }
    }
}