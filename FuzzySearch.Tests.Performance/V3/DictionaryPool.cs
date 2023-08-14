namespace FuzzySearch.Tests.Performance.V3;

public static class DictionaryPool
{
    private static readonly List<Dictionary<char, int>> Objects = new();

    public static Dictionary<char, int> GetDictionary()
    {
        if (Objects.Count == 0) 
            return new Dictionary<char, int>();
        
        var dict = Objects.First();
        Objects.Remove(dict);
        return dict;
    }

    public static void PutDictionary(Dictionary<char, int> dict)
    {
        Objects.Add(dict);
        dict.Clear();
    }
}