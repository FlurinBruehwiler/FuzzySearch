using FluentAssertions;

namespace FuzzySearch.Tests;

public class FuseTests
{
    [Fact]
    public void BasicTest()
    {
        var sut = new FuzzySearcher<string>(new[] { "Firefox", "Chrome", "Spotify" });

        var result = sut.Search("fi");

        result.Should().Equal(new List<SearchResult<string>>
        {
            new()
            {
                Score = 0.001,
                Value = "Firefox"
            },
            new()
            {
                Score = 1,
                Value = "Chrome"
            },
            new()
            {
                Score = 0.53,
                Value = "Spotify"
            }
        });
    }
}