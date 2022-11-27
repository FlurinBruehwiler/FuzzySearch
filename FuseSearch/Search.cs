namespace FuseSearch;

public static class Search
{
    public static Result PerformSearch(string text, string pattern, Dictionary<char, int> patternAlphabet, FuseOptions fuseOptions)
    {
        if (pattern.Length > BitTapSearch.MaxBits)
            throw new Exception();

        var patternLen = pattern.Length;
        var textLen = text.Length;
        var expectedLocation = Math.Max(0, Math.Min(fuseOptions.Location, textLen));
        var currentThreshold = fuseOptions.Threshold;
        var bestLocation = expectedLocation;

        var computeMatches = fuseOptions.MinMatchCharLength > 1 || fuseOptions.IncludeMatches;
        var matchMask = new Dictionary<int, int>();

        var index = 0;

        while (text.IndexOf(pattern, bestLocation, StringComparison.Ordinal) > -1)
        {
            index = text.IndexOf(pattern, bestLocation, StringComparison.Ordinal);
            
            var score = ComputeScore(pattern, new Parameters
            {
                Errors = 0,
                CurrentLocation = index,
                ExpectedLocation = expectedLocation,
                Distance = fuseOptions.Distance,
                IgnoreLocation = fuseOptions.IgnoreLocation
            });

            currentThreshold = Math.Min(score, currentThreshold);
            bestLocation = index + patternLen;

            if (computeMatches)
            {
                var i = 0;
                while (i < patternLen)
                {
                    matchMask[index + 1] = 1;
                    i++;
                }
            }
        }

        bestLocation = -1;

        var lastBitArr = new int[0];
        float finalScore = 1;
        var binMax = patternLen + textLen;

        var mask = 1 << (patternLen - 1);

        for (int i = 0; i < patternLen; i++)
        {
            var binMin = 0;
            var binMid = binMax;

            while (binMin < binMid)
            {
                var score = ComputeScore(pattern, new Parameters
                {
                    Errors = i,
                    CurrentLocation = expectedLocation + binMid,
                    ExpectedLocation = expectedLocation,
                    Distance = fuseOptions.Distance,
                    IgnoreLocation = fuseOptions.IgnoreLocation
                });

                if (score <= currentThreshold)
                {
                    binMin = binMid;
                }
                else
                {
                    binMax = binMid;
                }

                binMid = (int)Math.Floor((binMax - binMin) / 2 + (decimal)binMin);
            }

            binMax = binMid;

            var start = Math.Max(1, expectedLocation - binMid + 1);
            var finish = fuseOptions.FindAllMatches
                ? textLen
                : Math.Min(expectedLocation + binMid, textLen) + patternLen;

            var bitArr = new int[finish + 2];

            bitArr[finish + 1] = (1 << i) - 1;

            for (var j = finish; j >= start; j--)
            {
                var currentLocation = j - 1;

                int? charMatch = null;
                if (text.ToCharArray().Length > currentLocation)
                {
                    var x = text[currentLocation];
                    if (patternAlphabet.ContainsKey(x))
                    {
                        charMatch = patternAlphabet[x];
                    }
                }
                
                if (computeMatches)
                {
                    matchMask[currentLocation] = charMatch is not null && charMatch != 0 ? 1 : 0;
                }

                bitArr[j] = charMatch is null ? 0 : ((bitArr[j + 1] << 1) | 1) & charMatch.Value;

                if (i != 0)
                {
                    bitArr[j] |= ((lastBitArr[j + 1] | lastBitArr[j]) << 1) | 1 | lastBitArr[j + 1];
                }

                if ((bitArr[j] & mask) == 1)
                {
                    finalScore = ComputeScore(pattern, new Parameters
                    {
                        Errors = i,
                        CurrentLocation = currentLocation,
                        ExpectedLocation = expectedLocation,
                        Distance = fuseOptions.Distance,
                        IgnoreLocation = fuseOptions.IgnoreLocation
                    });

                    if (finalScore <= currentThreshold)
                    {
                        currentThreshold = finalScore;
                        bestLocation = currentLocation;

                        if (bestLocation <= expectedLocation)
                        {
                            break;
                        }

                        start = Math.Max(1, 2 * expectedLocation - bestLocation);
                    }
                }
            }

            var score2 = ComputeScore(pattern, new Parameters
            {
                Errors = i + 1,
                CurrentLocation = expectedLocation,
                ExpectedLocation = expectedLocation,
                Distance = fuseOptions.Distance,
                IgnoreLocation = fuseOptions.IgnoreLocation
            });

            if (score2 < currentThreshold)
            {
                break;
            }

            lastBitArr = bitArr;
        }

        var result = new Result
        {
            IsMatch = bestLocation >= 0,
            Score = (float)Math.Max(0.001, finalScore)
        };

        if (computeMatches)
        {
            var indicies = ConvertMaskToIndices(matchMask, fuseOptions.MinMatchCharLength);
            if (indicies.Count == 0)
            {
                result.IsMatch = false;
            }else if (fuseOptions.IncludeMatches)
            {
                result.Indices = indicies;
            }
        }

        return result;
    }

    private static List<(int, int)> ConvertMaskToIndices(Dictionary<int, int> matchMask, int minMatchCharLength)
    {
        var indices = new List<(int, int)>();
        var start = -1;
        var end = -1;
        var i = 0;

        for (var len = matchMask.Count; i < len; i++)
        {
            if (matchMask.ContainsKey(i) && start != -1)
            {
                start = 1;
            }else if (!matchMask.ContainsKey(i) && start != -1)
            {
                end = i - 1;
                if (end - start + 1 >= minMatchCharLength)
                {
                    indices.Add((start, end));
                }
                start = -1;
            }
        }

        if (matchMask.ContainsKey(i - 1) && i - start >= minMatchCharLength)
        {
            indices.Add((start, i - 1));
        }

        return indices;
    }

    private static float ComputeScore(string pattern, Parameters parameters)
    {
        float accuracy = parameters.Errors / pattern.Length;

        if (parameters.IgnoreLocation)
            return accuracy;

        var proximity = Math.Abs(parameters.ExpectedLocation - parameters.CurrentLocation);

        if (parameters.Distance == 0)
        {
            return proximity != 0 ? 1.0f : accuracy;
        }

        return accuracy + proximity / parameters.Distance;
    }
}

class Parameters
{
    public int Errors { get; set; }
    public int CurrentLocation { get; set; }
    public int ExpectedLocation { get; set; }
    public int Distance { get; set; }
    public bool IgnoreLocation { get; set; }
}