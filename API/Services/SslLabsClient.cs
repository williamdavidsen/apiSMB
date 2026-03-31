using System.Text.Json;

namespace SecurityAssessmentAPI.Services
{
    public interface ISslLabsClient
    {
        Task<SslLabsResponse> AnalyzeAsync(string domain, CancellationToken cancellationToken = default);
    }

    public class SslLabsClient : ISslLabsClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SslLabsClient> _logger;

        public SslLabsClient(HttpClient httpClient, ILogger<SslLabsClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<SslLabsResponse> AnalyzeAsync(string domain, CancellationToken cancellationToken = default)
        {
            try
            {
                var url = $"https://api.ssllabs.com/api/v3/analyze?host={Uri.EscapeDataString(domain)}&all=done";
                _logger.LogInformation("Calling SSL Labs API: {Url}", url);

                using var response = await _httpClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(json))
                {
                    _logger.LogWarning("SSL Labs API returned an empty response: {Domain}", domain);
                    throw new InvalidOperationException("SSL Labs API returned an empty response");
                }

                var result = ParseSslLabsResponse(json);

                _logger.LogInformation(
                    "Received SSL Labs API response: Status={Status}, Host={Host}, Endpoints={EndpointCount}, Certs={CertCount}",
                    result.Status,
                    result.Host,
                    result.Endpoints.Count,
                    result.Certs.Count);

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "SSL Labs API request failed: {Domain}", domain);
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "SSL Labs API JSON parsing failed: {Domain}", domain);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SSL Labs API processing error: {Domain}", domain);
                throw;
            }
        }

        private static SslLabsResponse ParseSslLabsResponse(string json)
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            var response = new SslLabsResponse
            {
                Status = GetString(root, "status") ?? "IN_PROGRESS",
                Host = GetString(root, "host") ?? string.Empty
            };

            if (root.TryGetProperty("certs", out var certsElement) && certsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var certElement in certsElement.EnumerateArray())
                {
                    response.Certs.Add(new SslLabsCert
                    {
                        NotBefore = GetInt64(certElement, "notBefore"),
                        NotAfter = GetInt64(certElement, "notAfter"),
                        IssuerSubject = GetString(certElement, "issuerSubject") ?? string.Empty,
                        Subject = GetString(certElement, "subject") ?? string.Empty,
                        CommonNames = GetStringArray(certElement, "commonNames"),
                        AltNames = GetStringArray(certElement, "altNames")
                    });
                }
            }

            if (root.TryGetProperty("endpoints", out var endpointsElement) && endpointsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var endpointElement in endpointsElement.EnumerateArray())
                {
                    var endpoint = new SslLabsEndpoint
                    {
                        IpAddress = GetString(endpointElement, "ipAddress") ?? string.Empty,
                        ServerName = GetString(endpointElement, "serverName") ?? string.Empty,
                        Grade = GetString(endpointElement, "grade") ?? "F",
                        Details = ParseEndpointDetails(endpointElement)
                    };

                    response.Endpoints.Add(endpoint);
                }
            }

            return response;
        }

        private static SslLabsEndpointDetails ParseEndpointDetails(JsonElement endpointElement)
        {
            var details = new SslLabsEndpointDetails();

            if (!endpointElement.TryGetProperty("details", out var detailsElement) || detailsElement.ValueKind != JsonValueKind.Object)
            {
                return details;
            }

            if (detailsElement.TryGetProperty("protocols", out var protocolsElement) && protocolsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var protocolElement in protocolsElement.EnumerateArray())
                {
                    details.Protocols.Add(new SslLabsProtocol
                    {
                        Name = GetString(protocolElement, "name") ?? string.Empty,
                        Version = GetString(protocolElement, "version") ?? string.Empty,
                        V2SuitesDisabled = GetNullableBool(protocolElement, "v2SuitesDisabled")
                    });
                }
            }

            if (detailsElement.TryGetProperty("namedGroups", out var namedGroupsElement) && namedGroupsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var namedGroupElement in namedGroupsElement.EnumerateArray())
                {
                    details.NamedGroups.Add(new SslLabsNamedGroup
                    {
                        Name = GetString(namedGroupElement, "name") ?? string.Empty,
                        Bits = GetNullableInt32(namedGroupElement, "bits")
                    });
                }
            }

            if (detailsElement.TryGetProperty("suites", out var suitesElement))
            {
                var groupsElement = suitesElement.ValueKind switch
                {
                    JsonValueKind.Array => suitesElement,
                    JsonValueKind.Object when suitesElement.TryGetProperty("list", out var nestedList) && nestedList.ValueKind == JsonValueKind.Array => nestedList,
                    _ => default
                };

                if (groupsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var groupElement in groupsElement.EnumerateArray())
                    {
                        var group = new SslLabsProtocolSuiteGroup
                        {
                            Protocol = GetString(groupElement, "protocol") ?? string.Empty
                        };

                        if (groupElement.TryGetProperty("list", out var suiteListElement) && suiteListElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var suiteElement in suiteListElement.EnumerateArray())
                            {
                                group.List.Add(new SslLabsSuite
                                {
                                    Name = GetString(suiteElement, "name") ?? string.Empty,
                                    CipherStrength = GetInt32(suiteElement, "cipherStrength"),
                                    Q = GetNullableInt32(suiteElement, "q"),
                                    NamedGroupName = GetString(suiteElement, "namedGroupName") ?? string.Empty,
                                    NamedGroupBits = GetNullableInt32(suiteElement, "namedGroupBits")
                                });
                            }
                        }

                        details.Suites.Add(group);
                    }
                }
            }

            return details;
        }

        private static string? GetString(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var property))
            {
                return null;
            }

            return property.ValueKind switch
            {
                JsonValueKind.String => property.GetString(),
                JsonValueKind.Number => property.ToString(),
                JsonValueKind.True => bool.TrueString,
                JsonValueKind.False => bool.FalseString,
                _ => null
            };
        }

        private static long GetInt64(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var property))
            {
                return 0;
            }

            if (property.ValueKind == JsonValueKind.Number && property.TryGetInt64(out var numberValue))
            {
                return numberValue;
            }

            if (property.ValueKind == JsonValueKind.String && long.TryParse(property.GetString(), out var stringValue))
            {
                return stringValue;
            }

            return 0;
        }

        private static int GetInt32(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var property))
            {
                return 0;
            }

            if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var numberValue))
            {
                return numberValue;
            }

            if (property.ValueKind == JsonValueKind.String && int.TryParse(property.GetString(), out var stringValue))
            {
                return stringValue;
            }

            return 0;
        }

        private static int? GetNullableInt32(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var property))
            {
                return null;
            }

            if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var numberValue))
            {
                return numberValue;
            }

            if (property.ValueKind == JsonValueKind.String && int.TryParse(property.GetString(), out var stringValue))
            {
                return stringValue;
            }

            return null;
        }

        private static bool? GetNullableBool(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var property))
            {
                return null;
            }

            return property.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String when bool.TryParse(property.GetString(), out var stringValue) => stringValue,
                _ => null
            };
        }

        private static List<string> GetStringArray(JsonElement element, string propertyName)
        {
            var values = new List<string>();

            if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.Array)
            {
                return values;
            }

            foreach (var item in property.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    var value = item.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        values.Add(value);
                    }
                }
            }

            return values;
        }
    }
}
