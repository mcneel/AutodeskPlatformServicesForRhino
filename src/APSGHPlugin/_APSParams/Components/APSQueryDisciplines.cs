using System;
using System.Linq;
using System.Collections.Generic;

using Grasshopper.Kernel;

using AutodeskPlatformServices;

using APSGHPlugin.Parameters;
using APSGHPlugin.Types;

namespace APSGHPlugin.Components
{
    public class APSQueryDisciplines : APSComponent
    {
        public override Guid ComponentGuid => new Guid("EA6A163D-8CDF-43DA-9CC8-AFB29C076E16");

        public APSQueryDisciplines()
          : base("Query Disciplines", "QD", "Query disciplines", "APS", "Parameters")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager PM)
        {
        }

        protected override void RegisterOutputParams(GH_OutputParamManager PM)
        {
            PM.AddParameter(new APSDisciplineParam(), "Disciplines", "D", "List of disciplines", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var disciplines = new HashSet<Discipline>();

            ListDisciplinesResult results = default;
            do
            {
                results = APSAPI.Parameters.ListDisciplines(results);
                disciplines.UnionWith(results.Disciplines);
            }
            while (results.HasMore);

            DA.SetDataList(0, disciplines.Select(d => new APSDiscipline(d)));
        }
    }
}