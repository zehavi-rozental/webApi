using Users.Models;
using ServiceUsers.interfaces;
using Microsoft.Extensions.DependencyInjection;
using Shared.Services;

namespace Users.Services;

    public class UserService : GenericJsonService<User>, IIUsers
    {
        public UserService() : base("Users.json")
        {
        }
    }
 public static class UserServiceExtension
{
    public static void AddUserService(this IServiceCollection services)
    {
        services.AddSingleton<IIUsers, UserService>();      
    }
}
 