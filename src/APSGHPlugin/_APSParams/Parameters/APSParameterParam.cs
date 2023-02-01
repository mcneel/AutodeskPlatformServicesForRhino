using System;

using Grasshopper.Kernel;

using APSGHPlugin.Types;

namespace APSGHPlugin.Parameters
{
    public class APSParameterParam : APSParam<APSParameter>
    {
        public override Guid ComponentGuid => new Guid("C48B0A3F-3592-457D-B50B-F01B5B305820");
        public override GH_Exposure Exposure => GH_Exposure.hidden;

        public APSParameterParam()
            : base("APS Parameter", "APSP", "APS parameter", string.Empty, string.Empty)
        {
        }
    }
}
