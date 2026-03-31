using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register the API surface and documentation endpoints first so every module is exposed consistently.
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// Logging configuration
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Keep external integrations isolated behind typed clients so timeouts and retries can be tuned per provider.
builder.Services.AddHttpClient<SecurityAssessmentAPI.Services.ISslLabsClient, SecurityAssessmentAPI.Services.SslLabsClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// HttpClient for Mozilla Observatory API
builder.Services.AddHttpClient<SecurityAssessmentAPI.Services.IMozillaObservatoryClient, SecurityAssessmentAPI.Services.MozillaObservatoryClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// HttpClient for direct HTTP header probing
builder.Services.AddHttpClient<SecurityAssessmentAPI.Services.IHttpHeadersProbeClient, SecurityAssessmentAPI.Services.HttpHeadersProbeClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(20);
});

// HttpClient for DNS-based email security analysis
builder.Services.AddHttpClient<SecurityAssessmentAPI.Services.IDnsAnalysisClient, SecurityAssessmentAPI.Services.DnsAnalysisClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(20);
});

// HttpClient for VirusTotal domain reputation analysis
builder.Services.AddHttpClient<SecurityAssessmentAPI.Services.IVirusTotalClient, SecurityAssessmentAPI.Services.VirusTotalClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(20);
});

// Compose the domain assessment pipeline from focused scoring services.
builder.Services.AddScoped<SecurityAssessmentAPI.Services.ISslCheckingService, SecurityAssessmentAPI.Services.SslCheckingService>();
builder.Services.AddScoped<SecurityAssessmentAPI.Services.IHeadersCheckingService, SecurityAssessmentAPI.Services.HeadersCheckingService>();
builder.Services.AddScoped<SecurityAssessmentAPI.Services.IEmailCheckingService, SecurityAssessmentAPI.Services.EmailCheckingService>();
builder.Services.AddScoped<SecurityAssessmentAPI.Services.IReputationCheckingService, SecurityAssessmentAPI.Services.ReputationCheckingService>();
builder.Services.AddScoped<SecurityAssessmentAPI.Services.IAssessmentCheckingService, SecurityAssessmentAPI.Services.AssessmentCheckingService>();
builder.Services.AddScoped<SecurityAssessmentAPI.Services.IPqcCheckingService, SecurityAssessmentAPI.Services.PqcCheckingService>();

// Hardenize fallback HTTP client and service
builder.Services.AddHttpClient<SecurityAssessmentAPI.Services.IHardenizeClient, SecurityAssessmentAPI.Services.HardenizeClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Use an in-memory store for local development and demo flows without requiring database setup.
builder.Services.AddDbContext<SecurityAssessmentAPI.DAL.ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("SecurityAssessmentDb"));

builder.Services.AddScoped<SecurityAssessmentAPI.DAL.Interfaces.ICustomerRepository, SecurityAssessmentAPI.DAL.Repositories.CustomerRepository>();
builder.Services.AddScoped<SecurityAssessmentAPI.DAL.Interfaces.IAssetRepository, SecurityAssessmentAPI.DAL.Repositories.AssetRepository>();
builder.Services.AddScoped<SecurityAssessmentAPI.DAL.Interfaces.IAssessmentRunRepository, SecurityAssessmentAPI.DAL.Repositories.AssessmentRunRepository>();
builder.Services.AddScoped<SecurityAssessmentAPI.DAL.Interfaces.ICheckTypeRepository, SecurityAssessmentAPI.DAL.Repositories.CheckTypeRepository>();
builder.Services.AddScoped<SecurityAssessmentAPI.DAL.Interfaces.ICheckResultRepository, SecurityAssessmentAPI.DAL.Repositories.CheckResultRepository>();
builder.Services.AddScoped<SecurityAssessmentAPI.DAL.Interfaces.IFindingRepository, SecurityAssessmentAPI.DAL.Repositories.FindingRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
