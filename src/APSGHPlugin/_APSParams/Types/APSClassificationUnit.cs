using System;
using Grasshopper.Kernel.Types;

using AutodeskPlatformServices;

namespace APSGHPlugin.Types
{
    public class APSClassificationUnit : APSType<ClassificationUnit>
    {
        public override bool IsValid => Value != null;
        public override string TypeName => $"APS Unit";
        public override string TypeDescription => $"Represents an APS unit";

        public APSClassificationUnit() { }

        public APSClassificationUnit(ClassificationUnit value)
        {
            Value = value;
        }

        public override IGH_Goo Duplicate() => new APSClassificationUnit(Value);

        public override string ToString() => IsValid ? $"Unit: {Value})" : $"Invalid {TypeName}";
    }
}
