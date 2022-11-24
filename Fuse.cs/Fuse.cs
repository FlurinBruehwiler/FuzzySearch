namespace Fuse.cs;

public class Fuse
{
    private IEnumerable<string> _docs;
    private FuseIndex _index;
    private FuseOptions _fuseOptions;

    public Fuse(IEnumerable<string> docs, FuseOptions? options = null, FuseIndex? index = null)
    {
        SetCollection(docs, index);
        _fuseOptions = options ?? new FuseOptions();
    }

    public void SetCollection(IEnumerable<string> docs, FuseIndex? index = null)
    {
        _docs = docs;
        _index = index ?? CreateIndex();
    }

    private FuseIndex CreateIndex()
    {
        return new FuseIndex();
    }

    public List<Result> Search(string query)
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

    private void CreateIndex()
    {
        var myIndex = new FuseIndex();
    }

    private void ComputeScore(List<Result> results)
    {
        foreach (var result in results)
        {
            var totalScore = 1;
            
            result.
        }
    }
}

public class FuseResult
{
    public string Item { get; set; }
    public TYPE Type { get; set; }
}