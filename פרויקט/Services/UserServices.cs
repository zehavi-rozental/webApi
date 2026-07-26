using MyMiddleware.Models;
using MyMiddleware.Data;
using ServiceUsers.interfaces;
using ServiceIceCream.interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace MyMiddleware.Services;

public class UserService : IIUsers
{
    private readonly AppDbContext dbContext;
    private readonly IIIceCreams? iceCreamService;

    public UserService(AppDbContext dbContext, IIIceCreams? iceCreamService = null)
    {
        this.dbContext = dbContext;
        this.iceCreamService = iceCreamService;
    }

    public List<User> GetAll()
    {
        return dbContext.Users.ToList();
    }

    public User? Get(int id)
    {
        return dbContext.Users.FirstOrDefault(u => u.Id == id);
    }

    public void Add(User user)
    {
        dbContext.Users.Add(user);
        dbContext.SaveChanges();
    }

    public void Update(User user)
    {
        var existing = dbContext.Users.FirstOrDefault(u => u.Id == user.Id);
        if (existing != null)
        {
            existing.Name = user.Name;
            existing.Password = user.Password;
            existing.Role = user.Role;
            dbContext.SaveChanges();
        }
    }

    public void Delete(int id)
    {
        var user = Get(id);
        if (user is not null && iceCreamService != null)
        {
            // Cascading delete: remove all ice creams of this user first
            iceCreamService.DeleteAllByUserId(user.Id.ToString());
        }

        // Now delete the user
        var userToDelete = dbContext.Users.FirstOrDefault(u => u.Id == id);
        if (userToDelete != null)
        {
            dbContext.Users.Remove(userToDelete);
            dbContext.SaveChanges();
        }
    }
}

public static class UserServiceExtension
{
    public static void AddUserService(this IServiceCollection services)
    {
        services.AddScoped<IIUsers, UserService>();
    }
}
