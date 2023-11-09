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

    public abstract class Result
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

    public class GetDisciplinesResult : Result
    {
        [JsonPropertyName("results")] public List<ClassificationDiscipline> Disciplines { get; set; } = new List<ClassificationDiscipline>();
    }

    public class GetClassificationSpecsResult : Result
    {
        [JsonPropertyName("results")] public List<ClassificationSpec> Specs { get; set; } = new List<ClassificationSpec>();
    }

    public class GetClassificationUnitsResult : Result
    {
        [JsonPropertyName("results")] public List<ClassificationUnit> Specs { get; set; } = new List<ClassificationUnit>();
    }

    public class GetClassificationGroupsResult : Result
    {
        [JsonPropertyName("results")] public List<ClassificationGroup> ClassificationGroups { get; set; } = new List<ClassificationGroup>();
    }

    public class GetClassificationCategoryResult : Result
    {
        [JsonPropertyName("results")] public List<ClassificationCategory> ClassificationCategories { get; set; } = new List<ClassificationCategory>();
    }

    public class GetGroupsResult : Result
    {
        [JsonPropertyName("results")] public List<Group> Groups { get; set; } = new List<Group>();
    }

    public class GetCollectionsResult : Result
    {
        [JsonPropertyName("results")] public List<Collection> Collections { get; set; } = new List<Collection>();
    }

    public class GetParametersResult : Result
    {
        [JsonPropertyName("results")] public List<Parameter> Parameters { get; set; } = new List<Parameter>();
    }
}
