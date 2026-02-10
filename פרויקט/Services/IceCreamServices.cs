using IceCreams.Models;
using ServiceIceCream.interfaces;
using Microsoft.Extensions.DependencyInjection;
using Shared.Services;

namespace IceCreams.Services;

    public class IceCreamService : GenericJsonService<IceCream>, IIIceCreams
    {
        public IceCreamService() : base("IceCream.json")
        {
        }
    }

 public static class IceCreamServiceExtension
{
    public static void AddIceCreamService(this IServiceCollection services)
    {
        services.AddSingleton<IIIceCreams, IceCreamService>();      
    }
}