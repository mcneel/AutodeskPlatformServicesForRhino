using System;

using Rhino;

using AutodeskPlatformServices;

namespace APSRHPlugin
{
    public class APSRHPluginConfigs
    {
        public const string DEFAULT_ACCOUNT_ID_ENV_VAR = "FORGE_ACCOUNT_ID";

        #region Configs
        public ConnectionInfo ConnectionInfo { get; set; }

        public string Account { get; set; }
        #endregion

        public bool HasConnection() => ConnectionInfo != null;

        public bool HasAccountId() => !string.IsNullOrEmpty(Account);

        public string GetAccountId()
        {
            if (ConnectionInfo is ConnectionInfoFromEnvVars)
                return Environment.GetEnvironmentVariable(Account);
            return Account;
        }

        public APSRHPluginConfigs() { }

        public static APSRHPluginConfigs Read()
        {
            var cfgs = new APSRHPluginConfigs();
            var settings = APSRHPlugin.Instance.Settings;

            // read account
            if (settings.TryGetString("account", out string account))
                cfgs.Account = account;

            // read connection
            if (settings.TryGetChild("connection", out PersistentSettings csettings))
            {
                int kind = csettings.GetInteger("kind");

                string id = csettings.GetString("id");
                string secret = csettings.GetString("secret");
                if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(secret))
                    return cfgs;

                string callbackPort = csettings.GetString("callback_port");
                switch (kind)
                {
                    case 1:
                        cfgs.ConnectionInfo = new ConnectionInfoFromEnvVars(id, secret, callbackPort);
                        break;

                    default:
                        if (uint.TryParse(callbackPort, out uint port))
                            cfgs.ConnectionInfo = new ConnectionInfo(id, secret, port);
                        else
                            cfgs.ConnectionInfo = new ConnectionInfo(id, secret);
                        break;
                }
            }

            return cfgs;
        }

        public void Write()
        {
            var settings = APSRHPlugin.Instance.Settings;

            // write account
            settings.SetString("account", Account);

            // write connection
            var csettings = settings.AddChild("connection");
            switch (ConnectionInfo)
            {
                case ConnectionInfoFromEnvVars civars:
                    csettings.SetInteger("kind", 1);
                    csettings.SetString("id", civars.IdEnvVar);
                    csettings.SetString("secret", civars.SecretEnvVar);
                    csettings.SetString("callback_port", civars.CallbackPortEnvVar);
                    break;

                case ConnectionInfo cinfo:
                    csettings.SetInteger("kind", 0);
                    csettings.SetString("id", cinfo.Id);
                    csettings.SetString("secret", cinfo.Secret);
                    csettings.SetInteger("callback_port", cinfo.CallbackUri.Port);
                    break;
            }

            APSRHPlugin.Instance.SaveSettings();
        }
    }
}
