using System;

using Grasshopper.Kernel;

using AutodeskPlatformServices;

using APSGHPlugin.Parameters;
using APSGHPlugin.Types;

namespace APSGHPlugin.Components
{
    public class APSDeconstructClassificationSpec : APSComponent
    {
        public override Guid ComponentGuid => new Guid("9A0AF733-8E5C-425D-B673-EFB2D5211F0F");
        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        public APSDeconstructClassificationSpec()
          : base("Deconstruct Classification Spec", "DCS", "Deconstruct classification spec", "APS", "Parameters")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager PM)
        {
            PM.AddParameter(new APSClassificationSpecParam(), "Classification Spec", "CS", "APS classification spec to deconstruct", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager PM)
        {
            PM.AddTextParameter("Id", "ID", "Spec id", GH_ParamAccess.item);
            PM.AddTextParameter("Name", "N", "Spec name", GH_ParamAccess.item);
            PM.AddParameter(new APSClassificationDisciplineParam(), "Discipline", "D", "Spec discipline", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            APSClassificationSpec apsSpec = default;
            if (DA.GetData(0, ref apsSpec))
            {
                DA.SetData(0, apsSpec.Value.Id);
                DA.SetData(1, apsSpec.Value.Name);

                if (apsSpec.Value.GetDiscipline() is ClassificationDiscipline discipline)
                    DA.SetData(2, new APSClassificationDiscipline(discipline));
            }
        }
    }
}