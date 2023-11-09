using System;
using System.IO;
using System.Reflection;

using Grasshopper.Kernel;

using SD = System.Drawing;

namespace APSGHPlugin
{
    public class PluginInfo : GH_AssemblyInfo
    {
        public override string Name => "AutodeskPlatformServices";

        public override string Description => "";

        public override Guid Id => new Guid("8AFC91A4-6940-483B-8854-45286B6EEF0F");

        public override string AuthorName => "Ehsan Iran-Nejad";

        public override string AuthorContact => "ehsan@mcneel.com";

        #region Plugin Resources
        static readonly Assembly s_assembly = typeof(PluginInfo).Assembly;

        static SD.Image GetIcon(Assembly assembly, string name)
        {
            string assemblyName = assembly.GetName().Name;
            try
            {
                using (Stream stream =
                    assembly.GetManifestResourceStream($"{assemblyName}.Icons.{name}.png"))
                {
                    return SD.Image.FromStream(stream);
                }
            }
            catch
            {
                return null;
            }
        }

        public static SD.Image GetIcon(string name) => GetIcon(s_assembly, name);

        public static SD.Bitmap GetBitmap(Assembly assembly, string name)
        {
            if (GetIcon(assembly, name) is SD.Image icon)
                return new SD.Bitmap(icon);
            return null;
        }
        #endregion
    }
}