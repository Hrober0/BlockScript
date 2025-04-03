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
        var parser = new IdentifierTokenParser();
        parser.AcceptChar('a').Should().Be(AcceptStatus.Possible);
        parser.AcceptChar('Z').Should().Be(AcceptStatus.Possible);
    }

    [Fact]
    public void AcceptChar_ShouldAcceptDigitsAfterLetters()
    {
        var parser = new IdentifierTokenParser();
        parser.AcceptChar('a');
        parser.AcceptChar('1').Should().Be(AcceptStatus.Possible);
    }

    [Fact]
    public void AcceptChar_ShouldDenyDigitAsFirstCharacter()
    {
        var parser = new IdentifierTokenParser();
        parser.AcceptChar('1').Should().Be(AcceptStatus.Deny);
    }

    [Fact]
    public void IsValid_ShouldReturnTrue_WhenIdentifierHasCharacters()
    {
        var parser = new IdentifierTokenParser();
        parser.AcceptChar('a');
        parser.IsValid().Should().BeTrue();
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenIdentifierIsEmpty()
    {
        var parser = new IdentifierTokenParser();
        parser.IsValid().Should().BeFalse();
    }

    [Fact]
    public void CreateToken_ShouldReturnCorrectTokenData()
    {
        var parser = new IdentifierTokenParser();
        parser.AcceptChar('x');
        parser.AcceptChar('y');
        var token = parser.CreateToken(1, 2);

        token.Line.Should().Be(1);
        token.Column.Should().Be(2);
        token.Type.Should().Be(TokenType.Identifier);
        token.Value.Should().Be("xy");
        token.CharacterLength.Should().Be(2);
    }

    [Fact]
    public void Reset_ShouldClearStoredCharacters()
    {
        var parser = new IdentifierTokenParser();
        parser.AcceptChar('x');
        parser.Reset();
        parser.IsValid().Should().BeFalse();
    }
}