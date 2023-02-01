using System;
using Grasshopper.Kernel;

using APSGHPlugin.Types;

namespace APSGHPlugin.Parameters
{
    public class APSParameterAssocParam : APSParam<APSParameterAssoc>
    {
        public override Guid ComponentGuid => new Guid("196E0461-3497-416C-B0D0-0584FFF07C8F");
        public override GH_Exposure Exposure => GH_Exposure.hidden;

        public APSParameterAssocParam()
            : base("APS Parameter Association", "APSPA", "APS parameter association", string.Empty, string.Empty)
        {
        }
    }
}
