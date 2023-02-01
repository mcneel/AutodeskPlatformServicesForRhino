using System;

using Rhino;
using Rhino.UI;
using Rhino.Commands;

namespace APSRHPlugin.Commands
{
    public class APSParamsCreatorCommand : Command
    {
        public APSParamsCreatorCommand()
        {
            Instance = this;
        }

        public static APSParamsCreatorCommand Instance { get; private set; }

        public override string EnglishName => "CreateAPSParameter";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            Rhino.UI.Dialogs.ShowMessage("Not yet implemented", "APS Parameters");
            return Result.Success;
        }
    }
}