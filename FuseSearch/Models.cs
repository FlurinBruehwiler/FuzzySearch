namespace FuseSearch;

internal struct ScoreInfo
{
    public required int Errors { get; init; }
    public required int CurrentLocation { get; init; }
    public required int ExpectedLocation { get; init; }
    public required int Distance { get; init; }
    public required bool IgnoreLocation { get; init; }
}

internal record struct InternalSearchResult(bool IsMatch, double Score);
internal record struct Index(List<Chunk> Chunks, string Pattern);

internal struct Chunk
{
    public required int StartIndex { get; init; }
    public required string Pattern { get; init; }
    public required Dictionary<char, int> Alphabet { get; init; }
}

