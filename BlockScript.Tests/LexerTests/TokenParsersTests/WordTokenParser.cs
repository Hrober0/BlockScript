using BlockScript.Lexer;
using BlockScript.Lexer.TokenParsers;
using FluentAssertions;
using Xunit;

namespace BlockScript.Tests.LexerTests.TokenParsersTests;

public class WordTokenParserTests
{
    [Fact]
    public void AcceptChar_ShouldReturnCompleted_WhenFullSequenceIsMatched()
    {
        // Arrange
        var parser = new WordTokenParser(TokenType.Operator, "while");

        // Act
        parser.AcceptChar('w');
        parser.AcceptChar('h');
        parser.AcceptChar('i');
        parser.AcceptChar('l');
        parser.AcceptChar('e');
        var result = parser.AcceptChar(' ');

        // Assert
        result.Should().Be(AcceptStatus.Completed);
    }

    [Fact]
    public void AcceptChar_ShouldReturnDeny_WhenCharacterDoesNotMatchSequence()
    {
        // Arrange
        var parser = new WordTokenParser(TokenType.Operator, "if");

        // Act
        parser.AcceptChar('x'); // First character mismatch
        var result = parser.AcceptChar('y');

        // Assert
        result.Should().Be(AcceptStatus.Deny);
    }

    [Fact]
    public void AcceptChar_ShouldReturnDeny_WhenFollowedByIncorrectCharacterAfterFullSequence()
    {
        // Arrange
        var parser = new WordTokenParser(TokenType.Operator, "for");
        parser.AcceptChar('f');
        parser.AcceptChar('o');
        parser.AcceptChar('r');

        // Act
        var result = parser.AcceptChar('1'); // Incorrect character after the sequence

        // Assert
        result.Should().Be(AcceptStatus.Deny);
    }

    [Fact]
    public void IsValid_ShouldReturnTrue_WhenFullSequenceIsAcceptedAndNoIncorrectCharacters()
    {
        // Arrange
        var parser = new WordTokenParser(TokenType.Operator, "break");

        // Act
        parser.AcceptChar('b');
        parser.AcceptChar('r');
        parser.AcceptChar('e');
        parser.AcceptChar('a');
        parser.AcceptChar('k');
        var result = parser.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenSequenceIsIncomplete()
    {
        // Arrange
        var parser = new WordTokenParser(TokenType.Operator, "switch");

        // Act
        parser.AcceptChar('s');
        parser.AcceptChar('w');
        parser.AcceptChar('i');
        var result = parser.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CreateToken_ShouldReturnCorrectTokenData()
    {
        // Arrange
        var parser = new WordTokenParser(TokenType.Operator, "continue");
        parser.AcceptChar('c');
        parser.AcceptChar('o');
        parser.AcceptChar('n');
        parser.AcceptChar('t');
        parser.AcceptChar('i');
        parser.AcceptChar('n');
        parser.AcceptChar('u');
        parser.AcceptChar('e');

        // Act
        var token = parser.CreateToken(1, 5);

        // Assert
        token.Line.Should().Be(1);
        token.Column.Should().Be(5);
        token.Type.Should().Be(TokenType.Operator);
        token.Value.Should().Be("continue");
        token.CharacterLength.Should().Be(8);
    }

    [Fact]
    public void CreateToken_ShouldInvokeParsingMethod_WhenProvided()
    {
        // Arrange
        CustomParsingMethod parsingMethod = (sequence) => sequence.ToUpper();
        var parser = new WordTokenParser(TokenType.Operator, "return", parsingMethod);
        parser.AcceptChar('r');
        parser.AcceptChar('e');
        parser.AcceptChar('t');
        parser.AcceptChar('u');
        parser.AcceptChar('r');
        parser.AcceptChar('n');

        // Act
        var token = parser.CreateToken(2, 10);

        // Assert
        token.Value.Should().Be("RETURN"); // Value should be in uppercase as defined by the custom parsing method
    }

    [Fact]
    public void Reset_ShouldClearStoredCharactersAndState()
    {
        // Arrange
        var parser = new WordTokenParser(TokenType.Operator, "class");
        parser.AcceptChar('c');
        parser.AcceptChar('l');
        parser.AcceptChar('a');
        parser.AcceptChar('s');

        // Act
        parser.Reset();
        var result = parser.IsValid();

        // Assert
        result.Should().BeFalse();
    }
}
