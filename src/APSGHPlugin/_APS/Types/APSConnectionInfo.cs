using System;

using Grasshopper.Kernel.Types;

using AutodeskPlatformServices;

namespace APSGHPlugin.Types
{
    public class APSConnectionInfo : APSType<ConnectionInfo>
    {
        public override bool IsValid => Value != null;
        public override string TypeName => $"APS Connection Info";
        public override string TypeDescription => $"Represents connection info for APS API";

        public string AccountId { get; }

        public APSConnectionInfo() { }

        public APSConnectionInfo(ConnectionInfo value, string accountId)
        {
            Value = value;
            AccountId = accountId ?? string.Empty;
        }

        public override IGH_Goo Duplicate() => new APSConnectionInfo(Value, AccountId);

        public override string ToString()
        {
            if (!IsValid)
                return $"Invalid {TypeName}";

            if (string.IsNullOrEmpty(AccountId))
                return $"Connection: {Value.Id}";

            return $"Connection: @{AccountId} ({Value.Id})";
        }
    }
}
