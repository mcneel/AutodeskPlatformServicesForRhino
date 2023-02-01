using System;

using Grasshopper.Kernel;

using APSGHPlugin.Parameters;
using APSGHPlugin.Types;

namespace APSGHPlugin.Components
{
    public class APSDeconstructDiscipline : APSComponent
    {
        public override Guid ComponentGuid => new Guid("3EED4846-AEAE-4710-A17A-CE1CB259A958");
        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        public APSDeconstructDiscipline()
          : base("Deconstruct Discipline", "DD", "Deconstruct discipline", "APS", "Parameters")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager PM)
        {
            PM.AddParameter(new APSDisciplineParam(), "Discipline", "D", "APS discipline to deconstruct", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager PM)
        {
            PM.AddTextParameter("Id", "T", "Discipline id", GH_ParamAccess.item);
            PM.AddTextParameter("Name", "N", "Discipline name", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            APSDiscipline apsDisc = default;
            if (DA.GetData(0, ref apsDisc))
            {
                DA.SetData(0, apsDisc.Value.Id);
                DA.SetData(1, apsDisc.Value.Name);
            }
        }
    }
}