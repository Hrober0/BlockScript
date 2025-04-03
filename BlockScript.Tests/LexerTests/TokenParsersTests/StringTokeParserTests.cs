using BlockScript.Lexer;
using BlockScript.Lexer.TokenParsers;
using FluentAssertions;
using Xunit;

namespace BlockScript.Tests.LexerTests.TokenParsersTests;

public class StringTokenParserTests
{
    [Fact]
    public void AcceptChar_ShouldReturnPossible_WhenOpeningQuoteIsEncountered()
    {
        // Arrange
        var parser = new StringTokenParser();

        // Act
        var result = parser.AcceptChar('\"');

        // Assert
        result.Should().Be(AcceptStatus.Possible);
    }

    [Fact]
    public void AcceptChar_ShouldReturnPossible_WhenCharacterIsInsideString()
    {
        // Arrange
        var parser = new StringTokenParser();
        parser.AcceptChar('\"'); // Opening quote
        parser.AcceptChar('h'); // Inside string

        // Act
        var result = parser.AcceptChar('e'); // Inside string

        // Assert
        result.Should().Be(AcceptStatus.Possible);
    }

    [Fact]
    public void AcceptChar_ShouldReturnCompleted_WhenClosingQuoteIsEncountered()
    {
        // Arrange
        var parser = new StringTokenParser();
        parser.AcceptChar('\"'); // Opening quote
        parser.AcceptChar('h');  // Inside string
        parser.AcceptChar('i');  // Inside string

        // Act
        var result = parser.AcceptChar('\"'); // Closing quote

        // Assert
        result.Should().Be(AcceptStatus.Completed);
    }

    [Fact]
    public void IsValid_ShouldReturnTrue_WhenStringIsCompleteWithQuotes()
    {
        // Arrange
        var parser = new StringTokenParser();
        parser.AcceptChar('\"');
        parser.AcceptChar('h');
        parser.AcceptChar('i');
        parser.AcceptChar('\"');

        // Act
        var result = parser.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenStringDoesNotHaveClosingQuote()
    {
        // Arrange
        var parser = new StringTokenParser();
        parser.AcceptChar('\"');
        parser.AcceptChar('h');
        parser.AcceptChar('i');

        // Act
        var result = parser.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CreateToken_ShouldReturnCorrectTokenData()
    {
        // Arrange
        var parser = new StringTokenParser();
        parser.AcceptChar('\"');
        parser.AcceptChar('h');
        parser.AcceptChar('e');
        parser.AcceptChar('\"');

        // Act
        var token = parser.CreateToken(2, 5);

        // Assert
        token.Line.Should().Be(2);
        token.Column.Should().Be(5);
        token.Type.Should().Be(TokenType.String);
        token.Value.Should().Be("he");
        token.CharacterLength.Should().Be(4); // Including the quotes
    }

    [Fact]
    public void GetErrorHint_ShouldReturnCorrectErrorMessage_WhenStringIsNotClosed()
    {
        // Arrange
        var parser = new StringTokenParser();
        parser.AcceptChar('\"');
        parser.AcceptChar('h');
        parser.AcceptChar('i');

        // Act
        var result = parser.GetErrorHint();

        // Assert
        result.Should().Be("Expected '\"' at the end of string");
    }

    [Fact]
    public void Reset_ShouldClearStateAndString()
    {
        // Arrange
        var parser = new StringTokenParser();
        parser.AcceptChar('\"');
        parser.AcceptChar('h');
        parser.AcceptChar('i');
        parser.AcceptChar('\"');

        // Act
        parser.Reset();
        var result = parser.IsValid();

        // Assert
        result.Should().BeFalse();
    }
}
