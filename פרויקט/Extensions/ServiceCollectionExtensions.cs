using Shared.Interfaces;
using MyMiddleware.Services;
using MyMiddleware.Interfaces;
using MyMiddleware.BackgroundServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace MyMiddleware.Extensions
{
    /// <summary>
    /// דוגמה לרישום כל השירותים הנדרשים באפליקציה
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMyServices(this IServiceCollection services, IConfiguration configuration)
        {
            // HttpContextAccessor להגשת מידע משתמש בתוך שירותים
            services.AddHttpContextAccessor();

            // Authentication and JWT
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = TokenService.GetTokenValidationParameters();
                });

            // SignalR
            services.AddSignalR();

            // שירות משתמש פעיל (Scoped כדי לספק מידע על משתמש הנוכחי)
            services.AddScoped<IActiveUser, ActiveUserService>();

            // שירות גלידה (Scoped כדי לעבוד עם משתמש הנוכחי)
            services.AddIceCreamService();

            // שירות משתמש (Scoped כדי להתמודד עם מחיקה של waterfall)
            services.AddUserService();

            // Background logging
            services.AddSingleton<BackgroundLogQueue>();
            services.AddHostedService<BackgroundLogWorker>();

            return services;
        }
    }
}
