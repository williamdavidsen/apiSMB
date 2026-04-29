using Microsoft.EntityFrameworkCore;
using SecurityAssessmentAPI.DAL;
using SecurityAssessmentAPI.DAL.Interfaces;
using SecurityAssessmentAPI.DAL.Repositories;
using SecurityAssessmentAPI.Services;

var builder = WebApplication.CreateBuilder(args);
AddLocalConfiguration(builder);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("SecurityAssessmentDb"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddScoped<IAssetRepository, AssetRepository>();
builder.Services.AddScoped<IAssessmentRunRepository, AssessmentRunRepository>();
builder.Services.AddScoped<ICheckTypeRepository, CheckTypeRepository>();
builder.Services.AddScoped<ICheckResultRepository, CheckResultRepository>();
builder.Services.AddScoped<IFindingRepository, FindingRepository>();

builder.Services.AddScoped<IAssessmentCheckingService, AssessmentCheckingService>();
builder.Services.AddScoped<ISslCheckingService, SslCheckingService>();
builder.Services.AddScoped<IHeadersCheckingService, HeadersCheckingService>();
builder.Services.AddScoped<IEmailCheckingService, EmailCheckingService>();
builder.Services.AddScoped<IReputationCheckingService, ReputationCheckingService>();
builder.Services.AddScoped<IPqcCheckingService, PqcCheckingService>();

builder.Services.AddHttpClient<IDnsAnalysisClient, DnsAnalysisClient>();
builder.Services.AddHttpClient<IHttpHeadersProbeClient, HttpHeadersProbeClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(20);
});
builder.Services.AddHttpClient<IMozillaObservatoryClient, MozillaObservatoryClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddHttpClient<ISslLabsClient, SslLabsClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddHttpClient<IVirusTotalClient, VirusTotalClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(20);
});

var app = builder.Build();

// Keep Swagger enabled in production as well.
app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontend");

// Return a simple response on the root route.
app.MapGet("/", () => Results.Ok(new
{
    message = "SecurityAssessment API is running",
    swagger = "/swagger"
}));

app.MapControllers();

void AddLocalConfiguration(WebApplicationBuilder webApplicationBuilder)
{
    var localSettingsFileName = "appsettings.Local.json";
    var candidatePaths = new[]
    {
        Path.Combine(webApplicationBuilder.Environment.ContentRootPath, localSettingsFileName),
        Path.Combine(webApplicationBuilder.Environment.ContentRootPath, "API", localSettingsFileName)
    };

    var localSettingsPath = candidatePaths.FirstOrDefault(File.Exists);
    if (!string.IsNullOrWhiteSpace(localSettingsPath))
    {
        webApplicationBuilder.Configuration.AddJsonFile(localSettingsPath, optional: true, reloadOnChange: true);
    }
}

app.Run();

public partial class Program { }
