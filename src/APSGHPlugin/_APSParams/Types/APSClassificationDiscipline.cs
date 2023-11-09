using System;
using Grasshopper.Kernel.Types;

using AutodeskPlatformServices;

namespace APSGHPlugin.Types
{
    public class APSClassificationDiscipline : APSType<ClassificationDiscipline>
    {
        public override bool IsValid => Value != null;
        public override string TypeName => $"APS Discipline";
        public override string TypeDescription => $"Represents an APS Discipline";

        public APSClassificationDiscipline() { }

        public APSClassificationDiscipline(ClassificationDiscipline value)
        {
            Value = value;
        }

        public override IGH_Goo Duplicate() => new APSClassificationDiscipline(Value);

        public override string ToString() => IsValid ? $"Discipline: {Value.Name} ({Value.Id})" : $"Invalid {TypeName}";
    }
}
