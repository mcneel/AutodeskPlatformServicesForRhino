using System;

using Grasshopper.Kernel;

using APSGHPlugin.Types;

namespace APSGHPlugin.Parameters
{
    public class APSConnectionInfoParam : APSParam<APSConnectionInfo>
    {
        public override Guid ComponentGuid => new Guid("95A0A2A5-58F9-4F55-830D-62FEA7C2ED6D");
        public override GH_Exposure Exposure => GH_Exposure.hidden;

        public APSConnectionInfoParam()
            : base("APS Connection Info", "APSCI", "APS connection info", "APS", "APS")
        {
        }
    }
}
