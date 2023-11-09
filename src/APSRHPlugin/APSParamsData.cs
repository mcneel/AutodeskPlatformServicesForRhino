using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Rhino.FileIO;
using Rhino.Collections;
using Rhino.DocObjects.Custom;

using AutodeskPlatformServices;

namespace APSRHPlugin
{
    [Guid("809E3EBD-DC76-4A28-9FDE-61E2AEF0734A")]
    public class APSParamsData : UserData
    {
        ArchivableDictionary _table = new ArchivableDictionary();

        public bool ContainsParameter(Parameter param) => _table.ContainsKey(param.Id);

        public IEnumerable<string> GetParameterIds() => _table.Keys.AsEnumerable();

        // FIXME: use autodesk api to deserialize value based on spec
        public bool GetParam(Parameter param, out object value) => _table.TryGetValue(param.Id, out value);

        // FIXME: use autodesk api to serialize value based on spec
        public bool SetParam(Parameter param, object value = default)
        {
            if (param.GetSpec() is ClassificationSpec spec)
            {
                if (spec.ValidateValue(value))
                    return _table.Set(param.Id, (value ?? string.Empty).ToString());

                throw new APSException($"Parameter value is invalid for \"{spec}\"");
            }

            throw new APSException($"Parameter has invalid spec");
        }

        public bool UnsetParam(Parameter param) => _table.Remove(param.Id);

        public override string Description => "Autodesk Parameters Table";

        public override bool ShouldWrite => _table.Any();

        protected override void OnDuplicate(UserData source)
        {
            if (source is APSParamsData src)
            {
                _table.Clear();
                _table.AddContentsFrom(src._table);
            }
        }

        protected override bool Read(BinaryArchiveReader archive)
        {
            _table = archive.ReadDictionary();
            return true;
        }

        protected override bool Write(BinaryArchiveWriter archive)
        {
            archive.WriteDictionary(_table);
            return true;
        }
    }
}
