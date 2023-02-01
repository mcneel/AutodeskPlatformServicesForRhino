using System;
using Grasshopper.Kernel.Types;

using AutodeskPlatformServices;

namespace APSGHPlugin.Types
{
    public class APSClassificationGroup : APSType<ClassificationGroup>
    {
        public override bool IsValid => Value != null;
        public override string TypeName => $"APS Classification Group";
        public override string TypeDescription => $"Represents an APS Classification Group";

        public APSClassificationGroup() { }

        public APSClassificationGroup(ClassificationGroup value)
        {
            Value = value;
        }

        public override IGH_Goo Duplicate() => new APSClassificationGroup(Value);

        public override string ToString() => IsValid ? $"Classification Group: {Value.Name} ({Value.Id})" : $"Invalid {TypeName}";
    }
}
