using Microsoft.EntityFrameworkCore;
using SecurityAssessmentAPI.DAL;
using SecurityAssessmentAPI.DAL.Interfaces;
using SecurityAssessmentAPI.DAL.Repositories;
using SecurityAssessmentAPI.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("SecurityAssessmentDb"));

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
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
builder.Services.AddHttpClient<IHardenizeClient, HardenizeClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(20);
});
builder.Services.AddHttpClient<IVirusTotalClient, VirusTotalClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(20);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
