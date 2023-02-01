using System;
using System.Web;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.Json.Serialization;

using RestSharp;

namespace AutodeskPlatformServices
{
    public class Pagination
    {
        [JsonPropertyName("offset")] public int Offset { get; set; }
        [JsonPropertyName("limit")] public int Limit { get; set; }
        [JsonPropertyName("totalResults")] public int TotalResults { get; set; }
        [JsonPropertyName("nextUrl")] public string NextUrl { get; set; }
    }

    public abstract class ListResult
    {
        [JsonPropertyName("pagination")] public Pagination Pages { get; set; }

        public bool HasMore => !string.IsNullOrEmpty(Pages?.NextUrl);

        public void UpdateRequest(RestRequest request)
        {
            if (string.IsNullOrEmpty(Pages.NextUrl))
                return;

            NameValueCollection parameters = HttpUtility.ParseQueryString(new Uri(Pages.NextUrl).Query);
            foreach (string key in parameters)
                request.AddParameter(key, parameters[key]);
        }
    }

    public class ListDisciplinesResult : ListResult
    {
        [JsonPropertyName("results")] public List<Discipline> Disciplines { get; set; } = new List<Discipline>();
    }

    public class ListSpecsResult : ListResult
    {
        [JsonPropertyName("results")] public List<Spec> Specs { get; set; } = new List<Spec>();
    }

    public class ListClassificationGroupResult : ListResult
    {
        [JsonPropertyName("results")] public List<ClassificationGroup> ClassificationGroups { get; set; } = new List<ClassificationGroup>();
    }

    public class ListClassificationCategoryResult : ListResult
    {
        [JsonPropertyName("results")] public List<ClassificationCategory> ClassificationCategories { get; set; } = new List<ClassificationCategory>();
    }

    public class ListCollectionsResult : ListResult
    {
        [JsonPropertyName("results")] public List<Collection> Collections { get; set; } = new List<Collection>();
    }

    public class ListParametersResult : ListResult
    {
        [JsonPropertyName("results")] public List<Parameter> Parameters { get; set; } = new List<Parameter>();
    }
}
