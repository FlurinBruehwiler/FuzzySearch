namespace FlurinBruehwiler.FuzzySearch;

internal static class Searcher
{
    public static InternalSearchResult Compare(string text, Index index, FuseOptions options)
    {
        if(!options.IsCaseSensitve)
            text = text.ToLowerInvariant();

        if (index.Pattern == text)
        {
            return new InternalSearchResult(true, 0);
        }

        double totalScore = 0;
        var hasMatches = false;
        
        foreach (var chunk in index.Chunks)
        {
            var sr = Search(text, chunk.Pattern, chunk.Alphabet, options);

            if (sr.IsMatch)
                hasMatches = true;
            
            totalScore += sr.Score;
        }

        return new InternalSearchResult(hasMatches, hasMatches ? totalScore / index.Chunks.Count : 1);
    }

    private static InternalSearchResult Search(string text, string pattern, IReadOnlyDictionary<char, int> patternAlphabet, FuseOptions options)
    {
        if (pattern.Length > 32)
        {
            throw new Exception();
        }

        var patternLen = pattern.Length;
        // Set starting location at beginning text and initialize the alphabet.
        var textLen = text.Length;
        // Handle the case when location > text.length
        var expectedLocation = Math.Max(0, Math.Min(options.Location, textLen));
        // Highest score beyond which we give up.
        var currentThreshold = options.Threshold;
        // Is there a nearby exact match? (speedup)
        var bestLocation = expectedLocation;

        // Performance: only computer matches when the minMatchCharLength > 1
        // OR if `includeMatches` is true.
        var computeMatches = options.MinMatchCharLength > 1 || options.IncludeMatches;
        // A mask of the matches, used for building the indices
        var matchMask = computeMatches ? stackalloc int[textLen] : Span<int>.Empty;

        int index;

        // Get all exact matches, here for speed up
        while ((index = text.IndexOf(pattern, bestLocation, StringComparison.Ordinal)) > -1)
        {
            var score = ComputeScore(pattern, new ScoreInfo
            {
                CurrentLocation = index,
                ExpectedLocation = expectedLocation,
                Distance = options.Distance,
                IgnoreLocation = options.IgnoreLocation,
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

        Span<int> lastBitArr = stackalloc int[0];
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
                    Distance = options.Distance,
                    IgnoreLocation = options.IgnoreLocation
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
            var finish = options.FindAllMatches
                ? textLen
                : Math.Min(expectedLocation + binMid, textLen) + patternLen;

            // Initialize the bit array
            Span<int> bitArr = stackalloc int[finish + 2];

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
                    Distance = options.Distance,
                    IgnoreLocation = options.IgnoreLocation
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
                Distance = options.Distance,
                IgnoreLocation = options.IgnoreLocation
            });

            if (score2 > currentThreshold)
            {
                break;
            }

            lastBitArr = bitArr;
        }
        
        return new InternalSearchResult(bestLocation >= 0, Math.Max(0.001, finalScore));
    }


    private static Dictionary<char, int> CreatePatternAlphabet(string pattern)
    {
        var mask = new Dictionary<char, int>(pattern.Length);

        for (var i = 0; i < pattern.Length; i += 1)
        {
            var c = pattern[i];
            mask[c] = (mask.TryGetValue(c, out var value) ? value : 0) | (1 << (pattern.Length - i - 1));
        }

        return mask;
    }

    private static List<Chunk> CreateChunks(string pattern)
    {
        const int maxBits = 32;
        var chunks = new List<Chunk>();

        if (pattern.Length > maxBits)
        {
            var i = 0;
            var remainder = pattern.Length % maxBits;
            var end = pattern.Length - remainder;

            while (i < end)
            {
                chunks.Add(AddChunkt(pattern.Substring(i, maxBits), i));
                i += maxBits;
            }

            if (remainder <= 0) 
                return chunks;
            
            var startIndex = pattern.Length - maxBits;
            chunks.Add(AddChunkt(pattern[startIndex..], startIndex));
        }
        else
        {
            chunks.Add(AddChunkt(pattern, 0));
        }

        return chunks;
    }

    public static Index CreateIndex(string pattern)
    {
        return new Index(CreateChunks(pattern), pattern);
    }
    
    private static Chunk AddChunkt(string pattern, int startIndex)
    {
        return new Chunk
        {
            Pattern = pattern,
            Alphabet = CreatePatternAlphabet(pattern),
            StartIndex = startIndex
        };
    }

    private static double ComputeScore(string pattern, ScoreInfo info)
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
}