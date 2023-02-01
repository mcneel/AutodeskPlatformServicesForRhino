using System;
using Grasshopper.Kernel.Types;

using AutodeskPlatformServices;

namespace APSGHPlugin.Types
{
    public class APSParameterAssoc : APSType<Parameter.Association>
    {
        public override bool IsValid => true;
        public override string TypeName => $"APS Parameter Association";
        public override string TypeDescription => $"Represents an APS parameter association";

        public APSParameterAssoc() { }

        public APSParameterAssoc(Parameter.Association value)
        {
            Value = value;
        }

        public override IGH_Goo Duplicate() => new APSParameterAssoc(Value);

        public override string ToString() => $"Association: {Value}";
    }
}
