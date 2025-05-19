namespace BlockScript.Lexer;

public record Position(int Line, int Column)
{
    public override string ToString() => $"{Line,2}, {Column,2}";
}