using BlockScript.Lexer;
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
    public void GetToken_ShouldReturnCommentToken_WhenCommentIsEncountered()
    {
        // Arrange
        var text = "#This is a comment";
        var reader = new StringReader(text);
        var lexer = new Lexer.Lexer(reader);

        // Act
        var token = lexer.GetToken();

        // Assert
        token.Type.Should().Be(TokenType.Comment);
        token.Value.Should().Be("This is a comment");
    }

    [Fact]
    public void GetToken_ShouldReturnIntegerToken_WhenIntegerIsEncountered()
    {
        // Arrange
        var text = "1234";
        var reader = new StringReader(text);
        var lexer = new Lexer.Lexer(reader);

        // Act
        var token = lexer.GetToken();

        // Assert
        token.Type.Should().Be(TokenType.Integer);
        token.Value.Should().Be(1234);
    }

    [Fact]
    public void GetToken_ShouldReturnStringToken_WhenStringIsEncountered()
    {
        // Arrange
        var text = "\"Hello, World!\"";
        var reader = new StringReader(text);
        var lexer = new Lexer.Lexer(reader);

        // Act
        var token = lexer.GetToken();

        // Assert
        token.Type.Should().Be(TokenType.String);
        token.Value.Should().Be("Hello, World!");
    }

    [Fact]
    public void GetToken_ShouldReturnNullToken_WhenNullIsEncountered()
    {
        // Arrange
        var text = "null";
        var reader = new StringReader(text);
        var lexer = new Lexer.Lexer(reader);

        // Act
        var token = lexer.GetToken();

        // Assert
        token.Type.Should().Be(TokenType.Null);
        token.Value.Should().Be("null");
    }

    [Fact]
    public void GetToken_ShouldReturnValidOperatorTokens()
    {
        // Arrange
        var text = "== <= >= != + ===";
        var reader = new StringReader(text);
        var lexer = new Lexer.Lexer(reader);

        // Act & Assert
        lexer.GetToken().Type.Should().Be(TokenType.Operator);
        lexer.GetToken().Type.Should().Be(TokenType.Operator);
        lexer.GetToken().Type.Should().Be(TokenType.Operator);
        lexer.GetToken().Type.Should().Be(TokenType.Operator);
        lexer.GetToken().Type.Should().Be(TokenType.Operator);
        lexer.GetToken().Value.Should().Be("==");
        lexer.GetToken().Value.Should().Be("=");
        lexer.GetToken().Type.Should().Be(TokenType.EndOfText);
    }

    [Fact]
    public void GetToken_ShouldReturnLoopKeywordToken_WhenLoopKeywordIsEncountered()
    {
        // Arrange
        var text = "loop";
        var reader = new StringReader(text);
        var lexer = new Lexer.Lexer(reader);

        // Act
        var token = lexer.GetToken();

        // Assert
        token.Type.Should().Be(TokenType.Loop);
        token.Value.Should().Be("loop");
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
        token.Value.Should().Be("loop");
    }
    
    [Fact]
    public void GetToken_ShouldThrowTokenException_WhenParserThrowException()
    {
        // Arrange
        var text = "";
        for (int i = 0; i < 256; i++)
        {
            text += "i";
        }
        var reader = new StringReader(text); // to long identifier
        var lexer = new Lexer.Lexer(reader);

        // Act
        Action act = () => lexer.GetToken();

        // Assert
        act.Should().Throw<TokenException>()
           .WithMessage("*Invalid token*")
           .Where(e => e.Message.Contains("exceeds")); // assuming error message includes a hint
    }
    
    #region TokenClosing
    
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

    [Fact]
    public void GetToken_ShouldNotThrowException_WhenWordIsNotFullyParsedBeforeNewLine()
    {
        // Arrange
        var text = "looping\n"; // Incomplete word 'looping'
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
        token.Line.Should().Be(1);
        token.Column.Should().Be(1);
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
        token.Line.Should().Be(1);
        token.Column.Should().Be(1);
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
        token.Line.Should().Be(1);
        token.Column.Should().Be(1);
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
        token.Line.Should().Be(1);
        token.Column.Should().Be(1);
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

        // Assert for token1 (variableName)
        token1.Should().NotBeNull();
        token1.Type.Should().Be(TokenType.Identifier);
        token1.Line.Should().Be(1);
        token1.Column.Should().Be(1);

        // Assert for token2 (;)
        token2.Should().NotBeNull();
        token2.Type.Should().Be(TokenType.EndOfStatement);
        token2.Line.Should().Be(1);
        token2.Column.Should().Be(13);

        // Assert for token3 (loop)
        token3.Should().NotBeNull();
        token3.Type.Should().Be(TokenType.Loop);
        token3.Line.Should().Be(2);
        token3.Column.Should().Be(1);

        // Assert for token4 (;)
        token4.Should().NotBeNull();
        token4.Type.Should().Be(TokenType.EndOfStatement);
        token4.Line.Should().Be(3);
        token4.Column.Should().Be(1);

        // Assert for token5 (x)
        token5.Should().NotBeNull();
        token5.Type.Should().Be(TokenType.Identifier);
        token5.Line.Should().Be(3);
        token5.Column.Should().Be(2);

        // Assert for token6 (=)
        token6.Should().NotBeNull();
        token6.Type.Should().Be(TokenType.Operator);
        token6.Line.Should().Be(3);
        token6.Column.Should().Be(4);

        // Assert for token7 (10)
        token7.Should().NotBeNull();
        token7.Type.Should().Be(TokenType.Integer);
        token7.Line.Should().Be(3);
        token7.Column.Should().Be(6);

        // Assert for token8 (;)
        token8.Should().NotBeNull();
        token8.Type.Should().Be(TokenType.EndOfStatement);
        token8.Line.Should().Be(3);
        token8.Column.Should().Be(8);

        // Assert for token9 (comment)
        token9.Should().NotBeNull();
        token9.Type.Should().Be(TokenType.Comment);
        token9.Line.Should().Be(4);
        token9.Column.Should().Be(1);
    }
    
#endregion
}
