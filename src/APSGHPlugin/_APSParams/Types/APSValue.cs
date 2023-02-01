using System;
using Grasshopper.Kernel.Types;

using AutodeskPlatformServices;

namespace APSGHPlugin.Types
{
    public class APSValue : APSType<object>
    {
        public static APSValue GetInvalidValue(Parameter param) => new APSValue(param, null);

        public override bool IsValid => Value != null;
        public override string TypeName => $"APS Value";
        public override string TypeDescription => $"Represents an APS parameter value";

        public Parameter Parameter { get; }

        public APSValue() { }

        public APSValue(Parameter param, object value)
        {
            Parameter = param;
            Value = value;
        }

        public override IGH_Goo Duplicate() => new APSValue(Parameter, Value);

        public override string ToString()
        {
            if (Parameter.GetSpec() is Spec spec)
                return IsValid ? $"{Value} <{spec.Name} ({spec.Id})>" : $"Invalid {TypeName}";
            return IsValid ? $"{Value} <Unknown Spec>" : $"Invalid {TypeName}";
        }
    }
}
