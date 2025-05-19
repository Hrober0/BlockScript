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
    
    [Fact]
    public void Parser_ShouldThrow_WhenStatementsAreNotSeparated()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = "a" },
            new TokenData { Type = TokenType.OperatorAssign },
            new TokenData { Type = TokenType.Integer, Value = 5 },
            new TokenData { Type = TokenType.Identifier, Value = "a" },
            new TokenData { Type = TokenType.OperatorAssign },
            new TokenData { Type = TokenType.Integer, Value = 5 },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var act = () => parser.ParserProgram();

        // Assert
        act.Should().Throw<TokenException>()
           .WithMessage("*Expected*");
    }

    #endregion
    
    #region Statements
    
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
    
    #region Assigment

    [Theory]
    [InlineData(TokenType.OperatorAssign)]
    [InlineData(TokenType.OperatorNullAssign)]
    public void Parser_ShouldParseAssigment(TokenType tokenType)
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = "true"},
            new TokenData { Type = tokenType},
            new TokenData { Type = TokenType.Boolean, Value = false },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var assigment = result.Statements.Should().ContainSingle()
                           .Which.Should().BeOfType<Assign>().Subject;
        assigment.Identifier.Should().Be("true");
        assigment.Value.Should().BeOfType<ConstFactor>().Which.Value.Should().Be(false);
    }
    
    [Fact]
    public void Parser_ShouldParseAssignment_WithExpressionValue()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = "x" },
            new TokenData { Type = TokenType.OperatorAssign },
            new TokenData { Type = TokenType.Integer, Value = 1 },
            new TokenData { Type = TokenType.OperatorAdd },
            new TokenData { Type = TokenType.Integer, Value = 2 },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var assignment = result.Statements.Should().ContainSingle()
                               .Which.Should().BeOfType<Assign>().Subject;

        assignment.Identifier.Should().Be("x");
        var expr = assignment.Value.Should().BeOfType<ArithmeticalExpression>().Subject;
        expr.Expressions.Should().HaveCount(2);
    }

    [Fact]
    public void Parser_ShouldParseAssignment_ToNull()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = "x" },
            new TokenData { Type = TokenType.OperatorAssign },
            new TokenData { Type = TokenType.Null },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var assignment = result.Statements.Should().ContainSingle()
                               .Which.Should().BeOfType<Assign>().Subject;

        assignment.Value.Should().BeOfType<ConstFactor>().Which.Value.Should().BeNull();
    }

    [Fact]
    public void Parser_ShouldThrow_WhenAssignmentValueMissing()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = "x" },
            new TokenData { Type = TokenType.OperatorAssign },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var act = () => parser.ParserProgram();
        
        // Assert
        act.Should().Throw<TokenException>()
            .WithMessage("*Expected statement*");
    }

    [Fact]
    public void Parser_ShouldThrow_WhenAssignmentMissingSemicolon()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = "x" },
            new TokenData { Type = TokenType.OperatorAssign },
            new TokenData { Type = TokenType.Integer, Value = 42 }
            // missing EndOfStatement
        );
        
        // Act
        var act = () => parser.ParserProgram();

        // Assert
        act.Should().Throw<TokenException>()
           .WithMessage("*Expected ';'*");
    }

    
    #endregion
    
    #region Lambda

    [Fact]
    public void Parser_ShouldParseLambda()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.ParenhticesOpen },
            new TokenData { Type = TokenType.Identifier, Value = "x" },
            new TokenData { Type = TokenType.ParenhticesClose },
            new TokenData { Type = TokenType.OperatorArrow },
            new TokenData { Type = TokenType.Integer, Value = 42 },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var lambda = result.Statements.Should().ContainSingle()
            .Which.Should().BeOfType<Lambda>().Subject;
        lambda.Arguments.Should().ContainSingle().Which.Should().Be( "x");
        lambda.Body.Should().BeOfType<ConstFactor>().Which.Value.Should().Be(42);
    }
    
    [Fact]
    public void Parser_ShouldParseLambda_WhenBodyIsBlock()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.ParenhticesOpen },
            new TokenData { Type = TokenType.Identifier, Value = "x" },
            new TokenData { Type = TokenType.ParenhticesClose },
            new TokenData { Type = TokenType.OperatorArrow },
            new TokenData { Type = TokenType.BraceOpen },
            new TokenData { Type = TokenType.BraceClose },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var lambda = result.Statements.Should().ContainSingle()
                           .Which.Should().BeOfType<Lambda>().Subject;
        lambda.Arguments.Should().ContainSingle().Which.Should().Be( "x");
        lambda.Body.Should().BeOfType<Block>().Which.Statements.Should().BeEmpty();
    }

    [Fact]
    public void Parser_ShouldParseLambda_WithMultipleArguments()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.ParenhticesOpen },
            new TokenData { Type = TokenType.Identifier, Value = "x" },
            new TokenData { Type = TokenType.Comma },
            new TokenData { Type = TokenType.Identifier, Value = "y" },
            new TokenData { Type = TokenType.ParenhticesClose },
            new TokenData { Type = TokenType.OperatorArrow },
            new TokenData { Type = TokenType.Identifier, Value = "z" },
            new TokenData { Type = TokenType.OperatorAdd },
            new TokenData { Type = TokenType.Identifier, Value = "k" },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var lambda = result.Statements.Should().ContainSingle()
            .Which.Should().BeOfType<Lambda>().Subject;
        lambda.Arguments.Should().BeEquivalentTo(["x", "y"]);

        var body = lambda.Body.Should().BeOfType<ArithmeticalExpression>().Subject;
        body.Expressions.Should().HaveCount(2);
        body.Expressions[0].Should().BeOfType<VariableFactor>().Which.Identifier.Should().Be("z");
        body.Expressions[1].Should().BeOfType<VariableFactor>().Which.Identifier.Should().Be("k");
        body.Operators.Should().ContainSingle().Which.Should().Be(TokenType.OperatorAdd);
    }

    [Fact]
    public void Parser_ShouldParseLambda_WithTrailingComma()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.ParenhticesOpen },
            new TokenData { Type = TokenType.Identifier, Value = "x" },
            new TokenData { Type = TokenType.Comma },
            new TokenData { Type = TokenType.ParenhticesClose },
            new TokenData { Type = TokenType.OperatorArrow },
            new TokenData { Type = TokenType.Identifier, Value = "y" },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var lambda = result.Statements.Should().ContainSingle()
            .Which.Should().BeOfType<Lambda>().Subject;
        lambda.Arguments.Should().ContainSingle().Which.Should().Be("x");

        var body = lambda.Body.Should().BeOfType<VariableFactor>()
            .Which.Identifier.Should().Be("y");
    }
    
    [Fact]
    public void Parser_ShouldThrow_WhenLambdaMissingBody()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.ParenhticesOpen },
            new TokenData { Type = TokenType.Identifier, Value = "x" },
            new TokenData { Type = TokenType.ParenhticesClose },
            new TokenData { Type = TokenType.OperatorArrow }
        );

        // Act
        var act = () => parser.ParserProgram();

        // Assert
        act.Should().Throw<TokenException>()
           .WithMessage("*expected statement*");
    }

    [Fact]
    public void Parser_ShouldThrow_WhenLambdaMissingArrow()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.ParenhticesOpen },
            new TokenData { Type = TokenType.Identifier, Value = "x" },
            new TokenData { Type = TokenType.ParenhticesClose },
            new TokenData { Type = TokenType.Integer, Value = 42 }
        );

        // Act
        var act = () => parser.ParserProgram();

        // Assert
        act.Should().Throw<TokenException>()
           .WithMessage("*expected '=>'*");
    }

    [Fact]
    public void Parser_ShouldThrow_WhenLambdaMissingParameterList()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.ParenhticesOpen },
            new TokenData { Type = TokenType.OperatorArrow },
            new TokenData { Type = TokenType.Integer, Value = 42 }
        );

        // Act
        var act = () => parser.ParserProgram();

        // Assert
        act.Should().Throw<TokenException>()
           .WithMessage("*expected ')'*");
    }

    #endregion
    
    #region Condition
    
    [Fact]
    public void Parser_ShouldParseCondition_WithIf()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.If },
            new TokenData { Type = TokenType.Boolean, Value = true },
            new TokenData { Type = TokenType.Integer, Value = 1 },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var condition = result.Statements.Should().ContainSingle()
                              .Which.Should().BeOfType<Condition>().Subject;
        condition.ConditionaryItems.Should().HaveCount(1);
        condition.ConditionaryItems[0].condition.Should().BeOfType<ConstFactor>().Which.Value.Should().Be(true);
        condition.ConditionaryItems[0].body.Should().BeOfType<ConstFactor>().Which.Value.Should().Be(1);
        condition.ElseBody.Should().BeNull();
    }
    
    [Fact]
    public void Parser_ShouldThrow_WhenNoIfBody()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.If },
            new TokenData { Type = TokenType.Boolean, Value = true }
        );

        // Act
        var act = () => parser.ParserProgram();

        // Assert
        act.Should().Throw<TokenException>()
           .WithMessage("*expected statement*");
    }
    
    [Fact]
    public void Parser_ShouldParseCondition_WithIfWithLongExpressions()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.If },
            new TokenData { Type = TokenType.Boolean, Value = true },
            new TokenData { Type = TokenType.OperatorSubtract },
            new TokenData { Type = TokenType.Integer, Value = 1 },
            new TokenData { Type = TokenType.Boolean, Value = false },
            new TokenData { Type = TokenType.OperatorDivide },
            new TokenData { Type = TokenType.Integer, Value = 2 },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var condition = result.Statements.Should().ContainSingle()
                              .Which.Should().BeOfType<Condition>().Subject;
        condition.ConditionaryItems.Should().HaveCount(1);
        condition.ElseBody.Should().BeNull();
        
        var conditionExpression = condition.ConditionaryItems[0].condition.Should().BeOfType<ArithmeticalExpression>().Subject;
        conditionExpression.Expressions.Should().HaveCount(2);
        conditionExpression.Expressions[0].Should().BeOfType<ConstFactor>().Which.Value.Should().Be(true);
        conditionExpression.Expressions[1].Should().BeOfType<ConstFactor>().Which.Value.Should().Be(1);
        conditionExpression.Operators.Should().ContainEquivalentOf(TokenType.OperatorSubtract);
        
        var conditionBody = condition.ConditionaryItems[0].body.Should().BeOfType<ArithmeticalExpression>().Subject;
        conditionBody.Expressions.Should().HaveCount(2);
        conditionBody.Expressions[0].Should().BeOfType<ConstFactor>().Which.Value.Should().Be(false);
        conditionBody.Expressions[1].Should().BeOfType<ConstFactor>().Which.Value.Should().Be(2);
        conditionBody.Operators.Should().ContainEquivalentOf(TokenType.OperatorDivide);
    }
    
    [Fact]
    public void Parser_ShouldParseCondition_WithIfElse()
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
        var condition = result.Statements.Should().ContainSingle()
            .Which.Should().BeOfType<Condition>().Subject;
        condition.ConditionaryItems.Should().HaveCount(1);
        condition.ConditionaryItems[0].condition.Should().BeOfType<ConstFactor>().Which.Value.Should().Be(true);
        condition.ConditionaryItems[0].body.Should().BeOfType<ConstFactor>().Which.Value.Should().Be(1);
        condition.ElseBody.Should().NotBeNull();
        condition.ElseBody.Should().BeOfType<ConstFactor>().Which.Value.Should().Be(2);
    }
    
    [Fact]
    public void Parser_ShouldParseCondition_WithIfElseIf()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.If },
            new TokenData { Type = TokenType.Boolean, Value = true },
            new TokenData { Type = TokenType.Integer, Value = 1 },
            new TokenData { Type = TokenType.Else },
            new TokenData { Type = TokenType.If },
            new TokenData { Type = TokenType.Boolean, Value = false },
            new TokenData { Type = TokenType.Integer, Value = 2 },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var condition = result.Statements.Should().ContainSingle()
                              .Which.Should().BeOfType<Condition>().Subject;
        condition.ConditionaryItems.Should().HaveCount(2);
        condition.ConditionaryItems[0].condition.Should().BeOfType<ConstFactor>().Which.Value.Should().Be(true);
        condition.ConditionaryItems[0].body.Should().BeOfType<ConstFactor>().Which.Value.Should().Be(1);
        condition.ConditionaryItems[1].condition.Should().BeOfType<ConstFactor>().Which.Value.Should().Be(false);
        condition.ConditionaryItems[1].body.Should().BeOfType<ConstFactor>().Which.Value.Should().Be(2);
        condition.ElseBody.Should().BeNull();
    }

    [Fact]
    public void Parser_ShouldParseCondition_WithIfAndMultipleElseIf()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.If },
            new TokenData { Type = TokenType.Boolean, Value = true },
            new TokenData { Type = TokenType.Integer, Value = 1 },
            new TokenData { Type = TokenType.Else },
            new TokenData { Type = TokenType.If },
            new TokenData { Type = TokenType.Boolean, Value = false },
            new TokenData { Type = TokenType.Integer, Value = 2 },
            new TokenData { Type = TokenType.Else },
            new TokenData { Type = TokenType.If },
            new TokenData { Type = TokenType.Integer, Value = 3 },
            new TokenData { Type = TokenType.String, Value = "4" },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var condition = result.Statements.Should().ContainSingle()
                              .Which.Should().BeOfType<Condition>().Subject;
        condition.ConditionaryItems.Should().HaveCount(3);
        condition.ConditionaryItems[0].condition.Should().BeOfType<ConstFactor>().Which.Value.Should().Be(true);
        condition.ConditionaryItems[0].body.Should().BeOfType<ConstFactor>().Which.Value.Should().Be(1);
        condition.ConditionaryItems[1].condition.Should().BeOfType<ConstFactor>().Which.Value.Should().Be(false);
        condition.ConditionaryItems[1].body.Should().BeOfType<ConstFactor>().Which.Value.Should().Be(2);
        condition.ConditionaryItems[2].condition.Should().BeOfType<ConstFactor>().Which.Value.Should().Be(3);
        condition.ConditionaryItems[2].body.Should().BeOfType<ConstFactor>().Which.Value.Should().Be("4");
        condition.ElseBody.Should().BeNull();
    }
    
    [Fact]
    public void Parser_ShouldThrow_WhenNoIfElseBody()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.If },
            new TokenData { Type = TokenType.Boolean, Value = true },
            new TokenData { Type = TokenType.Integer, Value = 1 },
            new TokenData { Type = TokenType.Else },
            new TokenData { Type = TokenType.If },
            new TokenData { Type = TokenType.Boolean, Value = false }
        );

        // Act
        var act = () => parser.ParserProgram();

        // Assert
        act.Should().Throw<TokenException>()
           .WithMessage("*expected statement*");
    }
    
    [Fact]
    public void Parser_ShouldParseCondition_WithIfElseIfElse()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.If },
            new TokenData { Type = TokenType.Boolean, Value = true },
            new TokenData { Type = TokenType.Integer, Value = 1 },
            new TokenData { Type = TokenType.Else },
            new TokenData { Type = TokenType.If },
            new TokenData { Type = TokenType.Boolean, Value = false },
            new TokenData { Type = TokenType.Integer, Value = 2 },
            new TokenData { Type = TokenType.Else },
            new TokenData { Type = TokenType.Integer, Value = 3 },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var condition = result.Statements.Should().ContainSingle()
                              .Which.Should().BeOfType<Condition>().Subject;
        condition.ConditionaryItems.Should().HaveCount(2);
        condition.ConditionaryItems[0].condition.Should().BeOfType<ConstFactor>().Which.Value.Should().Be(true);
        condition.ConditionaryItems[0].body.Should().BeOfType<ConstFactor>().Which.Value.Should().Be(1);
        condition.ConditionaryItems[1].condition.Should().BeOfType<ConstFactor>().Which.Value.Should().Be(false);
        condition.ConditionaryItems[1].body.Should().BeOfType<ConstFactor>().Which.Value.Should().Be(2);
        condition.ElseBody.Should().NotBeNull();
        condition.ElseBody.Should().BeOfType<ConstFactor>().Which.Value.Should().Be(3);
    }
    
    [Fact]
    public void Parser_ShouldThrow_WhenNoElseBody()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.If },
            new TokenData { Type = TokenType.Boolean, Value = true },
            new TokenData { Type = TokenType.Integer, Value = 1 },
            new TokenData { Type = TokenType.Else },
            new TokenData { Type = TokenType.If },
            new TokenData { Type = TokenType.Boolean, Value = false },
            new TokenData { Type = TokenType.Integer, Value = 2 },
            new TokenData { Type = TokenType.Else }
        );

        // Act
        var act = () => parser.ParserProgram();

        // Assert
        act.Should().Throw<TokenException>()
           .WithMessage("*expected statement*");
    }
    
    #endregion
    
    #region Loop

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

    [Fact]
    public void Parser_ShouldThrow_WithNoBody()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Loop },
            new TokenData { Type = TokenType.Identifier, Value = "cond" },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var act = () => parser.ParserProgram();

        // Assert
        act.Should().Throw<TokenException>()
           .WithMessage("*expected statement*");
    }

    [Fact]
    public void Parser_ShouldParseLoop_WithNestedLoopBody()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Loop },
            new TokenData { Type = TokenType.Boolean, Value = true },
            new TokenData { Type = TokenType.Loop },
            new TokenData { Type = TokenType.Boolean, Value = false },
            new TokenData { Type = TokenType.Integer, Value = 1 },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var outerLoop = result.Statements.Should().ContainSingle()
            .Which.Should().BeOfType<Loop>().Subject;

        outerLoop.Condition.Should().BeOfType<ConstFactor>()
            .Which.Value.Should().Be(true);

        var innerLoop = outerLoop.Body.Should().BeOfType<Loop>().Subject;

        innerLoop.Condition.Should().BeOfType<ConstFactor>()
            .Which.Value.Should().Be(false);

        innerLoop.Body.Should().BeOfType<ConstFactor>()
            .Which.Value.Should().Be(1);
    }
    
    #endregion

    #region Print

    [Fact]
    public void Parser_ShouldParsePrint()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Print },
            new TokenData { Type = TokenType.ParenhticesOpen },
            new TokenData { Type = TokenType.Integer, Value = 1 },
            new TokenData { Type = TokenType.ParenhticesClose },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var lambda = result.Statements.Should().ContainSingle()
                           .Which.Should().BeOfType<Print>().Subject;
        lambda.Body.Should().BeOfType<ConstFactor>().Which.Value.Should().Be(1);
    }
    
    [Fact]
    public void Parser_ShouldParsePrint_WithComplexExpression()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Print },
            new TokenData { Type = TokenType.ParenhticesOpen },
            new TokenData { Type = TokenType.Integer, Value = 1 },
            new TokenData { Type = TokenType.OperatorAdd },
            new TokenData { Type = TokenType.Integer, Value = 2 },
            new TokenData { Type = TokenType.ParenhticesClose },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var print = result.Statements.Should().ContainSingle()
                          .Which.Should().BeOfType<Print>().Subject;

        var expr = print.Body.Should().BeOfType<ArithmeticalExpression>().Subject;
        expr.Expressions.Should().HaveCount(2);
    }

    [Fact]
    public void Parser_ShouldThrow_WhenPrintMissingOpeningParen()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Print },
            new TokenData { Type = TokenType.Integer, Value = 1 },
            new TokenData { Type = TokenType.ParenhticesClose },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var act = () => parser.ParserProgram();

        // Assert
        act.Should().Throw<TokenException>()
           .WithMessage("*Expected '('*");
    }

    [Fact]
    public void Parser_ShouldThrow_WhenPrintMissingClosingParen()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Print },
            new TokenData { Type = TokenType.ParenhticesOpen },
            new TokenData { Type = TokenType.Integer, Value = 1 },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var act = () => parser.ParserProgram();

        // Assert
        act.Should().Throw<TokenException>()
           .WithMessage("*Expected ')'*");
    }
    
    [Fact]
    public void Parser_ShouldThrow_WhenPrintHasNoExpression()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Print },
            new TokenData { Type = TokenType.ParenhticesOpen },
            new TokenData { Type = TokenType.ParenhticesClose },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var act = () => parser.ParserProgram();

        // Assert
        act.Should().Throw<TokenException>()
           .WithMessage("*expected statement*");
    }


    #endregion
    
    #region FunctionCall

    [Fact]
    public void Parser_ShouldParseFunctionCall()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = "x" },
            new TokenData { Type = TokenType.ParenhticesOpen },
            new TokenData { Type = TokenType.ParenhticesClose },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var functionCall = result.Statements.Should().ContainSingle()
              .Which.Should().BeOfType<FunctionCall>().Subject;
        
        functionCall.Identifier.Should().Be("x");
        functionCall.Arguments.Should().BeEmpty();
    }

    [Fact]
    public void Parser_ShouldParseFunctionCall_WithOneArgument()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = "foo" },
            new TokenData { Type = TokenType.ParenhticesOpen },
            new TokenData { Type = TokenType.Integer, Value = 1 },
            new TokenData { Type = TokenType.ParenhticesClose },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var functionCall = result.Statements.Should().ContainSingle()
            .Which.Should().BeOfType<FunctionCall>().Subject;

        functionCall.Identifier.Should().Be("foo");
        functionCall.Arguments.Should().ContainSingle()
            .Which.Should().BeOfType<ConstFactor>()
            .Which.Value.Should().Be(1);
    }

    [Fact]
    public void Parser_ShouldParseFunctionCall_WithMultipleArguments()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = "sum" },
            new TokenData { Type = TokenType.ParenhticesOpen },
            new TokenData { Type = TokenType.Integer, Value = 1 },
            new TokenData { Type = TokenType.Comma },
            new TokenData { Type = TokenType.Integer, Value = 2 },
            new TokenData { Type = TokenType.Comma },
            new TokenData { Type = TokenType.Integer, Value = 3 },
            new TokenData { Type = TokenType.ParenhticesClose },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var functionCall = result.Statements.Should().ContainSingle()
            .Which.Should().BeOfType<FunctionCall>().Subject;

        functionCall.Identifier.Should().Be("sum");
        functionCall.Arguments.Should().HaveCount(3);
        functionCall.Arguments[0].Should().BeOfType<ConstFactor>().Which.Value.Should().Be(1);
        functionCall.Arguments[1].Should().BeOfType<ConstFactor>().Which.Value.Should().Be(2);
        functionCall.Arguments[2].Should().BeOfType<ConstFactor>().Which.Value.Should().Be(3);
    }
    
    [Fact]
    public void Parser_ShouldParseFunctionCall_WithTrailingComma()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = "sum" },
            new TokenData { Type = TokenType.ParenhticesOpen },
            new TokenData { Type = TokenType.Integer, Value = 1 },
            new TokenData { Type = TokenType.Comma },
            new TokenData { Type = TokenType.ParenhticesClose },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var functionCall = result.Statements.Should().ContainSingle()
            .Which.Should().BeOfType<FunctionCall>().Subject;

        functionCall.Identifier.Should().Be("sum");
        functionCall.Arguments.Should().ContainSingle().
            Which.Should().BeOfType<ConstFactor>().Which.Value.Should().Be(1);
    }
    
    [Fact]
    public void Parser_ShouldThrow_WhenArgumentsAreNotSeparatedByCommas()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = "sum" },
            new TokenData { Type = TokenType.ParenhticesOpen },
            new TokenData { Type = TokenType.Integer, Value = 1 },
            new TokenData { Type = TokenType.Integer, Value = 2 },
            new TokenData { Type = TokenType.ParenhticesClose },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var act = () => parser.ParserProgram();
        
        // Assert
        act.Should().Throw<TokenException>()
           .WithMessage("*expected ')'*");
    }
    
    [Fact]
    public void Parser_ShouldThrow_WhenFunctionCall_MissingClosingParenthesis()
    {
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = "foo" },
            new TokenData { Type = TokenType.ParenhticesOpen },
            new TokenData { Type = TokenType.Integer, Value = 1 },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        Action act = () => parser.ParserProgram();

        act.Should().Throw<TokenException>()
            .WithMessage("*')'*");
    }

    [Fact]
    public void Parser_ShouldThrow_WhenFunctionCall_MissingOpeningParenthesis()
    {
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = "foo" },
            new TokenData { Type = TokenType.Integer, Value = 1 },
            new TokenData { Type = TokenType.ParenhticesClose },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        Action act = () => parser.ParserProgram();

        act.Should().Throw<TokenException>()
            .WithMessage("*Expected ';'*");
    }

    [Fact]
    public void Parser_ShouldThrow_WhenFunctionCall_EmptyCommasBetweenArguments()
    {
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = "foo" },
            new TokenData { Type = TokenType.ParenhticesOpen },
            new TokenData { Type = TokenType.Comma },
            new TokenData { Type = TokenType.Comma },
            new TokenData { Type = TokenType.ParenhticesClose },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        Action act = () => parser.ParserProgram();

        act.Should().Throw<TokenException>()
            .WithMessage("*Expected ')'*");
    }
    
    [Fact]
    public void Parser_ShouldParseFunctionCall_WithExpressionArgument()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = "negate" },
            new TokenData { Type = TokenType.ParenhticesOpen },
            new TokenData { Type = TokenType.Integer, Value = 69, Position = new(1,69) },
            new TokenData { Type = TokenType.OperatorAdd },
            new TokenData { Type = TokenType.Integer, Value = 42, Position = new(1,42) },
            new TokenData { Type = TokenType.ParenhticesClose },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var functionCall = result.Statements.Should().ContainSingle()
            .Which.Should().BeOfType<FunctionCall>().Subject;

        functionCall.Identifier.Should().Be("negate");
        functionCall.Arguments.Should().ContainSingle();

        var expr = functionCall.Arguments[0].Should().BeOfType<ArithmeticalExpression>().Subject;
        expr.Operators.Should().ContainEquivalentOf(TokenType.OperatorAdd);
        expr.Expressions.Should().BeEquivalentTo([new ConstFactor(69, new(1, 69)), new ConstFactor(42, new(1, 42))]);
    }

    [Fact]
    public void Parser_ShouldParseNestedFunctionCalls_AsArguments()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = "ff" },
            new TokenData { Type = TokenType.ParenhticesOpen },
            new TokenData { Type = TokenType.Identifier, Value = "getValue" },
            new TokenData { Type = TokenType.ParenhticesOpen },
            new TokenData { Type = TokenType.Integer, Value = 7 },
            new TokenData { Type = TokenType.ParenhticesClose },
            new TokenData { Type = TokenType.ParenhticesClose },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var printCall = result.Statements.Should().ContainSingle()
            .Which.Should().BeOfType<FunctionCall>().Subject;

        printCall.Identifier.Should().Be("ff");
        printCall.Arguments.Should().ContainSingle();

        var innerCall = printCall.Arguments[0].Should().BeOfType<FunctionCall>().Subject;
        innerCall.Identifier.Should().Be("getValue");
        innerCall.Arguments.Should().ContainSingle()
            .Which.Should().BeOfType<ConstFactor>()
            .Which.Value.Should().Be(7);
    }

    #endregion
    
    #endregion
    
    #region Expressions

    [Theory]
    [InlineData(TokenType.OperatorMultiply)]
    [InlineData(TokenType.OperatorDivide)]
    [InlineData(TokenType.OperatorAdd)]
    [InlineData(TokenType.OperatorSubtract)]
    public void Parser_ShouldParseArithmeticalExpression(TokenType tokenType)
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Integer, Value = 1, Position = new(1,1)},
            new TokenData { Type = tokenType },
            new TokenData { Type = TokenType.Integer, Value = 2, Position = new(1,2) },
            new TokenData { Type = tokenType },
            new TokenData { Type = TokenType.Integer, Value = 3, Position = new(1,3) },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var expression = result.Statements.Should().ContainSingle()
              .Which.Should().BeOfType<ArithmeticalExpression>().Subject;
        expression.Expressions.Should().BeEquivalentTo([
            new ConstFactor(1, new(1,1)),
            new ConstFactor(2, new (1, 2)),
            new ConstFactor(3, new (1, 3))
        ]);
        expression.Operators.Should().BeEquivalentTo([tokenType, tokenType]);
    }
    
    [Theory]
    [InlineData(TokenType.OperatorEqual)]
    [InlineData(TokenType.OperatorNotEqual)]
    [InlineData(TokenType.OperatorGreater)]
    [InlineData(TokenType.OperatorGreaterEqual)]
    [InlineData(TokenType.OperatorLess)]
    [InlineData(TokenType.OperatorLessEqual)]
    public void Parser_ShouldParseCompereExpression(TokenType tokenType)
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Integer, Value = 1 },
            new TokenData { Type = tokenType },
            new TokenData { Type = TokenType.Integer, Value = 2 },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var expression = result.Statements.Should().ContainSingle()
                               .Which.Should().BeOfType<CompereExpression>().Subject;
        expression.LeftExpression.Should().BeOfType<ConstFactor>().Which.Value.Should().Be(1);
        expression.RightExpression.Should().BeOfType<ConstFactor>().Which.Value.Should().Be(2);
        expression.Operator.Should().Be(tokenType);
    }
    
    [Theory]
    [InlineData(TokenType.OperatorAnd)]
    [InlineData(TokenType.OperatorOr)]
    public void Parser_ShouldParseLogicalExpression(TokenType tokenType)
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Integer, Value = 1, Position = new(1, 1)},
            new TokenData { Type = tokenType },
            new TokenData { Type = TokenType.Integer, Value = 2, Position = new(1, 2) },
            new TokenData { Type = tokenType },
            new TokenData { Type = TokenType.Integer, Value = 3, Position = new(1, 3) },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var expression = result.Statements.Should().ContainSingle()
                               .Which.Should().BeOfType<LogicExpression>().Subject;
        expression.Expressions.Should().BeEquivalentTo([
            new ConstFactor(1, new(1,1)),
            new ConstFactor(2, new (1, 2)),
            new ConstFactor(3, new (1, 3))
        ]);
        expression.Operator.Should().Be(tokenType);
    }
    
    [Fact]
    public void Parser_ShouldParseLogicalExpressionInOrder()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Integer, Value = 1 },
            new TokenData { Type = TokenType.OperatorOr },
            new TokenData { Type = TokenType.Integer, Value = 2 },
            new TokenData { Type = TokenType.OperatorOr },
            
            new TokenData { Type = TokenType.Integer, Value = 3 },
            new TokenData { Type = TokenType.OperatorAnd },
            new TokenData { Type = TokenType.Integer, Value = 4 },
            new TokenData { Type = TokenType.OperatorAnd },
            new TokenData { Type = TokenType.Integer, Value = 5 },
            
            new TokenData { Type = TokenType.OperatorOr },
            new TokenData { Type = TokenType.Integer, Value = 6 },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var orExpression = result.Statements.Should().ContainSingle()
                               .Which.Should().BeOfType<LogicExpression>().Subject;
        orExpression.Operator.Should().Be(TokenType.OperatorOr);
        orExpression.Expressions.Should().HaveCount(4);
        orExpression.Expressions[0].Should().BeOfType<ConstFactor>().Which.Value.Should().Be(1);
        orExpression.Expressions[1].Should().BeOfType<ConstFactor>().Which.Value.Should().Be(2);
        
        var andExpression = orExpression.Expressions[2].Should().BeOfType<LogicExpression>().Subject;
        andExpression.Operator.Should().Be(TokenType.OperatorAnd);
        andExpression.Expressions.Should().HaveCount(3);
        andExpression.Expressions[0].Should().BeOfType<ConstFactor>().Which.Value.Should().Be(3);
        andExpression.Expressions[1].Should().BeOfType<ConstFactor>().Which.Value.Should().Be(4);
        andExpression.Expressions[2].Should().BeOfType<ConstFactor>().Which.Value.Should().Be(5);
        
        orExpression.Expressions[3].Should().BeOfType<ConstFactor>().Which.Value.Should().Be(6);
    }
    
    [Fact]
    public void Parser_ShouldParseArithmeticalExpression_FromManyAddEntries()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Integer, Value = 1 },
            new TokenData { Type = TokenType.OperatorAdd },
            new TokenData { Type = TokenType.String, Value = "2" },
            new TokenData { Type = TokenType.OperatorSubtract },
            new TokenData { Type = TokenType.Boolean, Value = false },
            new TokenData { Type = TokenType.OperatorAdd },
            new TokenData { Type = TokenType.Null },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var expression = result.Statements.Should().ContainSingle()
                               .Which.Should().BeOfType<ArithmeticalExpression>().Subject;
        expression.Expressions.Should().HaveCount(4);
        expression.Expressions[0].Should().BeOfType<ConstFactor>().Which.Value.Should().Be(1);
        expression.Expressions[1].Should().BeOfType<ConstFactor>().Which.Value.Should().Be("2");
        expression.Expressions[2].Should().BeOfType<ConstFactor>().Which.Value.Should().Be(false);
        expression.Expressions[3].Should().BeOfType<ConstFactor>().Which.Value.Should().Be(null);
        expression.Operators.Should().Equal(
            TokenType.OperatorAdd,
            TokenType.OperatorSubtract,
            TokenType.OperatorAdd
        );
    }
    
    [Fact]
    public void Parser_ShouldParseUnaryMinus()
    {
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.OperatorSubtract },
            new TokenData { Type = TokenType.Integer, Value = 42 },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        var result = parser.ParserProgram();

        var expr = result.Statements.Should().ContainSingle().Which
                         .Should().BeOfType<NotExpression>().Subject;

        expr.Factor.Should().BeOfType<ConstFactor>().Which.Value.Should().Be(42);
    }

    [Fact]
    public void Parser_ShouldParseNullCoalescingExpression()
    {
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = "a" },
            new TokenData { Type = TokenType.OperatorNullCoalescing },
            new TokenData { Type = TokenType.Identifier, Value = "b" },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        var result = parser.ParserProgram();

        var expr = result.Statements.Should().ContainSingle().Which
                         .Should().BeOfType<NullCoalescingExpression>().Subject;

        expr.Expressions.Should().HaveCount(2);
    }

    [Fact]
    public void Parser_ShouldParseParenthesizedExpression()
    {
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = "a" },
            new TokenData { Type = TokenType.OperatorMultiply },
            new TokenData { Type = TokenType.BraceOpen },
            new TokenData { Type = TokenType.Integer, Value = 1 },
            new TokenData { Type = TokenType.OperatorAdd },
            new TokenData { Type = TokenType.Integer, Value = 2 },
            new TokenData { Type = TokenType.BraceClose },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        var result = parser.ParserProgram();
        
        var multiplyExpression = result.Statements.Should().ContainSingle().Which.Should().BeOfType<ArithmeticalExpression>().Subject;
        multiplyExpression.Operators.Should().ContainEquivalentOf(TokenType.OperatorMultiply);
        multiplyExpression.Expressions.Should().HaveCount(2);
        multiplyExpression.Expressions[0].Should().BeOfType<VariableFactor>().Which.Identifier.Should().Be("a");
        
        var block = multiplyExpression.Expressions[1].Should().BeOfType<Block>().Subject;
        
        var addExpression = block.Statements.Should().ContainSingle().Which.Should().BeOfType<ArithmeticalExpression>().Subject;
        addExpression.Operators.Should().ContainEquivalentOf(TokenType.OperatorAdd);
        addExpression.Expressions.Should().HaveCount(2);
        addExpression.Expressions[0].Should().BeOfType<ConstFactor>().Which.Value.Should().Be(1);
        addExpression.Expressions[1].Should().BeOfType<ConstFactor>().Which.Value.Should().Be(2);
    }

    [Fact]
    public void Parser_ShouldParse_ComplexExpressionWithPrecedence()
    {
        // -2 + 3 * 4 > 5 || { if 6 > 7 { 8 } } ?? a;
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.OperatorSubtract },
            new TokenData { Type = TokenType.Integer, Value = 2 },
            new TokenData { Type = TokenType.OperatorAdd },
            new TokenData { Type = TokenType.Integer, Value = 3 },
            new TokenData { Type = TokenType.OperatorMultiply },
            new TokenData { Type = TokenType.Integer, Value = 4 },
            new TokenData { Type = TokenType.OperatorGreater },
            new TokenData { Type = TokenType.Integer, Value = 5 },
            new TokenData { Type = TokenType.OperatorOr },
            new TokenData { Type = TokenType.BraceOpen },
            new TokenData { Type = TokenType.If },
            new TokenData { Type = TokenType.Integer, Value = 6 },
            new TokenData { Type = TokenType.OperatorGreater },
            new TokenData { Type = TokenType.Integer, Value = 7 },
            new TokenData { Type = TokenType.BraceOpen },
            new TokenData { Type = TokenType.Integer, Value = 8 },
            new TokenData { Type = TokenType.BraceClose },
            new TokenData { Type = TokenType.BraceClose },
            new TokenData { Type = TokenType.OperatorNullCoalescing },
            new TokenData { Type = TokenType.Identifier, Value = "a" },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var logicExpr = result.Statements.Should().ContainSingle().Which
            .Should().BeOfType<LogicExpression>().Subject;

        // LogicLeft || LogicRight | (-2 + 3 * 4 > 5) || ({ if 6 > 7 { 8 } } ?? a);
        logicExpr.Operator.Should().Be(TokenType.OperatorOr);
        logicExpr.Expressions.Should().HaveCount(2);
        var logicLeft = logicExpr.Expressions[0].Should().BeOfType<CompereExpression>().Subject;
        var logicRight = logicExpr.Expressions[1].Should().BeOfType<NullCoalescingExpression>().Subject;

        // LogicLeft: CompLeft > 5 | (-2 + 3 * 4) > (5)
        logicLeft.Operator.Should().Be(TokenType.OperatorGreater);
        var compLeft = logicLeft.LeftExpression.Should().BeOfType<ArithmeticalExpression>().Subject;
        logicLeft.RightExpression.Should().BeOfType<ConstFactor>().Which.Value.Should().Be(5);

        // CompLeft: -2 + ArithmeticRight | (-2) + (3 * 4)
        compLeft.Operators.Should().ContainInOrder(TokenType.OperatorAdd);
        compLeft.Expressions.Should().HaveCount(2);
        compLeft.Expressions[0].Should().BeOfType<NotExpression>().Which.Factor
            .Should().BeOfType<ConstFactor>().Which.Value.Should().Be(2);
        var arithmeticRight = compLeft.Expressions[1].Should().BeOfType<ArithmeticalExpression>().Subject;
        
        // ArithmeticRight: 3 * 4
        arithmeticRight.Operators.Should().ContainInOrder(TokenType.OperatorMultiply);
        arithmeticRight.Expressions.Should().HaveCount(2);
        arithmeticRight.Expressions[0].Should().BeOfType<ConstFactor>().Which.Value.Should().Be(3);
        arithmeticRight.Expressions[1].Should().BeOfType<ConstFactor>().Which.Value.Should().Be(4);
        
        // LogicRight: NullCoalRight ?? a | ({ if 6 > 7 { 8 } }) ?? (a)
        logicRight.Expressions.Should().HaveCount(2);
        var nullCoalRight = logicRight.Expressions[0].Should().BeOfType<Block>()
                                      .Which.Statements.Should().ContainSingle()
                                      .Which.Should().BeOfType<Condition>().Subject;
        logicRight.Expressions[1].Should().BeOfType<VariableFactor>().Which.Identifier.Should().Be("a");
        
        // NullCoalRight: condition | if 6 > 7 { 8 }
        nullCoalRight.ConditionaryItems.Should().ContainSingle()
                     .Which.body.Should().BeOfType<Block>()
                     .Which.Statements.Should().ContainSingle()
                     .Which.Should().BeOfType<ConstFactor>().Which.Value.Should().Be(8);
        nullCoalRight.ElseBody.Should().BeNull();
    }
    
    #endregion

    #region Factors

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
    
    [Fact]
    public void Parser_ShouldParseVariableFactor()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = "variable" },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        result.Statements.Should().ContainSingle()
              .Which.Should().BeOfType<VariableFactor>()
              .Which.Identifier.Should().Be("variable");
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