namespace FuseSearch.Tests;

public class FuseTests
{
    [Fact]
    public void BasicTest()
    {
        var sut = new Fuse(new[] { "Firefox", "Chrome", "Discord" });

        var result = sut.Search("fi");
        
        //ToDo
    }
}