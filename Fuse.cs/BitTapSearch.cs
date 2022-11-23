namespace Fuse.cs;

public class BitTapSearch
{
    private readonly string _pattern;
    private readonly FuseOptions _fuseOptions;
    private readonly List<Chunk> _chunks;

    public BitTapSearch(string pattern, FuseOptions fuseOptions)
    {
        _pattern = fuseOptions.IsCaseSensitve ? pattern : pattern.ToLowerInvariant();
        _fuseOptions = fuseOptions;
        _chunks = new List<Chunk>();

    }

    private void AddChunk(string pattern, int startIndex)
    {
        _chunks.Add(new Chunk
        {
            Pattern = pattern,
            Alphabet = CreatePatternAlphabet(pattern),
            StartIndex = startIndex
        });
    }

    private string CreatePatternAlphabet(string pattern)
    {
        Dictionary<char, char> mask = new();

        for (int i = 0; i < pattern.Length; i++)
        {
            var c = pattern[i];
            mask[c] = 
        }
    }

    public float SearchIn(string text)
    {
        if(!_fuseOptions.IsCaseSensitve)
            text = text.ToLowerInvariant();

        if (_pattern == text)
        {
            return 0;
        }
        
        
    }
}