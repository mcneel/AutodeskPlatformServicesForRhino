using System;

using Grasshopper.Kernel;

using APSGHPlugin.Parameters;
using APSGHPlugin.Types;

namespace APSGHPlugin.Components
{
    public class APSDeconstructClassificationCategory : APSComponent
    {
        public override Guid ComponentGuid => new Guid("FBB1AEA4-64F6-43B0-AA15-D9DC7900C2D5");
        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        public APSDeconstructClassificationCategory()
          : base("Deconstruct Classification Category", "DCC", "Deconstruct classification category", "APS", "Parameters")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager PM)
        {
            PM.AddParameter(new APSClassificationCategoryParam(), "Classification Category", "CC", "APS classification category to deconstruct", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager PM)
        {
            PM.AddTextParameter("Id", "ID", "Category id", GH_ParamAccess.item);
            PM.AddTextParameter("Name", "N", "Category name", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            APSClassificationCategory apsCategory = default;
            if (DA.GetData(0, ref apsCategory))
            {
                DA.SetData(0, apsCategory.Value.Id);
                DA.SetData(1, apsCategory.Value.Name);
            }
        }
    }
}