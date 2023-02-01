using System;

using Grasshopper.Kernel;

using AutodeskPlatformServices;

using APSGHPlugin.Parameters;
using APSGHPlugin.Types;

namespace APSGHPlugin
{
    public class APSConstructConnectionInfoFromEnvVars : APSComponent
    {
        public override Guid ComponentGuid => new Guid("FB8A0507-39D1-4A3D-AF5F-0E80C48CD1E5");
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        public APSConstructConnectionInfoFromEnvVars()
          : base("Construct Connection Info (Env Vars)", "CCIE", "Construct connection info from environment variables", "APS", "APS")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager PM)
        {
            PM.AddTextParameter("App Id Variable", "ID", "Environment variable name for APS application identifier", GH_ParamAccess.item);
            PM.AddTextParameter("App Secret Variable", "S", "Environment variable name for APS application secret", GH_ParamAccess.item);
            PM.AddTextParameter("Account Id Variable", "AID", "Environment variable name for Autodesk Account Id", GH_ParamAccess.item);
            PM[2].Optional = true;
            PM.AddTextParameter("Listener Port Variable", "P", "Environment variable name for port to listen on to receive token from APS API", GH_ParamAccess.item);
            PM[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager PM)
        {
            PM.AddParameter(new APSConnectionInfoParam(), "Connection Info", "CI", "APS API connection info", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string appIdEnvVar = string.Empty;
            string appSecretEnvVar = string.Empty;
            string accountIdEnvVar = string.Empty;
            string portEnvVar = string.Empty;
            DA.GetData(0, ref appIdEnvVar);
            DA.GetData(1, ref appSecretEnvVar);
            DA.GetData(2, ref accountIdEnvVar);
            DA.GetData(3, ref portEnvVar);

            if (!(string.IsNullOrWhiteSpace(appIdEnvVar) || string.IsNullOrWhiteSpace(appSecretEnvVar)))
            {
                ConnectionInfo ci = new ConnectionInfoFromEnvVars(appIdEnvVar, appSecretEnvVar, portEnvVar);
                DA.SetData(0, new APSConnectionInfo(ci, Environment.GetEnvironmentVariable(accountIdEnvVar)));
            }
        }
    }
}
