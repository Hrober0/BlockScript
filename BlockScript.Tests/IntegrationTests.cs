using BlockScript.Interpreter;
using BlockScript.Lexer.FactorValues;
using BlockScript.Parser;
using FluentAssertions;
using Xunit;

namespace BlockScript.Tests;

public class IntegrationTests
{
    [Fact]
    public void Integration_ShouldCalculateFinocchi()
    {
        // Arrange
        var input = "fib = (i) => if i <= 1 1 else fib(i-1) + fib(i-2); fib(6);";
        
        // Act
        using TextReader reader = new StringReader(input);
        var lexer = new Lexer.Lexer(reader);
        var parser = new LanguageParser(lexer.GetToken);
        var program = parser.ParserProgram();
        var interpreter = new LanguageInterpreter([]);
        var returnValue = interpreter.ExecuteProgram(program);
        
        // Assert
        returnValue.Should().BeOfType<IntFactor>().Which.Value.Should().Be(13);
    }
}