using System;

using Grasshopper.Kernel.Types;

namespace APSGHPlugin
{
    public abstract class APSType<T> : GH_Goo<T>
    {
        public override bool IsValid => Value != null;

        public APSType() { }

        public APSType(T value)
        {
            Value = value;
        }
    }
}
