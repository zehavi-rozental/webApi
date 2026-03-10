using System.Text.Json;
namespace MyMiddleware.Services;

public abstract class GenericJsonService<T> where T : class
{
    protected List<T> Items;
    protected int nextId = 3;
    protected string filePath;

    public GenericJsonService(string dataFileName)
    {
        this.filePath = Path.Combine("Data", dataFileName);
        Items = new List<T>();
        LoadFromFile();
    }

    protected virtual void LoadFromFile()
    {
        try
        {
            using (var jsonFile = File.OpenText(filePath))
            {
                var content = jsonFile.ReadToEnd();
                var deserializedItems = JsonSerializer.Deserialize<List<T>>(content,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                
                Items = deserializedItems ?? new List<T>();

                // Calculate next ID based on existing items
                if (Items.Count > 0)
                {
                    var idProperty = typeof(T).GetProperty("Id");
                    if (idProperty != null)
                    {
                        var maxId = 0;
                        foreach (var item in Items)
                        {
                            var idValue = idProperty.GetValue(item);
                            if (idValue is int intId)
                            {
                                maxId = Math.Max(maxId, intId);
                            }
                        }
                        nextId = maxId + 1;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error loading from file: {ex.Message}");
            Items = new List<T>();
        }
    }

    protected void SaveToFile()
    {
        try
        {
            var text = JsonSerializer.Serialize(Items);
            File.WriteAllText(filePath, text);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error saving to file: {ex.Message}");
        }
    }

    public virtual List<T> GetAll() => Items;

    public virtual T? Get(int id)
    {
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty == null)
            return null;

        return Items.FirstOrDefault(i =>
        {
            var idValue = idProperty.GetValue(i);
            return idValue is int intId && intId == id;
        });
    }

    public virtual void Add(T item)
    {
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty != null && idProperty.CanWrite)
        {
            idProperty.SetValue(item, nextId++);
        }
        Items.Add(item);
        SaveToFile();
    }

    public virtual void Delete(int id)
    {
        var item = Get(id);
        if (item is null)
            return;
        Items.Remove(item);
        SaveToFile();
    }

    public virtual void Update(T item)
    {
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty == null)
            return;

        var itemIdValue = idProperty.GetValue(item);
        if (itemIdValue is not int itemId)
            return;

        var index = Items.FindIndex(i =>
        {
            var idValue = idProperty.GetValue(i);
            return idValue is int intId && intId == itemId;
        });

        if (index == -1)
            return;

        Items[index] = item;
        SaveToFile();
    }
}
