using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

using Autodesk.Forge;
using Autodesk.Forge.Client;

using RestSharp;

namespace AutodeskPlatformServices
{
    public delegate void StateChangedHandler(State from, State to);

    public enum State
    {
        Idle,
        Connecting,
        Errored,
        Connected,
    }

    public class Bearer : Autodesk.Forge.Model.Bearer
    {
        public static Bearer Empty { get; } = new Bearer();

        [Newtonsoft.Json.JsonConstructor]
        protected Bearer() { }

        private Bearer(Autodesk.Forge.Model.Bearer bearer)
        {
            TokenType = bearer.TokenType;
            ExpiresIn = bearer.ExpiresIn;
            ExpiresAt = bearer.ExpiresAt;
            AccessToken = bearer.AccessToken;
            RefreshToken = bearer.RefreshToken;
        }

        public Bearer(ConnectionInfo connection, ApiResponse<dynamic> bearer) : base(bearer)
        {
            AppId = connection.Id;
        }

        [DataMember(Name = "app_id", EmitDefaultValue = false)]
        public string AppId { get; set; } = string.Empty;

        [DataMember(Name = "created_at", EmitDefaultValue = false)]
        public long? CreatedAt { get; set; } = DateTimeOffset.Now.ToUnixTimeSeconds();

        public string GetToken() => TokenType + " " + AccessToken;

        public string GetRefreshToken() => TokenType + " " + RefreshToken;

        public bool IsExpired() => CreatedAt + ExpiresIn < DateTimeOffset.Now.ToUnixTimeSeconds();

        public static Bearer FromJson(string json)
        {
            var b = Newtonsoft.Json.JsonConvert.DeserializeObject<Bearer>(json);
            return new Bearer(b);
        }
    }

    [JsonConverter(typeof(ConnectionInfo.Converter))]
    public class ConnectionInfo : IEquatable<ConnectionInfo>
    {
        const uint DEFAULT_CALLBACK_PORT = 8080;

        public static ConnectionInfo Default { get; } = new ConnectionInfo();

        class Converter : JsonConverter<ConnectionInfo>
        {
            public override ConnectionInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException();

                int kind = 0;
                string id = string.Empty;
                string secret = string.Empty;
                string portEnvVar = string.Empty;
                uint port = DEFAULT_CALLBACK_PORT;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                        break;

                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        string propertyName = reader.GetString();
                        reader.Read();
                        switch (propertyName)
                        {
                            case "kind":
                                kind = reader.GetInt16();
                                break;

                            case "id":
                                id = reader.GetString();
                                break;

                            case "secret":
                                secret = reader.GetString();
                                break;

                            case "callback_port":
                                if (kind == 1)
                                    portEnvVar = reader.GetString();
                                else
                                    port = (uint)reader.GetInt32();
                                break;

                            default:
                                reader.Skip();
                                break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(secret))
                    throw new JsonException();

                switch (kind)
                {
                    case 1:
                        return new ConnectionInfoFromEnvVars(id, secret, portEnvVar);

                    default:
                        return new ConnectionInfo(id, secret, port);
                }
            }

            public override void Write(Utf8JsonWriter writer, ConnectionInfo value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();

                switch (value)
                {
                    case ConnectionInfoFromEnvVars cievar:
                        writer.WriteNumber("kind", 1);
                        writer.WriteString("id", cievar.IdEnvVar);
                        writer.WriteString("secret", cievar.SecretEnvVar);
                        writer.WriteString("callback_port", cievar.CallbackPortEnvVar);
                        break;

                    default:
                        writer.WriteNumber("kind", 0);
                        writer.WriteString("id", value.Id);
                        writer.WriteString("secret", value.Secret);
                        writer.WriteNumber("callback_port", value.CallbackUri.Port);
                        break;
                }

                writer.WriteEndObject();
            }
        }

        public virtual string Id { get; } = string.Empty;

        public virtual string Secret { get; } = string.Empty;

        public virtual Uri CallbackUri { get; } = new Uri($"http://localhost:{DEFAULT_CALLBACK_PORT}/");

        public Scope[] Scopes { get; } = new Scope[] {
            Scope.DataRead,
            Scope.DataWrite,
            Scope.DataSearch
        };

        protected ConnectionInfo() { }

        public ConnectionInfo(string id, string secret, uint port = DEFAULT_CALLBACK_PORT)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException("App Id can not be null");

            if (string.IsNullOrWhiteSpace(secret))
                throw new ArgumentNullException("App secret can not be null");

            Id = id;
            Secret = secret;

            // FIXME: find a way so this does not require this fix
            // http://stackoverflow.com/questions/4019466/httplistener-access-denied
            CallbackUri = new Uri($"http://localhost:{port}/");
        }

        public bool Equals(ConnectionInfo other)
        {
            if (other is null)
                return false;
            return GetHashCode() == other.GetHashCode();
        }

        public override bool Equals(object obj) => Equals(obj as ConnectionInfo);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Id.GetHashCode();
                hash = hash * 23 + Secret.GetHashCode();
                hash = hash * 23 + CallbackUri.GetHashCode();
                foreach (var scope in Scopes)
                    hash = hash * 23 + scope.GetHashCode();
                return hash;
            }
        }
    }

    public class ConnectionInfoFromEnvVars : ConnectionInfo
    {
        const string DEFAULT_APP_ID_ENV_VAR = "FORGE_APP_ID";
        const string DEFAULT_APP_SECRET_ENV_VAR = "FORGE_APP_SECRET";
        const string DEFAULT_CALLBACK_PORT_ENV_VAR = "FORGE_CALLBACK_PORT";

        public new static ConnectionInfoFromEnvVars Default { get; } = new ConnectionInfoFromEnvVars();

        public override string Id { get => Environment.GetEnvironmentVariable(IdEnvVar); }

        public override string Secret { get => Environment.GetEnvironmentVariable(SecretEnvVar); }

        public override Uri CallbackUri
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(CallbackPortEnvVar)
                        && Environment.GetEnvironmentVariable(CallbackPortEnvVar) is string port
                        && int.TryParse(port, out int portValue))
                    return new Uri($"http://localhost:{portValue}/");
                return ConnectionInfo.Default.CallbackUri;
            }
        }

        public string IdEnvVar { get; }

        public string SecretEnvVar { get; }

        public string CallbackPortEnvVar { get; }

        private ConnectionInfoFromEnvVars()
        {
            IdEnvVar = DEFAULT_APP_ID_ENV_VAR;
            SecretEnvVar = DEFAULT_APP_SECRET_ENV_VAR;
            CallbackPortEnvVar = DEFAULT_CALLBACK_PORT_ENV_VAR;
        }

        public ConnectionInfoFromEnvVars(string idEnvVar, string secretEnvVar, string portEnvVar)
        {
            IdEnvVar = idEnvVar ?? string.Empty;
            SecretEnvVar = secretEnvVar ?? string.Empty;
            CallbackPortEnvVar = portEnvVar ?? string.Empty;
        }

        public override bool Equals(object obj) => Equals(obj as ConnectionInfo);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + IdEnvVar.GetHashCode();
                hash = hash * 23 + SecretEnvVar.GetHashCode();
                hash = hash * 23 + CallbackPortEnvVar.GetHashCode();
                foreach (var scope in Scopes)
                    hash = hash * 23 + scope.GetHashCode();
                return hash;
            }
        }
    }

    public static class APSAPI
    {
        #region Fields
        const string TOKEN_CACHE_FILE = "apsapi_token.cache";
        static readonly RestClient _client = new RestClient("https://developer.api.autodesk.com/parameters/v1/");
        static readonly ManualResetEvent _init = new ManualResetEvent(false);

        static string s_tokenCache = Path.Combine(Path.GetTempPath(), TOKEN_CACHE_FILE);
        static Exception _error;
        #endregion

        public static ConnectionInfo DefaultConnectionInfo { get; } = ConnectionInfoFromEnvVars.Default;

        public static State State { get; private set; } = State.Idle;

        public static Bearer Bearer { get; private set; } = default;

        public static ConnectionInfo Connection { get; private set; }

        public static event StateChangedHandler StateChanged;

        class APSAPIListener
        {
            readonly ConnectionInfo _connection;
            readonly HttpListener _listener;
            readonly ThreeLeggedApi _api;

            State _currentState;

            private APSAPIListener(ConnectionInfo connectionInfo)
            {
                if (!HttpListener.IsSupported)
                    throw new Exception("HttpListener not supported");

                if (connectionInfo is null)
                    throw new ArgumentNullException(nameof(connectionInfo));

                _connection = connectionInfo;
                _listener = new HttpListener();
                _listener.Prefixes.Add(_connection.CallbackUri.AbsoluteUri.Replace("localhost", "+"));
                _listener.Start();

                _api = new ThreeLeggedApi();
            }

            public static void Listen(ConnectionInfo connectionInfo)
            {
                var cs = new APSAPIListener(connectionInfo)
                {
                    _currentState = State
                };

                cs._listener.BeginGetContext(WaitForTokenAsync, cs);

                string oauthUrl = cs._api.Authorize(
                    cs._connection.Id,
                    oAuthConstants.CODE,
                    cs._connection.CallbackUri.AbsoluteUri,
                    cs._connection.Scopes
                );
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(oauthUrl));
            }

            static async void WaitForTokenAsync(IAsyncResult ar)
            {
                APSAPIListener cs = (APSAPIListener)ar.AsyncState;

                try
                {
                    if (GetResponseCode(ar, cs, out string code))
                    {
                        ApiResponse<dynamic> bearer = await cs._api.GettokenAsyncWithHttpInfo(
                            cs._connection.Id,
                            cs._connection.Secret,
                            oAuthConstants.AUTHORIZATION_CODE,
                            code,
                            cs._connection.CallbackUri.AbsoluteUri
                         );

                        Bearer = new Bearer(cs._connection, bearer);
                        State = State.Connected;

                        StoreBearer();
                    }
                    else
                    {
                        _error = new Exception("Error receiving api token");
                        State = State.Errored;
                    }
                }
                catch (Exception ex)
                {
                    _error = ex;
                    State = State.Errored;
                }
                finally
                {
                    cs._listener.Stop();
                    _init.Set();

                    StateChanged?.Invoke(cs._currentState, State);
                }
            }

            static bool GetResponseCode(IAsyncResult ar, APSAPIListener cs, out string code)
            {
                var context = cs._listener.EndGetContext(ar);
                code = context.Request.QueryString[oAuthConstants.CODE];

                var responseString = "<html><body>You can now close this window!</body></html>";
                byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                var response = context.Response;
                response.ContentType = "text/html";
                response.ContentLength64 = buffer.Length;
                response.StatusCode = 200;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.OutputStream.Close();

                return !string.IsNullOrEmpty(code);
            }
        }

        public static State Connect(ConnectionInfo connectionInfo = null)
        {
            if (State.Connecting == State)
            {
                _init.WaitOne();
                return State;
            }

            else if (State.Errored == State)
            {
                _error = null;
                State = State.Idle;
            }

            else if (State.Connected == State)
            {
                _init.Reset();
                State = State.Idle;
                DeleteBearer();
            }

            try
            {
                Connection = connectionInfo ?? DefaultConnectionInfo;

                APSAPIListener.Listen(Connection);
                State = State.Connecting;

                _init.WaitOne();
            }
            catch (Exception ex)
            {
                _error = ex;
                State = State.Errored;
                return State;
            }

            return State;
        }

        public static Task<State> ConnectAsync(ConnectionInfo connectionInfo = null) => Task.Run(() => Connect(connectionInfo));

        public static State ReConnect(ConnectionInfo connectionInfo = null)
        {
            _init.Reset();
            State = State.Idle;
            DeleteBearer();
            return Connect(connectionInfo);
        }

        public static async Task<State> ReConnectAsync(ConnectionInfo connectionInfo = null)
        {
            State = State.Idle;
            DeleteBearer();
            return await ConnectAsync(connectionInfo);
        }

        public static void EnsureConnected()
        {
            switch (State)
            {
                case State.Connecting:
                    _init.WaitOne();
                    break;

                case State.Connected:
                    return;

                default:
                    throw new APINotAuthenticatedException();
            }
        }

        public static async Task EnsureConnectedAsync()
        {
            switch (State)
            {
                case State.Connecting:
                    await Task.Run(() => _init.WaitOne());
                    break;

                case State.Connected:
                    return;

                case State.Errored:
                case State.Idle:
                    throw new APINotAuthenticatedException();
            }
        }

        public static string GetErrorMessage() => _error?.Message ?? string.Empty;

        #region Parameters API
        public static class Parameters
        {
            static readonly ConcurrentDictionary<string, object> s_cache = new ConcurrentDictionary<string, object>();

            public static ListDisciplinesResult ListDisciplines(ListDisciplinesResult prev = null) => APIGet("disciplines", prev);

            public static Task<ListDisciplinesResult> ListDisciplinesAsync(ListDisciplinesResult prev = null) => APIGetAsync("disciplines", prev);

            public static Discipline GetDiscipline(string disciplineId)
            {
                if (string.IsNullOrWhiteSpace(disciplineId))
                    return null;

                if (s_cache.TryGetValue(disciplineId, out object discipline))
                    return (Discipline)discipline;

                EnsureConnected();
                var request = CreateRequest("disciplines");
                request.AddParameter("ids", disciplineId);

                ListDisciplinesResult res = _client.Get<ListDisciplinesResult>(request);
                if (res.Disciplines.FirstOrDefault() is Discipline d)
                {
                    s_cache.TryAdd(disciplineId, d);
                    return d;
                }

                return null;
            }

            public static async Task<Discipline> GetDisciplineAsync(string disciplineId)
            {
                if (string.IsNullOrWhiteSpace(disciplineId))
                    return null;

                if (s_cache.TryGetValue(disciplineId, out object discipline))
                    return (Discipline)discipline;

                await EnsureConnectedAsync();
                var request = CreateRequest("disciplines");
                request.AddParameter("ids", disciplineId);

                ListDisciplinesResult res = await _client.GetAsync<ListDisciplinesResult>(request);
                if (res.Disciplines.FirstOrDefault() is Discipline d)
                {
                    s_cache.TryAdd(disciplineId, d);
                    return d;
                }

                return null;
            }

            public static ListSpecsResult ListSpecs(ListSpecsResult prev = null) => APIGet("specs", prev);

            public static Task<ListSpecsResult> ListSpecsAsync(ListSpecsResult prev = null) => APIGetAsync("specs", prev);

            public static Spec GetSpec(string specId)
            {
                if (string.IsNullOrWhiteSpace(specId))
                    return null;

                if (s_cache.TryGetValue(specId, out object spec))
                    return (Spec)spec;

                EnsureConnected();
                var request = CreateRequest("specs");
                request.AddParameter("ids", specId);

                ListSpecsResult res = _client.Get<ListSpecsResult>(request);

                if (res.Specs.FirstOrDefault() is Spec s)
                {
                    s_cache.TryAdd(specId, s);
                    return s;
                }

                return null;
            }

            public static async Task<Spec> GetSpecAsync(string specId)
            {
                if (string.IsNullOrWhiteSpace(specId))
                    return null;

                if (s_cache.TryGetValue(specId, out object spec))
                    return (Spec)spec;

                await EnsureConnectedAsync();
                var request = CreateRequest("specs");
                request.AddParameter("ids", specId);

                ListSpecsResult res = await _client.GetAsync<ListSpecsResult>(request);

                if (res.Specs.FirstOrDefault() is Spec s)
                {
                    s_cache.TryAdd(specId, s);
                    return s;
                }

                return null;
            }

            public static ListClassificationGroupResult ListClassificationGroup(bool bindable, ListClassificationGroupResult prev = null)
                => APIGet(bindable ? $"classifications/groups?filter[bindable]=true" : $"classifications/groups", prev);

            public static Task<ListClassificationGroupResult> ListClassificationGroupAsync(bool bindable, ListClassificationGroupResult prev = null)
                => APIGetAsync(bindable ? $"classifications/groups?filter[bindable]=true" : $"classifications/groups", prev);

            public static ClassificationGroup GetGroup(string groupId)
            {
                if (string.IsNullOrWhiteSpace(groupId))
                    return null;

                if (s_cache.TryGetValue(groupId, out object category))
                    return (ClassificationGroup)category;

                EnsureConnected();
                var request = CreateRequest("classifications/groups");
                request.AddParameter("ids", groupId);

                ListClassificationGroupResult res = _client.Get<ListClassificationGroupResult>(request);

                if (res.ClassificationGroups.FirstOrDefault() is ClassificationGroup c)
                {
                    s_cache.TryAdd(groupId, c);
                    return c;
                }

                return null;
            }

            public static async Task<ClassificationGroup> GetGroupAsync(string groupId)
            {
                if (string.IsNullOrWhiteSpace(groupId))
                    return null;

                if (s_cache.TryGetValue(groupId, out object category))
                    return (ClassificationGroup)category;

                EnsureConnected();
                var request = CreateRequest("classifications/groups");
                request.AddParameter("ids", groupId);

                ListClassificationGroupResult res = await _client.GetAsync<ListClassificationGroupResult>(request);

                if (res.ClassificationGroups.FirstOrDefault() is ClassificationGroup c)
                {
                    s_cache.TryAdd(groupId, c);
                    return c;
                }

                return null;
            }

            public static IEnumerable<ClassificationGroup> GetGroups(IEnumerable<string> groupIds)
            {
                var categories = new HashSet<ClassificationGroup>();
                var missing = new HashSet<string>();
                foreach (string gid in groupIds)
                {
                    if (string.IsNullOrWhiteSpace(gid))
                        continue;

                    if (s_cache.TryGetValue(gid, out object group))
                        categories.Add((ClassificationGroup)group);
                    else
                        missing.Add(gid);
                }

                if (missing.Any())
                {
                    EnsureConnected();
                    var request = CreateRequest("classifications/groups");
                    request.AddParameter("ids", string.Join(",", missing));

                    ListClassificationGroupResult res = _client.Get<ListClassificationGroupResult>(request);
                    categories.UnionWith(res.ClassificationGroups);
                }

                return categories;
            }

            public static async Task<IEnumerable<ClassificationGroup>> GetGroupsAsync(IEnumerable<string> groupIds)
            {
                var categories = new HashSet<ClassificationGroup>();
                var missing = new HashSet<string>();
                foreach (string gid in groupIds)
                {
                    if (string.IsNullOrWhiteSpace(gid))
                        continue;

                    if (s_cache.TryGetValue(gid, out object group))
                        categories.Add((ClassificationGroup)group);
                    else
                        missing.Add(gid);
                }

                if (missing.Any())
                {
                    EnsureConnected();
                    var request = CreateRequest("classifications/groups");
                    request.AddParameter("ids", string.Join(",", missing));

                    ListClassificationGroupResult res = await _client.GetAsync<ListClassificationGroupResult>(request);
                    categories.UnionWith(res.ClassificationGroups);
                }

                return categories;
            }

            public static ListClassificationCategoryResult ListClassificationCategories(bool bindable, ListClassificationCategoryResult prev = null)
                => APIGet(bindable ? $"classifications/categories?filter[bindable]=true" : $"classifications/categories", prev);

            public static Task<ListClassificationCategoryResult> ListClassificationCategoriesAsync(bool bindable, ListClassificationCategoryResult prev = null)
                => APIGetAsync(bindable ? $"classifications/categories?filter[bindable]=true" : $"classifications/categories", prev);

            public static ClassificationCategory GetCategory(string categoryId)
            {
                if (string.IsNullOrWhiteSpace(categoryId))
                    return null;

                if (s_cache.TryGetValue(categoryId, out object category))
                    return (ClassificationCategory)category;

                EnsureConnected();
                var request = CreateRequest("classifications/categories");
                request.AddParameter("ids", categoryId);

                ListClassificationCategoryResult res = _client.Get<ListClassificationCategoryResult>(request);

                if (res.ClassificationCategories.FirstOrDefault() is ClassificationCategory c)
                {
                    s_cache.TryAdd(categoryId, c);
                    return c;
                }

                return null;
            }

            public static async Task<ClassificationCategory> GetCategoryAsync(string categoryId)
            {
                if (string.IsNullOrWhiteSpace(categoryId))
                    return null;

                if (s_cache.TryGetValue(categoryId, out object category))
                    return (ClassificationCategory)category;

                EnsureConnected();
                var request = CreateRequest("classifications/categories");
                request.AddParameter("ids", categoryId);

                ListClassificationCategoryResult res = await _client.GetAsync<ListClassificationCategoryResult>(request);

                if (res.ClassificationCategories.FirstOrDefault() is ClassificationCategory c)
                {
                    s_cache.TryAdd(categoryId, c);
                    return c;
                }

                return null;
            }

            public static IEnumerable<ClassificationCategory> GetCategories(IEnumerable<string> categoryIds)
            {
                var categories = new HashSet<ClassificationCategory>();
                var missing = new HashSet<string>();
                foreach (string cid in categoryIds)
                {
                    if (string.IsNullOrWhiteSpace(cid))
                        continue;

                    if (s_cache.TryGetValue(cid, out object category))
                        categories.Add((ClassificationCategory)category);
                    else
                        missing.Add(cid);
                }

                if (missing.Any())
                {
                    EnsureConnected();
                    var request = CreateRequest("classifications/categories");
                    request.AddParameter("ids", string.Join(",", missing));

                    ListClassificationCategoryResult res = _client.Get<ListClassificationCategoryResult>(request);
                    categories.UnionWith(res.ClassificationCategories);
                }

                return categories;
            }

            public static async Task<IEnumerable<ClassificationCategory>> GetCategoriesAsync(IEnumerable<string> categoryIds)
            {
                var categories = new HashSet<ClassificationCategory>();
                var missing = new HashSet<string>();
                foreach (string cid in categoryIds)
                {
                    if (string.IsNullOrWhiteSpace(cid))
                        continue;

                    if (s_cache.TryGetValue(cid, out object category))
                        categories.Add((ClassificationCategory)category);
                    else
                        missing.Add(cid);
                }

                if (missing.Any())
                {
                    EnsureConnected();
                    var request = CreateRequest("classifications/categories");
                    request.AddParameter("ids", string.Join(",", missing));

                    ListClassificationCategoryResult res = await _client.GetAsync<ListClassificationCategoryResult>(request);
                    categories.UnionWith(res.ClassificationCategories);
                }

                return categories;
            }

            public static ListCollectionsResult ListCollections(string accoundId, ListCollectionsResult prev = null)
            {
                if (string.IsNullOrWhiteSpace(accoundId))
                    throw new ArgumentNullException(nameof(accoundId));

                return APIGet($"accounts/{accoundId}/collections?offset=0", prev);
            }

            public static Task<ListCollectionsResult> ListCollectionsAsync(string accoundId, ListCollectionsResult prev = null)
            {
                if (string.IsNullOrWhiteSpace(accoundId))
                    throw new ArgumentNullException(nameof(accoundId));

                return APIGetAsync($"accounts/{accoundId}/collections?offset=0", prev);
            }

            public static ListParametersResult ListParameters(string accoundId, string collectionId, ListParametersResult prev = null)
            {
                if (string.IsNullOrWhiteSpace(accoundId))
                    throw new ArgumentNullException(nameof(accoundId));

                if (string.IsNullOrWhiteSpace(collectionId))
                    throw new ArgumentNullException(nameof(collectionId));

                return APIGet($"accounts/{accoundId}/collections/{collectionId}/parameters?offset=0", prev);
            }

            public static Task<ListParametersResult> ListParametersAsync(string accoundId, string collectionId, ListParametersResult prev = null)
            {
                if (string.IsNullOrWhiteSpace(accoundId))
                    throw new ArgumentNullException(nameof(accoundId));

                if (string.IsNullOrWhiteSpace(collectionId))
                    throw new ArgumentNullException(nameof(collectionId));

                return APIGetAsync($"accounts/{accoundId}/collections/{collectionId}/parameters?offset=0", prev);
            }

            public static Parameter GetParameter(string parameterId)
            {
                if (string.IsNullOrWhiteSpace(parameterId))
                    return null;

                if (s_cache.TryGetValue(parameterId, out object parameter))
                    return (Parameter)parameter;

                EnsureConnected();
                Parameter res = _client.Get<Parameter>(CreateRequest($"parameters/{parameterId}"));

                if (res is Parameter p)
                {
                    s_cache.TryAdd(parameterId, p);
                    return p;
                }

                return null;
            }

            public static async Task<Parameter> GetParameterAsync(string parameterId)
            {
                if (string.IsNullOrWhiteSpace(parameterId))
                    return null;

                if (s_cache.TryGetValue(parameterId, out object parameter))
                    return (Parameter)parameter;

                EnsureConnected();
                Parameter res = await _client.GetAsync<Parameter>(CreateRequest($"parameters/{parameterId}"));

                if (res is Parameter p && p.IsValid())
                {
                    s_cache.TryAdd(parameterId, p);
                    return p;
                }

                return null;
            }

            public static ListParametersResult SearchParameters(string accoundId, string collectionId, string searchTerm)
            {
                if (string.IsNullOrWhiteSpace(accoundId))
                    throw new ArgumentNullException(nameof(accoundId));

                if (string.IsNullOrWhiteSpace(collectionId))
                    throw new ArgumentNullException(nameof(collectionId));

                if (string.IsNullOrWhiteSpace(searchTerm))
                    throw new ArgumentNullException(nameof(searchTerm));

                EnsureConnected();
                var request = CreateRequest($"accounts/{accoundId}/collections/{collectionId}/parameters:search", Method.Post);
                request.AddBody($"{{\"searchedText\": \"{searchTerm}\"}}");

                return _client.Post<ListParametersResult>(request);
            }

            public static async Task<ListParametersResult> SearchParametersAsync(string accoundId, string collectionId, string searchTerm)
            {
                if (string.IsNullOrWhiteSpace(accoundId))
                    throw new ArgumentNullException(nameof(accoundId));

                if (string.IsNullOrWhiteSpace(collectionId))
                    throw new ArgumentNullException(nameof(collectionId));

                if (string.IsNullOrWhiteSpace(searchTerm))
                    throw new ArgumentNullException(nameof(searchTerm));

                await EnsureConnectedAsync();
                var request = CreateRequest($"accounts/{accoundId}/collections/{collectionId}/parameters:search", Method.Post);
                request.AddBody($"{{\"searchedText\": \"{searchTerm}\"}}");

                return await _client.PostAsync<ListParametersResult>(request);
            }
        }
        #endregion

        public static void Configure(string path)
        {
            Bearer = default;
            s_tokenCache = Path.Combine(path, TOKEN_CACHE_FILE);

            ReadBearer();
        }

        static RestRequest CreateRequest(string resource, Method method = Method.Get)
        {
            RestRequest request = new RestRequest(resource, method);
            request.AddHeader("Authorization", Bearer.GetToken());

            return request;
        }

        static RestRequest CreateRequest<T>(string resource, Method method = Method.Get, T prev = null) where T : ListResult
        {
            RestRequest request = CreateRequest(resource, method);
            prev?.UpdateRequest(request);

            return request;
        }

        static T APIGet<T>(string resource, T prev = null) where T : ListResult
        {
            EnsureConnected();
            return _client.Get<T>(CreateRequest(resource, Method.Get, prev));
        }

        static async Task<T> APIGetAsync<T>(string resource, T prev = null) where T : ListResult
        {
            await EnsureConnectedAsync();
            return await _client.GetAsync<T>(CreateRequest(resource, Method.Get, prev));
        }

        static APSAPI() => ReadBearer();

        static void ReadBearer()
        {
            Bearer = default;
            if (File.Exists(s_tokenCache))
            {
                try
                {
                    Bearer = Bearer.FromJson(File.ReadAllText(s_tokenCache));
                    if (Bearer is Bearer b)
                        State = b.IsExpired() ? State.Idle : State.Connected;
                }
                catch { }
            }
        }

        static void StoreBearer()
        {
            try
            {
                File.WriteAllText(s_tokenCache, Bearer.ToJson());
            }
            catch { }
        }

        static void DeleteBearer()
        {
            try
            {
                Bearer = default;
                File.Delete(s_tokenCache);
            }
            catch { }
        }
    }
}