using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using APSRHPlugin;
using APSGHPlugin.Parameters;
using APSGHPlugin.Types;

namespace APSGHPlugin.Components
{
    public class APSObjectParamGet : APSComponent
    {
        public override Guid ComponentGuid => new Guid("A2561CFD-9202-4DAF-8A34-99EA6370147F");
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        public APSObjectParamGet()
          : base("Get Param", "GP", "Get parameter from given object", "APS", "Parameters")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager PM)
        {
            PM.AddGeometryParameter("Object", "O", "Rhino object to get parameter from", GH_ParamAccess.item);
            PM.AddParameter(new APSParameterParam(), "Parameter", "P", "Parameter to get", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager PM)
        {
            PM.AddGeometryParameter("Object", "O", "Rhino object to get parameter from", GH_ParamAccess.item);
            PM.AddParameter(new APSParameterParam(), "Parameter", "P", "Parameter to get", GH_ParamAccess.list);
            PM.AddParameter(new APSValueParam(), "Value", "V", "Parameter value if found", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            IGH_GeometricGoo geom = default;
            if (DA.GetData(0, ref geom))
            {
                if (!geom.IsReferencedGeometry)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input must be referenced");
                    return;
                }

                var parameters = new List<APSParameter>();
                var values = new List<APSValue>();
                if (DA.GetDataList(1, parameters))
                {
                    foreach (var param in parameters)
                    {
                        if (APSRhino.Parameters.GetParameter(ToObjectRef(geom), param.Value, out object value))
                            values.Add(new APSValue(param.Value, value));
                    }

                    DA.SetDataList(1, parameters);
                    DA.SetDataList(2, values);
                }

                DA.SetData(0, geom);
            }
        }
    }
}