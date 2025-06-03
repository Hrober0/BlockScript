using BlockScript.Exceptions;
using BlockScript.Lexer;
using BlockScript.Parser;
using BlockScript.Lexer.FactorValues;
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
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("a") },
            new TokenData { Type = TokenType.OperatorAssign },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(1) }
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
            new TokenData { Type = TokenType.Comment, Value = new StringFactor("this is a comment") },
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("a") },
            new TokenData { Type = TokenType.OperatorAssign },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(1)},
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
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("a")},
            new TokenData { Type = TokenType.OperatorAssign },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(5)},
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("a")},
            new TokenData { Type = TokenType.OperatorAssign },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(5)},
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
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("a")},
            new TokenData { Type = TokenType.OperatorAssign },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(5)},
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
        assignment.Value.ShouldBeConstFactor(5);
    }
    
    #endregion
    
    #region Assigment

    [Fact]
    public void Parser_ShouldParseAssigment()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("true")},
            new TokenData { Type = TokenType.OperatorAssign},
            new TokenData { Type = TokenType.Boolean, Value = new BoolFactor(false) },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var assigment = result.Statements.Should().ContainSingle()
                           .Which.Should().BeOfType<Assign>().Subject;
        assigment.Identifier.Should().Be("true");
        assigment.Value.ShouldBeConstFactor(false);
    }
    
    [Fact]
    public void Parser_ShouldParseAssignment_WithExpressionValue()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("x") },
            new TokenData { Type = TokenType.OperatorAssign },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(1)},
            new TokenData { Type = TokenType.OperatorAdd },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(2)},
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var assignment = result.Statements.Should().ContainSingle()
                               .Which.Should().BeOfType<Assign>().Subject;

        assignment.Identifier.Should().Be("x");
        assignment.Value.Should().BeOfType<ArithmeticalAddExpression>();
    }
    
    [Fact]
    public void Parser_ShouldParseNullAssigment()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("true")},
            new TokenData { Type = TokenType.OperatorNullAssign},
            new TokenData { Type = TokenType.Boolean, Value = new BoolFactor(false) },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var assigment = result.Statements.Should().ContainSingle()
                              .Which.Should().BeOfType<NullAssign>().Subject;
        assigment.Identifier.Should().Be("true");
        assigment.Value.ShouldBeConstFactor(false);
    }
    
    [Fact]
    public void Parser_ShouldParseNullAssignment_WithExpressionValue()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("x") },
            new TokenData { Type = TokenType.OperatorNullAssign },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(1)},
            new TokenData { Type = TokenType.OperatorAdd },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(2)},
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var assignment = result.Statements.Should().ContainSingle()
                               .Which.Should().BeOfType<NullAssign>().Subject;

        assignment.Identifier.Should().Be("x");
        assignment.Value.Should().BeOfType<ArithmeticalAddExpression>();
    }
    
    [Fact]
    public void Parser_ShouldParseDeclaration()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("true")},
            new TokenData { Type = TokenType.OperatorDeclaration},
            new TokenData { Type = TokenType.Boolean, Value = new BoolFactor(false) },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var assigment = result.Statements.Should().ContainSingle()
                              .Which.Should().BeOfType<Declaration>().Subject;
        assigment.Identifier.Should().Be("true");
        assigment.Value.ShouldBeConstFactor(false);
    }
    
    [Fact]
    public void Parser_ShouldParseDeclaration_WithExpressionValue()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("x") },
            new TokenData { Type = TokenType.OperatorDeclaration },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(1)},
            new TokenData { Type = TokenType.OperatorAdd },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(2)},
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var assignment = result.Statements.Should().ContainSingle()
                               .Which.Should().BeOfType<Declaration>().Subject;

        assignment.Identifier.Should().Be("x");
        assignment.Value.Should().BeOfType<ArithmeticalAddExpression>();
    }

    [Theory]
    [InlineData(TokenType.OperatorAssign)]
    [InlineData(TokenType.OperatorNullAssign)]
    [InlineData(TokenType.OperatorDeclaration)]
    public void Parser_ShouldThrow_WhenAssignmentValueMissing(TokenType tokenType)
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("x") },
            new TokenData { Type = tokenType },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var act = () => parser.ParserProgram();
        
        // Assert
        act.Should().Throw<TokenException>()
            .WithMessage("*Expected statement*");
    }
    
    
    #endregion
    
    #region Lambda

    [Fact]
    public void Parser_ShouldParseLambda()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.ParenthesesOpen },
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("x") },
            new TokenData { Type = TokenType.ParenthesesClose },
            new TokenData { Type = TokenType.OperatorArrow },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(42) },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var lambda = result.Statements.Should().ContainSingle()
            .Which.Should().BeOfType<Lambda>().Subject;
        lambda.Arguments.Should().ContainSingle().Which.Should().Be( "x");
        lambda.Body.ShouldBeConstFactor(42);
    }
    
    [Fact]
    public void Parser_ShouldParseLambda_WhenBodyIsBlock()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.ParenthesesOpen },
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("x") },
            new TokenData { Type = TokenType.ParenthesesClose },
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
            new TokenData { Type = TokenType.ParenthesesOpen },
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("x") },
            new TokenData { Type = TokenType.Comma },
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("y") },
            new TokenData { Type = TokenType.ParenthesesClose },
            new TokenData { Type = TokenType.OperatorArrow },
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("z") },
            new TokenData { Type = TokenType.OperatorAdd },
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("k") },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var lambda = result.Statements.Should().ContainSingle()
            .Which.Should().BeOfType<Lambda>().Subject;
        lambda.Arguments.Should().BeEquivalentTo(["x", "y"]);

        var body = lambda.Body.Should().BeOfType<ArithmeticalAddExpression>().Subject;
        body.Lhs.Should().BeOfType<VariableFactor>().Which.Identifier.Should().Be("z");
        body.Rhs.Should().BeOfType<VariableFactor>().Which.Identifier.Should().Be("k");
    }

    [Fact]
    public void Parser_ShouldParseLambda_WithTrailingComma()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.ParenthesesOpen },
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("x") },
            new TokenData { Type = TokenType.Comma },
            new TokenData { Type = TokenType.ParenthesesClose },
            new TokenData { Type = TokenType.OperatorArrow },
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("y") },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var lambda = result.Statements.Should().ContainSingle()
            .Which.Should().BeOfType<Lambda>().Subject;
        lambda.Arguments.Should().ContainSingle().Which.Should().Be("x");

        lambda.Body.Should().BeOfType<VariableFactor>()
            .Which.Identifier.Should().Be("y");
    }
    
    [Fact]
    public void Parser_ShouldThrow_WhenLambdaMissingBody()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.ParenthesesOpen },
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("x") },
            new TokenData { Type = TokenType.ParenthesesClose },
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
            new TokenData { Type = TokenType.ParenthesesOpen },
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("x") },
            new TokenData { Type = TokenType.ParenthesesClose },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(42) }
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
            new TokenData { Type = TokenType.ParenthesesOpen },
            new TokenData { Type = TokenType.OperatorArrow },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(42) }
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
            new TokenData { Type = TokenType.Boolean, Value = new BoolFactor(true) },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(1)},
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var condition = result.Statements.Should().ContainSingle()
                              .Which.Should().BeOfType<Condition>().Subject;
        condition.ConditionaryItems.Should().HaveCount(1);
        condition.ConditionaryItems[0].condition.ShouldBeConstFactor(true);
        condition.ConditionaryItems[0].body.ShouldBeConstFactor(1);
        condition.ElseBody.Should().BeNull();
    }
    
    [Fact]
    public void Parser_ShouldThrow_WhenNoIfBody()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.If },
            new TokenData { Type = TokenType.Boolean, Value = new BoolFactor(true) }
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
            new TokenData { Type = TokenType.Boolean, Value = new BoolFactor(true) },
            new TokenData { Type = TokenType.OperatorSubtract },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(1)},
            new TokenData { Type = TokenType.Boolean, Value = new BoolFactor(false) },
            new TokenData { Type = TokenType.OperatorDivide },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(2)},
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var condition = result.Statements.Should().ContainSingle()
                              .Which.Should().BeOfType<Condition>().Subject;
        condition.ConditionaryItems.Should().HaveCount(1);
        condition.ElseBody.Should().BeNull();
        
        var conditionExpression = condition.ConditionaryItems[0].condition.Should().BeOfType<ArithmeticalSubtractExpression>().Subject;
        conditionExpression.Lhs.ShouldBeConstFactor(true);
        conditionExpression.Rhs.ShouldBeConstFactor(1);
        
        var conditionBody = condition.ConditionaryItems[0].body.Should().BeOfType<ArithmeticalDivideExpression>().Subject;
        conditionBody.Lhs.ShouldBeConstFactor(false);
        conditionBody.Rhs.ShouldBeConstFactor(2);
    }
    
    [Fact]
    public void Parser_ShouldParseCondition_WithIfElse()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.If },
            new TokenData { Type = TokenType.Boolean, Value = new BoolFactor(true) },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(1)},
            new TokenData { Type = TokenType.Else },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(2)},
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var condition = result.Statements.Should().ContainSingle()
            .Which.Should().BeOfType<Condition>().Subject;
        condition.ConditionaryItems.Should().HaveCount(1);
        condition.ConditionaryItems[0].condition.ShouldBeConstFactor(true);
        condition.ConditionaryItems[0].body.ShouldBeConstFactor(1);
        condition.ElseBody.Should().NotBeNull();
        condition.ElseBody.ShouldBeConstFactor(2);
    }
    
    [Fact]
    public void Parser_ShouldParseCondition_WithIfElseIf()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.If },
            new TokenData { Type = TokenType.Boolean, Value = new BoolFactor(true) },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(1)},
            new TokenData { Type = TokenType.Else },
            new TokenData { Type = TokenType.If },
            new TokenData { Type = TokenType.Boolean, Value = new BoolFactor(false) },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(2)},
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var condition = result.Statements.Should().ContainSingle()
                              .Which.Should().BeOfType<Condition>().Subject;
        condition.ConditionaryItems.Should().HaveCount(2);
        condition.ConditionaryItems[0].condition.ShouldBeConstFactor(true);
        condition.ConditionaryItems[0].body.ShouldBeConstFactor(1);
        condition.ConditionaryItems[1].condition.ShouldBeConstFactor(false);
        condition.ConditionaryItems[1].body.ShouldBeConstFactor(2);
        condition.ElseBody.Should().BeNull();
    }

    [Fact]
    public void Parser_ShouldParseCondition_WithIfAndMultipleElseIf()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.If },
            new TokenData { Type = TokenType.Boolean, Value = new BoolFactor(true) },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(1)},
            new TokenData { Type = TokenType.Else },
            new TokenData { Type = TokenType.If },
            new TokenData { Type = TokenType.Boolean, Value = new BoolFactor(false) },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(2)},
            new TokenData { Type = TokenType.Else },
            new TokenData { Type = TokenType.If },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(3)},
            new TokenData { Type = TokenType.String, Value = new StringFactor("4") },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var condition = result.Statements.Should().ContainSingle()
                              .Which.Should().BeOfType<Condition>().Subject;
        condition.ConditionaryItems.Should().HaveCount(3);
        condition.ConditionaryItems[0].condition.ShouldBeConstFactor(true);
        condition.ConditionaryItems[0].body.ShouldBeConstFactor(1);
        condition.ConditionaryItems[1].condition.ShouldBeConstFactor(false);
        condition.ConditionaryItems[1].body.ShouldBeConstFactor(2);
        condition.ConditionaryItems[2].condition.ShouldBeConstFactor(3);
        condition.ConditionaryItems[2].body.ShouldBeConstFactor("4");
        condition.ElseBody.Should().BeNull();
    }
    
    [Fact]
    public void Parser_ShouldThrow_WhenNoIfElseBody()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.If },
            new TokenData { Type = TokenType.Boolean, Value = new BoolFactor(true) },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(1)},
            new TokenData { Type = TokenType.Else },
            new TokenData { Type = TokenType.If },
            new TokenData { Type = TokenType.Boolean, Value = new BoolFactor(false) }
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
            new TokenData { Type = TokenType.Boolean, Value = new BoolFactor(true) },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(1)},
            new TokenData { Type = TokenType.Else },
            new TokenData { Type = TokenType.If },
            new TokenData { Type = TokenType.Boolean, Value = new BoolFactor(false) },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(2)},
            new TokenData { Type = TokenType.Else },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(3)},
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var condition = result.Statements.Should().ContainSingle()
                              .Which.Should().BeOfType<Condition>().Subject;
        condition.ConditionaryItems.Should().HaveCount(2);
        condition.ConditionaryItems[0].condition.ShouldBeConstFactor(true);
        condition.ConditionaryItems[0].body.ShouldBeConstFactor(1);
        condition.ConditionaryItems[1].condition.ShouldBeConstFactor(false);
        condition.ConditionaryItems[1].body.ShouldBeConstFactor(2);
        condition.ElseBody.Should().NotBeNull();
        condition.ElseBody.ShouldBeConstFactor(3);
    }
    
    [Fact]
    public void Parser_ShouldThrow_WhenNoElseBody()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.If },
            new TokenData { Type = TokenType.Boolean, Value = new BoolFactor(true) },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(1)},
            new TokenData { Type = TokenType.Else },
            new TokenData { Type = TokenType.If },
            new TokenData { Type = TokenType.Boolean, Value = new BoolFactor(false) },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(2)},
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
            new TokenData { Type = TokenType.Boolean, Value = new BoolFactor(true) },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(123) },
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
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("cond") },
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
            new TokenData { Type = TokenType.Boolean, Value = new BoolFactor(true) },
            new TokenData { Type = TokenType.Loop },
            new TokenData { Type = TokenType.Boolean, Value = new BoolFactor(false) },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(1)},
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var outerLoop = result.Statements.Should().ContainSingle()
            .Which.Should().BeOfType<Loop>().Subject;

        outerLoop.Condition.ShouldBeConstFactor(true);

        var innerLoop = outerLoop.Body.Should().BeOfType<Loop>().Subject;

        innerLoop.Condition.ShouldBeConstFactor(false);

        innerLoop.Body.ShouldBeConstFactor(1);
    }
    
    #endregion
    
    #region FunctionCall

    [Fact]
    public void Parser_ShouldParseFunctionCall()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("x") },
            new TokenData { Type = TokenType.ParenthesesOpen },
            new TokenData { Type = TokenType.ParenthesesClose },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var functionCall = result.Statements.Should().ContainSingle()
              .Which.Should().BeOfType<FunctionCall>().Subject;
        
        functionCall.Callable.Should().BeOfType<VariableFactor>().Which.Identifier.Should().Be("x");
        functionCall.Arguments.Should().BeEmpty();
    }

    [Fact]
    public void Parser_ShouldParseFunctionCall_WithOneArgument()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("foo") },
            new TokenData { Type = TokenType.ParenthesesOpen },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(1)},
            new TokenData { Type = TokenType.ParenthesesClose },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var functionCall = result.Statements.Should().ContainSingle()
            .Which.Should().BeOfType<FunctionCall>().Subject;

        functionCall.Callable.Should().BeOfType<VariableFactor>().Which.Identifier.Should().Be("foo");
        functionCall.Arguments.Should().ContainSingle()
            .Which.ShouldBeConstFactor(1);
    }

    [Fact]
    public void Parser_ShouldParseFunctionCall_WithMultipleArguments()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("sum") },
            new TokenData { Type = TokenType.ParenthesesOpen },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(1)},
            new TokenData { Type = TokenType.Comma },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(2)},
            new TokenData { Type = TokenType.Comma },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(3)},
            new TokenData { Type = TokenType.ParenthesesClose },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var functionCall = result.Statements.Should().ContainSingle()
            .Which.Should().BeOfType<FunctionCall>().Subject;

        functionCall.Callable.Should().BeOfType<VariableFactor>().Which.Identifier.Should().Be("sum");
        functionCall.Arguments.Should().HaveCount(3);
        functionCall.Arguments[0].ShouldBeConstFactor(1);
        functionCall.Arguments[1].ShouldBeConstFactor(2);
        functionCall.Arguments[2].ShouldBeConstFactor(3);
    }
    
    [Fact]
    public void Parser_ShouldParseFunctionCall_WithTrailingComma()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("sum") },
            new TokenData { Type = TokenType.ParenthesesOpen },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(1)},
            new TokenData { Type = TokenType.Comma },
            new TokenData { Type = TokenType.ParenthesesClose },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var functionCall = result.Statements.Should().ContainSingle()
            .Which.Should().BeOfType<FunctionCall>().Subject;

        functionCall.Callable.Should().BeOfType<VariableFactor>().Which.Identifier.Should().Be("sum");
        functionCall.Arguments.Should().ContainSingle().
            Which.ShouldBeConstFactor(1);
    }
    
    [Fact]
    public void Parser_ShouldThrow_WhenArgumentsAreNotSeparatedByCommas()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("sum") },
            new TokenData { Type = TokenType.ParenthesesOpen },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(1)},
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(2)},
            new TokenData { Type = TokenType.ParenthesesClose },
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
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("foo") },
            new TokenData { Type = TokenType.ParenthesesOpen },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(1)},
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
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("foo") },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(1)},
            new TokenData { Type = TokenType.ParenthesesClose },
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
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("foo") },
            new TokenData { Type = TokenType.ParenthesesOpen },
            new TokenData { Type = TokenType.Comma },
            new TokenData { Type = TokenType.Comma },
            new TokenData { Type = TokenType.ParenthesesClose },
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
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("negate") },
            new TokenData { Type = TokenType.ParenthesesOpen },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(69) },
            new TokenData { Type = TokenType.OperatorAdd },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(42) },
            new TokenData { Type = TokenType.ParenthesesClose },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var functionCall = result.Statements.Should().ContainSingle()
            .Which.Should().BeOfType<FunctionCall>().Subject;

        functionCall.Callable.Should().BeOfType<VariableFactor>().Which.Identifier.Should().Be("negate");
        functionCall.Arguments.Should().ContainSingle();

        var expr = functionCall.Arguments[0].Should().BeOfType<ArithmeticalAddExpression>().Subject;
        expr.Lhs.ShouldBeConstFactor(69);
        expr.Rhs.ShouldBeConstFactor(42);
    }

    [Fact]
    public void Parser_ShouldParseNestedFunctionCalls_AsArguments()
    {
        // ff(getValue(7));
        
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("ff") },
            new TokenData { Type = TokenType.ParenthesesOpen },
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("getValue") },
            new TokenData { Type = TokenType.ParenthesesOpen },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(7) },
            new TokenData { Type = TokenType.ParenthesesClose },
            new TokenData { Type = TokenType.ParenthesesClose },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var outsideCall = result.Statements.Should().ContainSingle()
            .Which.Should().BeOfType<FunctionCall>().Subject;

        outsideCall.Callable.Should().BeOfType<VariableFactor>().Which.Identifier.Should().Be("ff");
        outsideCall.Arguments.Should().ContainSingle();

        var innerCall = outsideCall.Arguments[0].Should().BeOfType<FunctionCall>().Subject;
        innerCall.Callable.Should().BeOfType<VariableFactor>().Which.Identifier.Should().Be("getValue");
        innerCall.Arguments.Should().ContainSingle()
            .Which.ShouldBeConstFactor(7);
    }
    
    [Fact]
    public void Parser_ShouldParseChainedFunctionCalls()
    {
        // f()();
        
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("f") },
            new TokenData { Type = TokenType.ParenthesesOpen },
            new TokenData { Type = TokenType.ParenthesesClose },
            new TokenData { Type = TokenType.ParenthesesOpen },
            new TokenData { Type = TokenType.ParenthesesClose },
            new TokenData { Type = TokenType.EndOfStatement }
        );
    
        // Act
        var result = parser.ParserProgram();
    
        // Assert
        var outsideCall = result.Statements.Should().ContainSingle()
                               .Which.Should().BeOfType<FunctionCall>().Subject;
        
        var innerCall = outsideCall.Callable.Should().BeOfType<FunctionCall>().Subject;
        outsideCall.Arguments.Should().BeEmpty();
        
        innerCall.Callable.Should().BeOfType<VariableFactor>().Which.Identifier.Should().Be("f");
        innerCall.Arguments.Should().BeEmpty();
    }
    
    [Fact]
    public void Parser_ShouldParseChainedFunctionCalls_WithManyCalls()
    {
        // f()()()()();
        
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("f") },
            new TokenData { Type = TokenType.ParenthesesOpen },
            new TokenData { Type = TokenType.ParenthesesClose },    // 1s call
            new TokenData { Type = TokenType.ParenthesesOpen },
            new TokenData { Type = TokenType.ParenthesesClose },    // 2s call
            new TokenData { Type = TokenType.ParenthesesOpen },
            new TokenData { Type = TokenType.ParenthesesClose },    // 3s call
            new TokenData { Type = TokenType.ParenthesesOpen },
            new TokenData { Type = TokenType.ParenthesesClose },    // 4s call
            new TokenData { Type = TokenType.ParenthesesOpen },
            new TokenData { Type = TokenType.ParenthesesClose },    // 5s call
            new TokenData { Type = TokenType.EndOfStatement }
        );
    
        // Act
        var result = parser.ParserProgram();
    
        // Assert
        var call = result.Statements.Should().ContainSingle().Which.Should().BeOfType<FunctionCall>().Subject;
        for (int i = 0; i < 4; i++)
        {
            call.Arguments.Should().BeEmpty();
            call = call.Callable.Should().BeOfType<FunctionCall>().Subject;
        }
        call.Arguments.Should().BeEmpty();
        call.Callable.Should().BeOfType<VariableFactor>().Which.Identifier.Should().Be("f");
    }
    
    [Fact]
    public void Parser_ShouldParseChainedFunctionCalls_WithArguments()
    {
        // f(arg1, arg2)(arg3);
        
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("f") },
            new TokenData { Type = TokenType.ParenthesesOpen },
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("arg1") },
            new TokenData { Type = TokenType.Comma },
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("arg2") },
            new TokenData { Type = TokenType.ParenthesesClose },
            new TokenData { Type = TokenType.ParenthesesOpen },
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("arg3") },
            new TokenData { Type = TokenType.ParenthesesClose },
            new TokenData { Type = TokenType.EndOfStatement }
        );
    
        // Act
        var result = parser.ParserProgram();
    
        // Assert
        var secondCall = result.Statements.Should().ContainSingle()
                              .Which.Should().BeOfType<FunctionCall>().Subject;

        var innerCall = secondCall.Callable.Should().BeOfType<FunctionCall>().Subject;
        secondCall.Arguments.Should().HaveCount(1);
        secondCall.Arguments[0].Should().BeOfType<VariableFactor>().Which.Identifier.Should().Be("arg3");
        
        innerCall.Callable.Should().BeOfType<VariableFactor>().Which.Identifier.Should().Be("f");
        innerCall.Arguments.Should().HaveCount(2);
        innerCall.Arguments[0].Should().BeOfType<VariableFactor>().Which.Identifier.Should().Be("arg1");
        innerCall.Arguments[1].Should().BeOfType<VariableFactor>().Which.Identifier.Should().Be("arg2");
    }
    
    [Fact]
    public void Parser_ShouldParseFunctionCall_WhenCallingBlock()
    {
        // {}();
        
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.BraceOpen },
            new TokenData { Type = TokenType.BraceClose },
            new TokenData { Type = TokenType.ParenthesesOpen },
            new TokenData { Type = TokenType.ParenthesesClose },
            new TokenData { Type = TokenType.EndOfStatement }
        );
    
        // Act
        var result = parser.ParserProgram();
    
        // Assert
        var outsideCall = result.Statements.Should().ContainSingle()
                                .Which.Should().BeOfType<FunctionCall>().Subject;
        
        outsideCall.Callable.Should().BeOfType<Block>().Which.Statements.Should().BeEmpty();
        outsideCall.Arguments.Should().BeEmpty();
    }
    
    [Fact]
    public void Parser_ShouldParseFunctionCall_WhenCallingBlock_WithArguments()
    {
        // {"blockContent"}(arg1, arg2);
        
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.BraceOpen },
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("blockContent") },
            new TokenData { Type = TokenType.BraceClose },
            new TokenData { Type = TokenType.ParenthesesOpen },
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("arg1") },
            new TokenData { Type = TokenType.Comma },
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("arg2") },
            new TokenData { Type = TokenType.ParenthesesClose },
            new TokenData { Type = TokenType.EndOfStatement }
        );
    
        // Act
        var result = parser.ParserProgram();
    
        // Assert
        var call = result.Statements.Should().ContainSingle()
                                .Which.Should().BeOfType<FunctionCall>().Subject;
        
        call.Callable.Should().BeOfType<Block>()
                   .Which.Statements.Should().ContainSingle()
                   .Which.Should().BeOfType<VariableFactor>()
                   .Which.Identifier.Should().Be("blockContent");
        call.Arguments.Should().HaveCount(2);
        call.Arguments[0].Should().BeOfType<VariableFactor>().Which.Identifier.Should().Be("arg1");
        call.Arguments[1].Should().BeOfType<VariableFactor>().Which.Identifier.Should().Be("arg2");
    }

    #endregion
    
    #endregion
    
    #region Expressions
    
    [Fact]
    public void Parser_ShouldParseArithmeticalAddExpression()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(1)},
            new TokenData { Type = TokenType.OperatorAdd },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(2)},
            new TokenData { Type = TokenType.OperatorAdd },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(3)},
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var topExpression = result.Statements.Should().ContainSingle()
                                  .Which.Should().BeOfType<ArithmeticalAddExpression>().Subject;
        topExpression.Lhs.ShouldBeConstFactor(1);
        
        var innerExpression = topExpression.Rhs.Should().BeOfType<ArithmeticalAddExpression>().Subject;
        innerExpression.Lhs.ShouldBeConstFactor(2);
        innerExpression.Rhs.ShouldBeConstFactor(3);
    }
    
    [Fact]
    public void Parser_ShouldParseArithmeticalSubtractExpression()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(1)},
            new TokenData { Type = TokenType.OperatorSubtract },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(2)},
            new TokenData { Type = TokenType.OperatorSubtract },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(3)},
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var topExpression = result.Statements.Should().ContainSingle()
                                  .Which.Should().BeOfType<ArithmeticalSubtractExpression>().Subject;
        topExpression.Lhs.ShouldBeConstFactor(1);
        
        var innerExpression = topExpression.Rhs.Should().BeOfType<ArithmeticalSubtractExpression>().Subject;
        innerExpression.Lhs.ShouldBeConstFactor(2);
        innerExpression.Rhs.ShouldBeConstFactor(3);
    }
    
    [Fact]
    public void Parser_ShouldParseArithmeticalMultiplyExpression()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(1)},
            new TokenData { Type = TokenType.OperatorMultiply },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(2)},
            new TokenData { Type = TokenType.OperatorMultiply },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(3)},
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var topExpression = result.Statements.Should().ContainSingle()
              .Which.Should().BeOfType<ArithmeticalMultiplyExpression>().Subject;
        topExpression.Lhs.ShouldBeConstFactor(1);
        
        var innerExpression = topExpression.Rhs.Should().BeOfType<ArithmeticalMultiplyExpression>().Subject;
        innerExpression.Lhs.ShouldBeConstFactor(2);
        innerExpression.Rhs.ShouldBeConstFactor(3);
    }
    
    [Fact]
    public void Parser_ShouldParseArithmeticalDivideExpression()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(1)},
            new TokenData { Type = TokenType.OperatorDivide },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(2)},
            new TokenData { Type = TokenType.OperatorDivide },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(3)},
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var topExpression = result.Statements.Should().ContainSingle()
                                  .Which.Should().BeOfType<ArithmeticalDivideExpression>().Subject;
        topExpression.Lhs.ShouldBeConstFactor(1);
        
        var innerExpression = topExpression.Rhs.Should().BeOfType<ArithmeticalDivideExpression>().Subject;
        innerExpression.Lhs.ShouldBeConstFactor(2);
        innerExpression.Rhs.ShouldBeConstFactor(3);
    }
    
    [Fact]
    public void Parser_ShouldParseCompereEqualsExpression()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(1)},
            new TokenData { Type = TokenType.OperatorEqual },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(2)},
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var expression = result.Statements.Should().ContainSingle()
                               .Which.Should().BeOfType<CompereEqualsExpression>().Subject;
        expression.Lhs.ShouldBeConstFactor(1);
        expression.Rhs.ShouldBeConstFactor(2);
    }
    
    [Fact]
    public void Parser_ShouldParseCompereNotEqualsExpression()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(1)},
            new TokenData { Type = TokenType.OperatorNotEqual },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(2)},
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var expression = result.Statements.Should().ContainSingle()
                               .Which.Should().BeOfType<CompereNotEqualsExpression>().Subject;
        expression.Lhs.ShouldBeConstFactor(1);
        expression.Rhs.ShouldBeConstFactor(2);
    }
    
    [Fact]
    public void Parser_ShouldParseCompereGreaterExpression()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(1)},
            new TokenData { Type = TokenType.OperatorGreater },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(2)},
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var expression = result.Statements.Should().ContainSingle()
                               .Which.Should().BeOfType<CompereGreaterExpression>().Subject;
        expression.Lhs.ShouldBeConstFactor(1);
        expression.Rhs.ShouldBeConstFactor(2);
    }
    
    [Fact]
    public void Parser_ShouldParseCompereGreaterEqualsExpression()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(1)},
            new TokenData { Type = TokenType.OperatorGreaterEqual },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(2)},
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var expression = result.Statements.Should().ContainSingle()
                               .Which.Should().BeOfType<CompereGreaterEqualExpression>().Subject;
        expression.Lhs.ShouldBeConstFactor(1);
        expression.Rhs.ShouldBeConstFactor(2);
    }
    
    [Fact]
    public void Parser_ShouldParseCompereLessExpression()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(1)},
            new TokenData { Type = TokenType.OperatorLess },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(2)},
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var expression = result.Statements.Should().ContainSingle()
                               .Which.Should().BeOfType<CompereLessExpression>().Subject;
        expression.Lhs.ShouldBeConstFactor(1);
        expression.Rhs.ShouldBeConstFactor(2);
    }
    
    [Fact]
    public void Parser_ShouldParseCompereLessEqualsExpression()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(1)},
            new TokenData { Type = TokenType.OperatorLessEqual },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(2)},
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var expression = result.Statements.Should().ContainSingle()
                               .Which.Should().BeOfType<CompereLessEqualExpression>().Subject;
        expression.Lhs.ShouldBeConstFactor(1);
        expression.Rhs.ShouldBeConstFactor(2);
    }
    
    [Fact]
    public void Parser_ShouldParseLogicalOrExpression()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(1) },
            new TokenData { Type = TokenType.OperatorOr },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(2) },
            new TokenData { Type = TokenType.OperatorOr },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(3) },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var topExpression = result.Statements.Should().ContainSingle()
                                  .Which.Should().BeOfType<LogicOrExpression>().Subject;
        topExpression.Lhs.ShouldBeConstFactor(1);
        
        var innerExpression = topExpression.Rhs.Should().BeOfType<LogicOrExpression>().Subject;
        innerExpression.Lhs.ShouldBeConstFactor(2);
        innerExpression.Rhs.ShouldBeConstFactor(3);
    }
    
    [Fact]
    public void Parser_ShouldParseLogicalAndExpression()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(1) },
            new TokenData { Type = TokenType.OperatorAnd },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(2) },
            new TokenData { Type = TokenType.OperatorAnd },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(3) },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var topExpression = result.Statements.Should().ContainSingle()
                                  .Which.Should().BeOfType<LogicAndExpression>().Subject;
        topExpression.Lhs.ShouldBeConstFactor(1);
        
        var innerExpression = topExpression.Rhs.Should().BeOfType<LogicAndExpression>().Subject;
        innerExpression.Lhs.ShouldBeConstFactor(2);
        innerExpression.Rhs.ShouldBeConstFactor(3);
    }
    
    [Fact]
    public void Parser_ShouldParseLogicalExpressionInOrder()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(1) },
            new TokenData { Type = TokenType.OperatorOr },          // orExpression1
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(2) },
            new TokenData { Type = TokenType.OperatorOr },          // orExpression2
            
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(3) },
            new TokenData { Type = TokenType.OperatorAnd },         // andExpression1
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(4) },
            new TokenData { Type = TokenType.OperatorAnd },         // andExpression2
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(5) },
            
            new TokenData { Type = TokenType.OperatorOr },          // orExpression3
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(6) },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var orExpression1 = result.Statements.Should().ContainSingle()
                               .Which.Should().BeOfType<LogicOrExpression>().Subject;
        orExpression1.Lhs.ShouldBeConstFactor(1);
        var orExpression2 = orExpression1.Rhs.Should().BeOfType<LogicOrExpression>().Subject;
        
        orExpression2.Lhs.ShouldBeConstFactor(2);
        var orExpression3 = orExpression2.Rhs.Should().BeOfType<LogicOrExpression>().Subject;
        
        var andExpression1 = orExpression3.Lhs.Should().BeOfType<LogicAndExpression>().Subject;
        orExpression3.Rhs.ShouldBeConstFactor(6);
        
        andExpression1.Lhs.ShouldBeConstFactor(3);
        var andExpression2 = andExpression1.Rhs.Should().BeOfType<LogicAndExpression>().Subject;
        
        andExpression2.Lhs.ShouldBeConstFactor(4);
        andExpression2.Rhs.ShouldBeConstFactor(5);
    }
    
    [Fact]
    public void Parser_ShouldParseUnaryMinus()
    {
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.OperatorSubtract },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(42) },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        var result = parser.ParserProgram();

        var expr = result.Statements.Should().ContainSingle().Which
                         .Should().BeOfType<NotExpression>().Subject;

        expr.Factor.ShouldBeConstFactor(42);
    }

    [Fact]
    public void Parser_ShouldParseNullCoalescingExpression()
    {
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("a") },
            new TokenData { Type = TokenType.OperatorNullCoalescing },
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("b") },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        var result = parser.ParserProgram();

        var expr = result.Statements.Should().ContainSingle().Which
                         .Should().BeOfType<NullCoalescingExpression>().Subject;
        expr.Lhs.Should().BeOfType<VariableFactor>().Which.Identifier.Should().Be("a");
        expr.Rhs.Should().BeOfType<VariableFactor>().Which.Identifier.Should().Be("b");
    }

    [Fact]
    public void Parser_ShouldParseParenthesizedExpression()
    {
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("a") },
            new TokenData { Type = TokenType.OperatorMultiply },
            new TokenData { Type = TokenType.BraceOpen },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(1) },
            new TokenData { Type = TokenType.OperatorAdd },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(2) },
            new TokenData { Type = TokenType.BraceClose },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        var result = parser.ParserProgram();
        
        var multiplyExpression = result.Statements.Should().ContainSingle().Which.Should().BeOfType<ArithmeticalMultiplyExpression>().Subject;
        multiplyExpression.Lhs.Should().BeOfType<VariableFactor>().Which.Identifier.Should().Be("a");
        
        var block = multiplyExpression.Rhs.Should().BeOfType<Block>().Subject;
        
        var addExpression = block.Statements.Should().ContainSingle().Which.Should().BeOfType<ArithmeticalAddExpression>().Subject;
        addExpression.Lhs.ShouldBeConstFactor(1);
        addExpression.Rhs.ShouldBeConstFactor(2);
    }

    [Fact]
    public void Parser_ShouldParse_ComplexExpressionWithPrecedence()
    {
        // -2 +new IntFactor(3)*new IntFactor(4)> new IntFactor(5)|| { if 6 > 7 { 8 } } ?? a;
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.OperatorSubtract },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(2) },
            new TokenData { Type = TokenType.OperatorAdd },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(3) },
            new TokenData { Type = TokenType.OperatorMultiply },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(4) },
            new TokenData { Type = TokenType.OperatorGreater },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(5) },
            new TokenData { Type = TokenType.OperatorOr },
            new TokenData { Type = TokenType.BraceOpen },
            new TokenData { Type = TokenType.If },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(6) },
            new TokenData { Type = TokenType.OperatorGreater },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(7) },
            new TokenData { Type = TokenType.BraceOpen },
            new TokenData { Type = TokenType.Integer, Value = new IntFactor(8) },
            new TokenData { Type = TokenType.BraceClose },
            new TokenData { Type = TokenType.BraceClose },
            new TokenData { Type = TokenType.OperatorNullCoalescing },
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("a") },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        var logicExpr = result.Statements.Should().ContainSingle().Which
            .Should().BeOfType<LogicOrExpression>().Subject;

        // LogicLeft || LogicRight | (-2 +new IntFactor(3)*new IntFactor(4)> 5) || ({ if 6 > 7 { 8 } } ?? a);
        var logicLeft = logicExpr.Lhs.Should().BeOfType<CompereGreaterExpression>().Subject;
        var logicRight = logicExpr.Rhs.Should().BeOfType<NullCoalescingExpression>().Subject;

        // LogicLeft: CompLeft > new IntFactor(5)| (-2 +new IntFactor(3)* 4) > (5)
        var compLeft = logicLeft.Lhs.Should().BeOfType<ArithmeticalAddExpression>().Subject;
        logicLeft.Rhs.ShouldBeConstFactor(5);

        // CompLeft: -2 + ArithmeticRight | (-2) + (3 * 4)
        compLeft.Lhs.Should().BeOfType<NotExpression>().Which.Factor
            .ShouldBeConstFactor(2);
        var arithmeticRight = compLeft.Rhs.Should().BeOfType<ArithmeticalMultiplyExpression>().Subject;
        
        // ArithmeticRight:new IntFactor(3)* 4
        arithmeticRight.Lhs.ShouldBeConstFactor(3);
        arithmeticRight.Rhs.ShouldBeConstFactor(4);
        
        // LogicRight: NullCoalRight ?? a | ({ if 6 > 7 { 8 } }) ?? (a)
        var nullCoalRight = logicRight.Lhs.Should().BeOfType<Block>()
                                      .Which.Statements.Should().ContainSingle()
                                      .Which.Should().BeOfType<Condition>().Subject;
        logicRight.Rhs.Should().BeOfType<VariableFactor>().Which.Identifier.Should().Be("a");
        
        // NullCoalRight: condition | if 6 > 7 { 8 }
        nullCoalRight.ConditionaryItems.Should().ContainSingle()
                     .Which.body.Should().BeOfType<Block>()
                     .Which.Statements.Should().ContainSingle()
                     .Which.ShouldBeConstFactor(8);
        nullCoalRight.ElseBody.Should().BeNull();
    }
    
    #endregion

    #region Factors

    public static IEnumerable<object[]> FactorCases =>
    [
        [TokenType.Integer, new IntFactor(42)],
        [TokenType.Boolean, new BoolFactor(true)],
        [TokenType.String, new StringFactor("aa")],
        [TokenType.Null, new NullFactor()]
    ];
    
    [Theory]
    [MemberData(nameof(FactorCases))]
    public void Parser_ShouldParseConstFactor(TokenType tokenType, IFactorValue factor)
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = tokenType, Value = factor },
            new TokenData { Type = TokenType.EndOfStatement }
        );

        // Act
        var result = parser.ParserProgram();

        // Assert
        result.Statements.Should().ContainSingle()
              .Which.Should().BeOfType<ConstFactor>()
              .Which.Value.Should().Be(factor);
    }
    
    [Fact]
    public void Parser_ShouldParseVariableFactor()
    {
        // Arrange
        var parser = CreateParserFromTokens(
            new TokenData { Type = TokenType.Identifier, Value = new StringFactor("variable") },
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