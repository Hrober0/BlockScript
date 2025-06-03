using BlockScript.Exceptions;
using BlockScript.Interpreter;
using BlockScript.Lexer.FactorValues;
using BlockScript.Reader;
using FluentAssertions;
using Xunit;

namespace BlockScript.Tests.InterpreterTests;

public class ContextTests
{
    [Fact]
    public void TryGetData_ShouldReturnFalse_WhenVariableNotDefined()
    {
        // Arrange
        var context = new Context(null);

        // Act
        var result = context.TryGetData("x", out var value);

        // Assert
        result.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void TryGetData_ShouldReturnTrueAndValue_WhenVariableIsDefined()
    {
        // Arrange
        var context = new Context(null);
        var expected = new IntFactor(42);
        context.AddData("x", expected);

        // Act
        var result = context.TryGetData("x", out var actual);

        // Assert
        result.Should().BeTrue();
        actual.Should().Be(expected);
    }

    [Fact]
    public void TryGetData_ShouldSearchParentContexts_WhenNotInCurrent()
    {
        // Arrange
        var parent = new Context(null);
        var expected = new IntFactor(100);
        parent.AddData("x", expected);
        var child = new Context(parent);

        // Act
        var result = child.TryGetData("x", out var actual);

        // Assert
        result.Should().BeTrue();
        actual.Should().Be(expected);
    }

    [Fact]
    public void GetContextData_ShouldReturnValue_WhenFound()
    {
        // Arrange
        var context = new Context(null);
        var expected = new StringFactor("hello");
        context.AddData("message", expected);

        // Act
        var actual = context.GetContextData("message", Position.Default);

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void GetContextData_ShouldThrowRuntimeException_WhenNotFound()
    {
        // Arrange
        var context = new Context(null);

        // Act
        var act = () => context.GetContextData("missing", Position.Default);

        // Assert
        act.Should()
           .Throw<RuntimeException>()
           .WithMessage("*Variable of name missing was not defined!*");
    }

    [Fact]
    public void AddData_ShouldOverrideExistingValueInSameContext()
    {
        // Arrange
        var context = new Context(null);
        context.AddData("x", new IntFactor(1));

        // Act
        context.AddData("x", new IntFactor(99));

        // Assert
        context.GetContextData("x", Position.Default)
               .Should().BeEquivalentTo(new IntFactor(99));
    }

    [Fact]
    public void SetData_ShouldUpdateValueInCurrentContext()
    {
        // Arrange
        var context = new Context(null);
        context.AddData("x", new IntFactor(1));

        // Act
        context.SetData("x", new IntFactor(2));

        // Assert
        context.GetContextData("x", Position.Default)
               .Should().BeEquivalentTo(new IntFactor(2));
    }

    [Fact]
    public void SetData_ShouldUpdateValueInParentContext()
    {
        // Arrange
        var parent = new Context(null);
        parent.AddData("x", new IntFactor(5));
        var child = new Context(parent);

        // Act
        child.SetData("x", new IntFactor(10));

        // Assert
        parent.GetContextData("x", Position.Default)
              .Should().BeEquivalentTo(new IntFactor(10));
    }

    [Fact]
    public void SetData_ShouldAddVariable_WhenNotFoundAnywhere()
    {
        // Arrange
        var context = new Context(null);

        // Act
        context.SetData("x", new IntFactor(7));

        // Assert
        context.GetContextData("x", Position.Default)
               .Should().BeEquivalentTo(new IntFactor(7));
    }

    [Fact]
    public void SetData_ShouldNotUpdateGrandparent_WhenParentDefinesVariable()
    {
        // Arrange
        var grandparent = new Context(null);
        grandparent.AddData("x", new IntFactor(1));

        var parent = new Context(grandparent);
        parent.AddData("x", new IntFactor(2));

        var child = new Context(parent);

        // Act
        child.SetData("x", new IntFactor(3));

        // Assert
        parent.GetContextData("x", Position.Default).Should().BeEquivalentTo(new IntFactor(3));
        grandparent.GetContextData("x", Position.Default).Should().BeEquivalentTo(new IntFactor(1));
    }
}