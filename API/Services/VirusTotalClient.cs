using System.Net;
using System.Text.Json;

namespace SecurityAssessmentAPI.Services
{
    public class VirusTotalDomainReport
    {
        public string Domain { get; set; } = string.Empty;
        public int Reputation { get; set; }
        public int MaliciousDetections { get; set; }
        public int SuspiciousDetections { get; set; }
        public int HarmlessDetections { get; set; }
        public int UndetectedDetections { get; set; }
        public int CommunityMaliciousVotes { get; set; }
        public int CommunityHarmlessVotes { get; set; }
        public DateTimeOffset? LastAnalysisDate { get; set; }
        public string Permalink { get; set; } = string.Empty;
    }

    public interface IVirusTotalClient
    {
        Task<VirusTotalDomainReport?> GetDomainReportAsync(string domain, CancellationToken cancellationToken = default);
    }

    public class VirusTotalClient : IVirusTotalClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<VirusTotalClient> _logger;

        public VirusTotalClient(HttpClient httpClient, IConfiguration configuration, ILogger<VirusTotalClient> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<VirusTotalDomainReport?> GetDomainReportAsync(string domain, CancellationToken cancellationToken = default)
        {
            var apiKey = _configuration["VirusTotal:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("VirusTotal API key is not configured.");
                return null;
            }

            var url = $"https://www.virustotal.com/api/v3/domains/{Uri.EscapeDataString(domain)}";

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("x-apikey", apiKey);

                _logger.LogInformation("Calling VirusTotal API for domain report: {Domain}", domain);

                using var response = await _httpClient.SendAsync(request, cancellationToken);
                var json = await response.Content.ReadAsStringAsync(cancellationToken);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("VirusTotal did not return a report for domain: {Domain}", domain);
                    return new VirusTotalDomainReport
                    {
                        Domain = domain,
                        Permalink = $"https://www.virustotal.com/gui/domain/{domain}"
                    };
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("VirusTotal returned non-success status: Domain={Domain}, Status={StatusCode}, Body={Body}",
                        domain, (int)response.StatusCode, json);
                    return null;
                }

                return ParseDomainReport(json);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "VirusTotal request failed: {Domain}", domain);
                return null;
            }
        }

        private static VirusTotalDomainReport ParseDomainReport(string json)
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            var data = root.GetProperty("data");
            var attributes = data.GetProperty("attributes");

            var stats = attributes.TryGetProperty("last_analysis_stats", out var statsElement) ? statsElement : default;
            var votes = attributes.TryGetProperty("total_votes", out var votesElement) ? votesElement : default;

            return new VirusTotalDomainReport
            {
                Domain = GetString(data, "id") ?? string.Empty,
                Reputation = GetInt32(attributes, "reputation"),
                MaliciousDetections = GetInt32(stats, "malicious"),
                SuspiciousDetections = GetInt32(stats, "suspicious"),
                HarmlessDetections = GetInt32(stats, "harmless"),
                UndetectedDetections = GetInt32(stats, "undetected"),
                CommunityMaliciousVotes = GetInt32(votes, "malicious"),
                CommunityHarmlessVotes = GetInt32(votes, "harmless"),
                LastAnalysisDate = GetUnixDateTimeOffset(attributes, "last_analysis_date"),
                Permalink = $"https://www.virustotal.com/gui/domain/{GetString(data, "id")}"
            };
        }

        private static string? GetString(JsonElement element, string propertyName)
        {
            if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(propertyName, out var property))
            {
                return null;
            }

            return property.ValueKind == JsonValueKind.String ? property.GetString() : property.ToString();
        }

        private static int GetInt32(JsonElement element, string propertyName)
        {
            if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(propertyName, out var property))
            {
                return 0;
            }

            if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var numberValue))
            {
                return numberValue;
            }

            return property.ValueKind == JsonValueKind.String && int.TryParse(property.GetString(), out var stringValue)
                ? stringValue
                : 0;
        }

        private static DateTimeOffset? GetUnixDateTimeOffset(JsonElement element, string propertyName)
        {
            var unixValue = GetInt32(element, propertyName);
            return unixValue > 0 ? DateTimeOffset.FromUnixTimeSeconds(unixValue) : null;
        }
    }
}
