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
            var descriptor = services.SingleOrDefault(service => service.ServiceType == typeof(IHeadersCheckingService));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddScoped<IHeadersCheckingService>(_ => new StubHeadersCheckingService());
        });
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
}
