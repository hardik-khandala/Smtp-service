using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.AspNetCore.RateLimiting;
using PortfolioBackend.Configuration;
using PortfolioBackend.Services;
using PortfolioBackend.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection(SmtpSettings.SectionName));
builder.Services.AddScoped<IEmailService, ZohoEmailService>();

// 3. Register FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<ContactRequestValidator>();

// 4. Configure CORS
const string ContactCorsPolicy = "PortfolioFrontendCors";
builder.Services.AddCors(options =>
{
    options.AddPolicy(ContactCorsPolicy, policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",             
                "https://hardik-khandala.github.io"    
              )
              .AllowAnyHeader()
              .WithMethods("POST");
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("ContactFormPolicy", httpContext =>
    {
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 3,                    
            Window = TimeSpan.FromMinutes(10),  
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0                       
        });
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Portfolio API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseCors(ContactCorsPolicy);

app.UseRateLimiter();

app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok("healthy"));

app.Run();