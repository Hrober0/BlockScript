using BlockScript.Lexer;

namespace BlockScript.Exceptions;

public class UnexpectedTokenException : TokenException
{
    public UnexpectedTokenException(TokenData providedToken, TokenType expectedToken, string? message = null) : base(
        providedToken.Line,
        providedToken.Column,
        $"Expected {expectedToken.TextValue()} but received '{providedToken.Value}'. {message}" )
    {
    }
}