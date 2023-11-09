using System;
using System.Linq;
using System.Collections.Generic;

using Grasshopper.Kernel;

using AutodeskPlatformServices;

using APSGHPlugin.Parameters;
using APSGHPlugin.Types;

namespace APSGHPlugin.Components
{
    public class APSQueryParameters : APSComponent
    {
        public override Guid ComponentGuid => new Guid("B7C6B657-E87A-4FBC-8B0C-EB0849D661A1");

        public APSQueryParameters()
          : base("Query Parameters", "QP", "Query parameters in given collections", "APS", "Parameters")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager PM)
        {
            PM.AddParameter(new APSCollectionParam(), "Collection", "C", "Parameter collection", GH_ParamAccess.item);
            PM[0].Optional = false;

            PM.AddTextParameter("Search Term", "T", "Search term to filter parameters", GH_ParamAccess.item);
            PM[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager PM)
        {
            PM.AddParameter(new APSParameterParam(), "Parameters", "P", "List of parameters", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            APSCollection collection = default;
            if (DA.GetData(0, ref collection))
            {
                GetParametersResult results = default;
                var parameters = new HashSet<Parameter>();
                string searchTerm = default;

                if (DA.GetData(1, ref searchTerm))
                {
                    results = APSAPI.Parameters.SearchParameters(collection.AccountId, collection.Group.Id, collection.Id, searchTerm);
                    parameters.UnionWith(results.Parameters);
                }
                else
                {
                    do
                    {
                        results = APSAPI.Parameters.GetParameters(collection.AccountId, collection.Group.Id, collection.Id, results);
                        parameters.UnionWith(results.Parameters);
                    }
                    while (results.HasMore);
                }

                DA.SetDataList(0, parameters.Select(p => new APSParameter(p)));
            }
        }
    }
}