using FuseSearch;

public class Fuse
{
    private IEnumerable<string> _docs;
    private FuseOptions _fuseOptions;

    public Fuse(IEnumerable<string> docs, FuseOptions? options = null, FuseIndex? index = null)
    {
        SetCollection(docs, index);
        _fuseOptions = options ?? new FuseOptions();
    }

    public void SetCollection(IEnumerable<string> docs, FuseIndex? index = null)
    {
        _docs = docs;
    }

    public List<Result> Search(string query)
    {
        var results = SearchStringList(query);
        
        ComputeScore(results);

        if (_fuseOptions.ShouldSort)
        {
            results.Sort(Comparison);
        }

        return results;
    }

    private int Comparison(Result x, Result y)
    {
        if (x.Score < y.Score)
        {
            return -1;
        }

        if (x.Score > y.Score)
        {
            return 1;
        }

        return 0;
    }

    private void ComputeScore(List<Result> results)
    {
    }
    
    private List<Result> SearchStringList(string query)
    {
        var searcher = new BitTapSearch(query, _fuseOptions);
        var results = new List<Result>();
        
        foreach (var text in _docs)
        {
            var result = searcher.SearchIn(text);

            if (result.IsMatch)
            {
                results.Add(result);
            }
        }

        return results;
    }
}