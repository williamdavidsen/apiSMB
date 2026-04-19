using SecurityAssessmentAPI.DTOs;
using SecurityAssessmentAPI.Services;

namespace API.UnitTests.TestSupport;

internal sealed class FakeSslCheckingService : ISslCheckingService
{
    private readonly SslCheckResult _result;

    public FakeSslCheckingService(SslCheckResult result)
    {
        _result = result;
    }

    public Task<SslCheckResult> CheckSslAsync(string domain, CancellationToken cancellationToken = default)
    {
        _result.Domain = domain;
        return Task.FromResult(_result);
    }

    public Task<SslDetailResult> GetSslDetailsAsync(string domain, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new SslDetailResult { Domain = domain });
    }
}

internal sealed class FakeHeadersCheckingService : IHeadersCheckingService
{
    private readonly HeadersCheckResult _result;

    public FakeHeadersCheckingService(HeadersCheckResult result)
    {
        _result = result;
    }

    public Task<HeadersCheckResult> CheckHeadersAsync(string domain, CancellationToken cancellationToken = default)
    {
        _result.Domain = domain;
        return Task.FromResult(_result);
    }
}

internal sealed class FakeEmailCheckingService : IEmailCheckingService
{
    private readonly EmailCheckResult _result;

    public FakeEmailCheckingService(EmailCheckResult result)
    {
        _result = result;
    }

    public Task<EmailCheckResult> CheckEmailAsync(string domain, CancellationToken cancellationToken = default)
    {
        _result.Domain = domain;
        return Task.FromResult(_result);
    }
}

internal sealed class FakeReputationCheckingService : IReputationCheckingService
{
    private readonly ReputationCheckResult _result;

    public FakeReputationCheckingService(ReputationCheckResult result)
    {
        _result = result;
    }

    public Task<ReputationCheckResult> CheckReputationAsync(string domain, CancellationToken cancellationToken = default)
    {
        _result.Domain = domain;
        return Task.FromResult(_result);
    }
}

internal sealed class FakePqcCheckingService : IPqcCheckingService
{
    private readonly PqcCheckResult _result;

    public FakePqcCheckingService(PqcCheckResult result)
    {
        _result = result;
    }

    public Task<PqcCheckResult> CheckPqcAsync(string domain, CancellationToken cancellationToken = default)
    {
        _result.Domain = domain;
        return Task.FromResult(_result);
    }
}
