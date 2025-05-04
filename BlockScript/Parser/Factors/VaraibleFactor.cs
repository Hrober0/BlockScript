namespace BlockScript.Parser.Factors;

public class VariableFactor(string identifier) : IFactor
{
    public override string ToString() => $"${identifier}";
}