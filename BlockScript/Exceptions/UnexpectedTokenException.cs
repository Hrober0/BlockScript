using BlockScript.Lexer;

namespace BlockScript.Exceptions;

public class UnexpectedTokenException : TokenException
{
    public UnexpectedTokenException(TokenData providedToken, TokenType expectedToken, string? message = null) : base(
        providedToken.Position,
        $"Expected {expectedToken.TextValue()} but received '{providedToken.Value}'. {message}" )
    {
    }
}