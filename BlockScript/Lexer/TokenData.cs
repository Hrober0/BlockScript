namespace BlockScript.Lexer;

public readonly struct TokenData
{
    public TokenType Type {  get; init; }
    public string Value { get; init; }
    public int Line { get; init; }
    public int Column { get; init; }

    public override string ToString() => $"[{Line,2},{Column,2}]: {Type} \'{Value}\'";
}