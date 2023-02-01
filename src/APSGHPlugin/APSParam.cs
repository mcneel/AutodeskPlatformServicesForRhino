using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using SD = System.Drawing;

namespace APSGHPlugin
{
    public abstract class APSParam<T> : GH_PersistentParam<T> where T : class, IGH_Goo
    {
        protected override SD.Bitmap Icon => ((SD.Bitmap)PluginInfo.GetIcon(GetType().Name)) ??
                                             ((SD.Bitmap)PluginInfo.GetIcon(typeof(APSParam<>).Name));

        protected APSParam(string name, string nickname, string description, string category, string subCategory)
            : base(name, nickname, description, category, subCategory)
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<T> values) => throw new NotImplementedException();
        protected override GH_GetterResult Prompt_Singular(ref T value) => throw new NotImplementedException();
    }
}
