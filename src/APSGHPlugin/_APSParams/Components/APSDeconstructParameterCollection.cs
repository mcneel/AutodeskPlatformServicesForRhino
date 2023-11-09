using System;

using Grasshopper.Kernel;

using APSGHPlugin.Parameters;
using APSGHPlugin.Types;

namespace APSGHPlugin.Components
{
    public class APSDeconstructParameterCollection : APSComponent
    {
        public override Guid ComponentGuid => new Guid("F7CA005F-EF7F-481D-BE27-D741B3745EBA");
        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        public APSDeconstructParameterCollection()
          : base("Deconstruct Parameter Collection", "DPC", "Deconstruct parameter collection", "APS", "Parameters")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager PM)
        {
            PM.AddParameter(new APSCollectionParam(), "Parameter Collection", "PC", "APS parameter collection to deconstruct", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager PM)
        {
            PM.AddTextParameter("Id", "ID", "Group id", GH_ParamAccess.item);
            PM.AddTextParameter("Name", "N", "Group name", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            APSCollection apsCollection = default;
            if (DA.GetData(0, ref apsCollection))
            {
                DA.SetData(0, apsCollection.Value.Id);
                DA.SetData(1, apsCollection.Value.Name);
            }
        }
    }
}