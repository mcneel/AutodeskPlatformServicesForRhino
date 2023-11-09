using System;
using System.Linq;

using Grasshopper.Kernel;

using AutodeskPlatformServices;

using APSGHPlugin.Parameters;
using APSGHPlugin.Types;

namespace APSGHPlugin.Components
{
    public class APSDeconstructParameter : APSComponent
    {
        public override Guid ComponentGuid => new Guid("46315A40-0B5C-438C-B3C7-59097369E21A");
        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        public APSDeconstructParameter()
          : base("Deconstruct Parameter", "DP", "Deconstruct parameter", "APS", "Parameters")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager PM)
        {
            PM.AddParameter(new APSParameterParam(), "Parameter", "P", "APS parameter to deconstruct", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager PM)
        {
            PM.AddTextParameter("Id", "ID", "Parameter id", GH_ParamAccess.item);
            PM.AddTextParameter("Name", "N", "Parameter name", GH_ParamAccess.item);
            PM.AddTextParameter("Description", "D", "Parameter description", GH_ParamAccess.item);
            PM.AddBooleanParameter("ReadOnly", "RO", "Is parameter readonly?", GH_ParamAccess.item);
            PM.AddParameter(new APSClassificationSpecParam(), "Spec", "S", "Parameter specification", GH_ParamAccess.item);
            PM.AddParameter(new APSClassificationGroupParam(), "Groups", "G", "Parameter classification groups", GH_ParamAccess.list);
            PM.AddParameter(new APSClassificationCategoryParam(), "Categories", "C", "Parameter classification categories", GH_ParamAccess.list);
            PM.AddBooleanParameter("Hidden", "H", "Is parameter hidden?", GH_ParamAccess.item);
            PM.AddParameter(new APSParameterAssocParam(), "Association", "A", "Parameter association", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            APSParameter apsParam = default;
            if (DA.GetData(0, ref apsParam))
            {
                Parameter p = apsParam.Value;

                DA.SetData(0, p.Id);
                DA.SetData(1, p.Name);
                DA.SetData(2, p.Description);
                DA.SetData(3, p.ReadOnly);

                if (p.GetSpec() is ClassificationSpec spec)
                    DA.SetData(4, new APSClassificationSpec(spec));

                if (p.GetGroup() is ClassificationGroup group)
                    DA.SetData(5, new APSClassificationGroup(group));
                DA.SetDataList(6, p.GetCategories().Select(c => new APSClassificationCategory(c)).ToArray());

                DA.SetData(7, p.IsHidden);
                DA.SetData(8, new APSParameterAssoc(p.GetAssociation()));
            }
        }
    }
}