namespace BlockScript.Parser.Factors;

public class ConstFactor(object value) : IFactor
{
    public override string ToString() => value.ToString();
}