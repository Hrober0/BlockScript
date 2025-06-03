using System.Linq.Expressions;
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
    private const int RECURSION_LIMIT = 1_000;
    private const int LOOP_COUNT_LIMIT = 1_000;
    
    private readonly Stack<Context> _contextStack = new();
    
    private Context CurrentContext => _contextStack.Peek();
    
    public IFactorValue ExecuteProgram(Block block)
    {
        var startContext = new Context(null);
        foreach (var method in buildInMethods)
        {
            startContext.AddData(method.Identifier, new ContextLambda(new Lambda(method.Arguments, method, method.Position), startContext));
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
            NullAssign s => Execute(s),
            Declaration s => Execute(s),
            Lambda s => Execute(s),
            BuildInMethod s => Execute(s),
            FunctionCall s => Execute(s),
            Condition s => Execute(s),
            Loop s => Execute(s),
            
            // expressions
            LogicOrExpression s => Execute(s),
            LogicAndExpression s => Execute(s),
            CompereEqualsExpression s => Execute(s),
            CompereNotEqualsExpression s => Execute(s),
            CompereGreaterExpression s => Execute(s),
            CompereGreaterEqualExpression s => Execute(s),
            CompereLessExpression s => Execute(s),
            CompereLessEqualExpression s => Execute(s),
            NullCoalescingExpression s => Execute(s),
            ArithmeticalAddExpression s => Execute(s),
            ArithmeticalSubtractExpression s => Execute(s),
            ArithmeticalMultiplyExpression s => Execute(s),
            ArithmeticalDivideExpression s => Execute(s),
            NotExpression s => Execute(s),
            
            // factors
            VariableFactor s => Execute(s),
            ConstFactor s => Execute(s),
            
            _ => throw new RuntimeException(statement.Position, $"Unknown statement type of {statement.GetType()}")
        };
    }

    #region Statements

    private IFactorValue Execute(Block block)
    {
        IFactorValue? returnValue = null;
        _contextStack.Push(new Context(CurrentContext));
        foreach (var statement in block.Statements)
        {
            returnValue = Execute(statement);
        }
        _contextStack.Pop();
        return returnValue ?? new NullFactor();
    }

    private IFactorValue Execute(Assign assign)
    {
        if (!CurrentContext.TryGetData(assign.Identifier, out _))
        {
            throw new RuntimeException(assign.Position, $"Variable of name {assign.Identifier} was not defined!");
        }

        var newValue = Execute(assign.Value);
        CurrentContext.SetData(assign.Identifier, newValue);
        return newValue;
    }
    
    private IFactorValue Execute(NullAssign nullAssign)
    {
        if (!CurrentContext.TryGetData(nullAssign.Identifier, out var dataValue))
        {
            throw new RuntimeException(nullAssign.Position, $"Variable of name {nullAssign.Identifier} was not defined!");
        }
        
        if (dataValue is not NullFactor)
        {
            return dataValue;
        }

        var newValue = Execute(nullAssign.Value);
        CurrentContext.SetData(nullAssign.Identifier, newValue);
        return newValue;
    }
    
    private IFactorValue Execute(Declaration declaration)
    {
        var newValue = Execute(declaration.Value);
        CurrentContext.AddData(declaration.Identifier, newValue);
        return newValue;
    }
    
    private ContextLambda Execute(Lambda lambda) => new(lambda, CurrentContext);
    
    private IFactorValue Execute(BuildInMethod buildInMethod) => buildInMethod.Execute(Execute, CurrentContext);

    private IFactorValue Execute(FunctionCall functionCall)
    {
        var dataValue = CurrentContext.GetContextData(functionCall.Identifier, functionCall.Position);

        if (dataValue is not ContextLambda contextLambda)
        {
            throw new RuntimeException(functionCall.Position, $"{dataValue} is not callable!");
        }
        
        var (lambda, localContext) = contextLambda;

        if (lambda.Arguments.Count != functionCall.Arguments.Count)
        {
            throw new RuntimeException(functionCall.Position, $"Function {functionCall.Identifier} requires {lambda.Arguments.Count} arguments, but given {functionCall.Arguments.Count} arguments!");
        }

        if (_contextStack.Count == RECURSION_LIMIT)
        {
            throw new RuntimeException(functionCall.Position, $"Exceeded recursion limit");
        }
        
        var newContext = new Context(localContext);
        for (int i = 0; i < functionCall.Arguments.Count; i++)
        {
            newContext.AddData(lambda.Arguments[i], Execute(functionCall.Arguments[i]));   
        }
        _contextStack.Push(newContext);
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

    private IFactorValue Execute(Loop loop)
    {
        IFactorValue? returnValue = null;
        int iterations = 0;
        while (true)
        {
            var conditionResult = Execute(loop.Condition);
            if (!ParseBool(conditionResult, loop.Condition.Position))
            {
                break;
            }
            
            _contextStack.Push(new Context(CurrentContext));
            returnValue = Execute(loop.Body);
            _contextStack.Pop();    
            
            iterations++;
            if (iterations > LOOP_COUNT_LIMIT)
            {
                throw new RuntimeException(loop.Position, $"Loop exceeded loop count limit");
            }
        }
        
        return returnValue ?? new NullFactor();
    }

    #endregion

    #region Expressions

    private BoolFactor Execute(LogicOrExpression logicOrExpression)
    {
        var left = ParseBool(Execute(logicOrExpression.Lhs), logicOrExpression.Lhs.Position);
        if (left)
        {
            return new BoolFactor(true);
        }
        var right = ParseBool(Execute(logicOrExpression.Rhs), logicOrExpression.Rhs.Position);
        return new BoolFactor(right);
    }
    private BoolFactor Execute(LogicAndExpression logicAndExpression)
    {
        var left = ParseBool(Execute(logicAndExpression.Lhs), logicAndExpression.Lhs.Position);
        if (!left)
        {
            return new BoolFactor(false);
        }
        var right = ParseBool(Execute(logicAndExpression.Rhs), logicAndExpression.Rhs.Position);
        return new BoolFactor(right);
    }
    
    private BoolFactor Execute(CompereEqualsExpression compereEqualsExpression)
    {
        var left = Execute(compereEqualsExpression.Lhs);
        var right = Execute(compereEqualsExpression.Rhs);
        var equals = AreEqual(left, compereEqualsExpression.Lhs.Position, right, compereEqualsExpression.Rhs.Position);
        return new BoolFactor(equals);
    }
    private BoolFactor Execute(CompereNotEqualsExpression compereNotEqualsExpression)
    {
        var left = Execute(compereNotEqualsExpression.Lhs);
        var right = Execute(compereNotEqualsExpression.Rhs);
        var equals = AreEqual(left, compereNotEqualsExpression.Lhs.Position, right, compereNotEqualsExpression.Rhs.Position);
        return new BoolFactor(!equals);
    }
    private BoolFactor Execute(CompereGreaterExpression compereGreaterExpression)
    {
        var left = Execute(compereGreaterExpression.Lhs);
        var right = Execute(compereGreaterExpression.Rhs);
        var grater = IsLeftGreater(left, compereGreaterExpression.Lhs.Position, right, compereGreaterExpression.Rhs.Position);
        return new BoolFactor(grater);
    }
    private BoolFactor Execute(CompereGreaterEqualExpression compereGreaterEqualExpression)
    {
        var left = Execute(compereGreaterEqualExpression.Lhs);
        var right = Execute(compereGreaterEqualExpression.Rhs);
        var less = IsLeftGreater(right, compereGreaterEqualExpression.Rhs.Position, left, compereGreaterEqualExpression.Lhs.Position);
        return new BoolFactor(!less);
    }
    private BoolFactor Execute(CompereLessExpression compereLessExpression)
    {
        var left = Execute(compereLessExpression.Lhs);
        var right = Execute(compereLessExpression.Rhs);
        var less = IsLeftGreater(right, compereLessExpression.Rhs.Position, left, compereLessExpression.Lhs.Position);
        return new BoolFactor(less);
    }
    private BoolFactor Execute(CompereLessEqualExpression compereLessEqualExpression)
    {
        var left = Execute(compereLessEqualExpression.Lhs);
        var right = Execute(compereLessEqualExpression.Rhs);
        var grater = IsLeftGreater(left, compereLessEqualExpression.Lhs.Position, right, compereLessEqualExpression.Rhs.Position);
        return new BoolFactor(!grater);
    }
    
    private IFactorValue Execute(NullCoalescingExpression nullCoalescingExpression)
    {
        var left = Execute(nullCoalescingExpression.Lhs);
        return left is not NullFactor ? left : Execute(nullCoalescingExpression.Rhs);
    }
    
    private IFactorValue Execute(ArithmeticalAddExpression arithmeticalAddExpression)
    {
        var left = Execute(arithmeticalAddExpression.Lhs);
        var right = Execute(arithmeticalAddExpression.Rhs);
     
        if (left is StringFactor || right is StringFactor)
        {
            var leftString = ParseString(left, arithmeticalAddExpression.Lhs.Position);
            var rightString = ParseString(right, arithmeticalAddExpression.Rhs.Position);
            return new StringFactor(leftString + rightString);
        }
        
        var leftNumber = ParseInt(left, arithmeticalAddExpression.Lhs.Position);
        var rightNumber = ParseInt(right, arithmeticalAddExpression.Rhs.Position);
        return new IntFactor(leftNumber + rightNumber);
    }
    
    private IntFactor Execute(ArithmeticalSubtractExpression arithmeticalSubtractExpression)
    {
        var left = Execute(arithmeticalSubtractExpression.Lhs);
        var right = Execute(arithmeticalSubtractExpression.Rhs);
        
        var leftNumber = ParseInt(left, arithmeticalSubtractExpression.Lhs.Position);
        var rightNumber = ParseInt(right, arithmeticalSubtractExpression.Rhs.Position);
        return new IntFactor(leftNumber - rightNumber);
    }
    private IntFactor Execute(ArithmeticalMultiplyExpression arithmeticalMultiplyExpression)
    {
        var left = Execute(arithmeticalMultiplyExpression.Lhs);
        var right = Execute(arithmeticalMultiplyExpression.Rhs);
        
        var leftNumber = ParseInt(left, arithmeticalMultiplyExpression.Lhs.Position);
        var rightNumber = ParseInt(right, arithmeticalMultiplyExpression.Rhs.Position);
        return new IntFactor(leftNumber * rightNumber);
    }
    private IntFactor Execute(ArithmeticalDivideExpression arithmeticalDivideExpression)
    {
        var left = Execute(arithmeticalDivideExpression.Lhs);
        var right = Execute(arithmeticalDivideExpression.Rhs);
        
        var leftNumber = ParseInt(left, arithmeticalDivideExpression.Lhs.Position);
        var rightNumber = ParseInt(right, arithmeticalDivideExpression.Rhs.Position);
        if (rightNumber == 0)
        {
            throw new RuntimeException(arithmeticalDivideExpression.Position, $"Cannot divide by zero!");
        }
        return new IntFactor(leftNumber / rightNumber);
    }

    private IFactorValue Execute(NotExpression notExpression)
    {
        var value = Execute(notExpression.Factor);
        return value switch
        {
            IntFactor intFact => new IntFactor(-intFact.Value),
            BoolFactor intFact => new BoolFactor(!intFact.Value),
            _ => throw new RuntimeException(notExpression.Position, $"Negation of {notExpression.Factor} is not defined!"),
        };
    }

    private static bool AreEqual(IFactorValue left, Position leftPosition, IFactorValue right, Position rightPosition)
    {
        if (left is StringFactor || right is StringFactor)
        {
            var leftString = ParseString(left, leftPosition);
            var rightString = ParseString(right, rightPosition);
            return leftString == rightString;
        }
        
        if (left is ContextLambda leftLambda && right is ContextLambda rightLambda)
        {
            return leftLambda == rightLambda;
        }

        if (left is ContextLambda || right is ContextLambda)
        {
            return false;
        }
        
        var leftInt = ParseInt(left, leftPosition);
        var rightInt = ParseInt(right, rightPosition);
        return leftInt == rightInt;
    }
    private static bool IsLeftGreater(IFactorValue left, Position leftPosition, IFactorValue right, Position rightPosition)
    {
        if (left is StringFactor || right is StringFactor)
        {
            var leftString = ParseString(left, leftPosition);
            var rightString = ParseString(right, rightPosition);
            for (int i = 0; i < Math.Min(leftString.Length, rightString.Length); i++)
            {
                if (leftString[i] > rightString[i])
                {
                    return true;
                }
            }
            return false;
        }
        
        var leftInt = ParseInt(left, leftPosition);
        var rightInt = ParseInt(right, rightPosition);
        return leftInt > rightInt;
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
            IntFactor v => v.Value.ToString(),
            StringFactor v => v.Value,
            BoolFactor v => v.Value ? "true" : "false",
            NullFactor => "",
            _ => throw new RuntimeException(position, $"{value} can not be parsed to int value!")
        };
    }
    
    #endregion
}