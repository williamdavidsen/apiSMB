using SecurityAssessmentAPI.Services;

namespace API.UnitTests.TestSupport;

internal sealed class FakeMozillaObservatoryClient : IMozillaObservatoryClient
{
    private readonly MozillaObservatoryScanResponse? _response;

    public FakeMozillaObservatoryClient(MozillaObservatoryScanResponse? response = null)
    {
        _response = response;
    }

    public Task<MozillaObservatoryScanResponse?> ScanAsync(string domain, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_response);
    }
}

internal sealed class FakeHttpHeadersProbeClient : IHttpHeadersProbeClient
{
    private readonly Dictionary<string, HttpHeadersProbeResult?> _responses = new(StringComparer.OrdinalIgnoreCase);

    public FakeHttpHeadersProbeClient WithResponse(string scheme, HttpHeadersProbeResult? response)
    {
        _responses[scheme] = response;
        return this;
    }

    public Task<HttpHeadersProbeResult?> ProbeAsync(string domain, string scheme = "https", CancellationToken cancellationToken = default)
    {
        _responses.TryGetValue(scheme, out var response);
        return Task.FromResult(response);
    }
}
