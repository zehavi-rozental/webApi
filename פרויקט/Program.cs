using MyMiddleware;
using MyMiddleware.Models;
using MyMiddleware.Extensions;
using MyMiddleware.Services;
using MyMiddleware.BackgroundServices;
using MyMiddleware.Data;
using KsIceCream.Hubs;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

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

// --- Database Initialization and Seeding ---
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        // Apply migrations
        dbContext.Database.Migrate();
        Log.Information("Database migrations applied successfully");

        // Seed data from JSON files if tables are empty
        if (!dbContext.Users.Any())
        {
            Log.Information("Seeding users from JSON file...");
            SeedUsersFromJson(dbContext);
        }

        if (!dbContext.IceCreams.Any())
        {
            Log.Information("Seeding ice creams from JSON file...");
            SeedIceCreamsFromJson(dbContext);
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error during database initialization or seeding");
    }
}

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

// --- Seeding Functions ---

void SeedUsersFromJson(AppDbContext dbContext)
{
    try
    {
        var jsonFilePath = Path.Combine("Data", "Users.json");
        if (File.Exists(jsonFilePath))
        {
            var json = File.ReadAllText(jsonFilePath);
            var users = JsonSerializer.Deserialize<List<User>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (users != null && users.Count > 0)
            {
                foreach (var user in users)
                {
                    if (!dbContext.Users.Any(u => u.Id == user.Id))
                    {
                        dbContext.Users.Add(user);
                    }
                }
                dbContext.SaveChanges();
                Log.Information($"Seeded {users.Count} users from JSON file");
            }
        }
        else
        {
            Log.Warning("Users.json file not found");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error seeding users from JSON");
    }
}

void SeedIceCreamsFromJson(AppDbContext dbContext)
{
    try
    {
        var jsonFilePath = Path.Combine("Data", "IceCream.json");
        if (File.Exists(jsonFilePath))
        {
            var json = File.ReadAllText(jsonFilePath);
            var iceCreams = JsonSerializer.Deserialize<List<IceCream>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (iceCreams != null && iceCreams.Count > 0)
            {
                foreach (var iceCream in iceCreams)
                {
                    if (!dbContext.IceCreams.Any(i => i.Id == iceCream.Id))
                    {
                        dbContext.IceCreams.Add(iceCream);
                    }
                }
                dbContext.SaveChanges();
                Log.Information($"Seeded {iceCreams.Count} ice creams from JSON file");
            }
        }
        else
        {
            Log.Warning("IceCream.json file not found");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error seeding ice creams from JSON");
    }
}
