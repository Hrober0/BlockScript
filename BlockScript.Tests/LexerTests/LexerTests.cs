using System.Text;
using BlockScript.Exceptions;
using BlockScript.Lexer;
using BlockScript.Lexer.FactorValues;
using FluentAssertions;
using Xunit;

namespace BlockScript.Tests.LexerTests;

public class LexerTests
{
    [Fact]
    public void GetToken_ShouldReturnEndOfTextToken_WhenEndOfTextIsReached()
    {
        // Arrange
        var text = "";
        var reader = new StringReader(text);
        var lexer = new Lexer.Lexer(reader);

        // Act
        var token = lexer.GetToken();

        // Assert
        token.Type.Should().Be(TokenType.EndOfText);
    }
    
    [Fact]
    public void GetToken_ShouldSkipWhitespaceAndReturnNextToken()
    {
        // Arrange
        var text = "  loop ";
        var reader = new StringReader(text);
        var lexer = new Lexer.Lexer(reader);

        // Act
        var token = lexer.GetToken();

        // Assert
        token.Type.Should().Be(TokenType.Loop);
        token.Value.Should().Be(new StringFactor("loop"));
    }
    
    #region comment
    
    [Fact]
    public void GetToken_ShouldReturnCommentToken_WhenCommentIsEncountered()
    {
        // Arrange
        var text = "#This is a comment";
        var reader = new StringReader(text);
        var lexer = new Lexer.Lexer(reader);

        // Act
        var token1 = lexer.GetToken();
        var token2 = lexer.GetToken();

        // Assert
        token1.Type.Should().Be(TokenType.Comment);
        token1.Value.Should().Be(new StringFactor("This is a comment"));
        token2.Type.Should().Be(TokenType.EndOfText);
    }
    
    [Fact]
    public void GetToken_ShouldThrow_WhenCommentIsTooLong()
    {
        // Arrange
        var text = new StringBuilder("#");
        for (int i = 0; i < 256; i++)
        {
            text.Append('a');
        }
        var reader = new StringReader(text.ToString());
        var lexer = new Lexer.Lexer(reader);
        
        // Act
        var act = () => lexer.GetToken();

        // Assert
        act.Should().Throw<TokenException>()
            .Where(e => e.Message.Contains("exceeds"));
    }
    
    [Fact]
    public void GetToken_ShouldNotThrowException_WhenCommentIsNotClosedBeforeEndOfFile()
    {
        // Arrange
        var text = "# This is an open comment"; // Comment without a closing new line or EOF
        var reader = new StringReader(text);
        var lexer = new Lexer.Lexer(reader);

        // Act
        var token = lexer.GetToken();
        var nextToken = lexer.GetToken();

        // Assert
        token.Should().NotBeNull();
        token.Type.Should().Be(TokenType.Comment);
        nextToken.Type.Should().Be(TokenType.EndOfText);
    }

    [Fact]
    public void GetToken_ShouldNotThrowException_WhenCommentIsNotClosedBeforeNewLine()
    {
        // Arrange
        var text = "# This is an open comment\n"; // Comment followed by new line
        var reader = new StringReader(text);
        var lexer = new Lexer.Lexer(reader);

        // Act
        var token = lexer.GetToken();
        var nextToken = lexer.GetToken();
        
        // Assert
        token.Should().NotBeNull();
        token.Type.Should().Be(TokenType.Comment);
        nextToken.Type.Should().Be(TokenType.EndOfText);
    }
    
    #endregion
    
    #region number
    
    [Fact]
    public void GetToken_ShouldReturnIntegerToken_WhenIntegerIsEncountered()
    {
        // Arrange
        var text = "2147483647";
        var reader = new StringReader(text);
        var lexer = new Lexer.Lexer(reader);

        // Act
        var token1 = lexer.GetToken();
        var token2 = lexer.GetToken();

        // Assert
        token1.Type.Should().Be(TokenType.Integer);
        token1.Value.Should().Be(new IntFactor(2147483647));
        token2.Type.Should().Be(TokenType.EndOfText);
    }
    
    [Fact]
    public void GetToken_ShouldReturnTwoIntegerToken_WhenTwoZerosEncountered()
    {
        // Arrange
        var text = "0010";
        var reader = new StringReader(text);
        var lexer = new Lexer.Lexer(reader);

        // Act
        var token1 = lexer.GetToken();
        var token2 = lexer.GetToken();

        // Assert
        token1.Type.Should().Be(TokenType.Integer);
        token1.Value.Should().Be(new IntFactor(10));
        token2.Type.Should().Be(TokenType.EndOfText);
    }

    [Fact]
    public void GetToken_ShouldThrow_WhenIntegerIsExceedsLimit()
    {
        // Arrange
        var text = "2147483648";
        var reader = new StringReader(text);
        var lexer = new Lexer.Lexer(reader);

        // Act
        var act = () => lexer.GetToken();

        // Assert
        act.Should().Throw<TokenException>()
            .Where(e => e.Message.Contains("exceeds"));
    }
    
    [Fact]
    public void GetToken_ShouldNotThrowException_WhenIntegerIsNotFullyParsedBeforeEndOfFile()
    {
        // Arrange
        var text = "1234";
        var reader = new StringReader(text);
        var lexer = new Lexer.Lexer(reader);

        // Act
        var token = lexer.GetToken();
        var nextToken = lexer.GetToken();

        // Assert
        token.Should().NotBeNull();
        token.Type.Should().Be(TokenType.Integer);
        nextToken.Type.Should().Be(TokenType.EndOfText);
    }

    [Fact]
    public void GetToken_ShouldNotThrowException_WhenIntegerIsNotFullyParsedBeforeNewLine()
    {
        // Arrange
        var text = "1234\n";
        var reader = new StringReader(text);
        var lexer = new Lexer.Lexer(reader);

        // Act
        var token = lexer.GetToken();
        var nextToken = lexer.GetToken();
        
        // Assert
        token.Should().NotBeNull();
        token.Type.Should().Be(TokenType.Integer);
        nextToken.Type.Should().Be(TokenType.EndOfText);
    }
    
    #endregion
    
    #region string
    
    [Fact]
    public void GetToken_ShouldReturnStringToken_WhenStringIsEncountered()
    {
        // Arrange
        var text = "\"Hello, World!\"";
        var reader = new StringReader(text);
        var lexer = new Lexer.Lexer(reader);

        // Act
        var token1 = lexer.GetToken();
        var token2 = lexer.GetToken();

        // Assert
        token1.Type.Should().Be(TokenType.String);
        token1.Value.Should().Be(new StringFactor("Hello, World!"));
        token2.Type.Should().Be(TokenType.EndOfText);
    }
    
    [Fact]
    public void GetToken_ShouldReturnEmptyStringToken_WhenEmptyStringIsEncountered()
    {
        // Arrange
        var text = "\"\"";
        var reader = new StringReader(text);
        var lexer = new Lexer.Lexer(reader);

        // Act
        var token = lexer.GetToken();

        // Assert
        token.Type.Should().Be(TokenType.String);
        token.Value.Should().Be(new StringFactor(""));
    }
    
    [Fact]
    public void GetToken_ShouldReturnStringToken_WhenStringAfterStringIsEncountered()
    {
        // Arrange
        var text = "\"Hello, World!\"\"a\"";
        var reader = new StringReader(text);
        var lexer = new Lexer.Lexer(reader);

        // Act
        var token1 = lexer.GetToken();
        var token2 = lexer.GetToken();
        var token3 = lexer.GetToken();

        // Assert
        token1.Type.Should().Be(TokenType.String);
        token1.Value.Should().Be(new StringFactor("Hello, World!"));
        token2.Type.Should().Be(TokenType.String);
        token2.Value.Should().Be(new StringFactor("a"));
        token3.Type.Should().Be(TokenType.EndOfText);
    }
    
    [Fact]
    public void GetToken_ShouldReturnStringToken_WhenStringContainSpecialCharacters()
    {
        // Arrange
        var text = "\"[\\\"]\"";
        var reader = new StringReader(text);
        var lexer = new Lexer.Lexer(reader);

        // Act
        var token = lexer.GetToken();

        // Assert
        token.Type.Should().Be(TokenType.String);
        token.Value.Should().Be(new StringFactor("[\\\"]"));
    }

    [Fact]
    public void GetToken_ShouldThrow_WhenStringIsTooLong()
    {
        // Arrange
        var text = new StringBuilder("\"");
        for (int i = 0; i < 257; i++)
        {
            text.Append('a');
        }
        var reader = new StringReader(text.ToString());
        var lexer = new Lexer.Lexer(reader);
        
        // Act
        var act = () => lexer.GetToken();

        // Assert
        act.Should().Throw<TokenException>()
            .Where(e => e.Message.Contains("exceeds"));
    }
    
    [Fact]
    public void GetToken_ShouldThrow_WhenStringIsNotClosed()
    {
        // Arrange
        var text = new StringBuilder("\"aaa\ndd");
        for (int i = 0; i < 256; i++)
        {
            text.Append('a');
        }
        var reader = new StringReader(text.ToString());
        var lexer = new Lexer.Lexer(reader);
        
        // Act
        var act = () => lexer.GetToken();

        // Assert
        act.Should().Throw<TokenException>()
            .Where(e => e.Message.Contains("expected '\"'"));
    }
    
    [Fact]
    public void GetToken_ShouldThrow_WhenStringIsNotClosedAndEndWithBackshlas()
    {
        // Arrange
        var text = new StringBuilder("\"\\");
        var reader = new StringReader(text.ToString());
        var lexer = new Lexer.Lexer(reader);
        
        // Act
        var act = () => lexer.GetToken();

        // Assert
        act.Should().Throw<TokenException>().
            Where(e => e.Message.Contains("after '\\'"));
    }
    
    [Fact]
    public void GetToken_ShouldThrowException_WhenStringIsNotClosedBeforeEndOfFile()
    {
        // Arrange
        var text = "\"This is an open string"; // Open string without a closing quote
        var reader = new StringReader(text);
        var lexer = new Lexer.Lexer(reader);

        // Act
        Action act = () => lexer.GetToken();

        // Assert
        act.Should().Throw<TokenException>();
    }

    [Fact]
    public void GetToken_ShouldThrowException_WhenStringIsNotClosedBeforeNewLine()
    {
        // Arrange
        var text = "\"This is an open string\n"; // Open string without a closing quote
        var reader = new StringReader(text);
        var lexer = new Lexer.Lexer(reader);

        // Act
        Action act = () => lexer.GetToken();

        // Assert
        act.Should().Throw<TokenException>();
    }
    
    #endregion
    
    #region keywords
    
    [Theory]
    [InlineData("null", TokenType.Null)]
    [InlineData("loop", TokenType.Loop)]
    [InlineData("if",   TokenType.If)]
    [InlineData("else", TokenType.Else)]
    public void GetToken_ShouldReturnValidKeyWordToken(string keyword, TokenType tokenType)
    {
        // Arrange
        var reader = new StringReader(keyword);
        var lexer = new Lexer.Lexer(reader);

        // Act
        var token1 = lexer.GetToken();
        var token2 = lexer.GetToken();

        // Assert
        token1.Type.Should().Be(tokenType);
        token2.Type.Should().Be(TokenType.EndOfText);
    }

    [Theory]
    [InlineData("null", TokenType.Null)]
    [InlineData("loop", TokenType.Loop)]
    [InlineData("if",   TokenType.If)]
    [InlineData("else", TokenType.Else)]
    public void GetToken_ShouldNotReturnValidKeyWordToken_WhenEncounterCharacterAfter(string keyword, TokenType tokenType)
    {
        // Arrange
        var reader = new StringReader(keyword + "a");
        var lexer = new Lexer.Lexer(reader);

        // Act
        var token1 = lexer.GetToken();
        var token2 = lexer.GetToken();

        // Assert
        token1.Type.Should().NotBe(tokenType);
        token2.Type.Should().Be(TokenType.EndOfText);
    }
    
    
    #endregion
    
    #region operators
    
    [Theory]
    [InlineData("==", TokenType.OperatorEqual)]
    [InlineData("<=", TokenType.OperatorLessEqual)]
    [InlineData("<", TokenType.OperatorLess)]
    [InlineData(">=", TokenType.OperatorGreaterEqual)]
    [InlineData(">", TokenType.OperatorGreater)]
    [InlineData("!=", TokenType.OperatorNotEqual)]
    [InlineData("||", TokenType.OperatorOr)]
    [InlineData("&&", TokenType.OperatorAnd)]
    [InlineData("??", TokenType.OperatorNullCoalescing)]
    [InlineData("+", TokenType.OperatorAdd)]
    [InlineData("-", TokenType.OperatorSubtract)]
    [InlineData("*", TokenType.OperatorMultiply)]
    [InlineData("/", TokenType.OperatorDivide)]
    [InlineData("=>", TokenType.OperatorArrow)]
    [InlineData(":=", TokenType.OperatorAssign)]
    [InlineData("=", TokenType.OperatorDeclaration)]
    [InlineData("?=", TokenType.OperatorNullAssign)]
    public void GetToken_ShouldReturnValidOperatorTokens(string @operator, TokenType tokenType)
    {
        // Arrange
        var reader = new StringReader(@operator);
        var lexer = new Lexer.Lexer(reader);

        // Act
        var token1 = lexer.GetToken();
        var token2 = lexer.GetToken();

        // Assert
        token1.Type.Should().Be(tokenType);
        token2.Type.Should().Be(TokenType.EndOfText);
    }
    
    [Fact]
    public void GetToken_ShouldReturnValidOperatorTokens_WhenTokesAreGrouped()
    {
        // Arrange
        var text = "==<=>=!=+===";
        var reader = new StringReader(text);
        var lexer = new Lexer.Lexer(reader);

        // Act & Assert
        lexer.GetToken().Type.Should().Be(TokenType.OperatorEqual);
        lexer.GetToken().Type.Should().Be(TokenType.OperatorLessEqual);
        lexer.GetToken().Type.Should().Be(TokenType.OperatorGreaterEqual);
        lexer.GetToken().Type.Should().Be(TokenType.OperatorNotEqual);
        lexer.GetToken().Type.Should().Be(TokenType.OperatorAdd);
        lexer.GetToken().Value.Should().Be(new StringFactor("=="));
        lexer.GetToken().Value.Should().Be(new StringFactor("="));
        lexer.GetToken().Type.Should().Be(TokenType.EndOfText);
    }
    
    #endregion
    
    #region identifier
    
    [Fact]
    public void GetToken_ShouldNotThrowException_WhenIdentifierIsNotFullyParsedBeforeEndOfFile()
    {
        // Arrange
        var text = "ident1234abc"; // The identifier isn't fully terminated
        var reader = new StringReader(text);
        var lexer = new Lexer.Lexer(reader);

        // Act
        var token = lexer.GetToken();
        var nextToken = lexer.GetToken();

        // Assert
        token.Should().NotBeNull();
        token.Type.Should().Be(TokenType.Identifier);
        nextToken.Type.Should().Be(TokenType.EndOfText);
    }

    [Fact]
    public void GetToken_ShouldNotThrowException_WhenIdentifierIsNotFullyParsedBeforeNewLine()
    {
        // Arrange
        var text = "ident1234abc\n"; // The identifier isn't fully terminated
        var reader = new StringReader(text);
        var lexer = new Lexer.Lexer(reader);

        // Act
        var token = lexer.GetToken();
        var nextToken = lexer.GetToken();

        // Assert
        token.Should().NotBeNull();
        token.Type.Should().Be(TokenType.Identifier);
        nextToken.Type.Should().Be(TokenType.EndOfText);
    }

    [Fact]
    public void GetToken_ShouldNotThrowException_WhenWordIsNotFullyParsedBeforeEndOfFile()
    {
        // Arrange
        var text = "looping"; // Incomplete word 'looping'
        var reader = new StringReader(text);
        var lexer = new Lexer.Lexer(reader);

        // Act
        var token = lexer.GetToken();
        var nextToken = lexer.GetToken();

        // Assert
        token.Should().NotBeNull();
        token.Type.Should().Be(TokenType.Identifier);
        nextToken.Type.Should().Be(TokenType.EndOfText);
    }
    
    #endregion
    
    #region TokenPositions

    [Fact]
    public void GetToken_ShouldCorrectlyCapturePosition_ForStringToken()
    {
        // Arrange
        var text = "\"Hello, world!\"";
        var reader = new StringReader(text);
        var lexer = new Lexer.Lexer(reader);

        // Act
        var token = lexer.GetToken();

        // Assert
        token.Should().NotBeNull();
        token.Type.Should().Be(TokenType.String);
        token.Position.Line.Should().Be(1);
        token.Position.Column.Should().Be(1);
    }

    [Fact]
    public void GetToken_ShouldCorrectlyCapturePosition_ForCommentToken()
    {
        // Arrange
        var text = "# This is a comment";
        var reader = new StringReader(text);
        var lexer = new Lexer.Lexer(reader);

        // Act
        var token = lexer.GetToken();

        // Assert
        token.Should().NotBeNull();
        token.Type.Should().Be(TokenType.Comment);
        token.Position.Line.Should().Be(1);
        token.Position.Column.Should().Be(1);
    }

    [Fact]
    public void GetToken_ShouldCorrectlyCapturePosition_ForIntegerToken()
    {
        // Arrange
        var text = "12345";
        var reader = new StringReader(text);
        var lexer = new Lexer.Lexer(reader);

        // Act
        var token = lexer.GetToken();

        // Assert
        token.Should().NotBeNull();
        token.Type.Should().Be(TokenType.Integer);
        token.Position.Line.Should().Be(1);
        token.Position.Column.Should().Be(1);
    }

    [Fact]
    public void GetToken_ShouldCorrectlyCapturePosition_ForIdentifierToken()
    {
        // Arrange
        var text = "variableName";
        var reader = new StringReader(text);
        var lexer = new Lexer.Lexer(reader);

        // Act
        var token = lexer.GetToken();

        // Assert
        token.Should().NotBeNull();
        token.Type.Should().Be(TokenType.Identifier);
        token.Position.Line.Should().Be(1);
        token.Position.Column.Should().Be(1);
    }

    [Fact]
    public void GetToken_ShouldCorrectlyCapturePosition_AfterMultipleLines()
    {
        // Arrange
        var text = "variableName;\nloop\n;x = 10;\n# This is a comment";
        var reader = new StringReader(text);
        var lexer = new Lexer.Lexer(reader);

        // Act
        var token1 = lexer.GetToken(); // variableName
        var token2 = lexer.GetToken(); // ;
        var token3 = lexer.GetToken(); // loop
        var token4 = lexer.GetToken(); // ;
        var token5 = lexer.GetToken(); // x
        var token6 = lexer.GetToken(); // =
        var token7 = lexer.GetToken(); // 10
        var token8 = lexer.GetToken(); // ;
        var token9 = lexer.GetToken(); // comment
        var token10 = lexer.GetToken(); // End

        // Assert for token1 (variableName)
        token1.Type.Should().Be(TokenType.Identifier);
        token1.Position.Line.Should().Be(1);
        token1.Position.Column.Should().Be(1);

        // Assert for token2 (;)
        token2.Type.Should().Be(TokenType.EndOfStatement);
        token2.Position.Line.Should().Be(1);
        token2.Position.Column.Should().Be(13);

        // Assert for token3 (loop)
        token3.Type.Should().Be(TokenType.Loop);
        token3.Position.Line.Should().Be(2);
        token3.Position.Column.Should().Be(1);

        // Assert for token4 (;)
        token4.Type.Should().Be(TokenType.EndOfStatement);
        token4.Position.Line.Should().Be(3);
        token4.Position.Column.Should().Be(1);

        // Assert for token5 (x)
        token5.Type.Should().Be(TokenType.Identifier);
        token5.Position.Line.Should().Be(3);
        token5.Position.Column.Should().Be(2);

        // Assert for token6 (=)
        token6.Type.Should().Be(TokenType.OperatorDeclaration);
        token6.Position.Line.Should().Be(3);
        token6.Position.Column.Should().Be(4);

        // Assert for token7 (10)
        token7.Type.Should().Be(TokenType.Integer);
        token7.Position.Line.Should().Be(3);
        token7.Position.Column.Should().Be(6);

        // Assert for token8 (;)
        token8.Type.Should().Be(TokenType.EndOfStatement);
        token8.Position.Line.Should().Be(3);
        token8.Position.Column.Should().Be(8);

        // Assert for token9 (comment)
        token9.Type.Should().Be(TokenType.Comment);
        token9.Position.Line.Should().Be(4);
        token9.Position.Column.Should().Be(1);
        
        // Assert for token10 (end)
        token10.Type.Should().Be(TokenType.EndOfText);
    }
    
#endregion
}
