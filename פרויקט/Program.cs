using MyMiddleware;
using MyMiddleware.Extensions;
using KsIceCream.Hubs;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

var builder = WebApplication.CreateBuilder(args);

// --- הגדרת נתיב לוגים ---
string logPath = Path.Combine(AppContext.BaseDirectory, "logs");
if (!Directory.Exists(logPath)) Directory.CreateDirectory(logPath);

// --- הגדרת Serilog עם צבעים (Themes) ותבנית נקייה ---
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information) // מחזיר את הודעות ה-URL המקוריות של המערכת
    .Enrich.FromLogContext()
    // הגדרת הקונסול עם צבעים כמו ב-Default של .NET
    .WriteTo.Console(
        applyThemeToRedirectedOutput: true,
        theme: AnsiConsoleTheme.Code, 
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    // כתיבה לקובץ (נשארת ללא שינוי ביעילות)
    .WriteTo.File(
        path: Path.Combine(logPath, "app-.txt"),
        rollingInterval: RollingInterval.Day,
        fileSizeLimitBytes: 52428800,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// --- רישום שירותים (Services) ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", b =>
    {
        b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// Swagger עם JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ice Cream API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer <token>"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

// רישום שירותים חיצוניים
builder.Services.AddMyServices(builder.Configuration);

var app = builder.Build();

// --- Pipeline (Middleware) ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseCors("AllowLocalhost");

// ה-Middleware שלך לתיעוד בקשות
app.UseMyLogMiddleware();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ActivityHub>("/activityHub");

// --- הרצה ---
try
{
    Log.Information("Starting Web Host...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}