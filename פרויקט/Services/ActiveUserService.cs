using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Shared.Interfaces;

namespace MyMiddleware.Services;
    public class ActiveUserService : IActiveUser
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public ActiveUserService(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public ActiveUserData? ActiveUser
        {
            get
            {
                var user = httpContextAccessor?.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated != true)
                    return null;

                var userId = user.FindFirst("userId")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
                var username = user.FindFirst("username")?.Value ?? user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
                var role = user.FindFirst(ClaimTypes.Role)?.Value ?? "User";

                if (string.IsNullOrEmpty(username))
                    return null;

                return new ActiveUserData
                {
                    Id = userId,
                    Username = username,
                    Role = role
                };
            }
        }
    }
