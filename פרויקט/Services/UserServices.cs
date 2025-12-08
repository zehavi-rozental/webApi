using Users.Models;
using ServiceUsers.interfaces;
using Microsoft.Extensions.DependencyInjection;
namespace Users.Services;

    public class UserService : IIUsers
    {
        
    private  List<User> Users;
    private  int nextId = 3;

    public UserService()
    {
        Users = new List<User>
        {
            new User { Id = 1, Name = "Lion" },
            new User { Id = 2, Name = "Moshe" }
        };
    }

    public  List<User> GetAll() => Users;
    public  User? Get(int id) => Users.FirstOrDefault(i => i.Id == id);

    public  void Add(User user)
    {
        user.Id = nextId++;
        Users.Add(user);
    }

    public  void Delete(int id)
    {
        var user = Get(id);
        if (user is null)
            return;
        Users.Remove(user);
    }

    public  void Update(User user)
    {
        var index = Users.FindIndex(i => i.Id == user.Id);
        if (index == -1)
            return;

        Users[index] = user;
    }

    }

 public static class UserServiceExtension
{
    public static void AddUserService(this IServiceCollection services)
    {
        services.AddSingleton<IIUsers, UserService>();      
    }
}