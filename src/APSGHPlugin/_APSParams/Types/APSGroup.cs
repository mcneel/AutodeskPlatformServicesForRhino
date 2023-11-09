using System;
using Grasshopper.Kernel.Types;

using AutodeskPlatformServices;

namespace APSGHPlugin.Types
{
    public class APSGroup : APSType<Group>
    {
        public override bool IsValid => Value != null;
        public override string TypeName => $"APS Parameter Group";
        public override string TypeDescription => $"Represents an APS Parameter Group";

        public string Id => Value?.Id.ToString() ?? string.Empty;
        public string AccountId { get; } = string.Empty;

        public APSGroup() { }

        public APSGroup(string accountId, Group value)
        {
            AccountId = accountId;
            Value = value;
        }

        public override IGH_Goo Duplicate() => new APSGroup(AccountId, Value);

        public override string ToString() => IsValid ? $"Group: {Value.Title} ({Value.Id})" : $"Invalid {TypeName}";
    }
}
