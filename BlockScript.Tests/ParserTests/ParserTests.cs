using BlockScript.Exceptions;
using BlockScript.Lexer;
using BlockScript.Parser;
using BlockScript.Parser.Expressions;
using BlockScript.Parser.Factors;
using BlockScript.Parser.Statements;
using FluentAssertions;
using Xunit;

namespace BlockScript.Tests.ParserTests;

public class ParserTests
{
    #region General

    [Fact]
    public void Parser_ShouldThrow_WhenMissingEndOfStatement()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier },
            new TokenData { Type = TokenType.OperatorAssign },
            new TokenData { Type = TokenType.Integer }
            // missing EndOfStatement
        );

        // Act
        Action act = () => parser.ParserProgram();

        // Assert
        act.Should().Throw<TokenException>()
           .WithMessage("*Statements should be separated by*");
    }
    
    [Fact]
    public void Parser_ShouldThrow_WhenUnexpectedToken()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.OperatorAdd }
        );

        // Act
        Action act = () => parser.ParserProgram();

        // Assert
        act.Should().Throw<TokenException>()
           .WithMessage("*Expected*");
    }

    [Fact]
    public void Parser_ShouldSkipComments()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Comment, Value = "this is a comment" },
            new TokenData { Type = TokenType.Identifier, Value = "a" },
            new TokenData { Type = TokenType.OperatorAssign },
            new TokenData { Type = TokenType.Integer, Value = 1 },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        result.Statements.Should().ContainSingle().Which.Should().BeOfType<Assign>();
    }

    #endregion
    
    #region Blocks

    [Fact]
    public void Parser_ShouldReturnEmptyBlock_WhenNoTokens()
    {
        // Arrange
        var parser = CreateParserFromTokens();

        // Act
        var result = parser.ParserProgram();

        // Assert
        result.Statements.Should().BeEmpty();
    }
    
    [Fact]
    public void Parser_ShouldParseEmptyBock()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.BraceOpen },
            new TokenData { Type = TokenType.BraceClose },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        result.Statements.Should().ContainSingle()
              .Which.Should().BeOfType<Block>()
              .Which.Statements.Should().BeEmpty();
    }
    
    [Fact]
    public void Parser_ShouldParseNestedBlock()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.BraceOpen },
            new TokenData { Type = TokenType.Identifier, Value = "a" },
            new TokenData { Type = TokenType.OperatorAssign },
            new TokenData { Type = TokenType.Integer, Value = 5 },
            new TokenData { Type = TokenType.EndOfStatement },
            new TokenData { Type = TokenType.BraceClose },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var firstBlock = result.Statements.Should().ContainSingle()
              .Which.Should().BeOfType<Block>().Subject;

        var assignment = firstBlock.Statements.Should().ContainSingle()
                                   .Which.Should().BeOfType<Assign>().Subject;
        assignment.Identifier.Should().Be("a");
        assignment.Value.Should().BeOfType<ConstFactor>().Which.Value.Should().Be(5);
    }
    
    #endregion

    #region Expresions

    [Theory]
    [InlineData(TokenType.Integer, 42)]
    [InlineData(TokenType.Boolean, true)]
    [InlineData(TokenType.Boolean, false)]
    [InlineData(TokenType.String, "aa")]
    [InlineData(TokenType.Null, null)]
    public void Parser_ShouldParseConstFactor(TokenType tokenType, object value)
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = tokenType, Value = value },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        result.Statements.Should().ContainSingle()
        .Which.Should().BeOfType<ConstFactor>()
        .Which.Value.Should().Be(value);
    }

    #endregion

    #region Statements

    [Fact]
    public void Parser_ShouldParseIfElseCondition()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.If },
            new TokenData { Type = TokenType.Boolean, Value = true },
            new TokenData { Type = TokenType.Integer, Value = 1 },
            new TokenData { Type = TokenType.Else },
            new TokenData { Type = TokenType.Integer, Value = 2 },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        result.Statements.Should().ContainSingle()
            .Which.Should().BeOfType<Condition>()
            .Which.ElseBody.Should().NotBeNull();
    }

    [Fact]
    public void Parser_ShouldParseLambda()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.ParenhticesOpen },
            new TokenData { Type = TokenType.Identifier, Value = "x" },
            new TokenData { Type = TokenType.ParenhticesClose },
            new TokenData { Type = TokenType.OperatorArrow },
            new TokenData { Type = TokenType.Identifier, Value = "x" },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        result.Statements.Should().ContainSingle()
            .Which.Should().BeOfType<Lambda>();
    }

    [Fact]
    public void Parser_ShouldParseLoop()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Loop },
            new TokenData { Type = TokenType.Boolean, Value = true },
            new TokenData { Type = TokenType.Integer, Value = 123 },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        result.Statements.Should().ContainSingle()
            .Which.Should().BeOfType<Loop>();
    }

    #endregion
    
    
    
    private static LanguageParser CreateParserFromTokens(params TokenData[] tokens)
    {
        var queue = new Queue<TokenData>(tokens);
        return new LanguageParser(GetTokenData);

        TokenData GetTokenData()
        {
            if (queue.TryDequeue(out var tokenData))
            {
                return tokenData;
            }

            return new TokenData { Type = TokenType.EndOfText };
        }
    }
}