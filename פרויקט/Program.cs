using MyMiddleware;
using MyMiddleware.Extensions;
using KsIceCream.Hubs;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// --- הגדרת Serilog ---
string executablePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
string binPath = Path.GetDirectoryName(executablePath) ?? AppContext.BaseDirectory;
string logPath = Path.Combine(binPath, "logs");

if (!Directory.Exists(logPath))
{
    Directory.CreateDirectory(logPath);
}

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // מונע הצפת לוגים של המערכת
    .WriteTo.Console() // מאפשר לראות את ה-URL והודעות בטרמינל
    .WriteTo.File(
        path: Path.Combine(logPath, "app-.txt"),
        rollingInterval: RollingInterval.Day,
        fileSizeLimitBytes: 52428800, // 50 MB
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// --- הגדרת CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// --- רישום שירותים (Services) ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// הגדרת Swagger עם תמיכה ב-JWT
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste here: Bearer <token>\n(Copy the token from /User/Login)"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// רישום שירותי האפליקציה דרך ה-Extension שלך
builder.Services.AddMyServices(builder.Configuration);

var app = builder.Build();

// --- הגדרת Pipeline (Middleware) ---

// שימוש ב-Middleware של הלוגים שלך
app.UseMyLogMiddleware();

// הפעלת CORS
app.UseCors("AllowLocalhost");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// מיפוי SignalR
app.MapHub<ActivityHub>("/activityHub");

// --- הרצת האפליקציה עם טיפול בשגיאות ---
try
{
    Log.Information("Application starting up...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start correctly");
}
finally
{
    Log.CloseAndFlush();
}