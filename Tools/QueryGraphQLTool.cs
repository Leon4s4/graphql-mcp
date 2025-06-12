using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Graphql.Mcp.Tools
{
    public class QueryGraphQlTool
    {
        private readonly HttpClient _httpClient;
        private readonly string _endpoint;
        private readonly Dictionary<string, string> _headers;
        private readonly bool _allowMutations;

        public QueryGraphQlTool(HttpClient httpClient, string endpoint, Dictionary<string, string> headers, bool allowMutations)
        {
            _httpClient = httpClient;
            _endpoint = endpoint;
            _headers = headers;
            _allowMutations = allowMutations;
        }

        public async Task<object> QueryAsync(string query, string? variables = null)
        {
            // Basic mutation detection (not a full parser)
            if (IsMutation(query) && !_allowMutations)
            {
                return new
                {
                    isError = true,
                    content = new[]
                    {
                        new { type = "text", text = "Mutations are not allowed unless you enable them in the configuration. Please use a query operation instead." }
                    }
                };
            }

           
                using var request = new HttpRequestMessage(HttpMethod.Post, _endpoint);
                foreach (var header in _headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
                var body = new
                {
                    query,
                    variables = string.IsNullOrWhiteSpace(variables) ? null : JsonSerializer.Deserialize<object>(variables)
                };
                request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                var response = await _httpClient.SendAsync(request);
                var responseText = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    return new
                    {
                        isError = true,
                        content = new[]
                        {
                            new { type = "text", text = $"GraphQL request failed: {response.ReasonPhrase}\n{responseText}" }
                        }
                    };
                }
                var data = JsonSerializer.Deserialize<JsonElement>(responseText);
                if (data.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Array && errors.GetArrayLength() > 0)
                {
                    return new
                    {
                        isError = true,
                        content = new[]
                        {
                            new { type = "text", text = $"The GraphQL response has errors, please fix the query: {JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true })}" }
                        }
                    };
                }
                return new
                {
                    content = new[]
                    {
                        new { type = "text", text = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }) }
                    }
                };
           
        }

        private bool IsMutation(string query)
        {
            // Very basic check for mutation operation
            var match = Regex.Match(query, @"\bmutation\b", RegexOptions.IgnoreCase);
            return match.Success;
        }
    }
}

