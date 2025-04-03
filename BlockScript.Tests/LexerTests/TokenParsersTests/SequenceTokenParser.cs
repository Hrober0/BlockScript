using BlockScript.Lexer;
using BlockScript.Lexer.TokenParsers;
using FluentAssertions;
using Xunit;

namespace BlockScript.Tests.LexerTests.TokenParsersTests;

public class SequenceTokenParserTests
{
    [Fact]
    public void AcceptChar_ShouldReturnCompleted_WhenFullSequenceIsMatched()
    {
        // Arrange
        var parser = new SequenceTokenParser(TokenType.Operator, "if");

        // Act
        var result1 = parser.AcceptChar('i');
        var result2 = parser.AcceptChar('f');

        // Assert
        result1.Should().Be(AcceptStatus.Possible);
        result2.Should().Be(AcceptStatus.Completed);
    }

    [Fact]
    public void AcceptChar_ShouldReturnDeny_WhenCharacterDoesNotMatchSequence()
    {
        // Arrange
        var parser = new SequenceTokenParser(TokenType.Operator, "if");

        // Act
        var result = parser.AcceptChar('x');

        // Assert
        result.Should().Be(AcceptStatus.Deny);
    }

    [Fact]
    public void AcceptChar_ShouldReturnDeny_WhenMismatchOccursMidSequence()
    {
        // Arrange
        var parser = new SequenceTokenParser(TokenType.Operator, "if");

        // Act
        parser.AcceptChar('i');
        var result = parser.AcceptChar('x');

        // Assert
        result.Should().Be(AcceptStatus.Deny);
    }

    [Fact]
    public void IsValid_ShouldReturnTrue_WhenFullSequenceIsAccepted()
    {
        // Arrange
        var parser = new SequenceTokenParser(TokenType.Operator, "return");

        // Act
        parser.AcceptChar('r');
        parser.AcceptChar('e');
        parser.AcceptChar('t');
        parser.AcceptChar('u');
        parser.AcceptChar('r');
        parser.AcceptChar('n');
        var result = parser.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenPartialSequenceIsAccepted()
    {
        // Arrange
        var parser = new SequenceTokenParser(TokenType.Operator, "while");

        // Act
        parser.AcceptChar('w');
        parser.AcceptChar('h');
        parser.AcceptChar('i');
        var result = parser.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CreateToken_ShouldReturnCorrectTokenData()
    {
        // Arrange
        var parser = new SequenceTokenParser(TokenType.Operator, "==");
        parser.AcceptChar('=');
        parser.AcceptChar('=');

        // Act
        var token = parser.CreateToken(5, 10);

        // Assert
        token.Line.Should().Be(5);
        token.Column.Should().Be(10);
        token.Type.Should().Be(TokenType.Operator);
        token.Value.Should().Be("==");
        token.CharacterLength.Should().Be(2);
    }

    [Fact]
    public void Reset_ShouldClearStoredCharacters()
    {
        // Arrange
        var parser = new SequenceTokenParser(TokenType.Operator, "for");
        parser.AcceptChar('f');
        parser.AcceptChar('o');

        // Act
        parser.Reset();
        var result = parser.IsValid();

        // Assert
        result.Should().BeFalse();
    }
}
