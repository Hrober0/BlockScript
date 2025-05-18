using BlockScript.Lexer;

namespace BlockScript.Parser.Factors;

public record VariableFactor(string Identifier, Position Position) : IFactor
{
    public override string ToString() => $"${Identifier}";
}