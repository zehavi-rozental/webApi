using Users.Models;

namespace ServiceUsers.interfaces
{
    public interface IIUsers
                    {
        List<User> GetAll();
        User? Get(int id);
        void Add(User user);
        void Delete(int id);
        void Update(User user);
    }
}
