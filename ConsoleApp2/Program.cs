var chunks = new List<Chunk>();

var res = Compare("firefox", "haha");

var options = new FuseOptions();


Console.WriteLine(res);

double Compare(string text, string pattern)
{
    CreateChunks(pattern);

    text = text.ToLowerInvariant();

    if (pattern == text)
    {
        return 0;
    }

    double totalScore = 0;
    
    foreach (var chunk in chunks)
    {
        var score = Search(text, chunk.Pattern, chunk.Alphabet);

        totalScore += score;
    }

    return totalScore;
}

double Search(string text, string pattern, IReadOnlyDictionary<char, int> patternAlphabet)
{
    // var location = 0;
    // var distance = 100;
    // var threshold = 0.6;
    // var findAllMatches = false;
    // var minMatchCharLength = 1;
    // var includeMatches = false;
    // var ignoreLocation = false;
    
    if (pattern.Length > 32)
    {
        throw new Exception();
    }

    var patternLen = pattern.Length;
    // Set starting location at beginning text and initialize the alphabet.
    var textLen = text.Length;
    // Handle the case when location > text.length
    var expectedLocation = Math.Max(0, Math.Min(location, textLen));
    // Highest score beyond which we give up.
    var currentThreshold = threshold;
    // Is there a nearby exact match? (speedup)
    var bestLocation = expectedLocation;

    // Performance: only computer matches when the minMatchCharLength > 1
    // OR if `includeMatches` is true.
    var computeMatches = minMatchCharLength > 1 || includeMatches;
    // A mask of the matches, used for building the indices
    var matchMask = computeMatches ? new int[textLen] : Array.Empty<int>();

    int index;

    // Get all exact matches, here for speed up
    while ((index = text.IndexOf(pattern, bestLocation, StringComparison.Ordinal)) > -1)
    {
        var score = ComputeScore(pattern, new ScoreInfo {
            CurrentLocation = index,
            ExpectedLocation = expectedLocation,
            Distance = distance,
            IgnoreLocation = ignoreLocation,
            Errors = 0
        });

        currentThreshold = Math.Min(score, currentThreshold);
        bestLocation = index + patternLen;

        if (!computeMatches) continue;
        
        var i = 0;
        while (i < patternLen)
        {
            matchMask[index + i] = 1;
            i += 1;
        }
    }

    var lastBitArr = Array.Empty<int>();
    double finalScore = 1;
    var binMax = patternLen + textLen;

    var mask = 1 << (patternLen - 1);
    
    for (var i = 0; i < patternLen; i += 1)
    {
        // Scan for the best match; each iteration allows for one more error.
        // Run a binary search to determine how far from the match location we can stray
        // at this error level.
        var binMin = 0;
        var binMid = binMax;

        while (binMin < binMid)
        {
            var score = ComputeScore(pattern, new ScoreInfo
            {
                Errors = i,
                CurrentLocation = expectedLocation + binMid,
                ExpectedLocation = expectedLocation,
                Distance = distance,
                IgnoreLocation = ignoreLocation
            });

            if (score <= currentThreshold)
            {
                binMin = binMid;
            }
            else
            {
                binMax = binMid;
            }

            binMid = (int)Math.Floor((binMax - binMin) / 2.0 + binMin);
        }

        // Use the result from this iteration as the maximum for the next.
        binMax = binMid;

        var start = Math.Max(1, expectedLocation - binMid + 1);
        var finish = findAllMatches
            ? textLen
            : Math.Min(expectedLocation + binMid, textLen) + patternLen;

        // Initialize the bit array
        var bitArr = new int[finish + 2];

        bitArr[finish + 1] = (1 << i) - 1;
        
        
        for (var j = finish; j >= start; j--)
        {
            var currentLocation = j - 1;

            int charMatch = 0;
            
            if (currentLocation < text.Length)
            {
                if (patternAlphabet.TryGetValue(text[currentLocation], out var x))
                {
                    charMatch = x;
                }
            }

            if (computeMatches)
            {
                // Speed up: quick bool to int conversion (i.e, `charMatch ? 1 : 0`)
                matchMask[currentLocation] = charMatch != 0 ? 1 : 0;
            }

            // First pass: exact match
            bitArr[j] = ((bitArr[j + 1] << 1) | 1) & charMatch;

            // Subsequent passes: fuzzy match
            if (i != 0)
            {
                bitArr[j] |= ((lastBitArr[j + 1] | lastBitArr[j]) << 1) | 1 | lastBitArr[j + 1];
            }

            if ((bitArr[j] & mask) == 0) continue;
            finalScore = ComputeScore(pattern, new ScoreInfo
            {
                Errors = i,
                CurrentLocation = currentLocation,
                ExpectedLocation = expectedLocation,
                Distance = distance,
                IgnoreLocation = ignoreLocation
            });

            // This match will almost certainly be better than any existing match.
            // But check anyway.
            if (!(finalScore <= currentThreshold)) continue;
            // Indeed it is
            currentThreshold = finalScore;
            bestLocation = currentLocation;

            // Already passed `loc`, downhill from here on in.
            if (bestLocation <= expectedLocation)
            {
                break;
            }

            // When passing `bestLocation`, don't exceed our current distance from `expectedLocation`.
            start = Math.Max(1, 2 * expectedLocation - bestLocation);
        }

        var score2 = ComputeScore(pattern, new ScoreInfo
        {
            Errors = i + 1,
            CurrentLocation = expectedLocation,
            ExpectedLocation = expectedLocation,
            Distance = distance,
            IgnoreLocation = ignoreLocation
        });

        if (score2 > currentThreshold)
        {
            break;
        }

        lastBitArr = bitArr;
    }

    return Math.Max(0.001, finalScore);
}


Dictionary<char, int> CreatePatternAlphabet(string pattern)
{
    var mask = new Dictionary<char, int>();

    for (int i = 0, len = pattern.Length; i < len; i += 1)
    {
        var c = pattern[i];
        mask[c] = (mask.TryGetValue(c, out var value) ? value : 0) | (1 << (len - i - 1));
    }

    return mask;
}

void CreateChunks(string pattern)
{
    const int maxBits = 32;
    
    if (pattern.Length > maxBits)
    {
        var i = 0;
        var remainder = pattern.Length % maxBits;
        var end = pattern.Length - remainder;

        while (i < end)
        {
            AddChunkt(pattern.Substring(i, maxBits), i);
            i += maxBits;
        }

        if (remainder <= 0) return;
        var startIndex = pattern.Length - maxBits;
        AddChunkt(pattern[startIndex..], startIndex);
    }
    else
    {
        AddChunkt(pattern, 0);
    }
}


void AddChunkt(string pattern, int startIndex)
{
    chunks.Add(new Chunk
    {
        Pattern = pattern,
        Alphabet = CreatePatternAlphabet(pattern),
        StartIndex = startIndex
    });
}

double ComputeScore(string pattern, ScoreInfo info)
{
    double errors = info.Errors;
    var currentLocation = info.CurrentLocation;
    var expectedLocation = info.ExpectedLocation;
    var distance = info.Distance;
    var ignoreLocation = info.IgnoreLocation;

    var accuracy = errors / pattern.Length;

    if (ignoreLocation)
    {
        return accuracy;
    }

    var proximity = Math.Abs(expectedLocation - currentLocation);

    if (distance == 0)
    {
        return proximity != 0 ? 1.0 : accuracy;
    }

    return accuracy + (double)proximity / distance;
}



public class ScoreInfo
{
     public required int Errors { get; init; }
     public required int CurrentLocation { get; init; }
     public required int ExpectedLocation { get; init; }
     public required int Distance { get; init; }
     public required bool IgnoreLocation { get; init; }
}


public class Chunk
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
    public float Threshold { get; set; } = 0.6f;
    public bool UseExtendedSearch { get; set; } = false;
}