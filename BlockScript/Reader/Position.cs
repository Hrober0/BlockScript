namespace BlockScript.Reader;

public record Position(int Line, int Column)
{
    public static Position Default = new(0, 0);
    public override string ToString() => $"{Line,2}, {Column,2}";
}