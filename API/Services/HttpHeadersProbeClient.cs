namespace SecurityAssessmentAPI.Services
{
    public class HttpHeadersProbeResult
    {
        public Uri FinalUri { get; set; } = new("https://localhost");
        public int StatusCode { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    public interface IHttpHeadersProbeClient
    {
        Task<HttpHeadersProbeResult?> ProbeAsync(string domain, CancellationToken cancellationToken = default);
    }

    public class HttpHeadersProbeClient : IHttpHeadersProbeClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HttpHeadersProbeClient> _logger;

        public HttpHeadersProbeClient(HttpClient httpClient, ILogger<HttpHeadersProbeClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<HttpHeadersProbeResult?> ProbeAsync(string domain, CancellationToken cancellationToken = default)
        {
            var targetUrl = $"https://{domain}";

            try
            {
                _logger.LogInformation("Probing HTTP security headers: {Url}", targetUrl);

                using var request = new HttpRequestMessage(HttpMethod.Get, targetUrl);
                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (var header in response.Headers)
                {
                    headers[header.Key] = string.Join(", ", header.Value);
                }

                foreach (var header in response.Content.Headers)
                {
                    headers[header.Key] = string.Join(", ", header.Value);
                }

                return new HttpHeadersProbeResult
                {
                    FinalUri = response.RequestMessage?.RequestUri ?? new Uri(targetUrl),
                    StatusCode = (int)response.StatusCode,
                    Headers = headers
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "HTTP header probe failed: {Domain}", domain);
                return null;
            }
        }
    }
}
