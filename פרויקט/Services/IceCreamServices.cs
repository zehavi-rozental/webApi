using IceCreams.Models;
using ServiceIceCream.interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
namespace IceCreams.Services;

    public class IceCreamService : IIIceCreams
    {
        
    private  List<IceCream> IceCreams;
    private  int nextId = 3;
    private string filePath;
    
 
        public IceCreamService()
        {
            IceCreams = new List<IceCream>()
              { new IceCream { Id = 1, Name = "Mocha", Milki = false },
               new IceCream { Id = 2, Name = "Vanilla", Milki = true }};
            //this.webHost = webHost;
            this.filePath = Path.Combine("Data", "IceCream.json");
            using (var jsonFile = File.OpenText(filePath))
            {
                var content = jsonFile.ReadToEnd();
                IceCreams = JsonSerializer.Deserialize<List<IceCream>>(content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
        }
        private void saveToFile()
        {
            var text = JsonSerializer.Serialize(IceCreams);
            File.WriteAllText(filePath, text);
        }


    public  List<IceCream> GetAll() => IceCreams;

    public  IceCream? Get(int id) => IceCreams.FirstOrDefault(i => i.Id == id);

    public  void Add(IceCream iceCream)
    {
        iceCream.Id = nextId++;
        IceCreams.Add(iceCream);
        saveToFile();
    }

    public  void Delete(int id)
    {
        var iceCream = Get(id);
        if (iceCream is null)
            return;
        IceCreams.Remove(iceCream);
        saveToFile();
    }

    public  void Update(IceCream iceCream)
    {
        var index = IceCreams.FindIndex(i => i.Id == iceCream.Id);
        if (index == -1)
            return;

        IceCreams[index] = iceCream;
        saveToFile();
    }

    }

 public static class IceCreamServiceExtension
{
    public static void AddIceCreamService(this IServiceCollection services)
    {
        services.AddSingleton<IIIceCreams, IceCreamService>();      
    }
}