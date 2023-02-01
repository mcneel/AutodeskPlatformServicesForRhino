using System;

using Grasshopper.Kernel;

using APSGHPlugin.Types;

namespace APSGHPlugin.Parameters
{
    public class APSCollectionParam : APSParam<APSCollection>
    {
        public override Guid ComponentGuid => new Guid("5BA9B248-C42A-423A-A927-8A05A3D94914");
        public override GH_Exposure Exposure => GH_Exposure.hidden;
        
        public APSCollectionParam()
            : base("APS Parameter Collection", "APSPC", "APS parameter collection", string.Empty, string.Empty)
        {
        }
    }
}
