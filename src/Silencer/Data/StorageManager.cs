using Newtonsoft.Json;

namespace Silencer.Data;

internal class StorageManager
{
    private readonly string _savePath;

    public List<SavedItem> Data { get; }

    public StorageManager(string basePath)
    {
        _savePath = Path.Combine(basePath, "data.json");
        Data = Load();
    }

    public void Add(string item, SavedItemType type)
    {
        Data.Add(new SavedItem(item, type));
        Save();
    }

    public void Remove(SavedItem item)
    {
        Data.Remove(item);
        Save();
    }

    public void Edit(SavedItem item, string newValue)
    {
        item.Value = newValue;
        Save();
    }

    private List<SavedItem> Load()
    {
        if (!File.Exists(_savePath))
        {
            return [];
        }

        string json = File.ReadAllText(_savePath);
        return JsonConvert.DeserializeObject<List<SavedItem>>(json);
    }

    private void Save()
    {
        string json = JsonConvert.SerializeObject(Data);
        File.WriteAllText(_savePath, json);
    }
}