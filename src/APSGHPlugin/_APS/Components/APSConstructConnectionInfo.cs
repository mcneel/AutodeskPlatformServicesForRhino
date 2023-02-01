using System;

using Grasshopper.Kernel;

using AutodeskPlatformServices;

using APSGHPlugin.Parameters;
using APSGHPlugin.Types;

namespace APSGHPlugin
{
    public class APSConstructConnectionInfo : APSComponent
    {
        public override Guid ComponentGuid => new Guid("4BB49AF7-F5F8-46F9-8CEF-CD607983F62D");
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        public APSConstructConnectionInfo()
          : base("Construct Connection Info", "CCI", "Construct connection info", "APS", "APS")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager PM)
        {
            PM.AddTextParameter("App Id", "ID", "APS application identifier", GH_ParamAccess.item);
            PM.AddTextParameter("App Secret", "S", "APS application secret", GH_ParamAccess.item);
            PM.AddTextParameter("Account Id", "AID", "Autodesk Account Id", GH_ParamAccess.item);
            PM[2].Optional = true;
            PM.AddIntegerParameter("Listener Port", "P", "Port to listen on to receive token from APS API", GH_ParamAccess.item);
            PM[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager PM)
        {
            PM.AddParameter(new APSConnectionInfoParam(), "Connection Info", "CI", "APS API connection info", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string appId = string.Empty;
            string appSecret = string.Empty;
            string accountId = string.Empty;
            int port = ConnectionInfo.Default.CallbackUri.Port;
            DA.GetData(0, ref appId);
            DA.GetData(1, ref appSecret);
            DA.GetData(2, ref accountId);
            DA.GetData(3, ref port);

            if (!(string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(appSecret)))
            {
                ConnectionInfo ci = new ConnectionInfo(appId, appSecret, (uint)port);
                DA.SetData(0, new APSConnectionInfo(ci, accountId));
            }
        }
    }
}
