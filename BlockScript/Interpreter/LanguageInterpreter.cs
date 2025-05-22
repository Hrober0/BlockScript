using BlockScript.Exceptions;
using BlockScript.Interpreter.BuildInMethods;
using BlockScript.Lexer;
using BlockScript.Lexer.FactorValues;
using BlockScript.Parser.Expressions;
using BlockScript.Parser.Factors;
using BlockScript.Parser.Statements;
using BlockScript.Reader;

namespace BlockScript.Interpreter;

public class LanguageInterpreter(List<BuildInMethod> buildInMethods)
{
    private const int RECURSION_LIMIT = 10;
    
    private readonly Stack<Context> _contextStack = new();
    
    private Context CurrentContext => _contextStack.Peek();
    
    public IFactorValue ExecuteProgram(Block block)
    {
        var startContext = new Context(null);
        foreach (var method in buildInMethods)
        {
            startContext.AddData(method.Identifier, new Lambda(method.Arguments, method, method.Position));
        }
        _contextStack.Push(startContext);
        return Execute(block);
    }
    
    private IFactorValue Execute(IStatement statement)
    {
        return statement switch
        {
            // statements
            Block s => Execute(s),
            Assign s => Execute(s),
            Lambda s => Execute(s),
            BuildInMethod s => Execute(s),
            FunctionCall s => Execute(s),
            Condition s => Execute(s),
            
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

    private IFactorValue Execute(Block block)
    {
        IFactorValue? returnValue = null;
        foreach (var statement in block.Statements)
        {
            returnValue = Execute(statement);
        }
        return returnValue ?? new NullFactor();
    }

    private IFactorValue Execute(Assign assign)
    {
        if (assign.NullAssign && CurrentContext.TryGetData(assign.Identifier, out var dataValue) && dataValue is not NullFactor)
        {
            return dataValue;
        }

        var newValue = Execute(assign.Value);
        CurrentContext.AddData(assign.Identifier, newValue);
        return newValue;
    }

    private IFactorValue Execute(Lambda lambda) => lambda;
    
    private IFactorValue Execute(BuildInMethod buildInMethod) => buildInMethod.Execute(CurrentContext);

    private IFactorValue Execute(FunctionCall functionCall)
    {
        var dataValue = CurrentContext.GetContextData(functionCall.Identifier, functionCall.Position);

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

    private IFactorValue Execute(Condition condition)
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

        return new NullFactor();
    }

    #endregion

    #region Expressions

    private IFactorValue Execute(CompereExpression compereExpression)
    {
        var left = ParseInt(Execute(compereExpression.Lhs), compereExpression.Lhs.Position);
        var right = ParseInt(Execute(compereExpression.Rhs), compereExpression.Rhs.Position);
        var result = compereExpression.Operator switch
        {
            TokenType.OperatorEqual => left == right,
            TokenType.OperatorLessEqual => left <= right,
            TokenType.OperatorLess => left < right,
            TokenType.OperatorGreaterEqual => left >= right,
            TokenType.OperatorGreater => left > right,
            TokenType.OperatorNotEqual => left != right,
            _ => throw new RuntimeException(compereExpression.Position, $"Unexpected {compereExpression.Operator.TextValue()} operator in compere expression!")
        };
        return new BoolFactor(result);
    }
    
    private IFactorValue Execute(ArithmeticalExpression arithmeticalExpression)
    {
        var left = Execute(arithmeticalExpression.Lhs);
        var right = Execute(arithmeticalExpression.Rhs);

        if (arithmeticalExpression.Operator is TokenType.OperatorAdd && left is StringFactor || right is StringFactor)
        {
            var leftString = ParseString(left, arithmeticalExpression.Lhs.Position);
            var rightString = ParseString(right, arithmeticalExpression.Rhs.Position);
            return new StringFactor(leftString + rightString);
        }
        
        var leftNumber = ParseInt(left, arithmeticalExpression.Lhs.Position);
        var rightNumber = ParseInt(right, arithmeticalExpression.Rhs.Position);
        int result = arithmeticalExpression.Operator switch
        {
            TokenType.OperatorAdd => leftNumber + rightNumber,
            TokenType.OperatorSubtract => leftNumber - rightNumber,
            TokenType.OperatorMultiply => leftNumber * rightNumber,
            TokenType.OperatorDivide => DivideSave(leftNumber, rightNumber),
            _ => throw new RuntimeException(arithmeticalExpression.Position, $"Unexpected {arithmeticalExpression.Operator.TextValue()} operator in compere expression!")
        };
        return new IntFactor(result);

        int DivideSave(int dividend, int divisor)
        {
            if (divisor == 0)
            {
                throw new RuntimeException(arithmeticalExpression.Position, $"Cannot divide by zero!");
            }
            return dividend / divisor;
        }
    }

    #endregion

    #region Factor

    private IFactorValue Execute(VariableFactor variable) => CurrentContext.GetContextData(variable.Identifier, variable.Position);

    private IFactorValue Execute(ConstFactor constFactor) => constFactor.Value;

    #endregion

    #region Parsing

    private static bool ParseBool(IFactorValue value, Position position)
    {
        return value switch
        {
            IntFactor v => v.Value > 0,
            StringFactor v => v.Value.Length > 0,
            BoolFactor v => v.Value,
            NullFactor => false,
            _ => throw new RuntimeException(position, $"{value} can not be parsed to bool value!")
        };
    }
    
    private static int ParseInt(IFactorValue value, Position position)
    {
        return value switch
        {
            IntFactor v => v.Value,
            BoolFactor v => v.Value ? 1 : 0,
            NullFactor => 0,
            _ => throw new RuntimeException(position, $"{value} can not be parsed to int value!")
        };
    }

    private static string ParseString(IFactorValue value, Position position)
    {
        return value switch
        {
            StringFactor v => v.Value,
            IntFactor v => v.Value.ToString(),
            BoolFactor v => v.Value ? "true" : "false",
            NullFactor => "",
            _ => throw new RuntimeException(position, $"{value} can not be parsed to int value!")
        };
    }
    
    #endregion
}