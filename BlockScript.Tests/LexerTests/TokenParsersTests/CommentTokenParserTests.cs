using BlockScript.Lexer;
using BlockScript.Lexer.TokenParsers;
using FluentAssertions;
using Xunit;

namespace BlockScript.Tests.LexerTests.TokenParsersTests;

public class CommentTokenParserTests
{
    [Fact]
    public void AcceptChar_ShouldReturnPossible_WhenHashSymbolIsEncountered()
    {
        // Arrange
        var parser = new CommentTokenParser();

        // Act
        var result = parser.AcceptChar('#');

        // Assert
        result.Should().Be(AcceptStatus.Possible);
    }

    [Fact]
    public void AcceptChar_ShouldReturnDeny_WhenCharacterBeforeHashSymbolIsEncountered()
    {
        // Arrange
        var parser = new CommentTokenParser();

        // Act
        var result = parser.AcceptChar('a'); // Invalid character before '#' symbol

        // Assert
        result.Should().Be(AcceptStatus.Deny);
    }

    [Fact]
    public void AcceptChar_ShouldAppendCharactersAfterHashSymbol()
    {
        // Arrange
        var parser = new CommentTokenParser();
        parser.AcceptChar('#'); // First # symbol

        // Act
        var result = parser.AcceptChar('T'); // Adding character to comment

        // Assert
        result.Should().Be(AcceptStatus.Possible);
    }
    
    [Fact]
    public void AcceptChar_ShouldThrowException_WhenEnterTooManyCharacters()
    {
        // Arrange
        var parser = new CommentTokenParser();
        parser.AcceptChar('#'); // First # symbol

        // Act
        var act = () =>
        {
            for (int i = 0; i < 256; i++)
            {
                parser.AcceptChar('a');
            }
        };

        // Assert
        act.Should().Throw<OverflowException>();
    }


    [Fact]
    public void IsValid_ShouldReturnTrue_WhenValidCommentIsParsed()
    {
        // Arrange
        var parser = new CommentTokenParser();
        parser.AcceptChar('#');
        parser.AcceptChar('T');
        parser.AcceptChar('h');
        parser.AcceptChar('i');
        parser.AcceptChar('s');
        
        // Act
        var result = parser.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CreateToken_ShouldReturnCorrectTokenData()
    {
        // Arrange
        var parser = new CommentTokenParser();
        parser.AcceptChar('#');
        parser.AcceptChar('T');
        parser.AcceptChar('h');
        parser.AcceptChar('i');
        parser.AcceptChar('s');
        
        // Act
        var token = parser.CreateToken(2, 5);

        // Assert
        token.Line.Should().Be(2);
        token.Column.Should().Be(5);
        token.Type.Should().Be(TokenType.Comment);
        token.Value.Should().Be("This");
        token.CharacterLength.Should().Be(5);
    }

    [Fact]
    public void Reset_ShouldClearState()
    {
        // Arrange
        var parser = new CommentTokenParser();
        parser.AcceptChar('#');
        parser.AcceptChar('T');
        parser.AcceptChar('h');
        parser.AcceptChar('i');
        parser.AcceptChar('s');

        // Act
        parser.Reset();
        var result = parser.IsValid();

        // Assert
        result.Should().BeFalse();
    }
}
