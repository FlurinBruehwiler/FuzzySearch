namespace FuseSearch;

public class BitTapSearch
{
    private readonly string _pattern;
    private readonly FuseOptions _fuseOptions;
    private readonly List<Chunk> _chunks;
    public static int MaxBits = 32;

    public BitTapSearch(string pattern, FuseOptions fuseOptions)
    {
        _pattern = fuseOptions.IsCaseSensitve ? pattern : pattern.ToLowerInvariant();
        _fuseOptions = fuseOptions;
        _chunks = new List<Chunk>();

        var len = _pattern.Length;

        if (len > MaxBits)
        {
            var i = 0;
            var remainder = len % MaxBits;
            var end = len - remainder;

            while (i < end)
            {
                AddChunk(_pattern.Substring(i, MaxBits), i);
                i += MaxBits;
            }

            if (remainder > 0)
            {
                var startIndex = len - MaxBits;
                AddChunk(_pattern.Substring(startIndex), startIndex);
            }
        }
        else
        {
            AddChunk(_pattern, 0);
        }
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

    private Dictionary<char, int> CreatePatternAlphabet(string pattern)
    {
        Dictionary<char, int> mask = new();

        var len = pattern.Length;
        for (int i = 0; i < len; i++)
        {
            var c = pattern[i];
            mask[c] = (mask.ContainsKey(c) ? mask[c] : 0) | (1 << (len - i - 1));
        }

        return mask;
    }

    public Result SearchIn(string text)
    {
        if (!_fuseOptions.IsCaseSensitve)
            text = text.ToLowerInvariant();

        if (_pattern == text)
        {
            var result = new Result
            {
                IsMatch = true,
                Score = 0,
            };
            
            if (_fuseOptions.IncludeMatches)
            {
                //Add indices to Result
            }
            
            return result;
        }

        List<(int, int)> allIndices = new();
        var totalScore = 0f;
        var hasMatches = false;

        foreach (var chunk in _chunks)
        {
            var result = Search.PerformSearch(text, chunk.Pattern, chunk.Alphabet, _fuseOptions);

            if (result.IsMatch)
            {
                hasMatches = true;
            }

            totalScore += result.Score;

            if (result.IsMatch && result.Indices is not null && result.Indices.Count != 0)
            {
                allIndices.AddRange(result.Indices);
            }
        }

        var result2 = new Result
        {
            IsMatch = hasMatches,
            Score = hasMatches ? totalScore / _chunks.Count : 1
        };

        if (hasMatches && _fuseOptions.IncludeMatches)
        {
            result2.Indices = allIndices;
        }

        return result2;
    }
}

public class Result
{
    public bool IsMatch { get; set; }
    public float Score { get; set; }
    public List<(int, int)>? Indices { get; set; }
}
