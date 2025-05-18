using BlockScript.Lexer;

namespace BlockScript.Exceptions;

public class RuntimeException : TokenException
{
    public RuntimeException(Position position, string message) : base(position, message)
    {
    }
}