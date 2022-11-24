namespace Fuse.cs;

public class Chunk
{
    public string Pattern { get; set; }
    public Dictionary<char, int> Alphabet { get; set; }
    public int StartIndex { get; set; }
}