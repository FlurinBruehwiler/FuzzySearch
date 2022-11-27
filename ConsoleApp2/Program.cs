// See https://aka.ms/new-console-template for more information


using System.Text.Json;

var x = CreatePatternAlphabet("firefox");

Console.WriteLine(JsonSerializer.Serialize(x));


Dictionary<char, int> CreatePatternAlphabet(string pattern)
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