using MyMiddleware;
using MyMiddleware.Models;
using MyMiddleware.Extensions;
using MyMiddleware.Services;
using MyMiddleware.BackgroundServices;
using KsIceCream.Hubs;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

// 1. קודם כל יוצרים את ה-builder
var builder = WebApplication.CreateBuilder(args); 

// 2. מגדירים את הנתיבים ללוג
string executablePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
string binPath = Path.GetDirectoryName(executablePath) ?? AppContext.BaseDirectory;
string logPath = Path.Combine(binPath, "logs");

if (!Directory.Exists(logPath))
{
    Directory.CreateDirectory(logPath);
}

var logFile = Path.Combine(logPath, "app-.txt");

// 3. מגדירים את ה-Logger
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .WriteTo.Console()
    .WriteTo.File(
        path: logFile,
        rollingInterval: RollingInterval.Day,
        buffered: false,
        shared: true,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

// 4. מחברים את Serilog ל-builder (רק פעם אחת!)
builder.Host.UseSerilog();

// --- 2. רישום שירותים (Dependency Injection) ---

// NOTE: Background logging services נרשמים ב-AddMyServices כדי לשמור על חלוקה ברורה של רישום שירותים.

// הגדרת CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins("http://localhost:5000", "https://localhost:5001", "http://127.0.0.1:5000", "https://127.0.0.1:5001")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// הגדרת Swagger עם תמיכה ב-JWT לשימוש בבקרים
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste here: Bearer <token>"
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

// רישום שירותי האפליקציה (גלידות, משתמשים וכו') דרך ה-Extension
builder.Services.AddMyServices(builder.Configuration);

var app = builder.Build();

// --- 3. הגדרת Pipeline (סדר ה-Middleware) ---

// --- 3. הגדרת Pipeline (סדר ה-Middleware) ---

// א. אבטחה וגישה
app.UseCors("AllowLocalhost");
app.UseHttpsRedirection();

// ב. קבצים סטטיים (חשוב שיופיעו לפני ה-Routing)
app.UseDefaultFiles();
app.UseStaticFiles();

// ג. ניתוב ואבטחה (Authentication חייב לבוא לפני Authorization)
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ד. המידלוור שלך - אחרי Authentication כדי שיהיה לו גישה ל-User
app.UseMyLogMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ה. מיפוי נקודות קצה (Controllers ו-SignalR)
app.MapControllers();
app.MapHub<ActivityHub>("/activityHub");

// --- 4. הרצת האפליקציה ---
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