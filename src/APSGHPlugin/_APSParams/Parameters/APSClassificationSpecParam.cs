using System;
using Grasshopper.Kernel;

using APSGHPlugin.Types;

namespace APSGHPlugin.Parameters
{
    public class APSClassificationSpecParam : APSParam<APSClassificationSpec>
    {
        public override Guid ComponentGuid => new Guid("9EEB70D9-0372-4B22-8B65-DFFA11219D36");
        public override GH_Exposure Exposure => GH_Exposure.hidden;

        public APSClassificationSpecParam()
            : base("APS Spec", "APSS", "APS spec", string.Empty, string.Empty)
        {
        }
    }
}
