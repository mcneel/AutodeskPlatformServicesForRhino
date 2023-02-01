using System;
using System.Linq;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using APSRHPlugin;
using APSGHPlugin.Parameters;
using APSGHPlugin.Types;

namespace APSGHPlugin.Components
{
    public class APSObjectParamSet : APSComponent
    {
        public override Guid ComponentGuid => new Guid("5AABE90B-2A4D-4894-9409-A972FE972A3C");
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        public APSObjectParamSet()
          : base("Set Param", "SP", "Set parameter on given object", "APS", "Parameters")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager PM)
        {
            PM.AddGeometryParameter("Object", "O", "Rhino object to set parameter", GH_ParamAccess.item);
            PM.AddParameter(new APSParameterParam(), "Parameter", "P", "Parameter to set", GH_ParamAccess.list);
            PM.AddGenericParameter("Value", "V", "Parameter value to set", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager PM)
        {
            PM.AddGeometryParameter("Object", "O", "Rhino object to set parameter", GH_ParamAccess.item);
            PM.AddParameter(new APSParameterParam(), "Parameter", "P", "Parameter to set", GH_ParamAccess.list);
            PM.AddParameter(new APSValueParam(), "Value", "V", "Validated parameter value", GH_ParamAccess.list);
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
                var values = new List<object>();
                var validValues = new List<APSValue>();
                if (DA.GetDataList(1, parameters) && DA.GetDataList(2, values))
                {
                    var paramsValues = parameters.Zip(values, (p, v) => new { Param = p.Value, Value = v });
                    foreach (var pv in paramsValues)
                    {
                        try
                        {
                            APSRhino.Parameters.SetParameter(ToObjectRef(geom), pv.Param, pv.Value);
                            validValues.Add(new APSValue(pv.Param, pv.Value));
                        }
                        catch (Exception ex)
                        {
                            validValues.Add(APSValue.GetInvalidValue(pv.Param));
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Invalid Value: {ex.Message}");
                        }
                    }

                    DA.SetDataList(1, parameters);
                    DA.SetDataList(2, validValues);
                }

                DA.SetData(0, geom);
            }
        }
    }
}