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

    public List<FuseResult> Search(string query)
    {
        var searcher = new BitTapSearch(query);
        
        foreach (var text in _docs)
        {
            searcher.SearchIn(text);
        }
        
        throw new NotImplementedException();
    }

}