using BlockScript.Interpreter;
using BlockScript.Interpreter.BuildInMethods;
using BlockScript.Lexer.FactorValues;
using BlockScript.Parser.Statements;
using FluentAssertions;
using Xunit;

namespace BlockScript.Tests.InterpreterTests.BuildInMethods;

public class PrintMethodTests
{
    [Fact]
    public void ExecutePrint_WhenValueIsInt()
    {
        // Arrange
        var output = new StringWriter();
        Console.SetOut(output);
        var context = new Context(null);
        var method = new PrintMethod();
        context.AddData(method.Arguments.First(), new IntFactor(3));

        // Act
        var result = method.Execute(Execute, context);

        // Assert
        result.Should().BeEquivalentTo(new IntFactor(3));
        var lines = output.ToString().Split(Environment.NewLine);
        lines.Should().HaveCountGreaterThan(0);
        lines[0].Should().Be(new IntFactor(3).ToString());
    }

    [Fact]
    public void ExecutePrint_WhenValueIsBool()
    {
        // Arrange
        var output = new StringWriter();
        Console.SetOut(output);
        var context = new Context(null);
        var method = new PrintMethod();
        context.AddData(method.Arguments.First(), new BoolFactor(true));

        // Act
        var result = method.Execute(Execute, context);

        // Assert
        result.Should().BeEquivalentTo(new BoolFactor(true));
        var lines = output.ToString().Split(Environment.NewLine);
        lines.Should().HaveCountGreaterThan(0);
        lines[0].Should().Be(new BoolFactor(true).ToString());
    }

    [Fact]
    public void ExecutePrint_WhenValueIsString()
    {
        // Arrange
        var output = new StringWriter();
        Console.SetOut(output);
        var context = new Context(null);
        var method = new PrintMethod();
        context.AddData(method.Arguments.First(), new StringFactor("Hello"));

        // Act
        var result = method.Execute(Execute, context);

        // Assert
        result.Should().BeEquivalentTo(new StringFactor("Hello"));
        var lines = output.ToString().Split(Environment.NewLine);
        lines.Should().HaveCountGreaterThan(0);
        lines[0].Should().Be(new StringFactor("Hello").ToString());
    }

    [Fact]
    public void ExecutePrint_WhenValueIsNull()
    {
        // Arrange
        var output = new StringWriter();
        Console.SetOut(output);
        var context = new Context(null);
        var method = new PrintMethod();
        context.AddData(method.Arguments.First(), new NullFactor());

        // Act
        var result = method.Execute(Execute, context);

        // Assert
        result.Should().BeOfType<NullFactor>();
        var lines = output.ToString().Split(Environment.NewLine);
        lines.Should().HaveCountGreaterThan(0);
        lines[0].Should().Be(new NullFactor().ToString());
    }
    
    [Fact]
    public void ExecutePrintMultipleTimes()
    {
        // Arrange
        var output = new StringWriter();
        Console.SetOut(output);
        var context = new Context(null);
        var method = new PrintMethod();

        // Act
        context.AddData(method.Arguments.First(), new IntFactor(1));
        method.Execute(Execute, context);
        context.AddData(method.Arguments.First(), new IntFactor(2));
        method.Execute(Execute, context);

        // Assert
        var lines = output.ToString().Split(Environment.NewLine);
        lines.Should().HaveCountGreaterThan(1);
        lines[0].Should().Be(new IntFactor(1).ToString());
        lines[1].Should().Be(new IntFactor(2).ToString());
    }

    private static IFactorValue Execute(IStatement statement) => throw new NotImplementedException();
}