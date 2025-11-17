using IceCreams.Models;

namespace ServiceIceCream.interfaces
{
    public interface IIIceCreams
    {
        List<IceCream> GetAll();
        IceCream? Get(int id);
        void Add(IceCream iceCream);
        void Delete(int id);
        void Update(IceCream iceCream);
    }
}
