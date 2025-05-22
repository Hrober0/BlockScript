namespace BlockScript.Lexer.FactorValues
{
    public record StringFactor(string Value) : IFactorValue
    {
        public static implicit operator StringFactor(string value) => new(value);
        public static explicit operator string(StringFactor factor) => factor.Value;
        
        public override string ToString() => $"#{Value}";
    }
}