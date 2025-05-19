using BlockScript.Lexer;

namespace BlockScript.Exceptions;

public class TokenException(Position position, string message) : Exception($"[{position}]: {message}")
{
    
}

