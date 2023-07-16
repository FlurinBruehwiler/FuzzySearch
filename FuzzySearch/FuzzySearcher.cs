namespace FuzzySearch;

public class FuzzySearcher<T>
{
    private readonly List<T> _records;
    private readonly SearchOptions<T> _options;

    public FuzzySearcher(IEnumerable<T> records, SearchOptions<T>? options = null)
    {
        _records = records.ToList();
        _options = options ?? new SearchOptions<T>();
    }

    public List<SearchResult<T>> Search(string text)
    {
        var results = new List<SearchResult<T>>();

        var index = Searcher.CreateIndex(text);

        foreach (var record in _records)
        {
            string value;
            
            if (record is string str)
            {
                value = str;
            }
            else if(_options.GetValue != null)
            {
                value = _options.GetValue(record);
            }
            else
            {
                throw new Exception();
            }
            
            var res = Searcher.Compare(value, index, _options);

            if (res.IsMatch)
            {
                results.Add(new SearchResult<T>(record, res.Score));
            }
        }

        return results;
    }
}

public class SearchOptions<T>
{
    public bool IsCaseSensitve { get; init; } = false;
    public int Distance { get; init; } = 100;
    public bool FindAllMatches { get; init; } = false;
    public bool IgnoreLocation { get; init; } = false;
    public bool IncludeMatches { get; init; } = false;
    public int Location { get; init; } = 0;
    public int MinMatchCharLength { get; init; } = 1;
    public double Threshold { get; init; } = 0.6f;

    public Func<T, string>? GetValue { get; set; }

}

public record struct SearchResult<T>(T Value, double Score);
