using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Entity Framework and DI
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
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
