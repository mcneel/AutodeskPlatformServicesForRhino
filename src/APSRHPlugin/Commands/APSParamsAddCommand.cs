using System;

using Rhino;
using Rhino.Commands;

using APSRHPlugin.Dialogs;
using System.Linq;

namespace APSRHPlugin.Commands
{
    public class APSParamsAddCommand : Command
    {
        public APSParamsAddCommand()
        {
            Instance = this;
        }

        public static APSParamsAddCommand Instance { get; private set; }

        public override string EnglishName => "AddAPSParameter";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var selected = doc.Objects.GetSelectedObjects(includeLights: true, includeGrips: false);
            if (!selected.Any())
            {
                Rhino.UI.Dialogs.ShowMessage("At least one object must be selected", "APS Parameters");
                return Result.Cancel;
            }

            var paramsdlg = new APSParamsAddDialog(selected);
            paramsdlg.ShowModal();

            return paramsdlg.Cancelled ? Result.Cancel : Result.Success;
        }
    }
}