using System.Text.Json;

namespace SecurityAssessmentAPI.Services
{
    public class MozillaObservatoryScanResponse
    {
        public long Id { get; set; }
        public string DetailsUrl { get; set; } = string.Empty;
        public int AlgorithmVersion { get; set; }
        public DateTimeOffset? ScannedAt { get; set; }
        public string? Error { get; set; }
        public string Grade { get; set; } = "UNKNOWN";
        public int Score { get; set; }
        public int StatusCode { get; set; }
        public int TestsFailed { get; set; }
        public int TestsPassed { get; set; }
        public int TestsQuantity { get; set; }
    }

    public interface IMozillaObservatoryClient
    {
        Task<MozillaObservatoryScanResponse?> ScanAsync(string domain, CancellationToken cancellationToken = default);
    }

    public class MozillaObservatoryClient : IMozillaObservatoryClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MozillaObservatoryClient> _logger;

        public MozillaObservatoryClient(HttpClient httpClient, ILogger<MozillaObservatoryClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<MozillaObservatoryScanResponse?> ScanAsync(string domain, CancellationToken cancellationToken = default)
        {
            var url = $"https://observatory-api.mdn.mozilla.net/api/v2/scan?host={Uri.EscapeDataString(domain)}";

            try
            {
                _logger.LogInformation("Calling Mozilla Observatory API: {Url}", url);

                using var response = await _httpClient.PostAsync(url, content: null, cancellationToken);
                var json = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Mozilla Observatory returned non-success status: Domain={Domain}, Status={StatusCode}, Body={Body}",
                        domain, (int)response.StatusCode, json);
                    return null;
                }

                if (string.IsNullOrWhiteSpace(json))
                {
                    _logger.LogWarning("Mozilla Observatory returned an empty response: {Domain}", domain);
                    return null;
                }

                var result = ParseScanResponse(json);

                _logger.LogInformation("Received Mozilla Observatory response: Domain={Domain}, Grade={Grade}, Score={Score}, TestsPassed={TestsPassed}/{TestsQuantity}",
                    domain, result.Grade, result.Score, result.TestsPassed, result.TestsQuantity);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Mozilla Observatory request failed: {Domain}", domain);
                return null;
            }
        }

        private static MozillaObservatoryScanResponse ParseScanResponse(string json)
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            return new MozillaObservatoryScanResponse
            {
                Id = GetInt64(root, "id"),
                DetailsUrl = GetString(root, "details_url") ?? string.Empty,
                AlgorithmVersion = GetInt32(root, "algorithm_version"),
                ScannedAt = GetDateTimeOffset(root, "scanned_at"),
                Error = GetString(root, "error"),
                Grade = GetString(root, "grade") ?? "UNKNOWN",
                Score = GetInt32(root, "score"),
                StatusCode = GetInt32(root, "status_code"),
                TestsFailed = GetInt32(root, "tests_failed"),
                TestsPassed = GetInt32(root, "tests_passed"),
                TestsQuantity = GetInt32(root, "tests_quantity")
            };
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
                JsonValueKind.Null => null,
                _ => null
            };
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

        private static DateTimeOffset? GetDateTimeOffset(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.String)
            {
                return null;
            }

            return DateTimeOffset.TryParse(property.GetString(), out var value) ? value : null;
        }
    }
}
