using BlockScript.Lexer.FactorValues;
using BlockScript.Reader;

namespace BlockScript.Lexer;

public readonly struct TokenData
{
    public TokenType Type {  get; init; }
    public IFactorValue Value { get; init; }
    public Position Position { get; init; }

    public override string ToString() => $"[{Position}]: {Type} \'{Value}\'";
}