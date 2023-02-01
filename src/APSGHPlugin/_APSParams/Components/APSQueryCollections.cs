using System;
using System.Linq;
using System.Collections.Generic;

using Grasshopper.Kernel;

using AutodeskPlatformServices;

using APSGHPlugin.Parameters;
using APSGHPlugin.Types;

namespace APSGHPlugin.Components
{
    public class APSQueryCollections : APSComponent
    {
        public override Guid ComponentGuid => new Guid("1A36C05C-6300-47A4-9CA3-F29924315738");

        public APSQueryCollections()
          : base("Query Collections", "QPC", "Query parameter for given Autodesk account", "APS", "Parameters")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager PM)
        {
            PM.AddParameter(new APSConnectionInfoParam(), "Connection Info", "CI", "APS API connection info", GH_ParamAccess.item);
            PM[0].Optional = false;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager PM)
        {
            PM.AddParameter(new APSCollectionParam(), "Collections", "C", "List of parameter collections", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            APSConnectionInfo conn = default;
            if (DA.GetData(0, ref conn))
            {
                var collections = new HashSet<Collection>();

                ListCollectionsResult results = default;
                do
                {
                    results = APSAPI.Parameters.ListCollections(conn.AccountId, results);
                    collections.UnionWith(results.Collections);
                }
                while (results.HasMore);

                DA.SetDataList(0, collections.Select(c => new APSCollection(conn.AccountId, c)));
            }
        }
    }
}