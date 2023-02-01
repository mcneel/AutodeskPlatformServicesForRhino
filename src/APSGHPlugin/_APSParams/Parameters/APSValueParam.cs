using System;

using Grasshopper.Kernel;

using APSGHPlugin.Types;

namespace APSGHPlugin.Parameters
{
    public class APSValueParam : APSParam<APSValue>
    {
        public override Guid ComponentGuid => new Guid("1B17B159-4AAC-4B0B-B61E-389543B4A5D6");
        public override GH_Exposure Exposure => GH_Exposure.hidden;

        public APSValueParam()
            : base("APS Parameter Value", "APSPV", "APS parameter value", string.Empty, string.Empty)
        {
        }
    }
}
