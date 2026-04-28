using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using SecurityAssessmentAPI.Controllers.Api;
using SecurityAssessmentAPI.DTOs;
using SecurityAssessmentAPI.Services;
using Xunit;

namespace API.UnitTests.Controllers;

public sealed class ControllerBranchCoverageTests
{
    [Fact]
    public async Task AssessmentPost_WhenModelStateIsInvalid_ReturnsBadRequest()
    {
        var controller = new AssessmentController(
            new DelegateAssessmentCheckingService((_, _) => Task.FromResult(CreateAssessmentResult())),
            NullLogger<AssessmentController>.Instance);
        controller.ModelState.AddModelError("Domain", "Required");

        var result = await controller.CheckAssessment(new AssessmentCheckRequest(), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AssessmentPost_WhenServiceSucceeds_ReturnsOk()
    {
        var controller = new AssessmentController(
            new DelegateAssessmentCheckingService((domain, _) => Task.FromResult(CreateAssessmentResult(domain))),
            NullLogger<AssessmentController>.Instance);

        var result = await controller.CheckAssessment(new AssessmentCheckRequest { Domain = "example.com" }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<AssessmentCheckResult>(ok.Value);
        Assert.Equal("example.com", payload.Domain);
    }

    [Fact]
    public async Task AssessmentPost_WhenServiceThrows_ReturnsInternalServerError()
    {
        var controller = new AssessmentController(
            new DelegateAssessmentCheckingService((_, _) => Task.FromException<AssessmentCheckResult>(new InvalidOperationException("boom"))),
            NullLogger<AssessmentController>.Instance);

        var result = await controller.CheckAssessment(new AssessmentCheckRequest { Domain = "example.com" }, CancellationToken.None);

        var error = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, error.StatusCode);
    }

    [Fact]
    public async Task AssessmentGet_WhenDomainIsBlank_ReturnsBadRequest()
    {
        var controller = new AssessmentController(
            new DelegateAssessmentCheckingService((_, _) => Task.FromResult(CreateAssessmentResult())),
            NullLogger<AssessmentController>.Instance);

        var result = await controller.GetAssessmentCheck(" ", CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AssessmentGet_WhenServiceSucceeds_ReturnsOk()
    {
        var controller = new AssessmentController(
            new DelegateAssessmentCheckingService((domain, _) => Task.FromResult(CreateAssessmentResult(domain))),
            NullLogger<AssessmentController>.Instance);

        var result = await controller.GetAssessmentCheck("example.com", CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<AssessmentCheckResult>(ok.Value);
        Assert.Equal("example.com", payload.Domain);
    }

    [Fact]
    public async Task AssessmentGet_WhenServiceThrows_ReturnsInternalServerError()
    {
        var controller = new AssessmentController(
            new DelegateAssessmentCheckingService((_, _) => Task.FromException<AssessmentCheckResult>(new InvalidOperationException("boom"))),
            NullLogger<AssessmentController>.Instance);

        var result = await controller.GetAssessmentCheck("example.com", CancellationToken.None);

        var error = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, error.StatusCode);
    }

    [Fact]
    public async Task EmailPost_WhenModelStateIsInvalid_ReturnsBadRequest()
    {
        var controller = new EmailController(
            new DelegateEmailCheckingService((_, _) => Task.FromResult(CreateEmailResult())),
            NullLogger<EmailController>.Instance);
        controller.ModelState.AddModelError("Domain", "Required");

        var result = await controller.CheckEmail(new EmailCheckRequest(), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task EmailPost_WhenServiceSucceeds_ReturnsOk()
    {
        var controller = new EmailController(
            new DelegateEmailCheckingService((domain, _) => Task.FromResult(CreateEmailResult(domain))),
            NullLogger<EmailController>.Instance);

        var result = await controller.CheckEmail(new EmailCheckRequest { Domain = "example.com" }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<EmailCheckResult>(ok.Value);
        Assert.Equal("example.com", payload.Domain);
    }

    [Fact]
    public async Task EmailPost_WhenServiceThrows_ReturnsInternalServerError()
    {
        var controller = new EmailController(
            new DelegateEmailCheckingService((_, _) => Task.FromException<EmailCheckResult>(new InvalidOperationException("boom"))),
            NullLogger<EmailController>.Instance);

        var result = await controller.CheckEmail(new EmailCheckRequest { Domain = "example.com" }, CancellationToken.None);

        var error = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, error.StatusCode);
    }

    [Fact]
    public async Task EmailGet_WhenDomainIsBlank_ReturnsBadRequest()
    {
        var controller = new EmailController(
            new DelegateEmailCheckingService((_, _) => Task.FromResult(CreateEmailResult())),
            NullLogger<EmailController>.Instance);

        var result = await controller.GetEmailCheck("", CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task EmailGet_WhenServiceSucceeds_ReturnsOk()
    {
        var controller = new EmailController(
            new DelegateEmailCheckingService((domain, _) => Task.FromResult(CreateEmailResult(domain))),
            NullLogger<EmailController>.Instance);

        var result = await controller.GetEmailCheck("example.com", CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<EmailCheckResult>(ok.Value);
        Assert.Equal("example.com", payload.Domain);
    }

    [Fact]
    public async Task EmailGet_WhenServiceThrows_ReturnsInternalServerError()
    {
        var controller = new EmailController(
            new DelegateEmailCheckingService((_, _) => Task.FromException<EmailCheckResult>(new InvalidOperationException("boom"))),
            NullLogger<EmailController>.Instance);

        var result = await controller.GetEmailCheck("example.com", CancellationToken.None);

        var error = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, error.StatusCode);
    }

    [Fact]
    public async Task HeadersPost_WhenModelStateIsInvalid_ReturnsBadRequest()
    {
        var controller = new HeadersController(
            new DelegateHeadersCheckingService((_, _) => Task.FromResult(CreateHeadersResult())),
            NullLogger<HeadersController>.Instance);
        controller.ModelState.AddModelError("Domain", "Required");

        var result = await controller.CheckHeaders(new HeadersCheckRequest(), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task HeadersPost_WhenServiceSucceeds_ReturnsOk()
    {
        var controller = new HeadersController(
            new DelegateHeadersCheckingService((domain, _) => Task.FromResult(CreateHeadersResult(domain))),
            NullLogger<HeadersController>.Instance);

        var result = await controller.CheckHeaders(new HeadersCheckRequest { Domain = "example.com" }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<HeadersCheckResult>(ok.Value);
        Assert.Equal("example.com", payload.Domain);
    }

    [Fact]
    public async Task HeadersPost_WhenServiceThrows_ReturnsInternalServerError()
    {
        var controller = new HeadersController(
            new DelegateHeadersCheckingService((_, _) => Task.FromException<HeadersCheckResult>(new InvalidOperationException("boom"))),
            NullLogger<HeadersController>.Instance);

        var result = await controller.CheckHeaders(new HeadersCheckRequest { Domain = "example.com" }, CancellationToken.None);

        var error = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, error.StatusCode);
    }

    [Fact]
    public async Task HeadersGet_WhenDomainIsBlank_ReturnsBadRequest()
    {
        var controller = new HeadersController(
            new DelegateHeadersCheckingService((_, _) => Task.FromResult(CreateHeadersResult())),
            NullLogger<HeadersController>.Instance);

        var result = await controller.GetHeadersCheck("", CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task HeadersGet_WhenServiceSucceeds_ReturnsOk()
    {
        var controller = new HeadersController(
            new DelegateHeadersCheckingService((domain, _) => Task.FromResult(CreateHeadersResult(domain))),
            NullLogger<HeadersController>.Instance);

        var result = await controller.GetHeadersCheck("example.com", CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<HeadersCheckResult>(ok.Value);
        Assert.Equal("example.com", payload.Domain);
    }

    [Fact]
    public async Task HeadersGet_WhenServiceThrows_ReturnsInternalServerError()
    {
        var controller = new HeadersController(
            new DelegateHeadersCheckingService((_, _) => Task.FromException<HeadersCheckResult>(new InvalidOperationException("boom"))),
            NullLogger<HeadersController>.Instance);

        var result = await controller.GetHeadersCheck("example.com", CancellationToken.None);

        var error = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, error.StatusCode);
    }

    [Fact]
    public async Task ReputationPost_WhenModelStateIsInvalid_ReturnsBadRequest()
    {
        var controller = new ReputationController(
            new DelegateReputationCheckingService((_, _) => Task.FromResult(CreateReputationResult())),
            NullLogger<ReputationController>.Instance);
        controller.ModelState.AddModelError("Domain", "Required");

        var result = await controller.CheckReputation(new ReputationCheckRequest(), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ReputationPost_WhenServiceSucceeds_ReturnsOk()
    {
        var controller = new ReputationController(
            new DelegateReputationCheckingService((domain, _) => Task.FromResult(CreateReputationResult(domain))),
            NullLogger<ReputationController>.Instance);

        var result = await controller.CheckReputation(new ReputationCheckRequest { Domain = "example.com" }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<ReputationCheckResult>(ok.Value);
        Assert.Equal("example.com", payload.Domain);
    }

    [Fact]
    public async Task ReputationPost_WhenServiceThrows_ReturnsInternalServerError()
    {
        var controller = new ReputationController(
            new DelegateReputationCheckingService((_, _) => Task.FromException<ReputationCheckResult>(new InvalidOperationException("boom"))),
            NullLogger<ReputationController>.Instance);

        var result = await controller.CheckReputation(new ReputationCheckRequest { Domain = "example.com" }, CancellationToken.None);

        var error = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, error.StatusCode);
    }

    [Fact]
    public async Task ReputationGet_WhenDomainIsBlank_ReturnsBadRequest()
    {
        var controller = new ReputationController(
            new DelegateReputationCheckingService((_, _) => Task.FromResult(CreateReputationResult())),
            NullLogger<ReputationController>.Instance);

        var result = await controller.GetReputationCheck("", CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ReputationGet_WhenServiceSucceeds_ReturnsOk()
    {
        var controller = new ReputationController(
            new DelegateReputationCheckingService((domain, _) => Task.FromResult(CreateReputationResult(domain))),
            NullLogger<ReputationController>.Instance);

        var result = await controller.GetReputationCheck("example.com", CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<ReputationCheckResult>(ok.Value);
        Assert.Equal("example.com", payload.Domain);
    }

    [Fact]
    public async Task ReputationGet_WhenServiceThrows_ReturnsInternalServerError()
    {
        var controller = new ReputationController(
            new DelegateReputationCheckingService((_, _) => Task.FromException<ReputationCheckResult>(new InvalidOperationException("boom"))),
            NullLogger<ReputationController>.Instance);

        var result = await controller.GetReputationCheck("example.com", CancellationToken.None);

        var error = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, error.StatusCode);
    }

    [Fact]
    public async Task SslPost_WhenModelStateIsInvalid_ReturnsBadRequest()
    {
        var controller = new SslController(
            new DelegateSslCheckingService(
                (domain, _) => Task.FromResult(CreateSslResult(domain)),
                (domain, _) => Task.FromResult(CreateSslDetailResult(domain))),
            NullLogger<SslController>.Instance);
        controller.ModelState.AddModelError("Domain", "Required");

        var result = await controller.CheckSsl(new SslCheckRequest(), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SslPost_WhenServiceSucceeds_ReturnsOk()
    {
        var controller = new SslController(
            new DelegateSslCheckingService(
                (domain, _) => Task.FromResult(CreateSslResult(domain)),
                (domain, _) => Task.FromResult(CreateSslDetailResult(domain))),
            NullLogger<SslController>.Instance);

        var result = await controller.CheckSsl(new SslCheckRequest { Domain = "example.com" }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<SslCheckResult>(ok.Value);
        Assert.Equal("example.com", payload.Domain);
    }

    [Fact]
    public async Task SslPost_WhenServiceThrows_ReturnsInternalServerError()
    {
        var controller = new SslController(
            new DelegateSslCheckingService(
                (_, _) => Task.FromException<SslCheckResult>(new InvalidOperationException("boom")),
                (domain, _) => Task.FromResult(CreateSslDetailResult(domain))),
            NullLogger<SslController>.Instance);

        var result = await controller.CheckSsl(new SslCheckRequest { Domain = "example.com" }, CancellationToken.None);

        var error = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, error.StatusCode);
    }

    [Fact]
    public async Task SslGet_WhenDomainIsBlank_ReturnsBadRequest()
    {
        var controller = new SslController(
            new DelegateSslCheckingService(
                (domain, _) => Task.FromResult(CreateSslResult(domain)),
                (domain, _) => Task.FromResult(CreateSslDetailResult(domain))),
            NullLogger<SslController>.Instance);

        var result = await controller.GetSslCheck("", CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SslGet_WhenServiceSucceeds_ReturnsOk()
    {
        var controller = new SslController(
            new DelegateSslCheckingService(
                (domain, _) => Task.FromResult(CreateSslResult(domain)),
                (domain, _) => Task.FromResult(CreateSslDetailResult(domain))),
            NullLogger<SslController>.Instance);

        var result = await controller.GetSslCheck("example.com", CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<SslCheckResult>(ok.Value);
        Assert.Equal("example.com", payload.Domain);
    }

    [Fact]
    public async Task SslGet_WhenServiceThrows_ReturnsInternalServerError()
    {
        var controller = new SslController(
            new DelegateSslCheckingService(
                (_, _) => Task.FromException<SslCheckResult>(new InvalidOperationException("boom")),
                (domain, _) => Task.FromResult(CreateSslDetailResult(domain))),
            NullLogger<SslController>.Instance);

        var result = await controller.GetSslCheck("example.com", CancellationToken.None);

        var error = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, error.StatusCode);
    }

    [Fact]
    public async Task SslDetailsGet_WhenDomainIsBlank_ReturnsBadRequest()
    {
        var controller = new SslController(
            new DelegateSslCheckingService(
                (domain, _) => Task.FromResult(CreateSslResult(domain)),
                (domain, _) => Task.FromResult(CreateSslDetailResult(domain))),
            NullLogger<SslController>.Instance);

        var result = await controller.GetSslDetails(" ", CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SslDetailsGet_WhenServiceSucceeds_ReturnsOk()
    {
        var controller = new SslController(
            new DelegateSslCheckingService(
                (domain, _) => Task.FromResult(CreateSslResult(domain)),
                (domain, _) => Task.FromResult(CreateSslDetailResult(domain))),
            NullLogger<SslController>.Instance);

        var result = await controller.GetSslDetails("example.com", CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<SslDetailResult>(ok.Value);
        Assert.Equal("example.com", payload.Domain);
    }

    [Fact]
    public async Task SslDetailsGet_WhenServiceThrows_ReturnsInternalServerError()
    {
        var controller = new SslController(
            new DelegateSslCheckingService(
                (domain, _) => Task.FromResult(CreateSslResult(domain)),
                (_, _) => Task.FromException<SslDetailResult>(new InvalidOperationException("boom"))),
            NullLogger<SslController>.Instance);

        var result = await controller.GetSslDetails("example.com", CancellationToken.None);

        var error = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, error.StatusCode);
    }

    [Fact]
    public async Task PqcGet_WhenDomainIsBlank_ReturnsBadRequest()
    {
        var controller = new PqcController(
            new DelegatePqcCheckingService((_, _) => Task.FromResult(CreatePqcResult())),
            NullLogger<PqcController>.Instance);

        var result = await controller.GetPqcCheck("", CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task PqcGet_WhenServiceSucceeds_ReturnsOk()
    {
        var controller = new PqcController(
            new DelegatePqcCheckingService((domain, _) => Task.FromResult(CreatePqcResult(domain))),
            NullLogger<PqcController>.Instance);

        var result = await controller.GetPqcCheck("example.com", CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<PqcCheckResult>(ok.Value);
        Assert.Equal("example.com", payload.Domain);
    }

    [Fact]
    public async Task PqcGet_WhenServiceThrows_ReturnsInternalServerError()
    {
        var controller = new PqcController(
            new DelegatePqcCheckingService((_, _) => Task.FromException<PqcCheckResult>(new InvalidOperationException("boom"))),
            NullLogger<PqcController>.Instance);

        var result = await controller.GetPqcCheck("example.com", CancellationToken.None);

        var error = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, error.StatusCode);
    }

    private static AssessmentCheckResult CreateAssessmentResult(string domain = "example.com")
        => new()
        {
            Domain = domain,
            OverallScore = 82,
            Status = "PASS",
            Grade = "B"
        };

    private static EmailCheckResult CreateEmailResult(string domain = "example.com")
        => new()
        {
            Domain = domain,
            OverallScore = 16,
            Status = "PASS",
            HasMailService = true,
            ModuleApplicable = true
        };

    private static HeadersCheckResult CreateHeadersResult(string domain = "example.com")
        => new()
        {
            Domain = domain,
            OverallScore = 14,
            Status = "PASS"
        };

    private static ReputationCheckResult CreateReputationResult(string domain = "example.com")
        => new()
        {
            Domain = domain,
            OverallScore = 12,
            Status = "PASS"
        };

    private static SslCheckResult CreateSslResult(string domain = "example.com")
        => new()
        {
            Domain = domain,
            OverallScore = 24,
            Status = "PASS"
        };

    private static SslDetailResult CreateSslDetailResult(string domain = "example.com")
        => new()
        {
            Domain = domain,
            OverallScore = 24,
            Status = "PASS",
            DataSource = "SSL_LABS"
        };

    private static PqcCheckResult CreatePqcResult(string domain = "example.com")
        => new()
        {
            Domain = domain,
            Status = "INFO",
            ReadinessLevel = "Unknown / not verifiable"
        };

    private sealed class DelegateAssessmentCheckingService : IAssessmentCheckingService
    {
        private readonly Func<string, CancellationToken, Task<AssessmentCheckResult>> _handler;

        public DelegateAssessmentCheckingService(Func<string, CancellationToken, Task<AssessmentCheckResult>> handler)
        {
            _handler = handler;
        }

        public Task<AssessmentCheckResult> CheckAssessmentAsync(string domain, CancellationToken cancellationToken = default)
            => _handler(domain, cancellationToken);
    }

    private sealed class DelegateEmailCheckingService : IEmailCheckingService
    {
        private readonly Func<string, CancellationToken, Task<EmailCheckResult>> _handler;

        public DelegateEmailCheckingService(Func<string, CancellationToken, Task<EmailCheckResult>> handler)
        {
            _handler = handler;
        }

        public Task<EmailCheckResult> CheckEmailAsync(string domain, CancellationToken cancellationToken = default)
            => _handler(domain, cancellationToken);
    }

    private sealed class DelegateHeadersCheckingService : IHeadersCheckingService
    {
        private readonly Func<string, CancellationToken, Task<HeadersCheckResult>> _handler;

        public DelegateHeadersCheckingService(Func<string, CancellationToken, Task<HeadersCheckResult>> handler)
        {
            _handler = handler;
        }

        public Task<HeadersCheckResult> CheckHeadersAsync(string domain, CancellationToken cancellationToken = default)
            => _handler(domain, cancellationToken);
    }

    private sealed class DelegateReputationCheckingService : IReputationCheckingService
    {
        private readonly Func<string, CancellationToken, Task<ReputationCheckResult>> _handler;

        public DelegateReputationCheckingService(Func<string, CancellationToken, Task<ReputationCheckResult>> handler)
        {
            _handler = handler;
        }

        public Task<ReputationCheckResult> CheckReputationAsync(string domain, CancellationToken cancellationToken = default)
            => _handler(domain, cancellationToken);
    }

    private sealed class DelegatePqcCheckingService : IPqcCheckingService
    {
        private readonly Func<string, CancellationToken, Task<PqcCheckResult>> _handler;

        public DelegatePqcCheckingService(Func<string, CancellationToken, Task<PqcCheckResult>> handler)
        {
            _handler = handler;
        }

        public Task<PqcCheckResult> CheckPqcAsync(string domain, CancellationToken cancellationToken = default)
            => _handler(domain, cancellationToken);
    }

    private sealed class DelegateSslCheckingService : ISslCheckingService
    {
        private readonly Func<string, CancellationToken, Task<SslCheckResult>> _checkHandler;
        private readonly Func<string, CancellationToken, Task<SslDetailResult>> _detailHandler;

        public DelegateSslCheckingService(
            Func<string, CancellationToken, Task<SslCheckResult>> checkHandler,
            Func<string, CancellationToken, Task<SslDetailResult>> detailHandler)
        {
            _checkHandler = checkHandler;
            _detailHandler = detailHandler;
        }

        public Task<SslCheckResult> CheckSslAsync(string domain, CancellationToken cancellationToken = default)
            => _checkHandler(domain, cancellationToken);

        public Task<SslDetailResult> GetSslDetailsAsync(string domain, CancellationToken cancellationToken = default)
            => _detailHandler(domain, cancellationToken);
    }
}
