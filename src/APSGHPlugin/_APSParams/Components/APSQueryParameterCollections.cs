using System;
using System.Linq;
using System.Collections.Generic;

using Grasshopper.Kernel;

using AutodeskPlatformServices;

using APSGHPlugin.Parameters;
using APSGHPlugin.Types;

namespace APSGHPlugin.Components
{
    public class APSQueryParameterCollections : APSComponent
    {
        public override Guid ComponentGuid => new Guid("1A36C05C-6300-47A4-9CA3-F29924315738");

        public APSQueryParameterCollections()
          : base("Query Collections", "QPC", "Query parameter collections for given Autodesk account", "APS", "Parameters")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager PM)
        {
            PM.AddParameter(new APSConnectionInfoParam(), "Connection Info", "CI", "APS API connection info", GH_ParamAccess.item);
            PM[0].Optional = false;
            PM.AddParameter(new APSGroupParam(), "Parameters Group", "PG", "APS parameters groups", GH_ParamAccess.item);
            PM[1].Optional = false;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager PM)
        {
            PM.AddParameter(new APSCollectionParam(), "Collections", "C", "List of parameter collections", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            APSConnectionInfo conn = default;
            APSGroup group = default;
            if (DA.GetData(0, ref conn)
                    && DA.GetData(1, ref group))
            {
                var collections = new HashSet<Collection>();

                GetCollectionsResult results = default;
                do
                {
                    results = APSAPI.Parameters.GetCollections(conn.AccountId, group.Id, results);
                    collections.UnionWith(results.Collections);
                }
                while (results.HasMore);

                DA.SetDataList(0, collections.Select(c => new APSCollection(conn.AccountId, group.Value, c)));
            }
        }
    }
}