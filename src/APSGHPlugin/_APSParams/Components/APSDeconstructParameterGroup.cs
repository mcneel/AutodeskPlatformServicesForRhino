using System;

using Grasshopper.Kernel;

using APSGHPlugin.Parameters;
using APSGHPlugin.Types;

namespace APSGHPlugin.Components
{
    public class APSDeconstructParameterGroup : APSComponent
    {
        public override Guid ComponentGuid => new Guid("6287FF81-1A6E-412E-908F-B5B45B9711C9");
        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        public APSDeconstructParameterGroup()
          : base("Deconstruct Parameter Group", "DPG", "Deconstruct parameter group", "APS", "Parameters")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager PM)
        {
            PM.AddParameter(new APSGroupParam(), "Parameter Group", "PG", "APS parameter group to deconstruct", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager PM)
        {
            PM.AddTextParameter("Id", "ID", "Group id", GH_ParamAccess.item);
            PM.AddTextParameter("Name", "N", "Group name", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            APSGroup apsGroup = default;
            if (DA.GetData(0, ref apsGroup))
            {
                DA.SetData(0, apsGroup.Value.Id);
                DA.SetData(1, apsGroup.Value.Title);
            }
        }
    }
}