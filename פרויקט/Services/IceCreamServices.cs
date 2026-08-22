using MyMiddleware.Models;
using MyMiddleware.Data;
using ServiceIceCream.interfaces;
using KsIceCream.Hubs;
using Microsoft.AspNetCore.SignalR;
using Shared.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace MyMiddleware.Services;

public class IceCreamService : IIIceCreams
{
    private readonly AppDbContext dbContext;
    private readonly IActiveUser activeUser;
    private readonly IHubContext<ActivityHub> hubContext;

    public IceCreamService(AppDbContext dbContext, IActiveUser activeUser, IHubContext<ActivityHub> hubContext)
    {
        this.dbContext = dbContext;
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
        return dbContext.IceCreams.Where(i => i.UserId == userId).ToList();
    }

    public List<IceCream> GetAll()
    {
        // Admins see all ice creams, regular users see only their own
        if (IsAdmin())
            return dbContext.IceCreams.ToList();
        return GetAllForCurrentUser();
    }

    public IceCream? Get(int id)
    {
        var userId = GetCurrentUserId();
        var item = dbContext.IceCreams.FirstOrDefault(i => i.Id == id);

        if (item == null)
            return null;

        // Admins can see any item, regular users can only see their own
        if (IsAdmin())
            return item;

        return item.UserId == userId ? item : null;
    }

    public void Add(IceCream iceCream)
    {
        dbContext.IceCreams.Add(iceCream);
        dbContext.SaveChanges();
        BroadcastActivityToUser("added", iceCream);
    }

    public void Update(IceCream iceCream)
    {
        // Find the existing item and update only Name and Milki (not UserId)
        var existing = dbContext.IceCreams.FirstOrDefault(i => i.Id == iceCream.Id);
        if (existing != null)
        {
            existing.Name = iceCream.Name;
            existing.Milki = iceCream.Milki;
            dbContext.SaveChanges();
            BroadcastActivityToUser("updated", existing);
        }
    }

    public void Delete(int id)
    {
        var item = dbContext.IceCreams.FirstOrDefault(i => i.Id == id);
        if (item != null)
        {
            dbContext.IceCreams.Remove(item);
            dbContext.SaveChanges();
            BroadcastActivityToUser("deleted", item);
        }
    }

    // For internal use: delete all items belonging to a user (cascading delete)
    public void DeleteAllByUserId(string userId)
    {
        var itemsToDelete = dbContext.IceCreams.Where(i => i.UserId == userId).ToList();
        foreach (var item in itemsToDelete)
        {
            dbContext.IceCreams.Remove(item);
        }
        dbContext.SaveChanges();
    }

    /// <summary>
    /// Example of raw SQL query using EF Core's FromSqlInterpolated.
    /// This demonstrates how to execute SQL directly while maintaining protection against SQL Injection.
    /// The query uses SQL interpolation with parameters, which is the safe approach.
    /// Use case: Performance optimization for complex queries that LINQ cannot efficiently translate.
    /// </summary>
    public List<IceCream> GetIceCreamsForUserSql(string userId)
    {
        // Safe SQL query using FromSqlInterpolated - parameters are handled automatically
        // This query selects all ice creams for a user, ordered by name
        var result = dbContext.IceCreams
            .FromSqlInterpolated($"SELECT * FROM IceCreams WHERE UserId = {userId} ORDER BY Name")
            .ToList();
        
        return result;
    }

    public List<UserIceCreamStatsDto> GetUserIceCreamStatsSql()
    {
        return dbContext.Database.SqlQuery<UserIceCreamStatsDto>($"""
            SELECT u.Name,
                   COUNT(i.Id) AS TotalCount,
                   SUM(CASE WHEN i.Milki = 1 THEN 1 ELSE 0 END) AS MilkiCount
            FROM Users u
            JOIN IceCreams i ON CAST(u.Id AS TEXT) = i.UserId
            GROUP BY u.Id, u.Name
            HAVING COUNT(i.Id) > 0
            ORDER BY TotalCount DESC
            """).ToList();
    }

    public List<UserIceCreamRankingDto> GetTopUserByIceCreamCountSql()
    {
        return dbContext.Database.SqlQuery<UserIceCreamRankingDto>($"""
            SELECT u.Name,
                   COUNT(i.Id) AS TotalCount,
                   RANK() OVER (ORDER BY COUNT(i.Id) DESC) AS UserRank
            FROM Users u
            JOIN IceCreams i ON CAST(u.Id AS TEXT) = i.UserId
            GROUP BY u.Id, u.Name
            """).ToList();
    }

    private void BroadcastActivityToUser(string action, IceCream? item)
    {
        var user = activeUser.ActiveUser;
        if (user != null && item != null)
        {
            // Only notify the current user's connections
            try
            {
                hubContext.Clients.User(user.Id).SendAsync("ReceiveActivity", new { username = user.Username, action, itemName = item.Name }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error broadcasting activity: {ex.Message}");
            }
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
