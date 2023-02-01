using System;
using Grasshopper.Kernel.Types;

using AutodeskPlatformServices;

namespace APSGHPlugin.Types
{
    public class APSParameter : APSType<Parameter>
    {
        public override bool IsValid => Value != null;
        public override string TypeName => $"APS Parameter";
        public override string TypeDescription => $"Represents an APS parameter";

        public APSParameter() { }

        public APSParameter(Parameter value)
        {
            Value = value;
        }

        public override IGH_Goo Duplicate() => new APSParameter(Value);

        public override string ToString() => IsValid ? $"Parameter: {Value.Name} ({Value.Id})" : $"Invalid {TypeName}";
    }
}
