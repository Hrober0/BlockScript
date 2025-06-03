using BlockScript.Reader;

namespace BlockScript.Parser.Factors;

public record VariableFactor(string Identifier, Position Position) : IFactor
{
    public VariableFactor(string identifier) : this(identifier, Position.Default) { }
    
    public override string ToString() => $"${Identifier}";
}