using System;

using Grasshopper.Kernel;

using APSGHPlugin.Types;

namespace APSGHPlugin.Parameters
{
    public class APSClassificationGroupParam : APSParam<APSClassificationGroup>
    {
        public override Guid ComponentGuid => new Guid("DC88998A-F00E-40AE-8DF3-292F818E8882");
        public override GH_Exposure Exposure => GH_Exposure.hidden;

        public APSClassificationGroupParam()
            : base("APS Classification Group", "APSCG", "APS classification group", string.Empty, string.Empty)
        {
        }
    }
}
