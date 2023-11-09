using System;

using Grasshopper.Kernel;

using APSGHPlugin.Types;

namespace APSGHPlugin.Parameters
{
    public class APSGroupParam : APSParam<APSGroup>
    {
        public override Guid ComponentGuid => new Guid("FB86E2A5-D25A-4AAF-8DFB-C6D2F47DBFDB");
        public override GH_Exposure Exposure => GH_Exposure.hidden;

        public APSGroupParam()
            : base("APS Parameter Group", "APSPG", "APS parameter group", string.Empty, string.Empty)
        {
        }
    }
}
