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
        result.Should().BeOfType<IntFactor>().Which.Value.Should().Be(69);
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
        result.Should().BeOfType<NullFactor>();
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
        result.Should().BeOfType<IntFactor>().Which.Value.Should().Be(69);
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
            new Assign("myVar", ConstFactor(1)),
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
    public void Interpreter_ShouldExecutePrint()
    {
        // Arrange
        var output = new StringWriter();
        Console.SetOut(output);
        List<IStatement> program = [
            new FunctionCall("print", [ConstFactor(2)]),
        ];
        
        // Act
        var result = ExecuteProgram(program, [new Print()]);
        
        // Assert
        result.Should().BeOfType<IntFactor>().Which.Value.Should().Be(2);
        
        var lines = output.ToString().Split(Environment.NewLine);
        lines.Should().HaveCountGreaterThan(0);
        lines[0].Should().Be(new IntFactor(2).ToString());
    }
    
    [Fact]
    public void Interpreter_ShouldExecuteTestBuildInMethod()
    {
        // Arrange
        var output = new List<IFactorValue>();
        List<IStatement> program = [
            new FunctionCall(TestMethod.IDENTIFIER, [ConstFactor(2)]),
            new FunctionCall(TestMethod.IDENTIFIER, [ConstFactor(3)]),
        ];
        
        // Act
        ExecuteProgram(program, [new TestMethod(output)]);
        
        // Assert
        output.Should().BeEquivalentTo([new IntFactor(2), new IntFactor(3)]);
    }
    
    [Fact]
    public void Interpreter_ShouldThrow_WhenBuildInMethodIsNotRegistered()
    {
        // Arrange
        List<IStatement> program = [
            new FunctionCall(TestMethod.IDENTIFIER, [ConstFactor(2)]),
        ];
        
        // Act
        var act = () => ExecuteProgram(program, []);
        
        // Assert
        act.Should().Throw<RuntimeException>()
           .WithMessage($"*Variable of name {TestMethod.IDENTIFIER} was not defined!*");
    }

    #endregion

    #region FunctionCall

    [Fact]
    public void Interpreter_ShouldExecuteFunctionCall()
    {
        // Arrange
        List<IStatement> program = [
            new Assign("x", new Lambda([], ConstFactor(1))),
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
            new FunctionCall(TestMethod.IDENTIFIER, [ConstFactor(1)]),
            new FunctionCall(TestMethod.IDENTIFIER, [ConstFactor(2)]),
        ];
        
        // Act
        ExecuteProgram(program, [new TestMethod(output)]);
        
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
            new FunctionCall(TestMethod.IDENTIFIER, [new FunctionCall("fc", [])]),  // should add 4 to output
            new Declaration("fc", new FunctionCall("f", [])),
            new FunctionCall(TestMethod.IDENTIFIER, [new FunctionCall("fc", [])]),  // should add 4 to output
            new FunctionCall(TestMethod.IDENTIFIER, [new FunctionCall("fc", [])]),  // should add 5 to output
            new FunctionCall(TestMethod.IDENTIFIER, [new FunctionCall("fc", [])]),  // should add 6 to output
        ];
        
        // Act
        ExecuteProgram(program, [new TestMethod(output)]);
        
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
    
    #endregion

    #region Expressions

    

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
}