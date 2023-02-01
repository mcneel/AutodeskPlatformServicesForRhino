using System;

using Grasshopper.Kernel;
using Rhino.PlugIns;

namespace APSGHPlugin
{
    public sealed class AssemblyPriority : GH_AssemblyPriority
    {
        static readonly Guid s_rhPluginId = new Guid("09985B32-FDB0-4CDF-92B0-193090A9E301");

        public override GH_LoadingInstruction PriorityLoad()
        {
            try
            {
                PlugIn.LoadPlugIn(s_rhPluginId);
                return GH_LoadingInstruction.Proceed;
            }
            catch
            {
                return GH_LoadingInstruction.Abort;
            }
        }
    }
}
