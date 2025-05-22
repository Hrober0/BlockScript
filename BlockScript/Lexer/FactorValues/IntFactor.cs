namespace BlockScript.Lexer.FactorValues
{
    public record IntFactor(int Value) : IFactorValue
    {
        public static implicit operator IntFactor(int value) => new(value);
        public static explicit operator int(IntFactor factor) => factor.Value;
        
        public override string ToString() => $"#{Value}";
    }
}