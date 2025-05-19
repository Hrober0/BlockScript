using BlockScript.Exceptions;
using BlockScript.Interpreter.BuildInStatements;
using BlockScript.Lexer;
using BlockScript.Parser.Expressions;
using BlockScript.Parser.Factors;
using BlockScript.Parser.Statements;
using BlockScript.Reader;

namespace BlockScript.Interpreter;

public class LanguageInterpreter
{
    private const int RECURSION_LIMIT = 10;
    
    private readonly Stack<Context> _contextStack = new();
    
    private Context CurrentContext => _contextStack.Peek();
    
    public object? ExecuteProgram(Block block)
    {
        var startContext = new Context(null);
        startContext.AddData("print", new Lambda([Print.PARAMTEER_NAME], new Print(Position.Default), Position.Default));
        _contextStack.Push(startContext);
        return Execute(block);
    }
    
    private object? Execute(IStatement statement)
    {
        return statement switch
        {
            // statements
            Block s => Execute(s),
            Assign s => Execute(s),
            Lambda s => Execute(s),
            FunctionCall s => Execute(s),
            Condition s => Execute(s),
            Print s => Execute(s),
            
            // expressions
            CompereExpression s => Execute(s),
            ArithmeticalExpression s => Execute(s),
            
            // factors
            VariableFactor s => Execute(s),
            ConstFactor s => Execute(s),
            
            _ => throw new Exception($"Unknown statement type of {statement.GetType()}")
        };
    }

    #region Statements

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

    private object? Execute(Condition condition)
    {
        foreach (var conditionItem in condition.ConditionaryItems)
        {
            if (ParseBool(Execute(conditionItem.condition), conditionItem.condition.Position))
            {
                return Execute(conditionItem.body);
            }  
        }

        if (condition.ElseBody != null)
        {
            return Execute(condition.ElseBody);
        }

        return null;
    }
    
    private object? Execute(Print _)
    {
        var value = GetContextData(Print.PARAMTEER_NAME, Position.Default);
        Console.WriteLine(value);
        return value;
    }
    
    #endregion

    #region Expressions

    private object? Execute(CompereExpression compereExpression)
    {
        var left = ParseInt(Execute(compereExpression.Lhs), compereExpression.Lhs.Position);
        var right = ParseInt(Execute(compereExpression.Rhs), compereExpression.Rhs.Position);
        return compereExpression.Operator switch
        {
            TokenType.OperatorEqual => left == right,
            TokenType.OperatorLessEqual => left <= right,
            TokenType.OperatorLess => left < right,
            TokenType.OperatorGreaterEqual => left >= right,
            TokenType.OperatorGreater => left > right,
            TokenType.OperatorNotEqual => left != right,
            _ => throw new RuntimeException(compereExpression.Position, $"Unexpected {compereExpression.Operator.TextValue()} operator in compere expression!")
        };
    }
    
    private object? Execute(ArithmeticalExpression arithmeticalExpression)
    {
        var left = Execute(arithmeticalExpression.Lhs);
        var right = Execute(arithmeticalExpression.Rhs);

        if (arithmeticalExpression.Operator is TokenType.OperatorAdd && left is string || right is string)
        {
            var leftString = ParseString(left, arithmeticalExpression.Lhs.Position);
            var rightString = ParseString(right, arithmeticalExpression.Rhs.Position);
            return leftString + rightString;
        }
        
        var leftNumber = ParseInt(left, arithmeticalExpression.Lhs.Position);
        var rightNumber = ParseInt(right, arithmeticalExpression.Rhs.Position);
        return arithmeticalExpression.Operator switch
        {
            TokenType.OperatorAdd => leftNumber + rightNumber,
            TokenType.OperatorSubtract => leftNumber - rightNumber,
            TokenType.OperatorMultiply => leftNumber * rightNumber,
            TokenType.OperatorDivide => () => {
                if (rightNumber == 0)
                {
                    throw new RuntimeException(arithmeticalExpression.Position, $"Cannot divide by zero!");
                }
                return leftNumber / rightNumber;
            },
            _ => throw new RuntimeException(arithmeticalExpression.Position, $"Unexpected {arithmeticalExpression.Operator.TextValue()} operator in compere expression!")
        };
    }

    #endregion

    #region Factor

    private object? Execute(VariableFactor variable) => GetContextData(variable.Identifier, variable.Position);

    private object? Execute(ConstFactor constFactor) => constFactor.Value;

    #endregion
    
    private object? GetContextData(string identifier, Position position)
    {
        if (!CurrentContext.TryGetData(identifier, out var dataValue))
        {
            throw new RuntimeException(position, $"Variable of name {identifier} was not defined!");
        }

        return dataValue;
    }

    #region Parsing

    private static bool ParseBool(object? value, Position position)
    {
        return value switch
        {
            int v => v > 0,
            string v => v.Length > 0,
            bool v => v,
            null => false,
            _ => throw new RuntimeException(position, $"{value} can not be parsed to bool value!")
        };
    }
    
    private static int ParseInt(object? value, Position position)
    {
        return value switch
        {
            int v => v,
            bool v => v ? 1 : 0,
            null => 0,
            _ => throw new RuntimeException(position, $"{value} can not be parsed to int value!")
        };
    }

    private static string ParseString(object? value, Position position)
    {
        return value switch
        {
            string v => v,
            int v => v.ToString(),
            bool v => v ? "true" : "false",
            null => "",
            _ => throw new RuntimeException(position, $"{value} can not be parsed to int value!")
        };
    }
    
    #endregion
}