using System.Diagnostics;
using FlurinBruehwiler.Helpers;
using Fuse;

var records = new List<string>
{   
    "Firefox",
    "Chrome",
    "Discord"
};

var pattern = "fi";

// var results = new List<Result>();

var index = Methods.CreateIndex(pattern);
var options = new FuseOptions();

// foreach (var record in records)
// {
//     var res = Methods.Compare(record, index, options);
//
//     if (res.IsMatch)
//     {
//         results.Add(new Result(record, res.Score));
//     }
// }
//
// results.Dump();

var start = Stopwatch.GetTimestamp();

foreach (var _ in 100_000)
{
    foreach (var record in records)
    {
        Methods.Compare(record, index, options);
    }
}

Stopwatch.GetElapsedTime(start).Milliseconds.Print();