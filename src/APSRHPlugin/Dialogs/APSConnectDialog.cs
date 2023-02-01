using System;

using Eto.Forms;

using AutodeskPlatformServices;

namespace APSRHPlugin.Dialogs
{
    public class APSConnectDialog : BaseDialog
    {
        readonly DropDown _connectionType = new DropDown
        {
            Items =
            {
                { "Default", "default" },
                { "App Id & Secret", "id_secret" },
                { "App Id & Secret (Environment Variables)", "id_secret_env_vars" },
            },
            SelectedIndex = 0
        };

        readonly Button _connectButton = new Button { Text = "Connect", Enabled = true };

        readonly Button _cancelButton = new Button { Text = "Cancel", Style = "apsrhplugin.dialogs.button" };

        readonly TextBox _appId = new TextBox { Style = "apsrhplugin.dialogs.textBox" };

        readonly TextBox _appSecret = new TextBox { Style = "apsrhplugin.dialogs.textBox" };

        readonly TextBox _accountId = new TextBox { Style = "apsrhplugin.dialogs.textBox" };

        public ConnectionInfo Connection { get; private set; }

        public string Account { get; private set; }

        public APSConnectDialog()
        {
            Title = "Connection Settings";

            _connectionType.SelectedKeyChanged += (s, e) => PrepareForm();
            _cancelButton.Click += (s, e) => Cancel();
            _connectButton.Click += (s, e) => Submit();

            Width = 450;

            UpdateForm();

            Content = new TableLayout
            {
                Style = "apsrhplugin.dialogs.tableContent",
                Rows =
                {
                    new TableLayout
                    {
                        Style = "apsrhplugin.dialogs.tableLayout",
                        Rows = { _connectionType  }
                    },
                    new TableLayout
                    {
                        Style = "apsrhplugin.dialogs.tableLayout",
                        Rows = {
                            new TableRow {
                                Cells = {
                                    new Label { Text = "App Id", Width = 80 },
                                    new TableCell { Control = _appId, ScaleWidth = true }
                                }
                            },
                            new TableRow {
                                Cells = {
                                    new Label { Text = "App Secret" , Width = 80 },
                                    new TableCell { Control = _appSecret, ScaleWidth = true }
                                }
                            },
                            new TableRow {
                                Cells = {
                                    new Label { Text = "Account Id" , Width = 80 },
                                    new TableCell { Control = _accountId, ScaleWidth = true }
                                }
                            },
                        }
                    },
                    new TableLayout
                    {
                        Style = "apsrhplugin.dialogs.tableLayout",
                        Rows = { new TableRow { Cells = {
                            null,
                            _cancelButton,
                            _connectButton,
                        } } }
                    },
                }
            };

            DefaultButton = _connectButton;
        }

        protected override void Submit()
        {
            if (ValidateValues())
            {
                base.Submit();
                switch (_connectionType.SelectedKey)
                {
                    case "default":
                        Connection = APSAPI.DefaultConnectionInfo;
                        break;

                    case "id_secret_env_vars":
                        Connection = new ConnectionInfoFromEnvVars(_appId.Text, _appSecret.Text, null);
                        break;

                    default:
                    case "id_secret":
                        Connection = new ConnectionInfo(_appId.Text, _appSecret.Text);
                        break;
                }

                Account = _accountId.Text;
            }
        }

        bool ValidateValues()
        {
            if (string.IsNullOrEmpty(_appId.Text))
            {
                APSRhino.ReportError("App Id is required");
                return false;
            }

            if (string.IsNullOrEmpty(_appSecret.Text))
            {
                APSRhino.ReportError("App Secret is required");
                return false;
            }

            if (string.IsNullOrEmpty(_accountId.Text))
            {
                APSRhino.ReportError("Account Id is required");
                return false;
            }

            if (_connectionType.SelectedKey == "id_secret_env_vars")
            {
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(_appId.Text)))
                {
                    APSRhino.ReportError("App Id variable is not set");
                    return false;
                }

                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(_appSecret.Text)))
                {
                    APSRhino.ReportError("App Secret variable is not set");
                    return false;
                }

                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(_accountId.Text)))
                {
                    APSRhino.ReportError("Account Id variable is not set");
                    return false;
                }
            }

            return true;
        }

        void UpdateForm()
        {
            if (APSRhino.Configs.HasConnection())
            {
                switch (APSRhino.Configs.Connection)
                {
                    case ConnectionInfoFromEnvVars cievar:
                        if (cievar.Equals(APSAPI.DefaultConnectionInfo))
                        {
                            PrepareFormDefault();
                        }
                        else
                        {
                            _connectionType.SelectedKey = "id_secret_env_vars";
                            _appId.Text = cievar.IdEnvVar;
                            _appSecret.Text = cievar.SecretEnvVar;
                            _accountId.Text = APSRhino.Configs.Account;
                        }
                        break;

                    case ConnectionInfo cinfo:
                        _connectionType.SelectedKey = "id_secret";
                        _appId.Text = cinfo.Id;
                        _appSecret.Text = cinfo.Secret;
                        _accountId.Text = APSRhino.Configs.Account;
                        break;
                };
            }
            else
                PrepareFormDefault();
        }

        void PrepareFormDefault()
        {
            _connectionType.SelectedKey = "default";
            PrepareForm();
        }

        void PrepareForm()
        {
            switch (_connectionType.SelectedKey)
            {
                case "id_secret_env_vars":
                    _appId.Enabled = _appSecret.Enabled = _accountId.Enabled = true;
                    _appId.Text = _appSecret.Text = _accountId.Text = string.Empty;
                    break;

                case "id_secret":
                    _appId.Enabled = _appSecret.Enabled = _accountId.Enabled = true;
                    _appId.Text = _appSecret.Text = _accountId.Text = string.Empty;
                    break;

                default:
                case "default":
                    _appId.Enabled = _appSecret.Enabled = _accountId.Enabled = false;
                    _appId.Text = ConnectionInfoFromEnvVars.Default.IdEnvVar;
                    _appSecret.Text = ConnectionInfoFromEnvVars.Default.SecretEnvVar;
                    _accountId.Text = APSRHPluginConfigs.DEFAULT_ACCOUNT_ID_ENV_VAR;
                    break;
            };
        }
    }
}
