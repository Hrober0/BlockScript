using BlockScript.Lexer;

namespace BlockScript.Exceptions;

public class UnexpectedTokenException : TokenException
{
    public UnexpectedTokenException(TokenData providedToken, TokenType expectedToken, string? message = null) : base(
        providedToken.Line,
        providedToken.Column,
        $"Expected '{expectedToken}' but received '{providedToken.Value}'. {message}" )
    {
    }
}