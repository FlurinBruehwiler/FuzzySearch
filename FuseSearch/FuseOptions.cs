namespace FuseSearch;

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
    public float Threshold { get; set; } = 0.6f;
    public bool UseExtendedSearch { get; set; } = false;
}