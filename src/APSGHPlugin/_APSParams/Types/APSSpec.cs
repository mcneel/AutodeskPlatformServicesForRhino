using System;
using Grasshopper.Kernel.Types;

using AutodeskPlatformServices;

namespace APSGHPlugin.Types
{
    public class APSSpec : APSType<Spec>
    {
        public override bool IsValid => Value != null;
        public override string TypeName => $"APS Spec";
        public override string TypeDescription => $"Represents an APS spec";

        public APSSpec() { }

        public APSSpec(Spec value)
        {
            Value = value;
        }

        public override IGH_Goo Duplicate() => new APSSpec(Value);

        public override string ToString() => IsValid ? $"Spec: {Value})" : $"Invalid {TypeName}";
    }
}
