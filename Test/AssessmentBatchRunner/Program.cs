using System.Net.Http.Json;

var apiBaseUrl = args.Length > 0 ? args[0].TrimEnd('/') : "http://localhost:5555";
var domainFile = args.Length > 1 ? args[1] : "domains.txt";

if (!File.Exists(domainFile))
{
    Console.Error.WriteLine($"Domain file was not found: {Path.GetFullPath(domainFile)}");
    return 1;
}

var domains = File.ReadAllLines(domainFile)
    .Select(line => line.Trim())
    .Where(line => line.Length > 0 && !line.StartsWith('#'))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToList();

if (domains.Count == 0)
{
    Console.Error.WriteLine("Domain file did not contain any domains.");
    return 1;
}

using var httpClient = new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl),
    Timeout = TimeSpan.FromMinutes(2)
};

Console.WriteLine($"Running assessment batch against {apiBaseUrl}");
Console.WriteLine($"Domains: {domains.Count}");
Console.WriteLine("domain,status,grade,overallScore");

foreach (var domain in domains)
{
    try
    {
        var response = await httpClient.PostAsJsonAsync("/api/assessment/check", new { domain });
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"{domain},HTTP_{(int)response.StatusCode},,");
            continue;
        }

        var result = await response.Content.ReadFromJsonAsync<AssessmentBatchResult>();
        Console.WriteLine($"{domain},{result?.Status},{result?.Grade},{result?.OverallScore}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{domain},ERROR,,\"{ex.Message.Replace("\"", "'")}\"");
    }
}

return 0;

internal sealed class AssessmentBatchResult
{
    public string Status { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public int OverallScore { get; set; }
}
