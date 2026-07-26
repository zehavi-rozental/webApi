using Shared.Interfaces;
using MyMiddleware.Services;
using MyMiddleware.Interfaces;
using MyMiddleware.BackgroundServices;
using MyMiddleware.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace MyMiddleware.Extensions
{
    /// <summary>
    /// דוגמה לרישום כל השירותים הנדרשים באפליקציה
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMyServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Entity Framework Core DbContext
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

            // HttpContextAccessor להגשת מידע משתמש בתוך שירותים
            services.AddHttpContextAccessor();

            // Authentication and JWT
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = TokenService.GetTokenValidationParameters();
                    // SignalR JWT configuration - read token from query string for negotiation
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            if (!string.IsNullOrEmpty(accessToken))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            // SignalR
            services.AddSignalR();
            services.AddSingleton<Microsoft.AspNetCore.SignalR.IUserIdProvider, UserIdProvider>();

            // שירות משתמש פעיל (Scoped כדי לספק מידע על משתמש הנוכחי)
            services.AddScoped<IActiveUser, ActiveUserService>();

            // שירות גלידה (Scoped כדי לעבוד עם משתמש הנוכחי)
            services.AddIceCreamService();

            // שירות משתמש (Scoped כדי להתמודד עם מחיקה של waterfall)
            services.AddUserService();

            // שירות אימות - כולל אימות Google ID Token ושחזור המשתמש הקיים
            services.AddScoped<IAuthService, AuthService>();

            // Background logging
            services.AddSingleton<BackgroundLogQueue>();
            services.AddHostedService<BackgroundLogWorker>();

            return services;
        }
    }

    public class UserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst("userId")?.Value;
        }
    }
}
