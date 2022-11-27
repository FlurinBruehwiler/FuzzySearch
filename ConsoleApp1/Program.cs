// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using FuseSearch;

var list = new List<string>
{
    "firefox",
    "chrome",
    "temp",
    "fire"
};

var fuse = new Fuse(list, new FuseOptions
{
    IncludeMatches = true,
    IncludeScore = true,
    ShouldSort = true,
    FindAllMatches = true
});

var res = fuse.Search("fi");

Console.WriteLine(JsonSerializer.Serialize(res));
