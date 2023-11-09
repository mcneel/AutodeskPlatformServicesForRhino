using System;
using System.Linq;
using System.Collections.Generic;

using Grasshopper.Kernel;

using AutodeskPlatformServices;

using APSGHPlugin.Parameters;
using APSGHPlugin.Types;

namespace APSGHPlugin.Components
{
    public class APSQueryClassificationCategories : APSComponent
    {
        public override Guid ComponentGuid => new Guid("449A8BE3-5CB1-45D2-9FDE-0FDC452E3652");

        public APSQueryClassificationCategories()
          : base("Query Classification Categories", "QCC", "Query classification categories", "APS", "Parameters")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager PM)
        {
            PM.AddBooleanParameter("Bindable", "B", "Query bindable classification groups only", GH_ParamAccess.item);
            PM[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager PM)
        {
            PM.AddParameter(new APSClassificationCategoryParam(), "Classification Categories", "C", "List of classification categories", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool bindable = true;
            DA.GetData(0, ref bindable);

            var categories = new HashSet<ClassificationCategory>();

            GetClassificationCategoryResult results = default;
            do
            {
                results = APSAPI.Parameters.Classifications.GetCategories(bindable, results);
                categories.UnionWith(results.ClassificationCategories);
            }
            while (results.HasMore);

            DA.SetDataList(0, categories.Select(cc => new APSClassificationCategory(cc)));
        }
    }
}