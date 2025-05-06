namespace BlockScript.Parser.Factors;

public class ConstFactor : IFactor
{
    public object Value { get; }
    
    public ConstFactor(object value)
    {
        Value = value;
    }
    
    public override string ToString() => Value.ToString();
}