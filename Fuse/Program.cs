using System.Buffers;
using ConsoleApp2;
using FlurinBruehwiler.Helpers;


var records = new List<string>
{   
    "Firefox",
    "Chrome",
    "Discord"
};

var pattern = "fi";

var results = new List<Result>();

var index = Methods.CreateIndex(pattern);
var options = new FuseOptions();

foreach (var record in records)
{
    var res = Methods.Compare(record, index, options);

    if (res.IsMatch)
    {
        results.Add(new Result(record, res.Score));
    }
}

results.Dump();