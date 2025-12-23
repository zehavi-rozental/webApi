using Users.Models;
using ServiceUsers.interfaces;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
namespace Users.Services;

    public class UserService : IIUsers
    {         

private  List<User> Users;
private  int nextId = 3;
private string filePath;


    public UserService()
    {
        Users = new List<User>()
        {
            new User { Id = 1, Name = "Lion" },
            new User { Id = 2, Name = "Moshe" }
        };
        this.filePath = Path.Combine("Data", "Users.json");
            using (var jsonFile = File.OpenText(filePath))
            {
                var content = jsonFile.ReadToEnd();
                Users = JsonSerializer.Deserialize<List<User>>(content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
    }
        private void saveToFile()
        {
            var text = JsonSerializer.Serialize(Users);
            File.WriteAllText(filePath, text);
        }
    

    public  List<User> GetAll() => Users;
    public  User? Get(int id) => Users.FirstOrDefault(i => i.Id == id);

    public  void Add(User user)
    {
        user.Id = nextId++;
        Users.Add(user);
              saveToFile();
    }

    public  void Delete(int id)
    {
        var user = Get(id);
        if (user is null)
            return;
        Users.Remove(user);
              saveToFile();
    }

    public  void Update(User user)
    {
        var index = Users.FindIndex(i => i.Id == user.Id);
        if (index == -1)
            return;

        Users[index] = user;
              saveToFile();
    }

    
}
 public static class UserServiceExtension
{
    public static void AddUserService(this IServiceCollection services)
    {
        services.AddSingleton<IIUsers, UserService>();      
    }
}
 