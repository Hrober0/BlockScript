using System.Text;
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
        token1.Value.Should().Be("This is a comment");
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
        token1.Value.Should().Be(2147483647);
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
        var token3 = lexer.GetToken();
        var token4 = lexer.GetToken();

        // Assert
        token1.Type.Should().Be(TokenType.Integer);
        token1.Value.Should().Be(0);
        token2.Type.Should().Be(TokenType.Integer);
        token2.Value.Should().Be(0);
        token3.Type.Should().Be(TokenType.Integer);
        token3.Value.Should().Be(10);
        token4.Type.Should().Be(TokenType.EndOfText);
    }

    [Fact]
    public void GetToken_ShouldThrow_WhenIntegerIsExedsLimit()
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
        token1.Value.Should().Be("Hello, World!");
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
        token.Value.Should().Be("");
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
        token1.Value.Should().Be("Hello, World!");
        token2.Type.Should().Be(TokenType.String);
        token2.Value.Should().Be("a");
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
        token.Value.Should().Be("[\\\"]");
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
    
    #endregion
    
    #region keywords and operators
    
    [Fact]
    public void GetToken_ShouldReturnNullToken_WhenNullIsEncountered()
    {
        // Arrange
        var text = "null;";
        var reader = new StringReader(text);
        var lexer = new Lexer.Lexer(reader);

        // Act
        var token1 = lexer.GetToken();
        var token2 = lexer.GetToken();
        var token3 = lexer.GetToken();

        // Assert
        token1.Type.Should().Be(TokenType.Null);
        token1.Value.Should().Be("null");
        
        token2.Type.Should().Be(TokenType.EndOfStatement);
        token2.Value.Should().Be(";");
        
        token3.Type.Should().Be(TokenType.EndOfText);
    }

    [Fact]
    public void GetToken_ShouldReturnValidOperatorTokens()
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
        token.Value.Should().Be("loop");
        token.Type.Should().Be(TokenType.Loop);
    }

    #endregion
    
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
        var token10 = lexer.GetToken(); // End

        // Assert for token1 (variableName)
        token1.Type.Should().Be(TokenType.Identifier);
        token1.Line.Should().Be(1);
        token1.Column.Should().Be(1);

        // Assert for token2 (;)
        token2.Type.Should().Be(TokenType.EndOfStatement);
        token2.Line.Should().Be(1);
        token2.Column.Should().Be(13);

        // Assert for token3 (loop)
        token3.Type.Should().Be(TokenType.Loop);
        token3.Line.Should().Be(2);
        token3.Column.Should().Be(1);

        // Assert for token4 (;)
        token4.Type.Should().Be(TokenType.EndOfStatement);
        token4.Line.Should().Be(3);
        token4.Column.Should().Be(1);

        // Assert for token5 (x)
        token5.Type.Should().Be(TokenType.Identifier);
        token5.Line.Should().Be(3);
        token5.Column.Should().Be(2);

        // Assert for token6 (=)
        token6.Type.Should().Be(TokenType.OperatorAssign);
        token6.Line.Should().Be(3);
        token6.Column.Should().Be(4);

        // Assert for token7 (10)
        token7.Type.Should().Be(TokenType.Integer);
        token7.Line.Should().Be(3);
        token7.Column.Should().Be(6);

        // Assert for token8 (;)
        token8.Type.Should().Be(TokenType.EndOfStatement);
        token8.Line.Should().Be(3);
        token8.Column.Should().Be(8);

        // Assert for token9 (comment)
        token9.Type.Should().Be(TokenType.Comment);
        token9.Line.Should().Be(4);
        token9.Column.Should().Be(1);
        
        // Assert for token10 (end)
        token10.Type.Should().Be(TokenType.EndOfText);
    }
    
#endregion
}
