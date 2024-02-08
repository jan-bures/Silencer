namespace Silencer.Data;

public class SavedItem(string value, SavedItemType type)
{
    public string Value { get; set; } = value;
    public SavedItemType Type { get; } = type;

    public override string ToString()
    {
        return Type == SavedItemType.Regex ? $"/{Value}/" : Value;
    }
}