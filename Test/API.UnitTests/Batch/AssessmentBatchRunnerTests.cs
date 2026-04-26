using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using API.UnitTests.TestSupport;
using AssessmentBatchRunnerTool;
using Xunit;

namespace API.UnitTests.Batch;

public sealed class AssessmentBatchRunnerTests
{
    [Fact]
    public async Task RunAsync_WhenAssessmentSucceeds_WritesCsvRow()
    {
        var handler = new StubHttpMessageHandler((request, _) =>
        {
            Assert.Equal("/api/assessment/check", request.RequestUri!.AbsolutePath);
            return Task.FromResult(HttpResponseFactory.Json(HttpStatusCode.OK, JsonSerializer.Serialize(new AssessmentBatchResult
            {
                Status = "PASS",
                Grade = "A",
                OverallScore = 95
            }), request.RequestUri));
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5555") };
        var output = new StringWriter();
        var errors = new StringWriter();

        var exitCode = await BatchAssessmentRunner.RunAsync(client, ["example.com"], output, errors);

        Assert.Equal(0, exitCode);
        Assert.Contains("example.com,PASS,A,95", output.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, errors.ToString());
    }

    [Fact]
    public async Task RunAsync_WhenRequestFails_WritesErrorRowAndContinues()
    {
        var callCount = 0;
        var handler = new StubHttpMessageHandler((request, _) =>
        {
            callCount++;
            if (callCount == 1)
            {
                return Task.FromResult(HttpResponseFactory.Json(HttpStatusCode.BadGateway, "{}", request.RequestUri));
            }

            throw new HttpRequestException("socket failure");
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5555") };
        var output = new StringWriter();
        var errors = new StringWriter();

        var exitCode = await BatchAssessmentRunner.RunAsync(client, ["first.example", "second.example"], output, errors);

        Assert.Equal(0, exitCode);
        var text = output.ToString();
        Assert.Contains("first.example,HTTP_502,,", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("second.example,ERROR,,\"socket failure\"", text, StringComparison.OrdinalIgnoreCase);
    }
}
