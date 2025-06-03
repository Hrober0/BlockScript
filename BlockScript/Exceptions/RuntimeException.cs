using BlockScript.Lexer;
using BlockScript.Reader;

namespace BlockScript.Exceptions;

public class RuntimeException : TokenException
{
    public RuntimeException(Position position, string message) : base(position, message)
    {
    }
}