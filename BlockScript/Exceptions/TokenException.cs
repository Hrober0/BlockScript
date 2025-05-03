using BlockScript.Lexer;

namespace BlockScript.Exceptions;

public class TokenException(int line, int column, string message) : Exception($"[{line,2}, {column,2}]: {message}")
{
    public int Line { get; } = line;
    public int Column { get; } = column;
}

