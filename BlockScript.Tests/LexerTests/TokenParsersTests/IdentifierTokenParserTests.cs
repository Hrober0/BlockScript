using BlockScript.Lexer;
using BlockScript.Lexer.TokenParsers;
using FluentAssertions;
using Xunit;

namespace BlockScript.Tests.LexerTests.TokenParsersTests;

public class IdentifierTokenParserTests
{
    [Fact]
    public void AcceptChar_ShouldAcceptLetters()
    {
        // Arrange
        var parser = new IdentifierTokenParser();

        // Act & Assert
        parser.AcceptChar('a').Should().Be(AcceptStatus.Possible);
        parser.AcceptChar('Z').Should().Be(AcceptStatus.Possible);
    }

    [Fact]
    public void AcceptChar_ShouldAcceptDigitsAfterLetters()
    {
        // Arrange
        var parser = new IdentifierTokenParser();

        // Act
        parser.AcceptChar('a');
        var result = parser.AcceptChar('1');

        // Assert
        result.Should().Be(AcceptStatus.Possible);
    }

    [Fact]
    public void AcceptChar_ShouldDenyDigitAsFirstCharacter()
    {
        // Arrange
        var parser = new IdentifierTokenParser();

        // Act
        var result = parser.AcceptChar('1');

        // Assert
        result.Should().Be(AcceptStatus.Deny);
    }

    [Fact]
    public void IsValid_ShouldReturnTrue_WhenIdentifierHasCharacters()
    {
        // Arrange
        var parser = new IdentifierTokenParser();
        parser.AcceptChar('a');

        // Act
        var result = parser.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenIdentifierIsEmpty()
    {
        // Arrange
        var parser = new IdentifierTokenParser();

        // Act
        var result = parser.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CreateToken_ShouldReturnCorrectTokenData()
    {
        // Arrange
        var parser = new IdentifierTokenParser();
        parser.AcceptChar('x');
        parser.AcceptChar('y');

        // Act
        var token = parser.CreateToken(1, 2);

        // Assert
        token.Line.Should().Be(1);
        token.Column.Should().Be(2);
        token.Type.Should().Be(TokenType.Identifier);
        token.Value.Should().Be("xy");
        token.CharacterLength.Should().Be(2);
    }

    [Fact]
    public void Reset_ShouldClearStoredCharacters()
    {
        // Arrange
        var parser = new IdentifierTokenParser();
        parser.AcceptChar('x');

        // Act
        parser.Reset();
        var result = parser.IsValid();

        // Assert
        result.Should().BeFalse();
    }
}
