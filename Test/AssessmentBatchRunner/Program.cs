using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

const string defaultBaseUrl = "http://localhost:1111";
const string defaultDomainsFileName = "domains.txt";

var projectDirectory = AppContext.BaseDirectory;
for (var i = 0; i < 4; i++)
{
    projectDirectory = Directory.GetParent(projectDirectory)?.FullName ?? projectDirectory;
}

var baseUrl = args.Length > 0 && !string.IsNullOrWhiteSpace(args[0])
    ? args[0].TrimEnd('/')
    : defaultBaseUrl;

var domainsFile = args.Length > 1 && !string.IsNullOrWhiteSpace(args[1])
    ? args[1]
    : Path.Combine(projectDirectory, defaultDomainsFileName);

if (!File.Exists(domainsFile))
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"Domain listesi bulunamadi: {domainsFile}");
    Console.WriteLine("Ayri satirlarda domain iceren bir domains.txt dosyasi ekleyin.");
    Console.ResetColor();
    return;
}

var domains = File.ReadAllLines(domainsFile)
    .Select(line => line.Trim())
    .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith('#'))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToList();

if (domains.Count == 0)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Calistirilacak domain bulunamadi. domains.txt bos veya yorum satirlarindan olusuyor.");
    Console.ResetColor();
    return;
}

Console.WriteLine($"Base URL: {baseUrl}");
Console.WriteLine($"Domain sayisi: {domains.Count}");
Console.WriteLine();

using var httpClient = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(60)
};

var jsonOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    WriteIndented = true
};

try
{
    using var preflightResponse = await httpClient.GetAsync($"{baseUrl}/swagger/v1/swagger.json");
    if (!preflightResponse.IsSuccessStatusCode)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"API erisilebilir ama beklenen Swagger cevabi alinmadi: {(int)preflightResponse.StatusCode} {preflightResponse.StatusCode}");
        Console.WriteLine("API'nin dogru portta ve dogru uygulama ile calistigini kontrol et.");
        Console.ResetColor();
        return;
    }
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"API'ye baglanilamadi: {baseUrl}");
    Console.WriteLine($"Detay: {ex.Message}");
    Console.WriteLine("Once API projesini calistir, sonra batch runner'i tekrar baslat.");
    Console.ResetColor();
    return;
}

var results = new List<BatchAssessmentResult>();
var rawResponses = new List<RawAssessmentResponse>();

var outputDirectory = Path.Combine(AppContext.BaseDirectory, "output");
Directory.CreateDirectory(outputDirectory);

var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
var runDirectory = Path.Combine(outputDirectory, $"run-{timestamp}");
var rawDirectory = Path.Combine(runDirectory, "raw-json");
Directory.CreateDirectory(runDirectory);
Directory.CreateDirectory(rawDirectory);

foreach (var domain in domains)
{
    var url = $"{baseUrl}/api/assessment/check/{Uri.EscapeDataString(domain)}";
    Console.WriteLine($"Kontrol ediliyor: {domain}");

    try
    {
        using var response = await httpClient.GetAsync(url);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            rawResponses.Add(new RawAssessmentResponse
            {
                Domain = domain,
                RequestSucceeded = false,
                HttpStatusCode = (int)response.StatusCode,
                RawJson = body
            });

            results.Add(new BatchAssessmentResult
            {
                Domain = domain,
                RequestSucceeded = false,
                HttpStatusCode = (int)response.StatusCode,
                Error = $"HTTP {(int)response.StatusCode} {response.StatusCode}"
            });

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  HTTP hatasi: {(int)response.StatusCode} {response.StatusCode}");
            Console.ResetColor();
            continue;
        }

        var rawJsonPath = Path.Combine(rawDirectory, $"{MakeSafeFileName(domain)}.json");
        await File.WriteAllTextAsync(rawJsonPath, body);

        var assessment = JsonSerializer.Deserialize<AssessmentResponse>(body, jsonOptions);
        if (assessment is null)
        {
            rawResponses.Add(new RawAssessmentResponse
            {
                Domain = domain,
                RequestSucceeded = false,
                HttpStatusCode = (int)HttpStatusCode.OK,
                RawJson = body
            });

            results.Add(new BatchAssessmentResult
            {
                Domain = domain,
                RequestSucceeded = false,
                HttpStatusCode = (int)HttpStatusCode.OK,
                Error = "JSON parse edilemedi."
            });

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  JSON parse edilemedi.");
            Console.ResetColor();
            continue;
        }

        rawResponses.Add(new RawAssessmentResponse
        {
            Domain = assessment.Domain ?? domain,
            RequestSucceeded = true,
            HttpStatusCode = (int)HttpStatusCode.OK,
            RawJson = body
        });

        var result = new BatchAssessmentResult
        {
            Domain = assessment.Domain ?? domain,
            RequestSucceeded = true,
            HttpStatusCode = (int)HttpStatusCode.OK,
            OverallScore = assessment.OverallScore,
            MaxScore = assessment.MaxScore,
            Status = assessment.Status,
            Grade = assessment.Grade,
            EmailModuleIncluded = assessment.EmailModuleIncluded,
            PqcReadinessLevel = assessment.PqcReadiness?.ReadinessLevel,
            PqcStatus = assessment.PqcReadiness?.Status,
            SslStatus = assessment.Modules?.SslTls?.Status,
            HeadersStatus = assessment.Modules?.HttpHeaders?.Status,
            EmailStatus = assessment.Modules?.EmailSecurity?.Status,
            ReputationStatus = assessment.Modules?.Reputation?.Status,
            AlertTypes = assessment.Alerts?.Select(a => a.Type ?? string.Empty).ToList() ?? [],
            AlertMessages = assessment.Alerts?.Select(a => a.Message ?? string.Empty).ToList() ?? []
        };

        results.Add(result);

        Console.ForegroundColor = GetScoreColor(result.Status);
        Console.WriteLine($"  Skor: {result.OverallScore}/{result.MaxScore} | Status: {result.Status} | Grade: {result.Grade}");
        Console.ResetColor();
    }
    catch (Exception ex)
    {
        results.Add(new BatchAssessmentResult
        {
            Domain = domain,
            RequestSucceeded = false,
            Error = ex.Message
        });

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  Istek hatasi: {ex.Message}");
        Console.ResetColor();
    }
}

var jsonOutputPath = Path.Combine(runDirectory, $"assessment-results-{timestamp}.json");
var csvOutputPath = Path.Combine(runDirectory, $"assessment-results-{timestamp}.csv");
var rawBundlePath = Path.Combine(runDirectory, $"assessment-raw-responses-{timestamp}.json");

await File.WriteAllTextAsync(jsonOutputPath, JsonSerializer.Serialize(results, jsonOptions));
await File.WriteAllTextAsync(csvOutputPath, BuildCsv(results));
await File.WriteAllTextAsync(rawBundlePath, JsonSerializer.Serialize(rawResponses, jsonOptions));

Console.WriteLine();
Console.WriteLine("Ozet");
Console.WriteLine($"  Basarili: {results.Count(r => r.RequestSucceeded)}");
Console.WriteLine($"  Hatali: {results.Count(r => !r.RequestSucceeded)}");
Console.WriteLine($"  PASS: {results.Count(r => r.Status == "PASS")}");
Console.WriteLine($"  WARNING: {results.Count(r => r.Status == "WARNING")}");
Console.WriteLine($"  PARTIAL: {results.Count(r => r.Status == "PARTIAL")}");
Console.WriteLine($"  FAIL: {results.Count(r => r.Status == "FAIL")}");
Console.WriteLine();
Console.WriteLine($"Oturum klasoru: {runDirectory}");
Console.WriteLine($"Ozet JSON:     {jsonOutputPath}");
Console.WriteLine($"Ozet CSV:      {csvOutputPath}");
Console.WriteLine($"Ham JSON paket:{rawBundlePath}");
Console.WriteLine($"Ham JSON klasor:{rawDirectory}");

static ConsoleColor GetScoreColor(string? status) =>
    status switch
    {
        "PASS" => ConsoleColor.Green,
        "WARNING" => ConsoleColor.Yellow,
        "PARTIAL" => ConsoleColor.DarkYellow,
        "FAIL" => ConsoleColor.Red,
        _ => ConsoleColor.Gray
    };

static string BuildCsv(IEnumerable<BatchAssessmentResult> results)
{
    var rows = new List<string>
    {
        "Domain,RequestSucceeded,HttpStatusCode,OverallScore,MaxScore,Status,Grade,EmailModuleIncluded,SslStatus,HeadersStatus,EmailStatus,ReputationStatus,PqcStatus,PqcReadinessLevel,AlertTypes,AlertMessages,Error"
    };

    foreach (var result in results)
    {
        rows.Add(string.Join(",",
            Csv(result.Domain),
            Csv(result.RequestSucceeded.ToString()),
            Csv(result.HttpStatusCode?.ToString() ?? string.Empty),
            Csv(result.OverallScore?.ToString() ?? string.Empty),
            Csv(result.MaxScore?.ToString() ?? string.Empty),
            Csv(result.Status),
            Csv(result.Grade),
            Csv(result.EmailModuleIncluded?.ToString() ?? string.Empty),
            Csv(result.SslStatus),
            Csv(result.HeadersStatus),
            Csv(result.EmailStatus),
            Csv(result.ReputationStatus),
            Csv(result.PqcStatus),
            Csv(result.PqcReadinessLevel),
            Csv(string.Join(" | ", result.AlertTypes)),
            Csv(string.Join(" | ", result.AlertMessages)),
            Csv(result.Error)));
    }

    return string.Join(Environment.NewLine, rows);
}

static string Csv(string? value)
{
    value ??= string.Empty;
    value = value.Replace("\"", "\"\"");
    return $"\"{value}\"";
}

static string MakeSafeFileName(string value)
{
    var invalidChars = Path.GetInvalidFileNameChars();
    var sanitized = new string(value.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray());
    return string.IsNullOrWhiteSpace(sanitized) ? "domain" : sanitized;
}

public sealed class BatchAssessmentResult
{
    public string Domain { get; set; } = string.Empty;
    public bool RequestSucceeded { get; set; }
    public int? HttpStatusCode { get; set; }
    public int? OverallScore { get; set; }
    public int? MaxScore { get; set; }
    public string? Status { get; set; }
    public string? Grade { get; set; }
    public bool? EmailModuleIncluded { get; set; }
    public string? SslStatus { get; set; }
    public string? HeadersStatus { get; set; }
    public string? EmailStatus { get; set; }
    public string? ReputationStatus { get; set; }
    public string? PqcStatus { get; set; }
    public string? PqcReadinessLevel { get; set; }
    public List<string> AlertTypes { get; set; } = [];
    public List<string> AlertMessages { get; set; } = [];
    public string? Error { get; set; }
}

public sealed class RawAssessmentResponse
{
    public string Domain { get; set; } = string.Empty;
    public bool RequestSucceeded { get; set; }
    public int? HttpStatusCode { get; set; }
    public string RawJson { get; set; } = string.Empty;
}

public sealed class AssessmentResponse
{
    public string? Domain { get; set; }
    public int OverallScore { get; set; }
    public int MaxScore { get; set; }
    public string? Status { get; set; }
    public string? Grade { get; set; }
    public bool EmailModuleIncluded { get; set; }
    public PqcReadinessResponse? PqcReadiness { get; set; }
    public AssessmentModulesResponse? Modules { get; set; }
    public List<AssessmentAlertResponse>? Alerts { get; set; }
}

public sealed class PqcReadinessResponse
{
    public string? Status { get; set; }
    public string? ReadinessLevel { get; set; }
}

public sealed class AssessmentModulesResponse
{
    [JsonPropertyName("sslTls")]
    public AssessmentModuleResponse? SslTls { get; set; }

    [JsonPropertyName("httpHeaders")]
    public AssessmentModuleResponse? HttpHeaders { get; set; }

    [JsonPropertyName("emailSecurity")]
    public AssessmentModuleResponse? EmailSecurity { get; set; }

    [JsonPropertyName("reputation")]
    public AssessmentModuleResponse? Reputation { get; set; }
}

public sealed class AssessmentModuleResponse
{
    public string? Status { get; set; }
}

public sealed class AssessmentAlertResponse
{
    public string? Type { get; set; }
    public string? Message { get; set; }
}
