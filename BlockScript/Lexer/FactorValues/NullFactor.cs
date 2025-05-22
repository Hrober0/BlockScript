namespace BlockScript.Lexer.FactorValues
{
    public record NullFactor : IFactorValue
    {
        public override string ToString() => $"#Null";
    }
}