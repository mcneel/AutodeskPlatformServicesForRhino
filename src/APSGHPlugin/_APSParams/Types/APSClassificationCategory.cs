using System;
using Grasshopper.Kernel.Types;

using AutodeskPlatformServices;

namespace APSGHPlugin.Types
{
    public class APSClassificationCategory : APSType<ClassificationCategory>
    {
        public override bool IsValid => Value != null;
        public override string TypeName => $"APS Classification Category";
        public override string TypeDescription => $"Represents an APS Classification Category";

        public APSClassificationCategory() { }

        public APSClassificationCategory(ClassificationCategory value)
        {
            Value = value;
        }

        public override IGH_Goo Duplicate() => new APSClassificationCategory(Value);

        public override string ToString() => IsValid ? $"Classification Category: {Value.Name} ({Value.Id})" : $"Invalid {TypeName}";
    }
}
