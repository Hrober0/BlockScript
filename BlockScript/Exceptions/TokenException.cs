using BlockScript.Lexer;
using BlockScript.Reader;

namespace BlockScript.Exceptions;

public class TokenException(Position position, string message) : Exception($"[{position}]: {message}")
{
    
}

