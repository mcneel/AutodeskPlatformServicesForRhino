using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.DocObjects;

using SD = System.Drawing;

namespace APSGHPlugin
{
    public abstract class APSComponent : GH_Component
    {
        protected override SD.Bitmap Icon => ((SD.Bitmap)PluginInfo.GetIcon(GetType().Name)) ??
                                             ((SD.Bitmap)PluginInfo.GetIcon(typeof(APSComponent).Name));

        protected APSComponent(string name, string nickname, string description, string category, string subCategory)
            : base(name, nickname, description, category, subCategory)
        {
        }

        /// <summary>
        /// Utility method to run an async function synchronosly on UI thread
        /// </summary>
        /// <typeparam name="TRes">Return type of async function</typeparam>
        /// <param name="function">async function to run</param>
        /// <returns>Value returned from async function</returns>
        protected TRes RunSync<TRes>(Func<Task<TRes>> function) => Task.Run<TRes>(() => function()).GetAwaiter().GetResult();

        /// <summary>
        /// Return object reference that points to given geometry data
        /// </summary>
        /// <param name="geom">Geometry data to find object reference to</param>
        /// <returns>Object reference</returns>
        protected ObjRef ToObjectRef(IGH_GeometricGoo geom) => new ObjRef(null, geom.ReferenceID);
    }
}