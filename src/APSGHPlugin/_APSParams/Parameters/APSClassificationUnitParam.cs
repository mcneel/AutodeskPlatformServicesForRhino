using System;
using Grasshopper.Kernel;

using APSGHPlugin.Types;

namespace APSGHPlugin.Parameters
{
    public class APSClassificationUnitParam : APSParam<APSClassificationUnit>
    {
        public override Guid ComponentGuid => new Guid("98A48985-FFB1-44C4-8ADE-BD9CAB7B1EDE");
        public override GH_Exposure Exposure => GH_Exposure.hidden;

        public APSClassificationUnitParam()
            : base("APS Unit", "APSU", "APS unit", string.Empty, string.Empty)
        {
        }
    }
}
