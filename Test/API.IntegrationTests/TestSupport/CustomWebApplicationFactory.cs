using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using SecurityAssessmentAPI.DTOs;
using SecurityAssessmentAPI.Services;

namespace API.IntegrationTests.TestSupport;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            RemoveIfRegistered<ISslCheckingService>(services);
            RemoveIfRegistered<IHeadersCheckingService>(services);
            RemoveIfRegistered<IEmailCheckingService>(services);
            RemoveIfRegistered<IReputationCheckingService>(services);
            RemoveIfRegistered<IPqcCheckingService>(services);
            RemoveIfRegistered<IAssessmentCheckingService>(services);
            services.AddScoped<ISslCheckingService>(_ => new StubSslCheckingService());
            services.AddScoped<IHeadersCheckingService>(_ => new StubHeadersCheckingService());
            services.AddScoped<IEmailCheckingService>(_ => new StubEmailCheckingService());
            services.AddScoped<IReputationCheckingService>(_ => new StubReputationCheckingService());
            services.AddScoped<IPqcCheckingService>(_ => new StubPqcCheckingService());
            services.AddScoped<IAssessmentCheckingService>(_ => new StubAssessmentCheckingService());
        });
    }

    private static void RemoveIfRegistered<TService>(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(service => service.ServiceType == typeof(TService));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }
    }

    private sealed class StubHeadersCheckingService : IHeadersCheckingService
    {
        public Task<HeadersCheckResult> CheckHeadersAsync(string domain, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new HeadersCheckResult
            {
                Domain = domain,
                OverallScore = 10,
                Status = "PASS"
            });
        }
    }

    private sealed class StubSslCheckingService : ISslCheckingService
    {
        public Task<SslCheckResult> CheckSslAsync(string domain, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new SslCheckResult
            {
                Domain = domain,
                OverallScore = 24,
                Status = "WARNING"
            });
        }

        public Task<SslDetailResult> GetSslDetailsAsync(string domain, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new SslDetailResult
            {
                Domain = domain,
                OverallScore = 24,
                Status = "WARNING",
                DataSource = "DIRECT_TLS",
                Alerts =
                [
                    new SslAlert
                    {
                        Type = "INFO",
                        Message = "The result was produced by a direct TLS probe."
                    }
                ]
            });
        }
    }

    private sealed class StubEmailCheckingService : IEmailCheckingService
    {
        public Task<EmailCheckResult> CheckEmailAsync(string domain, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new EmailCheckResult
            {
                Domain = domain,
                HasMailService = false,
                ModuleApplicable = true,
                Status = "ERROR",
                Alerts =
                [
                    new EmailAlert
                    {
                        Type = "WARNING",
                        Message = "Email security DNS lookups could not be completed reliably. MX lookup could not be completed."
                    }
                ]
            });
        }
    }

    private sealed class StubReputationCheckingService : IReputationCheckingService
    {
        public Task<ReputationCheckResult> CheckReputationAsync(string domain, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ReputationCheckResult
            {
                Domain = domain,
                OverallScore = 12,
                Status = "WARNING",
                Summary = new ReputationSummary
                {
                    SuspiciousDetections = 2
                },
                Alerts =
                [
                    new ReputationAlert
                    {
                        Type = "CRITICAL_WARNING",
                        Message = "VirusTotal reports 2 suspicious detection(s) for this domain."
                    }
                ]
            });
        }
    }

    private sealed class StubPqcCheckingService : IPqcCheckingService
    {
        public Task<PqcCheckResult> CheckPqcAsync(string domain, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PqcCheckResult
            {
                Domain = domain,
                Status = "INFO",
                ReadinessLevel = "Unknown / not verifiable",
                Mode = "Classical TLS with modern groups",
                Confidence = "LOW"
            });
        }
    }

    private sealed class StubAssessmentCheckingService : IAssessmentCheckingService
    {
        public Task<AssessmentCheckResult> CheckAssessmentAsync(string domain, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new AssessmentCheckResult
            {
                Domain = domain,
                OverallScore = 80,
                Status = "PARTIAL",
                Grade = "B",
                Alerts =
                [
                    new AssessmentAlert
                    {
                        Type = "WARNING",
                        Message = "Email security analysis could not be completed reliably, so the module was excluded from the final weighted score."
                    }
                ]
            });
        }
    }
}
