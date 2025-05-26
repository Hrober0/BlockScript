using BlockScript.Exceptions;
using BlockScript.Interpreter;
using BlockScript.Interpreter.BuildInMethods;
using BlockScript.Lexer;
using BlockScript.Lexer.FactorValues;
using BlockScript.Parser.Expressions;
using BlockScript.Parser.Factors;
using BlockScript.Parser.Statements;
using BlockScript.Reader;
using FluentAssertions;
using Xunit;

namespace BlockScript.Tests.InterpreterTests;

public class InterpreterTests
{
    [Fact]
    public void Interpreter_ShouldExecuteEmptyProgram()
    {
        // Arrange
        List<IStatement> program = [];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().BeOfType<NullFactor>();
    }
    
    #region Statements

    #region Block

    [Fact]
    public void Interpreter_ShouldExecuteBlock()
    {
        // Arrange
        List<IStatement> program = [
            new Block([ConstFactor(42)]),
            new Block([ConstFactor(69)]),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().BeEquivalentTo(new IntFactor(69));
    }

    [Fact]
    public void Interpreter_ShouldExecuteBlock_WhenIsEmpty()
    {
        // Arrange
        List<IStatement> program = [
            new Block([]),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().BeEquivalentTo(new NullFactor());
    }
    
    [Fact]
    public void Interpreter_ShouldExecuteBlock_WhenIsNested()
    {
        // Arrange
        List<IStatement> program = [
            new Block([ConstFactor(42)]),
            new Block([
                new Block([ConstFactor(69)]),
            ]),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().BeEquivalentTo(new IntFactor(69));
    }

    #endregion

    #region Assign

    [Fact]
    public void Interpreter_ShouldExecuteAssign()
    {
        // Arrange
        List<IStatement> program = [
            new Declaration("myVar", ConstFactor()),
            new Assign("myVar", ConstFactor(69)),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().BeEquivalentTo(new IntFactor(69));
    }
    
    [Fact]
    public void Interpreter_ShouldExecuteAssignAndBeAssign()
    {
        // Arrange
        List<IStatement> program = [
            new Declaration("myVar", ConstFactor(1)),
            new Assign("myVar", ConstFactor(2)),
            new Assign("myVar", ConstFactor(3)),
            new Declaration("another", ConstFactor(4)),
            new VariableFactor("myVar"),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().BeEquivalentTo(new IntFactor(3));
    }
    
    [Fact]
    public void Interpreter_ShouldThrow_WhenExecuteAssign_AndVariableIsNotDefined()
    {
        // Arrange
        List<IStatement> program = [
            new Assign("myVar", ConstFactor(69)),
        ];
        
        // Act
        var act = () => ExecuteProgram(program);
        
        // Assert
        act.Should().Throw<RuntimeException>()
           .WithMessage("*Variable of name myVar was not defined*");
    }
    
    [Fact]
    public void Interpreter_ShouldExecuteAssignAndBeAssignOnlyInGivenContext()
    {
        // Arrange
        List<IStatement> program = [
            new Declaration("myVar", ConstFactor(1)),
            new Block([
                new Assign("inner", ConstFactor(2)),
            ]),
            new VariableFactor("inner"),
        ];
        
        // Act
        var act = () => ExecuteProgram(program);
        
        // Assert
        act.Should().Throw<RuntimeException>()
           .WithMessage($"*Variable of name inner was not defined!*");
    }

    [Fact]
    public void Interpreter_ShouldExecuteAssign_WhenAssignFromInnerContext()
    {
        // Arrange
        List<IStatement> program = [
            new Declaration("myVar", ConstFactor(1)),
            new Block([
                new Assign("myVar", ConstFactor(2)),
            ]),
            new VariableFactor("myVar"),
        ];
        
        // Act
        var act = ExecuteProgram(program);
        
        // Assert
        act.Should().BeEquivalentTo(new IntFactor(2));
    }
    
    #endregion

    #region NullAssign

    [Fact]
    public void Interpreter_ShouldExecuteNullAssign()
    {
        // Arrange
        List<IStatement> program = [
            new Declaration("myVar", ConstFactor()),
            new NullAssign("myVar", ConstFactor(69)),
            new VariableFactor("myVar"),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().BeEquivalentTo(new IntFactor(69));
    }
    
    [Fact]
    public void Interpreter_ShouldThrow_WhenExecuteNullAssign_AndVariableIsNotDefined()
    {
        // Arrange
        List<IStatement> program = [
            new NullAssign("myVar", ConstFactor(69)),
        ];
        
        // Act
        var act = () => ExecuteProgram(program);
        
        // Assert
        act.Should().Throw<RuntimeException>()
           .WithMessage("*Variable of name myVar was not defined*");
    }
    
    [Fact]
    public void Interpreter_ShouldExecuteNullAssign_WhenVariableExistAndHasNullValue()
    {
        // Arrange
        List<IStatement> program = [
            new Assign("myVar", ConstFactor()),
            new NullAssign("myVar", ConstFactor(69)),
            new VariableFactor("myVar"),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().BeEquivalentTo(new IntFactor(69));
    }
    
    #endregion
    
    #region Declaration

    [Fact]
    public void Interpreter_ShouldExecuteDeclaration()
    {
        // Arrange
        List<IStatement> program = [
            new Declaration("myVar", ConstFactor(69)),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().BeEquivalentTo(new IntFactor(69));
    }
    
    [Fact]
    public void Interpreter_ShouldExecuteDeclarationAndBeAssign()
    {
        // Arrange
        List<IStatement> program = [
            new Declaration("myVar", ConstFactor(69)),
            new Declaration("another", ConstFactor(42)),
            new VariableFactor("myVar"),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().BeEquivalentTo(new IntFactor(69));
    }
    
    [Fact]
    public void Interpreter_ShouldExecuteDeclaration_AndOverrideLastValue_WhenVariableAlreadyDefined()
    {
        // Arrange
        List<IStatement> program = [
            new Declaration("myVar", ConstFactor(69)),
            new Declaration("myVar", ConstFactor(42)),
            new VariableFactor("myVar"),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().BeEquivalentTo(new IntFactor(42));
    }
    
    [Fact]
    public void Interpreter_ShouldExecuteDeclaration_AndBeAssignOnlyInGivenContext()
    {
        // Arrange
        List<IStatement> program = [
            new Declaration("myVar", ConstFactor(1)),
            new Block([
                new Declaration("inner", ConstFactor(2)),
            ]),
            new VariableFactor("inner"),
        ];
        
        // Act
        var act = () => ExecuteProgram(program);
        
        // Assert
        act.Should().Throw<RuntimeException>()
           .WithMessage($"*Variable of name inner was not defined!*");
    }

    [Fact]
    public void Interpreter_ShouldExecuteDeclaration_AndNotOverrideValue_WhenDeclarationFromInnerContext()
    {
        // Arrange
        List<IStatement> program = [
            new Declaration("myVar", ConstFactor(1)),
            new Block([
                new Declaration("myVar", ConstFactor(2)),
            ]),
            new VariableFactor("myVar"),
        ];
        
        // Act
        var act = ExecuteProgram(program);
        
        // Assert
        act.Should().BeEquivalentTo(new IntFactor(1));
    }

    #endregion

    #region Lambda

    [Fact]
    public void Interpreter_ShouldExecuteLambda()
    {
        // Arrange
        List<IStatement> program = [
            new Lambda(["a1"], ConstFactor(1)),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        var contextLambda = result.Should().BeOfType<ContextLambda>().Subject;
        contextLambda.Lambda.Arguments.Should().BeEquivalentTo(["a1"]);
        contextLambda.Lambda.Body.ShouldBeConstFactor(1);
        contextLambda.Context.Should().NotBeNull();
    }
    
    [Fact]
    public void Interpreter_ShouldExecuteLambdaWithoutContextExecution()
    {
        // Arrange
        List<IStatement> program = [
            new Lambda(["a1"], new VariableFactor("myVar")),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        var lambda = result.Should().BeOfType<Lambda>().Subject;
        lambda.Arguments.Should().BeEquivalentTo(["a1"]);
        lambda.Body.Should().BeOfType<VariableFactor>();
    }

    #endregion

    #region BuildInMethod
    
    [Fact]
    public void Interpreter_ShouldExecuteTestBuildInMethod()
    {
        // Arrange
        var output = new List<IFactorValue>();
        List<IStatement> program = [
            new FunctionCall(DebugMethod.IDENTIFIER, [ConstFactor(2)]),
            new FunctionCall(DebugMethod.IDENTIFIER, [ConstFactor(3)]),
        ];
        
        // Act
        ExecuteProgram(program, [new DebugMethod(output)]);
        
        // Assert
        output.Should().BeEquivalentTo([new IntFactor(2), new IntFactor(3)]);
    }
    
    [Fact]
    public void Interpreter_ShouldThrow_WhenBuildInMethodIsNotRegistered()
    {
        // Arrange
        List<IStatement> program = [
            new FunctionCall(DebugMethod.IDENTIFIER, [ConstFactor(2)]),
        ];
        
        // Act
        var act = () => ExecuteProgram(program, []);
        
        // Assert
        act.Should().Throw<RuntimeException>()
           .WithMessage($"*Variable of name {DebugMethod.IDENTIFIER} was not defined!*");
    }

    #endregion

    #region FunctionCall

    [Fact]
    public void Interpreter_ShouldExecuteFunctionCall()
    {
        // Arrange
        List<IStatement> program = [
            new Declaration("x", new Lambda([], ConstFactor(1))),
            new FunctionCall("x", []),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().BeEquivalentTo(new IntFactor(1));
    }
    
    [Fact]
    public void Interpreter_ShouldThrow_WhenExecuteFunctionCallNotRegistered()
    {
        // Arrange
        List<IStatement> program = [
            new Declaration("x", new Lambda([], ConstFactor(1))),
            new FunctionCall("y", []),
        ];
        
        // Act
        var act = () => ExecuteProgram(program);
        
        // Assert
        act.Should().Throw<RuntimeException>()
           .WithMessage($"*Variable of name y was not defined!*");
    }
    
    [Fact]
    public void Interpreter_ShouldExecuteFunctionCall_WithArguments()
    {
        // Arrange
        List<IStatement> program = [
            new Declaration("x", new Lambda(["a1", "a2"], new VariableFactor("a2"))),
            new FunctionCall("x", [ConstFactor(1), ConstFactor(2)]),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().BeEquivalentTo(new IntFactor(2));
    }
    
    [Fact]
    public void Interpreter_ShouldThrow_WhenExecuteFunctionCal_lHasIncorrectNumberOfArguments()
    {
        // Arrange
        List<IStatement> program = [
            new Declaration("x", new Lambda(["a1"], ConstFactor(1))),
            new FunctionCall("x", []),
        ];
        
        // Act
        var act = () => ExecuteProgram(program);
        
        // Assert
        act.Should().Throw<RuntimeException>()
           .WithMessage($"*requires 1 arguments, but given 0 arguments!*");
    }
    
    [Fact]
    public void Interpreter_ShouldThrow_WhenExecuteFunctionCall_IsNotCallable()
    {
        // Arrange
        List<IStatement> program = [
            new Declaration("x", ConstFactor(1)),
            new FunctionCall("x", []),
        ];
        
        // Act
        var act = () => ExecuteProgram(program);
        
        // Assert
        act.Should().Throw<RuntimeException>()
           .WithMessage($"*is not callable*");
    }
    
    [Fact]
    public void Interpreter_ShouldExecuteFunctionCall_MultipleTimes()
    {
        // Arrange
        var output = new List<IFactorValue>();
        List<IStatement> program = [
            new FunctionCall(DebugMethod.IDENTIFIER, [ConstFactor(1)]),
            new FunctionCall(DebugMethod.IDENTIFIER, [ConstFactor(2)]),
        ];
        
        // Act
        ExecuteProgram(program, [new DebugMethod(output)]);
        
        // Assert
        output.Should().BeEquivalentTo([new IntFactor(1), new IntFactor(2)]);
    }
    
    [Fact]
    public void Interpreter_ShouldExecuteFunctionCall_InCorrectContext()
    {
        /*
          f = () => {
	            a = 2;
	            ff = () => a = a + 1;
	            a = 3;
	            ff;
            };
            
            fc = f();
            print(fc());	# returns 4
            fc = f();
            print(fc());	# returns 4
            print(fc());	# returns 5
            print(fc());	# returns 6
        */
        
        // Arrange
        var output = new List<IFactorValue>();
        List<IStatement> program = [
            new Declaration("f", new Lambda([], new Block([
                new Declaration("a", ConstFactor(2)),
                new Declaration("ff", new Lambda([], new Assign("a", new ArithmeticalExpression(new VariableFactor("a"), ConstFactor(1), TokenType.OperatorAdd)))),
                new Assign("a", ConstFactor(3)),
                new VariableFactor("ff"),
            ]))),
            
            new Declaration("fc", new FunctionCall("f", [])),
            AddToOutput(new FunctionCall("fc", [])),                    // should add 4 to output
            new Declaration("fc", new FunctionCall("f", [])),
            AddToOutput(new FunctionCall("fc", [])),                    // should add 4 to output
            AddToOutput(new FunctionCall("fc", [])),                    // should add 5 to output
            AddToOutput(new FunctionCall("fc", [])),                    // should add 6 to output
        ];
        
        // Act
        ExecuteProgram(program, [new DebugMethod(output)]);
        
        // Assert
        output.Should().BeEquivalentTo([new IntFactor(4), new IntFactor(4), new IntFactor(5), new IntFactor(6)]);
    }
    
    [Fact]
    public void Interpreter_ShouldThrow_WhenExecuteFunctionCall_ReachRecursionLimit()
    {
        // Arrange
        List<IStatement> program = [
            new Declaration("x", new Lambda([], new FunctionCall("x", []))),
            new FunctionCall("x", []),
        ];
        
        // Act
        var act = () => ExecuteProgram(program);
        
        // Assert
        act.Should().Throw<RuntimeException>()
           .WithMessage($"*Exceeded recursion limit*");
    }

    #endregion

    #region Condition

    [Fact]
    public void Interpreter_ShouldExecuteConditionReturnFirstBodyResult_WhenFirstConditionTrue()
    {
        // Arrange
        List<IStatement> program = [
            new Condition([(ConstFactor(true), ConstFactor(1))], null),
        ];
        
        // Act
        var result = ExecuteProgram(program);

        // Assert
        result.Should().BeEquivalentTo(new IntFactor(1));
    }

    [Fact]
    public void Interpreter_ShouldExecuteConditionReturnFirstBodyResult_WhenFirstConditionFalse()
    {
        // Arrange
        List<IStatement> program = [
            new Condition([(ConstFactor(false), ConstFactor(1))], ConstFactor(2)),
        ];
        
        // Act
        var result = ExecuteProgram(program);

        // Assert
        result.Should().BeEquivalentTo(new IntFactor(2));
    }

    [Fact]
    public void Interpreter_ShouldExecuteConditionReturnNull_WhenFirstConditionFalse_AndElseBodyIsNull()
    {
        // Arrange
        List<IStatement> program = [
            new Condition([(ConstFactor(false), ConstFactor(1))], null),
        ];
        
        // Act
        var result = ExecuteProgram(program);

        // Assert
        result.Should().BeOfType<NullFactor>();
    }
    
    [Fact]
    public void Interpreter_ShouldExecuteConditionReturnSecondBody_WhenFirstConditionFalse_AndSecondConditionIfTrue()
    {
        // Arrange
        List<IStatement> program = [
            new Condition([
                (ConstFactor(false), ConstFactor(1)),
                (ConstFactor(true), ConstFactor(2)),
                (ConstFactor(true), ConstFactor(3)),
            ], ConstFactor(0)),
        ];
        
        // Act
        var result = ExecuteProgram(program);

        // Assert
        result.Should().BeEquivalentTo(new IntFactor(2));
    }
    
    [Fact]
    public void Interpreter_ShouldExecuteCondition_WhenConditionIsNotABoolean()
    {
        // Arrange
        List<IStatement> program = [
            new Condition([
                (ConstFactor(""), ConstFactor(1)),
                (ConstFactor(false), ConstFactor(2)),
                (ConstFactor(0), ConstFactor(3)),
                (ConstFactor(), ConstFactor(4)),
                (new Block([]), ConstFactor(5)),
                (new Block([ConstFactor(3)]), ConstFactor(6)),
            ], ConstFactor(0)),
        ];
        
        // Act
        var result = ExecuteProgram(program);

        // Assert
        result.Should().BeEquivalentTo(new IntFactor(6));
    }

    #endregion
    
    #region Loop
    
    [Fact]
    public void Interpreter_ShouldExecuteLoop()
    {
        /*
         * v = true;
         * loop v v := false;
         */
        
        // Arrange
        List<IStatement> program = [
            new Declaration("v", ConstFactor(true)),
            new Loop(new VariableFactor("v"), new Assign("v", ConstFactor(false))),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().BeEquivalentTo(new BoolFactor(false));
    }
    
    [Fact]
    public void Interpreter_ShouldExecuteLoop_WhenConditionIsFalse()
    {
        /*
         * loop false print(true);
         */
        
        // Arrange
        var output = new List<IFactorValue>();
        List<IStatement> program = [
            new Loop(ConstFactor(false), AddToOutput(ConstFactor(true))),
        ];
        
        // Act
        var result = ExecuteProgram(program, [new DebugMethod(output)]);
        
        // Assert
        result.Should().BeOfType<NullFactor>();
        output.Should().BeEmpty();
    }
    
    [Fact]
    public void Interpreter_ShouldThrow_WhenExecuteLoopExceedLoopLimit()
    {
        /*
         * loop true true;
         */
        
        // Arrange
        List<IStatement> program = [
            new Loop(ConstFactor(true), ConstFactor(true)),
        ];
        
        // Act
        var act = () => ExecuteProgram(program);
        
        // Assert
        act.Should().Throw<RuntimeException>()
           .WithMessage($"*Loop exceeded loop count limit*");
    }
    
    [Fact]
    public void Interpreter_ShouldExecuteLoop_WhenConditionReturnInt_AndIsDecreasedFromBodyBlock()
    {
        /*
         * i = 5;
         * loop i { i := i - 1; print(i) };
         */
        
        // Arrange
        var output = new List<IFactorValue>();
        List<IStatement> program = [
            new Declaration("i", ConstFactor(5)),
            new Loop(new VariableFactor("i"),
                new Block([
                    new Assign("i", new ArithmeticalExpression(new VariableFactor("i"), ConstFactor(1), TokenType.OperatorSubtract)),
                    AddToOutput("i"),
                ])
            ),
        ];
        
        // Act
        var result = ExecuteProgram(program, [new DebugMethod(output)]);
        
        // Assert
        result.Should().BeEquivalentTo(new IntFactor(0));
        output.Should().BeEquivalentTo([
            new IntFactor(4),
            new IntFactor(3),
            new IntFactor(2),
            new IntFactor(1),
            new IntFactor(0)
        ]);
    }
    
    [Fact]
    public void Interpreter_ShouldExecuteLoop_WhenConditionReturnInt_AndIsDecreasedFromConditionBlock()
    {
        /*
         * i = 2;
         * loop { i := i + 1; i < 6 } print(i);
         */
        
        // Arrange
        var output = new List<IFactorValue>();
        List<IStatement> program = [
            new Declaration("i", ConstFactor(2)),
            new Loop(
                new Block([
                    new Assign("i", new ArithmeticalExpression(new VariableFactor("i"), ConstFactor(1), TokenType.OperatorAdd)),
                    new CompereExpression(new VariableFactor("i"), ConstFactor(6), TokenType.OperatorLess) ,
                ]),
            AddToOutput("i")
            ),
        ];
        
        // Act
        var result = ExecuteProgram(program, [new DebugMethod(output)]);
        
        // Assert
        result.Should().BeEquivalentTo(new IntFactor(5));
        output.Should().BeEquivalentTo([
            new IntFactor(3),
            new IntFactor(4),
            new IntFactor(5),
        ]);
    }
    
    #endregion
    
    #endregion

    #region Expressions

    #region Logical

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, false)]
    public void Interpreter_ShouldExecuteLogicAndExpression_ForBooleans(bool left, bool right, bool expected)
    {
        // Arrange
        List<IStatement> program = [
            new LogicAndExpression(ConstFactor(left), ConstFactor(right)),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().BeEquivalentTo(new BoolFactor(expected));
    }
    
    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(false, false, false)]
    public void Interpreter_ShouldExecuteLogicOrExpression_ForBooleans(bool left, bool right, bool expected)
    {
        // Arrange
        List<IStatement> program = [
            new LogicOrExpression(ConstFactor(left), ConstFactor(right)),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().BeEquivalentTo(new BoolFactor(expected));
    } 

    [Theory]
    [InlineData(0, true, false)]
    [InlineData(1, true, true)]
    [InlineData(true, 0, false)]
    [InlineData(true, 1, true)]
    [InlineData(1, 1, true)]
    [InlineData("", true, false)]
    [InlineData("asd", true, true)]
    [InlineData(true, "", false)]
    [InlineData(true, "asd", true)]
    [InlineData("asd", "asd", true)]
    [InlineData(true, null, false)]
    [InlineData(null, true,false)]
    public void Interpreter_ShouldExecuteLogicAndExpression_WhenFactorsAreNotBooleans(object? left, object? right, bool expected)
    {
        // Arrange
        List<IStatement> program = [
            new LogicAndExpression(ConstFactor(left), ConstFactor(right)),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().BeEquivalentTo(new BoolFactor(expected));
    }
    
    [Theory]
    [InlineData(0, false, false)]
    [InlineData(1, false, true)]
    [InlineData(false, 0, false)]
    [InlineData(false, 1, true)]
    [InlineData(1, 1, true)]
    [InlineData("", false, false)]
    [InlineData("asd", false, true)]
    [InlineData(false, "", false)]
    [InlineData(false, "asd", true)]
    [InlineData("asd", "asd", true)]
    [InlineData(true, null, true)]
    [InlineData(null, true,true)]
    public void Interpreter_ShouldExecuteLogicOrExpression_WhenFactorsAreNotBooleans(object? left, object? right, bool expected)
    {
        // Arrange
        List<IStatement> program = [
            new LogicAndExpression(ConstFactor(left), ConstFactor(right)),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().BeEquivalentTo(new BoolFactor(expected));
    } 
    
    #endregion

    #region Compere

    [Theory]
    // ==
    [InlineData(TokenType.OperatorEqual,         2, 2, true)]
    [InlineData(TokenType.OperatorEqual,         2, 1, false)]
    [InlineData(TokenType.OperatorEqual,         1, 3, false)]
    // <=
    [InlineData(TokenType.OperatorLessEqual,     2, 2, true)]
    [InlineData(TokenType.OperatorLessEqual,     1, 2, true)]
    [InlineData(TokenType.OperatorLessEqual,     3, 2, false)]

    // <
    [InlineData(TokenType.OperatorLess,          1, 2, true)]
    [InlineData(TokenType.OperatorLess,          2, 2, false)]
    [InlineData(TokenType.OperatorLess,          3, 2, false)]

    // >=
    [InlineData(TokenType.OperatorGreaterEqual,  2, 2, true)]
    [InlineData(TokenType.OperatorGreaterEqual,  3, 2, true)]
    [InlineData(TokenType.OperatorGreaterEqual,  1, 2, false)]

    // >
    [InlineData(TokenType.OperatorGreater,       3, 2, true)]
    [InlineData(TokenType.OperatorGreater,       2, 2, false)]
    [InlineData(TokenType.OperatorGreater,       1, 2, false)]

    // !=
    [InlineData(TokenType.OperatorNotEqual,      2, 1, true)]
    [InlineData(TokenType.OperatorNotEqual,      2, 2, false)]
    [InlineData(TokenType.OperatorNotEqual,      1, 2, true)]
    public void Interpreter_ShouldExecuteCompereExpression_WhenTwoFactorsAreInts(TokenType tokenType, int left, int right, bool expected)
    {
        // Arrange
        List<IStatement> program = [
            new CompereExpression(ConstFactor(left), ConstFactor(right), tokenType),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().BeEquivalentTo(new BoolFactor(expected));
    }
    
    [Theory]
    [InlineData("foo",    "foo",    true)]
    [InlineData("foo",    "bar",    false)]
    [InlineData(true,     true,     true)]
    [InlineData(true,     false,    false)]
    [InlineData(null,     null,     true)]
    [InlineData(null,     "text",   false)]
    [InlineData("text",   null,     false)]
    [InlineData(0,        true,     false)]
    [InlineData(0,        false,     true)]
    public void Interpreter_ShouldExecuteCompereExpression_WhenTwoFactorsAreNotInt(object? left, object? right, bool expected)
    {
        // Arrange
        List<IStatement> program = [
            new CompereExpression(ConstFactor(left), ConstFactor(right), TokenType.OperatorEqual),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().BeEquivalentTo(new BoolFactor(expected));
    }

    #endregion

    #endregion

    private static IFactorValue ExecuteProgram(List<IStatement> statements, List<BuildInMethod>? methods = null)
    {
        var program = new Block(statements);
        return new LanguageInterpreter(methods ?? []).ExecuteProgram(program);
    }
    
    private static ConstFactor ConstFactor() => new ConstFactor(new NullFactor(), Position.Default);
    private static ConstFactor ConstFactor(int value) => new ConstFactor(new IntFactor(value), Position.Default);
    private static ConstFactor ConstFactor(bool value) => new ConstFactor(new BoolFactor(value), Position.Default);
    private static ConstFactor ConstFactor(string value) => new ConstFactor(new StringFactor(value), Position.Default);

    private static ConstFactor ConstFactor(object? value) => value switch
    {
        null => ConstFactor(),
        int v => ConstFactor(v),
        bool v => ConstFactor(v),
        string v => ConstFactor(v),
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };
    
    private static FunctionCall AddToOutput(IExpression expression) => new FunctionCall(DebugMethod.IDENTIFIER, [expression]);
    private static FunctionCall AddToOutput(string variableName) => AddToOutput(new VariableFactor(variableName));
}