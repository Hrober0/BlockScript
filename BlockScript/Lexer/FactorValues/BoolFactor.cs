
namespace BlockScript.Lexer.FactorValues
{
    public record BoolFactor(bool Value) : IFactorValue
    {
        public static implicit operator BoolFactor(bool value) => new(value);
        public static explicit operator bool(BoolFactor factor) => factor.Value;
        
        public override string ToString() => $"#{Value}";
    }
}