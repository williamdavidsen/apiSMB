using SecurityAssessmentAPI.Services;

namespace API.UnitTests.TestSupport;

internal sealed class FakeSslLabsClient : ISslLabsClient
{
    private readonly Queue<SslLabsResponse> _responses = new();
    private readonly Exception? _exception;

    public FakeSslLabsClient(params SslLabsResponse[] responses)
    {
        foreach (var response in responses)
        {
            _responses.Enqueue(response);
        }
    }

    public FakeSslLabsClient(Exception exception)
    {
        _exception = exception;
    }

    public Task<SslLabsResponse> AnalyzeAsync(string domain, CancellationToken cancellationToken = default)
    {
        if (_exception != null)
        {
            throw _exception;
        }

        if (_responses.Count == 0)
        {
            return Task.FromResult(new SslLabsResponse
            {
                Host = domain,
                Status = "ERROR"
            });
        }

        return Task.FromResult(_responses.Dequeue());
    }
}

internal sealed class FakeVirusTotalClient : IVirusTotalClient
{
    private readonly VirusTotalDomainReport? _report;

    public FakeVirusTotalClient(VirusTotalDomainReport? report)
    {
        _report = report;
    }

    public Task<VirusTotalDomainReport?> GetDomainReportAsync(string domain, CancellationToken cancellationToken = default)
    {
        if (_report != null)
        {
            _report.Domain = domain;
        }

        return Task.FromResult(_report);
    }
}
