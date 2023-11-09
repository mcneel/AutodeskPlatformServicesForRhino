using System;
using Grasshopper.Kernel.Types;

using AutodeskPlatformServices;

namespace APSGHPlugin.Types
{
    public class APSCollection : APSType<Collection>
    {
        public override bool IsValid => Value != null;
        public override string TypeName => $"APS Parameter Collection";
        public override string TypeDescription => $"Represents an APS Parameter Collection";

        public string Id => Value?.Id.ToString() ?? string.Empty;
        public string AccountId { get; } = string.Empty;
        public Group Group { get; }

        public APSCollection() { }

        public APSCollection(string accountId, Group group, Collection value)
        {
            AccountId = accountId;
            Group = group;
            Value = value;
        }

        public override IGH_Goo Duplicate() => new APSCollection(AccountId, Group, Value);

        public override string ToString() => IsValid ? $"Collection: {Value.Name} ({Value.Id})" : $"Invalid {TypeName}";
    }
}
