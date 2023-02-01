using System;
using System.Linq;
using System.Collections.Generic;

using Grasshopper.Kernel;

using AutodeskPlatformServices;

using APSGHPlugin.Parameters;
using APSGHPlugin.Types;

namespace APSGHPlugin.Components
{
    public class APSQueryClassificationGroups : APSComponent
    {
        public override Guid ComponentGuid => new Guid("6DFA8862-7C60-46B5-B99C-5144FB463FD9");

        public APSQueryClassificationGroups()
          : base("Query Classification Groups", "QCG", "Query classification groups", "APS", "Parameters")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager PM)
        {
            PM.AddBooleanParameter("Bindable", "B", "Query bindable classification groups only", GH_ParamAccess.item);
            PM[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager PM)
        {
            PM.AddParameter(new APSClassificationGroupParam(), "Classification Groups", "G", "List of classification groups", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool bindable = true;
            DA.GetData(0, ref bindable);

            var groups = new HashSet<ClassificationGroup>();

            ListClassificationGroupResult results = default;
            do
            {
                results = APSAPI.Parameters.ListClassificationGroup(bindable, results);
                groups.UnionWith(results.ClassificationGroups);
            }
            while (results.HasMore);

            DA.SetDataList(0, groups.Select(cg => new APSClassificationGroup(cg)));
        }
    }
}