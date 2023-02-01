using System;
using System.Linq;
using System.Collections.Generic;

using Grasshopper.Kernel;

using AutodeskPlatformServices;

using APSGHPlugin.Parameters;
using APSGHPlugin.Types;

namespace APSGHPlugin.Components
{
    public class APSQuerySpecs : APSComponent
    {
        public override Guid ComponentGuid => new Guid("9A8B08B1-2206-451A-A497-2A626698A2F3");

        public APSQuerySpecs()
          : base("Query Specs", "QS", "Query specs", "APS", "Parameters")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager PM)
        {
        }

        protected override void RegisterOutputParams(GH_OutputParamManager PM)
        {
            PM.AddParameter(new APSSpecParam(), "Specs", "S", "List of specs", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var specs = new HashSet<Spec>();

            ListSpecsResult results = default;
            do
            {
                results = APSAPI.Parameters.ListSpecs(results);
                specs.UnionWith(results.Specs);
            }
            while (results.HasMore);

            DA.SetDataList(0, specs.Select(s => new APSSpec(s)));
        }
    }
}