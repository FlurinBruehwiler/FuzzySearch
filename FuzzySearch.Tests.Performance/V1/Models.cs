namespace FuzzySearch.Tests.Performance.V1;

internal class ScoreInfo
{ 
    public int Errors { get; init; }
    public int CurrentLocation { get; init; }
    public int ExpectedLocation { get; init; }
    public int Distance { get; init; }
    public bool IgnoreLocation { get; init; }
}

internal record InternalSearchResult(bool IsMatch, double Score);

internal record Index(List<Chunk> Chunks, string Pattern);

internal class Chunk
{
    public int StartIndex { get; init; }
    public string Pattern { get; init; }
    public Dictionary<char, int> Alphabet { get; init; }
}