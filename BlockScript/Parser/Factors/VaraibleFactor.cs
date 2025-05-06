namespace BlockScript.Parser.Factors;

public class VariableFactor : IFactor
{
    public string Identifier { get; }

    public VariableFactor(string identifier)
    {
        Identifier = identifier;
    }

    public override string ToString() => $"${Identifier}";
}