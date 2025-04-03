using BlockScript.Lexer;
using BlockScript.Lexer.TokenParsers;
using FluentAssertions;
using Xunit;

namespace BlockScript.Tests.LexerTests.TokenParsersTests;

public class IntTokenParserTests
{
    [Fact]
    public void AcceptChar_ShouldReturnPossible_WhenValidDigitIsEncountered()
    {
        // Arrange
        var parser = new IntTokenParser();

        // Act
        var result = parser.AcceptChar('5');

        // Assert
        result.Should().Be(AcceptStatus.Possible);
    }

    [Fact]
    public void AcceptChar_ShouldReturnDeny_WhenNonDigitCharacterIsEncountered()
    {
        // Arrange
        var parser = new IntTokenParser();

        // Act
        var result = parser.AcceptChar('a'); // Invalid character

        // Assert
        result.Should().Be(AcceptStatus.Deny);
    }

    [Fact]
    public void AcceptChar_ShouldReturnDeny_WhenLeadingZeroIsFollowedByAnotherDigit()
    {
        // Arrange
        var parser = new IntTokenParser();
        parser.AcceptChar('0'); // Leading zero

        // Act
        var result = parser.AcceptChar('5'); // Invalid after leading zero

        // Assert
        result.Should().Be(AcceptStatus.Deny);
    }

    [Fact]
    public void AcceptChar_ShouldReturnPossible_WhenValidSequenceOfDigitsIsAccepted()
    {
        // Arrange
        var parser = new IntTokenParser();

        // Act
        parser.AcceptChar('1');
        var result = parser.AcceptChar('2'); // Valid sequence of digits

        // Assert
        result.Should().Be(AcceptStatus.Possible);
    }

    [Fact]
    public void IsValid_ShouldReturnTrue_WhenValidIntegerIsParsed()
    {
        // Arrange
        var parser = new IntTokenParser();
        parser.AcceptChar('1');
        parser.AcceptChar('2');
        parser.AcceptChar('3');

        // Act
        var result = parser.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CreateToken_ShouldReturnCorrectTokenData()
    {
        // Arrange
        var parser = new IntTokenParser();
        parser.AcceptChar('1');
        parser.AcceptChar('2');
        parser.AcceptChar('3');

        // Act
        var token = parser.CreateToken(3, 7);

        // Assert
        token.Line.Should().Be(3);
        token.Column.Should().Be(7);
        token.Type.Should().Be(TokenType.Integer);
        token.Value.Should().Be(123);
        token.CharacterLength.Should().Be(3);
    }

    [Fact]
    public void Reset_ShouldClearState()
    {
        // Arrange
        var parser = new IntTokenParser();
        parser.AcceptChar('1');
        parser.AcceptChar('2');
        parser.AcceptChar('3');

        // Act
        parser.Reset();
        var result = parser.IsValid();

        // Assert
        result.Should().BeFalse();
    }
}
