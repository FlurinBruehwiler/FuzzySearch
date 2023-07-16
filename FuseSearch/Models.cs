namespace FuseSearch;

internal record struct ScoreInfo(int Errors, int CurrentLocation, int ExpectedLocation, int Distance,
    bool IgnoreLocation);

internal record struct InternalSearchResult(bool IsMatch, double Score);

internal record struct Index(List<Chunk> Chunks, string Pattern);

internal record struct Chunk(int StartIndex, string Pattern, Dictionary<char, int> Alphabet);