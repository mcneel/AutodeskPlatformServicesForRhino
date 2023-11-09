using System;
using Grasshopper.Kernel.Types;

using AutodeskPlatformServices;

namespace APSGHPlugin.Types
{
    public class APSClassificationSpec : APSType<ClassificationSpec>
    {
        public override bool IsValid => Value != null;
        public override string TypeName => $"APS Spec";
        public override string TypeDescription => $"Represents an APS spec";

        public APSClassificationSpec() { }

        public APSClassificationSpec(ClassificationSpec value)
        {
            Value = value;
        }

        public override IGH_Goo Duplicate() => new APSClassificationSpec(Value);

        public override string ToString() => IsValid ? $"Spec: {Value})" : $"Invalid {TypeName}";
    }
}
