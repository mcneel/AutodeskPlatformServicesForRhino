using System;
using System.Linq;
using System.Collections.Generic;

using Grasshopper.Kernel;

using AutodeskPlatformServices;

using APSGHPlugin.Parameters;
using APSGHPlugin.Types;

namespace APSGHPlugin.Components
{
    public class APSQueryParameterGroups : APSComponent
    {
        public override Guid ComponentGuid => new Guid("13DEAEE2-356A-4799-ACB0-037ECE6CAC1F");

        public APSQueryParameterGroups()
          : base("Query Groups", "QPG", "Query parameter groups for given Autodesk account", "APS", "Parameters")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager PM)
        {
            PM.AddParameter(new APSConnectionInfoParam(), "Connection Info", "CI", "APS API connection info", GH_ParamAccess.item);
            PM[0].Optional = false;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager PM)
        {
            PM.AddParameter(new APSGroupParam(), "Groups", "G", "List of parameter groups", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            APSConnectionInfo conn = default;
            if (DA.GetData(0, ref conn))
            {
                var groups = new HashSet<Group>();

                GetGroupsResult results = default;
                do
                {
                    results = APSAPI.Parameters.GetGroups(conn.AccountId, results);
                    groups.UnionWith(results.Groups);
                }
                while (results.HasMore);

                DA.SetDataList(0, groups.Select(c => new APSGroup(conn.AccountId, c)));
            }
        }
    }
}