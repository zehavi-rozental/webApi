using MyMiddleware.Models;
using ServiceUsers.interfaces;
using Microsoft.Extensions.DependencyInjection;
using ServiceIceCream.interfaces;

namespace MyMiddleware.Services;
public class UserService : GenericJsonService<User>, IIUsers
{
    private readonly IIIceCreams? iceCreamService;

    public UserService(IIIceCreams? iceCreamService = null) : base("Users.json")
    {
        this.iceCreamService = iceCreamService;
    }

    public override void Delete(int id)
    {
        var user = Get(id);
        if (user is not null && iceCreamService != null)
        {
            // מחיקה waterfall: הסר את כל פריטי הגלידה של המשתמש הזה
            iceCreamService.DeleteAllByUserId(user.Id.ToString());
        }
        base.Delete(id);
    }
}

public static class UserServiceExtension
{
    public static void AddUserService(this IServiceCollection services)
    {
        services.AddScoped<IIUsers, UserService>();
    }
}