using MyMiddleware.Models;
using ServiceIceCream.interfaces;
using KsIceCream.Hubs;
using Microsoft.Extensions.DependencyInjection;
using Shared.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Linq;

namespace MyMiddleware.Services;
public class IceCreamService : GenericJsonService<IceCream>, IIIceCreams
{
    private readonly IActiveUser activeUser;
    private readonly IHubContext<ActivityHub> hubContext;

    public IceCreamService(IActiveUser activeUser, IHubContext<ActivityHub> hubContext) : base("IceCream.json")
    {
        this.activeUser = activeUser;
        this.hubContext = hubContext;
    }

    private string GetCurrentUserId()
    {
        var user = activeUser.ActiveUser;
        return user?.Id ?? "unknown";
    }

    private bool IsAdmin()
    {
        var user = activeUser.ActiveUser;
        return user?.Role == "Admin";
    }

    public List<IceCream> GetAllForCurrentUser()
    {
        // Returns ice creams for the current user only
        var userId = GetCurrentUserId();
        return Items.Where(i => i.UserId == userId).ToList();
    }

    public override List<IceCream> GetAll()
    {
        // Admins see all ice creams, regular users see only their own
        if (IsAdmin())
            return Items;
        return GetAllForCurrentUser();
    }

    public override IceCream? Get(int id)
    {
        var userId = GetCurrentUserId();
        var idProperty = typeof(IceCream).GetProperty("Id");
        if (idProperty == null)
            return null;

        var item = Items.FirstOrDefault(i =>
        {
            var idValue = idProperty.GetValue(i);
            if (idValue is not int intId || intId != id)
                return false;
            return true;
        });

        if (item == null)
            return null;

        // Admins can see any item, regular users can only see their own
        if (IsAdmin())
            return item;

        return item.UserId == userId ? item : null;
    }

    public override void Add(IceCream item)
    {
        base.Add(item);
        BroadcastActivityToUser("added", item.Name);
    }

    public override void Delete(int id)
    {
        var item = Get(id);
        if (item != null)
        {
            BroadcastActivityToUser("deleted", item.Name);
        }
        base.Delete(id);
    }

    public override void Update(IceCream item)
    {
        // Do not change UserId here, as it should remain the same
        base.Update(item);
        BroadcastActivityToUser("updated", item.Name);
    }

    // For internal use: delete all items belonging to a user (cascading delete)
    public void DeleteAllByUserId(string userId)
    {
        var itemsToDelete = Items.Where(i => i.UserId == userId).ToList();
        foreach (var item in itemsToDelete)
        {
            Items.Remove(item);
        }
        SaveToFile();
    }

    private void BroadcastActivityToUser(string action, string itemName)
    {
        var user = activeUser.ActiveUser;
        if (user != null)
        {
            // Only notify the current user's connections
            hubContext.Clients.User(user.Id).SendAsync("ReceiveActivity", user.Username, action, itemName);
        }
    }
}

public static class IceCreamServiceExtension
{
    public static void AddIceCreamService(this IServiceCollection services)
    {
        services.AddScoped<IIIceCreams, IceCreamService>();
    }
}