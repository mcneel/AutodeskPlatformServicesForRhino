using System;
using System.Linq;
using Grasshopper.Kernel;

using Rhino;

using APSRHPlugin;
using APSGHPlugin.Parameters;
using APSGHPlugin.Types;

namespace APSGHPlugin.Components
{
    public class APSQueryDocumentParameters : APSComponent
    {
        public override Guid ComponentGuid => new Guid("C773D80C-BEB6-4CDC-B687-7266050E7F9C");

        public APSQueryDocumentParameters()
          : base("Query Document Parameters", "QDP", "Query parameters used in active document", "APS", "Parameters")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager PM)
        {
            PM.AddTextParameter("Search Term", "T", "Search term to filter parameters", GH_ParamAccess.item);
            PM[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager PM)
        {
            PM.AddParameter(new APSParameterParam(), "Parameters", "P", "List of parameters", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string searchTerm = default;
            if (DA.GetData(0, ref searchTerm))
                DA.SetDataList(0, APSRhino.Parameters.GetParameters(RhinoDoc.ActiveDoc)
                                                     .Where(p => p.Name.Contains(searchTerm))
                                                     .Select(p => new APSParameter(p)));
            else
                DA.SetDataList(0, APSRhino.Parameters.GetParameters(RhinoDoc.ActiveDoc)
                                                     .Select(p => new APSParameter(p)));
        }
    }
}