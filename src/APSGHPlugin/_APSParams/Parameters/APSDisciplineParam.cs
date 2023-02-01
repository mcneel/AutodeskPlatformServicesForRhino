using System;

using Grasshopper.Kernel;

using APSGHPlugin.Types;

namespace APSGHPlugin.Parameters
{
    public class APSDisciplineParam : APSParam<APSDiscipline>
    {
        public override Guid ComponentGuid => new Guid("553DE7AF-5668-423E-ABA3-3B2EA2AD0E09");
        public override GH_Exposure Exposure => GH_Exposure.hidden;

        public APSDisciplineParam()
            : base("APS Discipline", "APSD", "APS discipline", string.Empty, string.Empty)
        {
        }
    }
}
