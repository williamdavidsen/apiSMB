using System.Net.Http.Json;

namespace SecurityAssessmentAPI.Services
{
    public class HardenizeCertificateDiscoveryResponse
    {
        public string? Status { get; set; }
        public List<HardenizeCertificateRecord>? Records { get; set; }
    }

    public class HardenizeCertificateRecord
    {
        public long? valid_from { get; set; }
        public long? valid_until { get; set; }
        public string? issuer { get; set; }
        public string? subject { get; set; }
    }

    public interface IHardenizeClient
    {
        Task<HardenizeCertificateDiscoveryResponse?> GetCertificateDiscoveryAsync(string domain, CancellationToken cancellationToken = default);
    }

    public class HardenizeClient : IHardenizeClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HardenizeClient> _logger;

        public HardenizeClient(HttpClient httpClient, ILogger<HardenizeClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<HardenizeCertificateDiscoveryResponse?> GetCertificateDiscoveryAsync(string domain, CancellationToken cancellationToken = default)
        {
            try
            {
                // Hardenize certificate discovery endpoint
                var url = $"https://api.hardenize.com/v1/certificate-discovery?hostname={Uri.EscapeDataString(domain)}";
                _logger.LogInformation("Calling Hardenize API: {Url}", url);

                var response = await _httpClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<HardenizeCertificateDiscoveryResponse>(cancellationToken: cancellationToken);
                if (result == null)
                {
                    _logger.LogWarning("Hardenize API returned an empty response: {Domain}", domain);
                }

                _logger.LogInformation("Received Hardenize API response: Domain={Domain}, RecordCount={Count}", domain, result?.Records?.Count ?? 0);
                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Hardenize API request failed: {Domain}", domain);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hardenize API processing error: {Domain}", domain);
                return null;
            }
        }
    }
}
