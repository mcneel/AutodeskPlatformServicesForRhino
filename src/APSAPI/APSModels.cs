using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AutodeskPlatformServices
{
    public class ClassificationDiscipline
    {
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }

        public override int GetHashCode() => Id.GetHashCode();
    }

    public class ClassificationSpec
    {
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("disciplineId")] public string DisciplineId { get; set; }
        [JsonPropertyName("applicableUnitIds")] public List<string> ApplicableUnitIds { get; set; }
        [JsonPropertyName("storageUnitId")] public string StorageUnitId { get; set; }

        public ClassificationDiscipline GetDiscipline() => APSAPI.Parameters.Classifications.GetDiscipline(DisciplineId);

        public async Task<ClassificationDiscipline> GetDisciplineAsync() => await APSAPI.Parameters.Classifications.GetDisciplineAsync(DisciplineId);

        // FIXME: use autodesk api to serialize validate value based on spec
        public bool ValidateValue(object _)
        {
            return true;
        }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Id))
                return "Invalid Spec";
            return $"{Name} ({Id})";
        }

        public override int GetHashCode() => Id.GetHashCode();
    }

    public class ClassificationUnit
    {
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("symbolIds")] public List<string> SymbolIds { get; set; }

        public override int GetHashCode() => Id.GetHashCode();
    }

    public class ClassificationGroup
    {
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("bindingId")] public string BindingId { get; set; }

        [JsonIgnore] public bool IsBindable => !string.IsNullOrEmpty(BindingId);

        public override int GetHashCode() => Id.GetHashCode();
    }

    // NOTE: this is equivalent to Revit Categories
    public class ClassificationCategory
    {
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("disciplineIds")] public List<string> DisciplineIds { get; set; }
        [JsonPropertyName("parentCategoryId")] public string ParentCategoryId { get; set; }
        [JsonPropertyName("bindingId")] public string BindingId { get; set; }

        public override int GetHashCode() => Id.GetHashCode();
    }

    public class Group
    {
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("title")] public string Title { get; set; }
        [JsonPropertyName("description")] public string Description { get; set; }
        [JsonPropertyName("createdBy")] public string CreatedBy { get; set; }
        [JsonPropertyName("createdAt")] public string CreatedAt { get; set; }
        [JsonPropertyName("updatedBy")] public string UpdatedBy { get; set; }
        [JsonPropertyName("updatedAt")] public string UpdatedAt { get; set; }

        public override int GetHashCode() => Id.GetHashCode();
    }

    public class Collection
    {
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("permissions")] public List<string> Permissions { get; set; }

        public override int GetHashCode() => Id.GetHashCode();
    }

    public class Parameter
    {
        [JsonConverter(typeof(MetadataEntry.Converter))]
        public abstract class MetadataEntry
        {
            public class Converter : JsonConverter<MetadataEntry>
            {
                public override MetadataEntry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                {
                    if (reader.TokenType != JsonTokenType.StartObject)
                        throw new JsonException();

                    reader.Read();
                    if (reader.TokenType != JsonTokenType.PropertyName)
                        throw new JsonException();

                    string propertyName = reader.GetString();
                    if (propertyName != idName)
                        throw new JsonException();

                    reader.Read();
                    if (reader.TokenType != JsonTokenType.String)
                        throw new JsonException();

                    string id = reader.GetString();
                    MetadataEntry entry = default;

                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.EndObject)
                            return entry;

                        if (reader.TokenType == JsonTokenType.PropertyName)
                        {
                            propertyName = reader.GetString();
                            if (propertyName != valueName)
                                throw new JsonException();

                            reader.Read();
                            switch (id)
                            {
                                case IsHiddenEntry.EntryId:
                                    entry = new IsHiddenEntry()
                                    {
                                        Value = reader.GetBoolean()
                                    };
                                    break;

                                case AssociationEntry.EntryId:
                                    Association ev = Association.None;
                                    switch (reader.GetString())
                                    {
                                        case "NONE":
                                            ev = Association.None;
                                            break;
                                        case "INSTANCE":
                                            ev = Association.Instance;
                                            break;
                                        case "TYPE":
                                            ev = Association.Type;
                                            break;
                                    }
                                    entry = new AssociationEntry()
                                    {
                                        Value = ev
                                    };
                                    break;

                                case CategoryEntry.EntryId:
                                    entry = new CategoryEntry()
                                    {
                                        Value = JsonSerializer.Deserialize<List<ClassificationCategory>>(ref reader, options)
                                    };
                                    break;

                                case GroupEntry.EntryId:
                                    entry = new GroupEntry()
                                    {
                                        Value = JsonSerializer.Deserialize<ClassificationGroup>(ref reader, options)
                                    };
                                    break;

                                default:
                                    reader.Skip();
                                    break;
                            }
                        }
                    }

                    throw new JsonException();
                }

                public override void Write(Utf8JsonWriter writer, MetadataEntry value, JsonSerializerOptions options)
                {
                    writer.WriteStartObject();
                    writer.WriteString(idName, value.Id);

                    switch (value)
                    {
                        case IsHiddenEntry ishidden:
                            writer.WriteBoolean(valueName, ishidden.Value);
                            break;

                        case AssociationEntry assoc:
                            switch (assoc.Value)
                            {
                                case Association.None:
                                    writer.WriteString(valueName, "NONE");
                                    break;
                                case Association.Instance:
                                    writer.WriteString(valueName, "INSTANCE");
                                    break;
                                case Association.Type:
                                    writer.WriteString(valueName, "TYPE");
                                    break;
                            }
                            break;

                        case CategoryEntry cat:
                            JsonSerializer.Serialize(writer, cat.Value, options);
                            break;

                        case GroupEntry group:
                            JsonSerializer.Serialize(writer, group.Value, options);
                            break;
                    }

                    writer.WriteEndObject();
                }
            }

            protected const string idName = "id";
            protected const string valueName = "value";

            [JsonPropertyName(idName)] public abstract string Id { get; }
        }

        public abstract class MetadataEntry<T> : MetadataEntry
        {
            [JsonPropertyName(valueName)] public T Value { get; set; }
        }

        public sealed class IsHiddenEntry : MetadataEntry<bool>
        {
            public const string EntryId = "isHidden";
            public override string Id { get; } = EntryId;
        }

        public enum Association
        {
            None,
            Instance,
            Type,
        }

        public sealed class AssociationEntry : MetadataEntry<Association>
        {
            public const string EntryId = "instanceTypeAssociation";
            public override string Id { get; } = EntryId;
        }

        public sealed class CategoryEntry : MetadataEntry<List<ClassificationCategory>>
        {
            public const string EntryId = "categories";
            public override string Id { get; } = EntryId;
        }

        public sealed class GroupEntry : MetadataEntry<ClassificationGroup>
        {
            public const string EntryId = "group";
            public override string Id { get; } = EntryId;
        }

        const string idName = "id";
        const string nameName = "name";
        const string descriptionName = "description";
        const string specIdName = "specId";
        const string readOnlyName = "readOnly";
        const string metadataName = "metadata";

        [JsonPropertyName(idName)] public string Id { get; set; }
        [JsonPropertyName(nameName)] public string Name { get; set; }
        [JsonPropertyName(descriptionName)] public string Description { get; set; }
        [JsonPropertyName(specIdName)] public string SpecId { get; set; }
        [JsonPropertyName(readOnlyName)] public bool ReadOnly { get; set; }

        [JsonPropertyName(metadataName)] public List<MetadataEntry> Metadata { get; set; } = new List<MetadataEntry>();

        public bool IsValid() => !string.IsNullOrEmpty(Id);

        public ClassificationSpec GetSpec() => APSAPI.Parameters.Classifications.GetSpec(SpecId);

        public async Task<ClassificationSpec> GetSpecAsync() => await APSAPI.Parameters.Classifications.GetSpecAsync(SpecId);

        public bool IsHidden
        {
            get
            {
                if (Metadata.OfType<IsHiddenEntry>()
                            .FirstOrDefault() is IsHiddenEntry ishidden)
                    return ishidden.Value;
                return false;
            }
        }

        public Association GetAssociation()
        {
            if (Metadata.OfType<AssociationEntry>()
                        .FirstOrDefault() is AssociationEntry assoc)
                return assoc.Value;
            return Association.None;
        }

        public IEnumerable<ClassificationCategory> GetCategories()
        {
            if (Metadata.OfType<CategoryEntry>()
                        .FirstOrDefault() is CategoryEntry cat
                        && cat.Value is IEnumerable<ClassificationCategory> cats)
                return APSAPI.Parameters.Classifications.GetCategories(cats.Select(c => c.Id));
            return Enumerable.Empty<ClassificationCategory>();
        }

        public async Task<IEnumerable<ClassificationCategory>> GetCategoriesAsync()
        {
            if (Metadata.OfType<CategoryEntry>()
                        .FirstOrDefault() is CategoryEntry cat
                        && cat.Value is IEnumerable<ClassificationCategory> cats)
                return await APSAPI.Parameters.Classifications.GetCategoriesAsync(cats.Select(c => c.Id));
            return Enumerable.Empty<ClassificationCategory>();
        }

        public ClassificationGroup GetGroup()
        {
            if (Metadata.OfType<GroupEntry>()
                        .FirstOrDefault() is GroupEntry group
                        && group.Value is ClassificationGroup g)
                return APSAPI.Parameters.Classifications.GetGroup(g.Id);
            return null;
        }

        public async Task<ClassificationGroup> GetGroupAsync()
        {
            if (Metadata.OfType<GroupEntry>()
                        .FirstOrDefault() is GroupEntry group
                        && group.Value is ClassificationGroup g)
                return await APSAPI.Parameters.Classifications.GetGroupAsync(g.Id);
            return null;
        }

        public override int GetHashCode() => Id.GetHashCode();
    }
}
