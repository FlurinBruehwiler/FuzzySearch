namespace FuzzySearch.Tests;

public class FuseTests
{
    [Fact]
    public void BasicTest()
    {
        var sut = new FuzzySearcher<string>(new[] { "Firefox", "Chrome", "Discord" });

        var result = sut.Search("fi");
        
        //ToDo
    }
}