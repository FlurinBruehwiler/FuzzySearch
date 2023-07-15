namespace Fuse;

public struct ScoreInfo
{
    public required int Errors { get; init; }
    public required int CurrentLocation { get; init; }
    public required int ExpectedLocation { get; init; }
    public required int Distance { get; init; }
    public required bool IgnoreLocation { get; init; }
}

public record struct SearchResult(bool IsMatch, double Score);
public record struct Index(List<Chunk> Chunks, string Pattern);

public record struct Result(string Text, double Score);

public struct Chunk
{
    public required int StartIndex { get; init; }
    public required string Pattern { get; init; }
    public required Dictionary<char, int> Alphabet { get; init; }
}

public class FuseOptions
{
    public bool IsCaseSensitve { get; set; } = false;
    public int Distance { get; set; } = 100;

    public bool FindAllMatches { get; set; } = false;
    //getFn
    public bool IgnoreLocation { get; set; } = false;
    public bool IgnoreFieldNorm { get; set; } = false;
    public int FieldNormWeight { get; set; } = 1;
    public bool IncludeMatches { get; set; } = false;

    public bool IncludeScore { get; set; } = false;
    //keys
    public int Location { get; set; } = 0;
    public int MinMatchCharLength { get; set; } = 1;

    public bool ShouldSort { get; set; } = true;
    //sortFn
    public double Threshold { get; set; } = 0.6f;
    public bool UseExtendedSearch { get; set; } = false;
}