using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WestDesk.Application.Interfaces;
using WestDesk.Infrastructure.Persistence;
using WestDesk.Infrastructure.Services;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Hangfire;
using WestDesk.Application.Services;
using WestDesk.Infrastructure.Jobs;
using WestDesk.Infrastructure.Hubs;
using Hangfire.PostgreSql;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "https://westdesk-frontend.onrender.com"
              )
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
    });
});

builder.Services.AddDbContext<WestDeskDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));

builder.Services.AddHangfireServer();

builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(
    StackExchange.Redis.ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IStripeService, StripeService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddHttpClient<IAIService, GeminiAIService>();
builder.Services.AddSignalR();


var jwtSecret = builder.Configuration["Jwt:Secret"]!;
var key = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

    })
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();


builder.Services.AddRateLimiter(options =>
{

    options.RejectionStatusCode = 429;


    options.AddFixedWindowLimiter("fixed", limiterOptions =>
    {
        limiterOptions.PermitLimit = 20;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// --- OpenAPI (native .NET 10 desteği - Swashbuckle yerine) ---
builder.Services.AddOpenApi();

var port = Environment.GetEnvironmentVariable("PORT") ?? "5004";
builder.WebHost.UseUrls($"http://+:{port}");
var app = builder.Build();

// --- OpenAPI endpoint + Scalar UI ---
app.MapOpenApi();
app.MapScalarApiReference(); // arayüz: /scalar/v1


app.UseHttpsRedirection();

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.UseHangfireDashboard("/hangfire");
app.UseRateLimiter();

app.MapControllers();
app.MapHub<TicketHub>("/hubs/ticket");

RecurringJob.AddOrUpdate<TicketReminderJob>(
    "stale-ticket-reminder",
    job => job.SendStaleTicketRemindersAsync(),
    "0 9 * * *"
);

app.Run();