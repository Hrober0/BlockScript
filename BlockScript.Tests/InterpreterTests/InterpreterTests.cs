using BlockScript.Exceptions;
using BlockScript.Interpreter;
using BlockScript.Interpreter.BuildInMethods;
using BlockScript.Interpreter.InternalFactorValues;
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
        result.Should().Be(new NullFactor());
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

    [Fact]
    public void Interpreter_ShouldExecuteBreak_WithoutArgument_AndExitFromCurrentBlock()
    {
        // Arrange
        List<IStatement> program = [
            ConstFactor(1),
            new Break(null),
            ConstFactor(2),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().Be(new IntFactor(1));
    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(true)]
    public void Interpreter_ShouldExecuteBreak_WithPositiveArgument_AndExitFromCurrentBlock(object value)
    {
        // Arrange
        List<IStatement> program = [
            ConstFactor(1),
            new Break(ConstFactor(value)),
            ConstFactor(2),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().Be(new IntFactor(1));
    }
    
    [Fact]
    public void Interpreter_ShouldThrow_WhenAttemptedToBreakBlock_ThatIsTopLevelBlock()
    {
        // Arrange
        List<IStatement> program = [
            ConstFactor(1),
            new Break(ConstFactor(2)),
            ConstFactor(2),
        ];
        
        // Act
        var act = () => ExecuteProgram(program);
        
        // Assert
        act.Should().Throw<RuntimeException>()
           .WithMessage("*Attempted to break from top level block*");
    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(true)]
    [InlineData(false)]
    [InlineData(null)]
    [InlineData(-1000)]
    public void Interpreter_ShouldExecuteBreak_NotOverrideLastValue(object? value)
    {
        // Arrange
        List<IStatement> program = [
            ConstFactor(69),
            new Break(ConstFactor(value)),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().Be(new IntFactor(69));
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(false)]
    [InlineData(null)]
    [InlineData(-1000)]
    public void Interpreter_ShouldExecuteBreak_WithNotPositiveArgument_AndBeIgnored(object? value)
    {
        // Arrange
        List<IStatement> program = [
            ConstFactor(1),
            new Break(ConstFactor(value)),
            ConstFactor(2),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().Be(new IntFactor(2));
    }
    
    [Fact]
    public void Interpreter_ShouldExecuteBreak_WithInt2Argument_AndExitFromTwoBlocks()
    {
        /*
         1
         {
            11
            break 2
            12
         }
         2
         */
        // Arrange
        List<IStatement> program = [
            ConstFactor(1),
            new Block([
                ConstFactor(11),
                new Break(ConstFactor(2)),
                ConstFactor(12),
            ]),
            ConstFactor(2),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().Be(new IntFactor(11));
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
            new Declaration("myVar", ConstFactor()),
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
        var contextLambda = result.Should().BeOfType<FunctionContext>().Subject;
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
        var (lambda, context) = result.Should().BeOfType<FunctionContext>().Subject;
        lambda.Arguments.Should().BeEquivalentTo(["a1"]);
        lambda.Body.Should().BeOfType<VariableFactor>();
        context.Should().NotBeNull();
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
            new FunctionCall(new Lambda([], ConstFactor(1)), []),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().BeEquivalentTo(new IntFactor(1));
    }
    
    [Fact]
    public void Interpreter_ShouldExecuteFunctionCall_FromVariable()
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
    public void Interpreter_ShouldExecuteFunctionCall_FromBlock()
    {
        // Arrange
        List<IStatement> program = [
            new FunctionCall(new Block([new Lambda([], ConstFactor(1))]), []),
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
    public void Interpreter_ShouldThrow_WhenExecuteFunctionCall_IsNotCallableBlock()
    {
        // Arrange
        List<IStatement> program = [
            new FunctionCall(new Block([]), []),
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
                new Declaration("ff", new Lambda([], new Assign("a", new ArithmeticalAddExpression(new VariableFactor("a"), ConstFactor(1))))),
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
                    new Assign("i", new ArithmeticalSubtractExpression(new VariableFactor("i"), ConstFactor(1))),
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
                    new Assign("i", new ArithmeticalAddExpression(new VariableFactor("i"), ConstFactor(1))),
                    new CompereLessExpression(new VariableFactor("i"), ConstFactor(6)),
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
    
    [Fact]
    public void Interpreter_ShouldExecuteBreak_WithInt2Argument_AndExitFromLoop()
    {
        /*
         1;
         loop true {
            11;
            break 2;
            12;
         };
         2;
         */
        // Arrange
        List<IStatement> program = [
            ConstFactor(1),
            new Loop(ConstFactor(true), new Block([
                ConstFactor(11),
                new Break(ConstFactor(2)),
                ConstFactor(12),
            ])),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().Be(new IntFactor(11));
    }
    
    [Fact]
    public void Interpreter_ShouldExecuteBreak_WithInt2Argument_AndSkipLoopBlock()
    {
        /*
         a = true;
         loop a {
            a := false;
            11;
            break 1;
            12;
         };
         2;
         */
        // Arrange
        List<IStatement> program = [
            new Declaration("a", ConstFactor(true)),
            new Loop(new VariableFactor("a"), new Block([
                new Assign("a", ConstFactor(false)),
                ConstFactor(11),
                new Break(ConstFactor(1)),
                ConstFactor(12),
            ])),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().Be(new IntFactor(11));
    }
    
    [Fact]
    public void Interpreter_ShouldExecuteBreak_WithInt1Argument_AndExitFromLoop()
    {
        /*
         loop true break;
         */
        // Arrange
        List<IStatement> program = [
            new Loop(ConstFactor(true), new Break(null)),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().Be(new NullFactor());
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
    
    [Fact]
    public void Interpreter_ShouldExecuteLogicAndExpression_AndNotExecuteRightExpression_WhenLeftIsFalse()
    {
        // Arrange
        var output = new List<IFactorValue>();
        List<IStatement> program =
        [
            new LogicAndExpression(ConstFactor(false), AddToOutput(ConstFactor(true))),
        ];

        // Act
        var result = ExecuteProgram(program, [new DebugMethod(output)]);

        // Assert
        result .Should().Be(new BoolFactor(false));
        output.Should().BeEmpty();
    }
    
    [Fact]
    public void Interpreter_ShouldExecuteLogicOrExpression_AndNotExecuteRightExpression_WhenLeftIsTrue()
    {
        // Arrange
        var output = new List<IFactorValue>();
        List<IStatement> program =
        [
            new LogicOrExpression(ConstFactor(true), AddToOutput(ConstFactor(false))),
        ];

        // Act
        var result = ExecuteProgram(program, [new DebugMethod(output)]);

        // Assert
        result .Should().Be(new BoolFactor(true));
        output.Should().BeEmpty();
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
            new LogicOrExpression(ConstFactor(left), ConstFactor(right)),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().BeEquivalentTo(new BoolFactor(expected));
    }

    [Fact]
    public void Interpreter_ShouldThrow_WhenLogicAndExpression_EncounterUnsupportedFactor()
    {
        // Arrange
        List<IStatement> program =
        [
            new LogicAndExpression(ReturnDummyLambda(), ConstFactor(1)),
        ];

        // Act
        var act = () => ExecuteProgram(program);

        // Assert
        act.Should().Throw<RuntimeException>()
           .WithMessage("*can not be parsed*");
    }
    
    [Fact]
    public void Interpreter_ShouldThrow_WhenLogicOrExpression_EncounterUnsupportedFactor()
    {
        // Arrange
        List<IStatement> program =
        [
            new LogicOrExpression(ReturnDummyLambda(), ConstFactor(1)),
        ];

        // Act
        var act = () => ExecuteProgram(program);

        // Assert
        act.Should().Throw<RuntimeException>()
           .WithMessage("*can not be parsed*");
    }

    #endregion
    
    #region Compere
    
    [Theory]
    [InlineData(2, 2, true)]
    [InlineData(2, 1, false)]
    [InlineData(1, 3, false)]
    [InlineData(1, true, true)]
    [InlineData(0, true, false)]
    [InlineData(1, false, false)]
    [InlineData(0, false, true)]
    [InlineData(1, null, false)]
    [InlineData(0, null, true)]
    [InlineData(12, "12", true)]
    [InlineData(0, "0", true)]
    [InlineData(-1, "-1", true)]
    [InlineData(2, "00", false)]
    [InlineData(true,  true, true)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    [InlineData(true,  "true", true)]
    [InlineData(false, "false", true)]
    [InlineData(true, "s", false)]
    [InlineData(null, "", true)]
    [InlineData(null, "s", false)]
    [InlineData("", "", true)]
    [InlineData("asd", "asd", true)]
    [InlineData("qwe", "qwr", false)]
    public void Interpreter_ShouldExecuteCompereEqualsExpression(object? left, object? right, bool expected)
    {
        // Arrange
        List<IStatement> program = [
            new CompereEqualsExpression(ConstFactor(left), ConstFactor(right)),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().BeEquivalentTo(new BoolFactor(expected));
    }
    
    [Theory]
    [InlineData(2, 2, false)]
    [InlineData(2, 1, true)]
    [InlineData(1, 3, true)]
    [InlineData(1, true, false)]
    [InlineData(0, true, true)]
    [InlineData(1, false, true)]
    [InlineData(0, false, false)]
    [InlineData(1, null, true)]
    [InlineData(0, null, false)]
    [InlineData(12, "12", false)]
    [InlineData(0, "0", false)]
    [InlineData(-1, "-1", false)]
    [InlineData(2, "00", true)]
    [InlineData(true,  true, false)]
    [InlineData(false, false, false)]
    [InlineData(false, true, true)]
    [InlineData(true,  "true", false)]
    [InlineData(false, "false", false)]
    [InlineData(true, "s", true)]
    [InlineData(null, "", false)]
    [InlineData(null, "s", true)]
    [InlineData("", "", false)]
    [InlineData("asd", "asd", false)]
    [InlineData("qwe", "qwr", true)]
    public void Interpreter_ShouldExecuteCompereNotEqualsExpression(object? left, object? right, bool expected)
    {
        // Arrange
        List<IStatement> program = [
            new CompereNotEqualsExpression(ConstFactor(left), ConstFactor(right)),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().BeEquivalentTo(new BoolFactor(expected));
    }

    [Theory]
    [InlineData(1, 2, true)]
    [InlineData(2, 2, false)]
    [InlineData(3, 2, false)]
    [InlineData("a", "b", true)]
    [InlineData("abc", "abz", true)]
    [InlineData("z", "a", false)]
    [InlineData("a", "a", false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, false)]
    [InlineData(false, true, true)]
    [InlineData(true, "true", false)]
    [InlineData(false, "false", false)]
    [InlineData(true, "s", false)]
    [InlineData(null, "", false)]
    [InlineData(null, "s", false)]
    [InlineData("", "", false)]
    [InlineData("asd", "asd", false)]
    [InlineData("qwe", "qwr", true)]
    public void Interpreter_ShouldExecuteCompereLessExpression(object? left, object? right, bool expected)
    {
        // Arrange
        List<IStatement> program = [
            new CompereLessExpression(ConstFactor(left), ConstFactor(right)),
        ];

        // Act
        var result = ExecuteProgram(program);

        // Assert
        result.Should().BeEquivalentTo(new BoolFactor(expected));
    }

    [Fact]
    public void Interpreter_ShouldThrow_CompereLessExpression_EncounterUnsupportedFactor()
    {
        // Arrange
        List<IStatement> program = [
            new CompereLessExpression(ReturnDummyLambda(), ConstFactor(1)),
        ];

        // Act
        var act = () => ExecuteProgram(program);

        // Assert
        act.Should().Throw<RuntimeException>()
           .WithMessage("*can not be parsed*");
    }
    
    [Theory]
    [InlineData(1, 2, true)]
    [InlineData(2, 2, true)]
    [InlineData(3, 2, false)]
    [InlineData("a", "b", true)]
    [InlineData("abc", "abz", true)]
    [InlineData("z", "a", false)]
    [InlineData("a", "a", true)]
    [InlineData(true, true, true)]
    [InlineData(false, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, "true", true)]
    [InlineData(false, "false", true)]
    [InlineData(true, "s", false)]
    [InlineData(null, "", true)]
    [InlineData(null, "s", true)]
    [InlineData("", "", true)]
    [InlineData("asd", "asd", true)]
    [InlineData("qwe", "qwr", true)]
    public void Interpreter_ShouldExecuteCompereLessEqualExpression(object? left, object? right, bool expected)
    {
        // Arrange
        List<IStatement> program = [
            new CompereLessEqualExpression(ConstFactor(left), ConstFactor(right)),
        ];

        // Act
        var result = ExecuteProgram(program);

        // Assert
        result.Should().BeEquivalentTo(new BoolFactor(expected));
    }

    [Fact]
    public void Interpreter_ShouldThrow_CompereLessEqualExpression_EncounterUnsupportedFactor()
    {
        // Arrange
        List<IStatement> program = [
            new CompereLessEqualExpression(ReturnDummyLambda(), ConstFactor(1)),
        ];

        // Act
        var act = () => ExecuteProgram(program);

        // Assert
        act.Should().Throw<RuntimeException>()
           .WithMessage("*can not be parsed*");
    }
    
    [Theory]
    [InlineData(1, 2, false)]
    [InlineData(2, 2, false)]
    [InlineData(3, 2, true)]
    [InlineData("a", "b", false)]
    [InlineData("abc", "abz", false)]
    [InlineData("z", "a", true)]
    [InlineData("a", "a", false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, "true", false)]
    [InlineData(false, "false", false)]
    [InlineData(true, "s", true)]
    [InlineData(null, "", false)]
    [InlineData(null, "s", false)]
    [InlineData("", "", false)]
    [InlineData("asd", "asd", false)]
    [InlineData("qwe", "qwr", false)]
    public void Interpreter_ShouldExecuteCompereGreaterExpression(object? left, object? right, bool expected)
    {
        // Arrange
        List<IStatement> program = [
            new CompereGreaterExpression(ConstFactor(left), ConstFactor(right)),
        ];

        // Act
        var result = ExecuteProgram(program);

        // Assert
        result.Should().BeEquivalentTo(new BoolFactor(expected));
    }

    [Fact]
    public void Interpreter_ShouldThrow_CompereGreaterExpression_EncounterUnsupportedFactor()
    {
        // Arrange
        List<IStatement> program = [
            new CompereGreaterExpression(ReturnDummyLambda(), ConstFactor(1)),
        ];

        // Act
        var act = () => ExecuteProgram(program);

        // Assert
        act.Should().Throw<RuntimeException>()
           .WithMessage("*can not be parsed*");
    }
    
    [Theory]
    [InlineData(1, 2, false)]
    [InlineData(2, 2, true)]
    [InlineData(3, 2, true)]
    [InlineData("a", "b", false)]
    [InlineData("abc", "abz", false)]
    [InlineData("z", "a", true)]
    [InlineData("a", "a", true)]
    [InlineData(true, true, true)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    [InlineData(true, "true", true)]
    [InlineData(false, "false", true)]
    [InlineData(true, "s", true)]
    [InlineData(null, "", true)]
    [InlineData(null, "s", true)]
    [InlineData("", "", true)]
    [InlineData("asd", "asd", true)]
    [InlineData("qwe", "qwr", false)]
    public void Interpreter_ShouldExecuteCompereGreaterEqualExpression(object? left, object? right, bool expected)
    {
        // Arrange
        List<IStatement> program = [
            new CompereGreaterEqualExpression(ConstFactor(left), ConstFactor(right)),
        ];

        // Act
        var result = ExecuteProgram(program);

        // Assert
        result.Should().BeEquivalentTo(new BoolFactor(expected));
    }

    [Fact]
    public void Interpreter_ShouldThrow_CompereGreaterEqualExpression_EncounterUnsupportedFactor()
    {
        // Arrange
        List<IStatement> program = [
            new CompereGreaterEqualExpression(ReturnDummyLambda(), ConstFactor(1)),
        ];

        // Act
        var act = () => ExecuteProgram(program);

        // Assert
        act.Should().Throw<RuntimeException>()
           .WithMessage("*can not be parsed*");
    }
    
    #endregion

    #region Null Coalescing

    [Fact]
    private void Interpreter_ShouldExecuteNullCheckExpression_AndNotAssign_WhenValueIsNotNull()
    {
        // Arrange
        List<IStatement> program = [
            new NullCoalescingExpression(ConstFactor(1), ConstFactor(2)),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().Be(new IntFactor(1));
    }
    
    [Fact]
    private void Interpreter_ShouldExecuteNullCheckExpression_AndAssign_WhenValueIsNull()
    {
        // Arrange
        List<IStatement> program = [
            new NullCoalescingExpression(ConstFactor(), ConstFactor(2)),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().Be(new IntFactor(2));
    }

    [Fact]
    private void Interpreter_ShouldExecuteNullCheckExpression_AndNotExecuteRightSide_WhenValueIsNotNull()
    {
        // Arrange
        var debug = new List<IFactorValue>();
        List<IStatement> program = [
            new NullCoalescingExpression(ConstFactor(1), 
                new FunctionCall(DebugMethod.IDENTIFIER, [ConstFactor(2)]))
        ];
        
        // Act
        var result = ExecuteProgram(program, [new DebugMethod(debug)]);
        
        // Assert
        result.Should().Be(new IntFactor(1));
        debug.Should().BeEmpty();
    }
    
    #endregion
    
    #region Arithmetical

    [Theory]
    // strings
    [InlineData("ab", "cd", "abcd")]
    [InlineData("abcd", null, "abcd")]
    [InlineData(null, "abcd","abcd")]
    [InlineData("abcd", 1, "abcd1")]
    [InlineData(3, "abcd","3abcd")]
    [InlineData("abcd", true, "abcdtrue")]
    [InlineData(false, "abcd", "falseabcd")]
    // ints
    [InlineData(2, 2, 4)]
    [InlineData(2, -4, -2)]
    [InlineData(2, 40000, 40002)]
    [InlineData(2, true, 3)]
    [InlineData(true, 2, 3)]
    [InlineData(2, false, 2)]
    [InlineData(false, 2, 2)]
    [InlineData(2, null, 2)]
    [InlineData(null, 2, 2)]
    public void Interpreter_ShouldExecuteArithmeticalAddExpression(object? left, object? right, object expected)
    {
        // Arrange
        List<IStatement> program = [
            new ArithmeticalAddExpression(ConstFactor(left), ConstFactor(right)),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().Be(ConstFactor(expected).Value);
    }

    [Fact]
    public void Interpreter_ShouldThrow_WhenArithmeticalAddExpression_EncounterUnsupportedFactor()
    {
        // Arrange
        List<IStatement> program = [
            new ArithmeticalAddExpression(ReturnDummyLambda(), ConstFactor(1)),
        ];
        
        // Act
        var act = () => ExecuteProgram(program);
        
        // Assert
        act.Should().Throw<RuntimeException>()
           .WithMessage("*can not be parsed*");
    }

    [Theory]
    [InlineData(2, 2, 0)]
    [InlineData(2, -4, 6)]
    [InlineData(2, 40000, -39998)]
    [InlineData(2, true, 1)]
    [InlineData(true, 2, -1)]
    [InlineData(2, false, 2)]
    [InlineData(false, 2, -2)]
    [InlineData(2, null, 2)]
    [InlineData(null, 2, -2)]
    public void Interpreter_ShouldExecuteArithmeticalSubtractExpression(object? left, object? right, object expected)
    {
        // Arrange
        List<IStatement> program = [
            new ArithmeticalSubtractExpression(ConstFactor(left), ConstFactor(right)),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().Be(ConstFactor(expected).Value);
    }

    [Fact]
    public void Interpreter_ShouldThrow_WhenArithmeticalSubtractExpression_EncounterUnsupportedStringFactor()
    {
        // Arrange
        List<IStatement> program = [
            new ArithmeticalSubtractExpression(ConstFactor("a"), ConstFactor(1)),
        ];
        
        // Act
        var act = () => ExecuteProgram(program);
        
        // Assert
        act.Should().Throw<RuntimeException>()
           .WithMessage("*can not be parsed*");
    }
    
    [Fact]
    public void Interpreter_ShouldThrow_WhenArithmeticalSubtractExpression_EncounterUnsupportedFactor()
    {
        // Arrange
        List<IStatement> program = [
            new ArithmeticalSubtractExpression(ReturnDummyLambda(), ConstFactor(1)),
        ];
        
        // Act
        var act = () => ExecuteProgram(program);
        
        // Assert
        act.Should().Throw<RuntimeException>()
           .WithMessage("*can not be parsed*");
    }
    
    [Theory]
    [InlineData(2, 2, 4)]
    [InlineData(2, -4, -8)]
    [InlineData(2, 40000, 80000)]
    [InlineData(2, true, 2)]
    [InlineData(true, 2, 2)]
    [InlineData(2, false, 0)]
    [InlineData(false, 2, 0)]
    [InlineData(2, null, 0)]
    [InlineData(null, 2, 0)]
    public void Interpreter_ShouldExecuteArithmeticalMultiplyExpression(object? left, object? right, object expected)
    {
        // Arrange
        List<IStatement> program = [
            new ArithmeticalMultiplyExpression(ConstFactor(left), ConstFactor(right)),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().Be(ConstFactor(expected).Value);
    }

    [Fact]
    public void Interpreter_ShouldThrow_WhenArithmeticalMultiplyExpression_EncounterUnsupportedStringFactor()
    {
        // Arrange
        List<IStatement> program = [
            new ArithmeticalMultiplyExpression(ConstFactor("a"), ConstFactor(1)),
        ];
        
        // Act
        var act = () => ExecuteProgram(program);
        
        // Assert
        act.Should().Throw<RuntimeException>()
           .WithMessage("*can not be parsed*");
    }
    
    [Fact]
    public void Interpreter_ShouldThrow_WhenArithmeticalMultiplyExpression_EncounterUnsupportedFactor()
    {
        // Arrange
        List<IStatement> program = [
            new ArithmeticalMultiplyExpression(ReturnDummyLambda(), ConstFactor(1)),
        ];
        
        // Act
        var act = () => ExecuteProgram(program);
        
        // Assert
        act.Should().Throw<RuntimeException>()
           .WithMessage("*can not be parsed*");
    }
    
    [Theory]
    [InlineData(2, 2, 1)]
    [InlineData(6, -3, -2)]
    [InlineData(40000, 2, 20000)]
    [InlineData(7, 3, 2)]
    [InlineData(8, 3, 2)]
    [InlineData(1, 2, 0)]
    [InlineData(0, 2, 0)]
    [InlineData(2, true, 2)]
    [InlineData(true, 2, 0)]
    [InlineData(false, 2, 0)]
    [InlineData(null, 2, 0)]
    public void Interpreter_ShouldExecuteArithmeticalDivideExpression(object? left, object? right, object expected)
    {
        // Arrange
        List<IStatement> program = [
            new ArithmeticalDivideExpression(ConstFactor(left), ConstFactor(right)),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().Be(ConstFactor(expected).Value);
    }

    [Fact]
    public void Interpreter_ShouldThrow_WhenArithmeticalDivideExpression_EncounterUnsupportedStringFactor()
    {
        // Arrange
        List<IStatement> program = [
            new ArithmeticalDivideExpression(ConstFactor("a"), ConstFactor(1)),
        ];
        
        // Act
        var act = () => ExecuteProgram(program);
        
        // Assert
        act.Should().Throw<RuntimeException>()
           .WithMessage("*can not be parsed*");
    }
    
    [Fact]
    public void Interpreter_ShouldThrow_WhenArithmeticalDivideExpression_EncounterUnsupportedFactor()
    {
        // Arrange
        List<IStatement> program = [
            new ArithmeticalDivideExpression(ReturnDummyLambda(), ConstFactor(1)),
        ];
        
        // Act
        var act = () => ExecuteProgram(program);
        
        // Assert
        act.Should().Throw<RuntimeException>()
           .WithMessage("*can not be parsed*");
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(null)]
    [InlineData(false)]
    public void Interpreter_ShouldThrow_WhenArithmeticalDivideExpression_EncounterDividedByZero(object? divider)
    {
        // Arrange
        List<IStatement> program = [
            new ArithmeticalDivideExpression(ConstFactor(1), ConstFactor(divider)),
        ];
        
        // Act
        var act = () => ExecuteProgram(program);
        
        // Assert
        act.Should().Throw<RuntimeException>()
           .WithMessage("*divide by zero*");
    }
    
    #endregion
    
    #region Not

    [Theory]
    [InlineData(1, -1)]
    [InlineData(0, 0)]
    [InlineData(-2, 2)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    private void Interpreter_ShouldExecuteNotExpression(object value, object expected)
    {
        // Arrange
        List<IStatement> program = [
            new NotExpression(ConstFactor(value)),
        ];
        
        // Act
        var result = ExecuteProgram(program);
        
        // Assert
        result.Should().Be(ConstFactor(expected).Value);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("as")]
    private void Interpreter_ShouldThrow_WhenFactorIsNotSupportedByNotExpression(object? value)
    {
        // Arrange
        List<IStatement> program = [
            new NotExpression(ConstFactor(value)),
        ];
        
        // Act
        var act = () => ExecuteProgram(program);
        
        // Assert
        act.Should().Throw<RuntimeException>()
           .WithMessage("*not defined*");
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

    private static Block ReturnDummyLambda() => new Block([new Lambda([], ConstFactor())]);
}