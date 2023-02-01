using System;
using System.IO;
using System.Reflection;

using Rhino;
using Rhino.PlugIns;

using APSRHPlugin.Panels;

using SD = System.Drawing;

namespace APSRHPlugin
{
    public class APSRHPlugin : PlugIn
    {
        public APSRHPlugin()
        {
            Instance = this;
            Rhino.UI.Panels.RegisterPanel(this, typeof(APSParamsEditor), "APS Parameter Editor", GetIcon(typeof(APSParamsEditor).Name));
        }

        public static APSRHPlugin Instance { get; private set; }

        public override PlugInLoadTime LoadTime { get; } = PlugInLoadTime.AtStartup;

        protected override LoadReturnCode OnLoad(ref string errorMessage) => LoadReturnCode.Success;

        #region Plugin Resources
        static readonly Assembly s_assembly = typeof(APSRHPlugin).Assembly;

        static SD.Icon GetIcon(Assembly assembly, string name)
        {
            string assemblyName = assembly.GetName().Name;
            try
            {
                using (Stream stream =
                    assembly.GetManifestResourceStream($"{assemblyName}.Icons.{name}.png"))
                {
                    var img = SD.Image.FromStream(stream);
                    return SD.Icon.FromHandle(((SD.Bitmap)img).GetHicon());
                }
            }
            catch
            {
                return null;
            }
        }

        public static SD.Icon GetIcon(string name) => GetIcon(s_assembly, name);
        #endregion
    }
}
