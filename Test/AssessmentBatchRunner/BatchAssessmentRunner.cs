using System.Net.Http.Json;

namespace AssessmentBatchRunnerTool;

public static class BatchAssessmentRunner
{
    public static async Task<int> RunAsync(
        HttpClient httpClient,
        IReadOnlyCollection<string> domains,
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken = default)
    {
        if (domains.Count == 0)
        {
            await standardError.WriteLineAsync("Domain file did not contain any domains.");
            return 1;
        }

        await standardOutput.WriteLineAsync($"Running assessment batch against {httpClient.BaseAddress}");
        await standardOutput.WriteLineAsync($"Domains: {domains.Count}");
        await standardOutput.WriteLineAsync("domain,status,grade,overallScore");

        foreach (var domain in domains)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync("/api/assessment/check", new { domain }, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    await standardOutput.WriteLineAsync($"{domain},HTTP_{(int)response.StatusCode},,");
                    continue;
                }

                var result = await response.Content.ReadFromJsonAsync<AssessmentBatchResult>(cancellationToken);
                await standardOutput.WriteLineAsync($"{domain},{result?.Status},{result?.Grade},{result?.OverallScore}");
            }
            catch (Exception ex)
            {
                await standardOutput.WriteLineAsync($"{domain},ERROR,,\"{ex.Message.Replace("\"", "'")}\"");
            }
        }

        return 0;
    }
}

public sealed class AssessmentBatchResult
{
    public string Status { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public int OverallScore { get; set; }
}
