using BlockScript.Exceptions;
using BlockScript.Lexer;
using BlockScript.Parser.Factors;
using BlockScript.Parser.Statements;

namespace BlockScript.Interpreter;

public class LanguageInterpreter
{
    private const int RECURSION_LIMIT = 10;
    
    private readonly Stack<Context> _contextStack = new();
    
    private Context CurrentContext => _contextStack.Peek();
    
    public object? ExecuteProgram(Block block)
    {
        _contextStack.Push(new Context(null));
        return Execute(block);
    }
    private object? Execute(IStatement statement)
    {
        return statement switch
        {
            Block typedStatement => Execute(typedStatement),
            Assign typedStatement => Execute(typedStatement),
            Lambda typedStatement => Execute(typedStatement),
            FunctionCall typedStatement => Execute(typedStatement),
            VariableFactor typedStatement => Execute(typedStatement),
            ConstFactor typedStatement => Execute(typedStatement),
            _ => throw new Exception($"Unknown statement type of {statement.GetType()}")
        };
    }
    
    private object? Execute(Block block)
    {
        object? returnValue = null;
        foreach (var statement in block.Statements)
        {
            returnValue = Execute(statement);
        }
        return returnValue;
    }

    private object? Execute(Assign assign)
    {
        if (assign.NullAssign && CurrentContext.TryGetData(assign.Identifier, out var dataValue) && dataValue != null)
        {
            return dataValue;
        }

        var newValue = Execute(assign.Value);
        CurrentContext.AddData(assign.Identifier, newValue);
        return newValue;
    }

    private object? Execute(Lambda lambda) => lambda;

    private object? Execute(FunctionCall functionCall)
    {
        var dataValue = GetContextData(functionCall.Identifier, functionCall.Position);

        if (dataValue is not Lambda lambda)
        {
            throw new RuntimeException(functionCall.Position, $"{dataValue} is not callable!");
        }

        if (lambda.Arguments.Count != functionCall.Arguments.Count)
        {
            throw new RuntimeException(functionCall.Position, $"Function {functionCall.Identifier} requires {lambda.Arguments.Count} arguments, but given {functionCall.Arguments.Count} arguments!");
        }

        if (_contextStack.Count == RECURSION_LIMIT)
        {
            throw new RuntimeException(functionCall.Position, $"Exceeded recursion limit");
        }
        
        _contextStack.Push(new Context(CurrentContext));
        for (int i = 0; i < functionCall.Arguments.Count; i++)
        {
            CurrentContext.AddData(lambda.Arguments[i], Execute(functionCall.Arguments[i]));   
        }
        var result = Execute(lambda.Body);
        _contextStack.Pop();
        return result;
    }

    private object? Execute(VariableFactor variable)
    {
        return GetContextData(variable.Identifier, variable.Position);
    }

    private object? Execute(ConstFactor constFactor)
    {
        return constFactor.Value;
    }
    
    private object? GetContextData(string identifier, Position position)
    {
        if (!CurrentContext.TryGetData(identifier, out var dataValue))
        {
            throw new RuntimeException(position, $"Variable of name {identifier} was not defined!");
        }

        return dataValue;
    }
}