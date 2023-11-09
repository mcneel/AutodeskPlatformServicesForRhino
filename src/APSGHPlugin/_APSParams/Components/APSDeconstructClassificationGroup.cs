using System;

using Grasshopper.Kernel;

using APSGHPlugin.Parameters;
using APSGHPlugin.Types;

namespace APSGHPlugin.Components
{
    public class APSDeconstructClassificationGroup : APSComponent
    {
        public override Guid ComponentGuid => new Guid("A11DDF6C-820C-48D1-A43F-EB31F7B57EC8");
        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        public APSDeconstructClassificationGroup()
          : base("Deconstruct Classification Group", "DCG", "Deconstruct classification group", "APS", "Parameters")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager PM)
        {
            PM.AddParameter(new APSClassificationGroupParam(), "Classification Group", "CG", "APS classification group to deconstruct", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager PM)
        {
            PM.AddTextParameter("Id", "ID", "Group id", GH_ParamAccess.item);
            PM.AddTextParameter("Name", "N", "Group name", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            APSClassificationGroup apsGroup = default;
            if (DA.GetData(0, ref apsGroup))
            {
                DA.SetData(0, apsGroup.Value.Id);
                DA.SetData(1, apsGroup.Value.Name);
            }
        }
    }
}