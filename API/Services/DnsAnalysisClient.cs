using System.Text.Json;

namespace SecurityAssessmentAPI.Services
{
    public class DnsLookupResult
    {
        public List<string> Records { get; set; } = new();
    }

    public interface IDnsAnalysisClient
    {
        Task<DnsLookupResult> QueryAsync(string name, string type, CancellationToken cancellationToken = default);
    }

    public class DnsAnalysisClient : IDnsAnalysisClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DnsAnalysisClient> _logger;

        public DnsAnalysisClient(HttpClient httpClient, ILogger<DnsAnalysisClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<DnsLookupResult> QueryAsync(string name, string type, CancellationToken cancellationToken = default)
        {
            var url = $"https://dns.google/resolve?name={Uri.EscapeDataString(name)}&type={Uri.EscapeDataString(type)}";

            try
            {
                _logger.LogInformation("Calling DNS analysis endpoint: {Url}", url);

                using var response = await _httpClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return ParseLookupResponse(json, type);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "DNS analysis failed: Name={Name}, Type={Type}", name, type);
                return new DnsLookupResult();
            }
        }

        private static DnsLookupResult ParseLookupResponse(string json, string type)
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            var result = new DnsLookupResult();

            if (!root.TryGetProperty("Answer", out var answersElement) || answersElement.ValueKind != JsonValueKind.Array)
            {
                return result;
            }

            foreach (var answerElement in answersElement.EnumerateArray())
            {
                if (!answerElement.TryGetProperty("data", out var dataElement) || dataElement.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                var value = NormalizeRecordValue(dataElement.GetString(), type);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    result.Records.Add(value);
                }
            }

            return result;
        }

        private static string NormalizeRecordValue(string? value, string type)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var trimmed = value.Trim();

            if (string.Equals(type, "TXT", StringComparison.OrdinalIgnoreCase))
            {
                trimmed = trimmed.Replace("\"", string.Empty);
            }

            if (string.Equals(type, "MX", StringComparison.OrdinalIgnoreCase))
            {
                var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return parts.Length >= 2 ? parts[1].TrimEnd('.') : trimmed.TrimEnd('.');
            }

            return trimmed;
        }
    }
}
