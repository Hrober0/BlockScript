namespace BlockScript.Lexer.TokenParsers;

public enum AcceptStatus
{
    Deny,
    Possible,
    Completed,
}

public abstract class TokenParser
{
    public abstract AcceptStatus AcceptChar(char c);
    public abstract bool IsValid();
    public abstract TokenData CreateToken(int line, int column);
    public virtual void Reset() {}

    public virtual string? GetErrorHint() => null;
}
