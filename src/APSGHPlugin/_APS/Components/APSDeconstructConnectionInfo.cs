using System;

using Grasshopper.Kernel;

using AutodeskPlatformServices;

using APSGHPlugin.Parameters;
using APSGHPlugin.Types;

namespace APSGHPlugin
{
    public class APSDeconstructConnectionInfo : APSComponent
    {
        public override Guid ComponentGuid => new Guid("75C0C9F2-E65E-4CD5-94B3-D456D268F42C");
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        public APSDeconstructConnectionInfo()
          : base("Deconstruct Connection Info", "DCCI", "Deconstruct connection info", "APS", "APS")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager PM)
        {
            PM.AddParameter(new APSConnectionInfoParam(), "Connection Info", "CI", "APS API connection info", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager PM)
        {
            PM.AddTextParameter("App Id", "ID", "APS application identifier", GH_ParamAccess.item);
            PM.AddTextParameter("App Secret", "S", "APS application secret", GH_ParamAccess.item);
            PM.AddTextParameter("Account Id", "AID", "Autodesk Account Id", GH_ParamAccess.item);
            PM.AddTextParameter("Callback Uri", "U", "Callback uri", GH_ParamAccess.item);
            PM.AddTextParameter("Token", "T", "APS connection token", GH_ParamAccess.item);
            PM.AddTextParameter("Refresh Token", "RT", "APS connection refresh token", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            APSConnectionInfo conn = default;
            if (DA.GetData(0, ref conn))
            {
                ConnectionInfo info = conn.Value;
                DA.SetData(0, info.Id);
                DA.SetData(1, info.Secret);
                DA.SetData(2, conn.AccountId);
                DA.SetData(3, info.CallbackUri.ToString());
                DA.SetData(4, APSAPI.Bearer?.GetToken());
                DA.SetData(5, APSAPI.Bearer?.GetRefreshToken());
            }
        }
    }
}
