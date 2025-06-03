using BlockScript.Exceptions;
using BlockScript.Interpreter;
using BlockScript.Interpreter.BuildInMethods;
using BlockScript.Lexer.FactorValues;
using BlockScript.Parser.Statements;
using FluentAssertions;
using Xunit;

namespace BlockScript.Tests.InterpreterTests.BuildInMethods;

public class DebugMethodTests
{
    [Fact]
    public void Execute_ShouldAddValueToStack_AndReturnSameValue_WhenInputIsInt()
    {
        // Arrange
        var stack = new List<IFactorValue>();
        var context = new Context(null);
        var value = new IntFactor(42);
        var method = new DebugMethod(stack);
        context.AddData(method.Arguments.First(), value);

        // Act
        var result = method.Execute(Execute, context);

        // Assert
        result.Should().Be(value);
        stack.Should().ContainSingle().Which.Should().Be(value);
    }

    [Fact]
    public void Execute_ShouldAddValueToStack_AndReturnSameValue_WhenInputIsString()
    {
        // Arrange
        var stack = new List<IFactorValue>();
        var context = new Context(null);
        var value = new StringFactor("debug info");
        var method = new DebugMethod(stack);
        context.AddData(method.Arguments.First(), value);

        // Act
        var result = method.Execute(Execute, context);

        // Assert
        result.Should().Be(value);
        stack.Should().ContainSingle().Which.Should().Be(value);
    }

    [Fact]
    public void Execute_ShouldThrow_WhenInputVariableMissing()
    {
        // Arrange
        var stack = new List<IFactorValue>();
        var context = new Context(null);
        var method = new DebugMethod(stack);

        // Act
        var act = () => method.Execute(Execute, context);

        // Assert
        act.Should()
           .Throw<RuntimeException>()
           .WithMessage($"*Variable of name {method.Arguments.First()} was not defined!*");
        stack.Should().BeEmpty();
    }

    [Fact]
    public void Execute_ShouldWorkMultipleTimes()
    {
        // Arrange
        var stack = new List<IFactorValue>();
        var method = new DebugMethod(stack);

        var context1 = new Context(null);
        var val1 = new IntFactor(1);
        context1.AddData(method.Arguments.First(), val1);

        var context2 = new Context(null);
        var val2 = new IntFactor(2);
        context2.AddData(method.Arguments.First(), val2);

        // Act
        var result1 = method.Execute(Execute, context1);
        var result2 = method.Execute(Execute, context2);

        // Assert
        result1.Should().Be(val1);
        result2.Should().Be(val2);
        stack.Should().HaveCount(2);
        stack[0].Should().Be(val1);
        stack[1].Should().Be(val2);
    }

    private static IFactorValue Execute(IStatement statement) =>
        throw new System.NotImplementedException();
}