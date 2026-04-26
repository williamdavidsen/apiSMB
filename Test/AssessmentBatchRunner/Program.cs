using AssessmentBatchRunnerTool;

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

using var httpClient = new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl),
    Timeout = TimeSpan.FromMinutes(2)
};

return await BatchAssessmentRunner.RunAsync(httpClient, domains, Console.Out, Console.Error);
