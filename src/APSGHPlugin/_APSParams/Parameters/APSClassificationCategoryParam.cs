using System;

using Grasshopper.Kernel;

using APSGHPlugin.Types;

namespace APSGHPlugin.Parameters
{
    public class APSClassificationCategoryParam : APSParam<APSClassificationCategory>
    {
        public override Guid ComponentGuid => new Guid("8095D2EF-451A-47CB-A336-634AFE45D894");
        public override GH_Exposure Exposure => GH_Exposure.hidden;

        public APSClassificationCategoryParam()
            : base("APS Classification Category", "APSCC", "APS classification category", string.Empty, string.Empty)
        {
        }
    }
}
