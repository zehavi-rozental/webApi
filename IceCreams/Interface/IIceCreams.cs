using IceCreams.Models;

namespace ServiceIceCream.interfaces
{
    public interface IIIceCreams
    {
        public  List<IceCream> GetAll();
        public  IceCream? Get(int id);
        public  void Add(IceCream iceCream);
        public  void Delete(int id);
        public  void Update(IceCream iceCream);
    }
}
