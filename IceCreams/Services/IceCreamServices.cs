using IceCreams.Models;
using ServiceIceCream.interfaces;
using Microsoft.Extensions.DependencyInjection;
namespace IceCreams.Services;

    public class IceCreamService : IIIceCreams
    {
        
    private  List<IceCream> IceCreams;
    private  int nextId = 3;

    public IceCreamService()
    {
        IceCreams = new List<IceCream>
        {
            new IceCream { Id = 1, Name = "Mocha", Milki = false },
            new IceCream { Id = 2, Name = "Vanilla", Milki = true }
        };
    }

    public  List<IceCream> GetAll() => IceCreams;

    public  IceCream? Get(int id) => IceCreams.FirstOrDefault(i => i.Id == id);

    public  void Add(IceCream iceCream)
    {
        iceCream.Id = nextId++;
        IceCreams.Add(iceCream);
    }

    public  void Delete(int id)
    {
        var iceCream = Get(id);
        if (iceCream is null)
            return;
        IceCreams.Remove(iceCream);
    }

    public  void Update(IceCream iceCream)
    {
        var index = IceCreams.FindIndex(i => i.Id == iceCream.Id);
        if (index == -1)
            return;

        IceCreams[index] = iceCream;
    }

    }

 public static class IceCreamServiceExtension
{
    public static void AddIceCreamService(this IServiceCollection services)
    {
        services.AddSingleton<IIIceCreams, IceCreamService>();      
    }
}