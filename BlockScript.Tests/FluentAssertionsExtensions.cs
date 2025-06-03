using BlockScript.Lexer.FactorValues;
using BlockScript.Parser.Factors;
using BlockScript.Parser.Statements;
using FluentAssertions;

namespace BlockScript.Tests
{
    public static class FluentAssertionsExtensions
    {
        public static void ShouldBeConstFactor(this IStatement value, int expected)
        {
            value.Should().BeOfType<ConstFactor>()
                 .Which.Value.Should().BeOfType<IntFactor>()
                 .Which.Value.Should().Be(expected);
        }
        
        public static void ShouldBeConstFactor(this IStatement value, bool expected)
        {
            value.Should().BeOfType<ConstFactor>()
                 .Which.Value.Should().BeOfType<BoolFactor>()
                 .Which.Value.Should().Be(expected);
        }
        
        public static void ShouldBeConstFactor(this IStatement value, string expected)
        {
            value.Should().BeOfType<ConstFactor>()
                 .Which.Value.Should().BeOfType<StringFactor>()
                 .Which.Value.Should().Be(expected);
        }
    }
}