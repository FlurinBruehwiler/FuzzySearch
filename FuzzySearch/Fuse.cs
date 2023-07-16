namespace FlurinBruehwiler.FuzzySearch;

public class Fuse
{
    private readonly List<string> _records;
    private readonly FuseOptions _options;

    public Fuse(IEnumerable<string> records, FuseOptions? fuseOptions = null)
    {
        _records = records.ToList();
        _options = fuseOptions ?? new FuseOptions();
    }

    public List<SearchResult> Search(string text)
    {
        var results = new List<SearchResult>();

        var index = Searcher.CreateIndex(text);

        foreach (var record in _records)
        {
            var res = Searcher.Compare(record, index, _options);

            if (res.IsMatch)
            {
                results.Add(new SearchResult(record, res.Score));
            }
        }

        return results;
    }
}

public class FuseOptions
{
    public bool IsCaseSensitve { get; init; } = false;
    public int Distance { get; init; } = 100;
    public bool FindAllMatches { get; init; } = false;
    public bool IgnoreLocation { get; init; } = false;
    public bool IncludeMatches { get; init; } = false;
    public int Location { get; init; } = 0;
    public int MinMatchCharLength { get; init; } = 1;
    public double Threshold { get; init; } = 0.6f;
}

public record struct SearchResult(string Text, double Score);
