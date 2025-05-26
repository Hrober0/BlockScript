using BlockScript.Exceptions;
using BlockScript.Lexer;
using BlockScript.Lexer.FactorValues;
using BlockScript.Parser.Expressions;
using BlockScript.Parser.Factors;
using BlockScript.Parser.Statements;
using BlockScript.Utilities;

namespace BlockScript.Parser;

public class LanguageParser
{
    private readonly StreamBuffer<TokenData> _tokenBuffer;
    
    public LanguageParser(Func<TokenData> getTokenData)
    {
        _tokenBuffer = new (FilterToken);
        return;

        TokenData FilterToken()
        {
            var token = getTokenData();
            return token.Type is TokenType.Comment ? FilterToken() : token;
        }
    }
    
    public Block ParserProgram()
    {
        var position = _tokenBuffer.Current.Position;
        var statements = new List<IStatement>();

        var newStatement = TryParseStatement();
        while (newStatement != null)
        {
            statements.Add(newStatement);
            TakeTokenOrThrow(TokenType.EndOfStatement, $"Statements should be separated by {TokenType.EndOfStatement.TextValue()}");
            
            newStatement = TryParseStatement();
        }
        
        TakeTokenOrThrow(TokenType.EndOfText, $"Expected end of program");
        
        return new Block(statements, position);
    }

    #region Statements

    private IStatement ParseStatement(string message)
    {
        var statement = TryParseStatement();
        if (statement == null)
        {
            throw new TokenException(_tokenBuffer.Current.Position, $"Unexpected token '{_tokenBuffer.Current.Value}', expected statement. {message}");
        }

        return statement;
    }
    
    private IStatement? TryParseStatement() =>
        TryParseAssign()
        ?? TryParseNullAssign()
        ?? TryParseDeclaration()
        ?? TryParseLambda()
        ?? TryParseCondition()
        ?? TryParseLoop()
        ??  TryParseExpression();

    private IStatement? TryParseAssign()
    {
        if (_tokenBuffer.Current.Type is not TokenType.Identifier || _tokenBuffer.Next.Type is not TokenType.OperatorAssign)
        {
            return null;
        }
        var position = _tokenBuffer.Current.Position;
        var identifierToken = _tokenBuffer.Take();
        _tokenBuffer.Take();
        var statement = ParseStatement("Assigment require value");
        return new Assign((string)(StringFactor)identifierToken.Value, statement, position);
    }
    
    private IStatement? TryParseNullAssign()
    {
        if (_tokenBuffer.Current.Type is not TokenType.Identifier || _tokenBuffer.Next.Type is not TokenType.OperatorNullAssign)
        {
            return null;
        }
        var position = _tokenBuffer.Current.Position;
        var identifierToken = _tokenBuffer.Take();
        _tokenBuffer.Take();
        var statement = ParseStatement("Assigment require value");
        return new NullAssign((string)(StringFactor)identifierToken.Value, statement, position);
    }
    
    private IStatement? TryParseDeclaration()
    {
        if (_tokenBuffer.Current.Type is not TokenType.Identifier || _tokenBuffer.Next.Type is not TokenType.OperatorDeclaration)
        {
            return null;
        }
        var position = _tokenBuffer.Current.Position;
        var identifierToken = _tokenBuffer.Take();
        _tokenBuffer.Take();
        var statement = ParseStatement("Declaration require value");
        return new Declaration((string)(StringFactor)identifierToken.Value, statement, position);
    }
    
    private IStatement? TryParseLambda()
    {
        if (!TryTakeToken(TokenType.ParenthesesOpen, out var startToken))
        {
            return null;
        }

        var arguments = new List<string>();
        while (true)
        {
            if (!TryTakeToken(TokenType.Identifier, out var identifierToken))
            {
                break;
            }
            arguments.Add((string)(StringFactor)identifierToken.Value);
            if (!TryTakeToken(TokenType.Comma, out _))
            {
                break;
            }
        }

        var errorMessage = "Lambda should have \"(args) => expression\" syntax";
        
        TakeTokenOrThrow(TokenType.ParenthesesClose, errorMessage);
        TakeTokenOrThrow(TokenType.OperatorArrow, errorMessage);

        var body = ParseStatement(errorMessage);
        
        return new Lambda(arguments, body, startToken.Position);
    }

    private IStatement? TryParseCondition()
    {
        if (!TryTakeToken(TokenType.If, out var startToken))
        {
            return null;
        }
        
        var errorMessage = $"Condition should have \"{TokenType.If.TextValue()} expression expression\" or \"{TokenType.If.TextValue()} expression expression {TokenType.Else.TextValue()} expression\" or \"{TokenType.If.TextValue()} expression expression {TokenType.Else.TextValue()} {TokenType.If.TextValue()} expression expression\" syntax";

        var conditionaryItems = new List<(IExpression condition, IStatement body)>();

        while (true)
        {
            var condition = ParseExpression(errorMessage);
            var body = ParseStatement(errorMessage);
            conditionaryItems.Add((condition, body));

            if (_tokenBuffer.Current.Type is not TokenType.Else || _tokenBuffer.Next.Type is not TokenType.If)
            {
                break;
            }
            
            _tokenBuffer.Take();
            _tokenBuffer.Take();
        }

        if (TryTakeToken(TokenType.Else, out _))
        {
            var elseBody = ParseStatement(errorMessage);
            return new Condition(conditionaryItems, elseBody, startToken.Position);            
        }
        
        return new Condition(conditionaryItems, null, startToken.Position);
    }
    
    private IStatement? TryParseLoop()
    {
        if (!TryTakeToken(TokenType.Loop, out var startToken))
        {
            return null;
        }

        var errorMessage = $"Loop should have \"{TokenType.Loop.TextValue()} expression expression\" syntax";
        
        var condition = ParseExpression(errorMessage);
        var body = ParseStatement(errorMessage);
        
        return new Loop(condition, body, startToken.Position);
    }
    
    #endregion

    #region Expressions

    private IExpression ParseExpression(string message)
    {
        var expression = TryParseExpression();
        if (expression == null)
        {
            throw new TokenException(_tokenBuffer.Current.Position, $"Unexpected token '{_tokenBuffer.Current.Value}'. {message}");
        }

        return expression;
    }
    
    private IExpression? TryParseExpression() => TryParseOrExpression();
    
    private IExpression? TryParseOrExpression() => TryParseMultipleExpression([TokenType.OperatorOr],
        (lhs, rhs, operatorToken) => new LogicOrExpression(lhs, rhs, operatorToken.Position),
        TryParseAndExpression);

    private IExpression? TryParseAndExpression() => TryParseMultipleExpression([TokenType.OperatorAnd],
        (lhs, rhs, operatorToken) => new LogicAndExpression(lhs, rhs, operatorToken.Position),
        TryParseCompereExpression);

    private IExpression? TryParseCompereExpression() => TryParseSingleExpression([
            TokenType.OperatorEqual,
            TokenType.OperatorNotEqual,
            TokenType.OperatorLess,
            TokenType.OperatorLessEqual,
            TokenType.OperatorGreater,
            TokenType.OperatorGreaterEqual
        ],
        (lhs, rhs, operatorToken) => operatorToken.Type switch
        {
            TokenType.OperatorEqual => new CompereEqualsExpression(lhs, rhs, operatorToken.Position),
            TokenType.OperatorNotEqual => new CompereNotEqualsExpression(lhs, rhs, operatorToken.Position),
            TokenType.OperatorLess => new CompereLessExpression(lhs, rhs, operatorToken.Position),
            TokenType.OperatorLessEqual => new CompereLessEqualExpression(lhs, rhs, operatorToken.Position),
            TokenType.OperatorGreater => new CompereGreaterExpression(lhs, rhs, operatorToken.Position),
            TokenType.OperatorGreaterEqual => new CompereGreaterEqualExpression(lhs, rhs, operatorToken.Position),
            _ => throw new ArgumentOutOfRangeException()
        },
        TryParseNullColExpression);
    
    private IExpression? TryParseNullColExpression() => TryParseMultipleExpression([TokenType.OperatorNullCoalescing],
        (lhs, rhs, operatorToken) => new NullCoalescingExpression(lhs, rhs, operatorToken.Position),
        TryParseAddExpression);
    
    private IExpression? TryParseAddExpression() => TryParseMultipleExpression([TokenType.OperatorAdd, TokenType.OperatorSubtract],
        (lhs, rhs, operatorToken) => operatorToken.Type switch
        {
            TokenType.OperatorAdd => new ArithmeticalAddExpression(lhs, rhs, operatorToken.Position),
            TokenType.OperatorSubtract => new ArithmeticalSubtractExpression(lhs, rhs, operatorToken.Position),
            _ => throw new ArgumentOutOfRangeException()
        },
        TryParseMultiplicationExpression);
    
    private IExpression? TryParseMultiplicationExpression() => TryParseMultipleExpression([TokenType.OperatorMultiply, TokenType.OperatorDivide],
        (lhs, rhs, operatorToken) => operatorToken.Type switch
        {
            TokenType.OperatorMultiply => new ArithmeticalMultiplyExpression(lhs, rhs, operatorToken.Position),
            TokenType.OperatorDivide => new ArithmeticalDivideExpression(lhs, rhs, operatorToken.Position),
            _ => throw new ArgumentOutOfRangeException()
        },
        TryParseNotExpression);
    
    private IExpression? TryParseNotExpression()
    {
        if (!TryTakeToken(TokenType.OperatorSubtract, out _))
        {
            return TryParseFactor();
        }

        var factor = TryParseFactor();
        if (factor == null)
        {
            throw new TokenException(_tokenBuffer.Current.Position, $"Unexpected token '{_tokenBuffer.Current.Value}', after {TokenType.OperatorSubtract.TextValue()} should be factor.");
        }
        return new NotExpression(factor);
    }

    private delegate IExpression CreateExpression(IExpression lhs, IExpression rhs, TokenData operatorToken);
    private IExpression? TryParseMultipleExpression(TokenType[] acceptableOperators, CreateExpression expressionConstructor, Func<IExpression?> childExpressionParser)
    {
        var expression = childExpressionParser();
        if (expression == null)
        {
            return null;
        }
        var expressions = new List<IExpression>();
        var operators = new List<TokenData>();
        expressions.Add(expression);
        while (acceptableOperators.Contains(_tokenBuffer.Current.Type))
        {
            operators.Add(_tokenBuffer.Take());
            expression = childExpressionParser();
            if (expression == null)
            {
                throw new TokenException(_tokenBuffer.Current.Position, $"Expected factor, but received, {_tokenBuffer.Current.Value}");
            }
            expressions.Add(expression);
        }

        if (expressions.Count == 1)
        {
            return expressions[0];
        }

        return CreateExpression(0);

        IExpression CreateExpression(int index)
        {
            var nextExpression = index < operators.Count - 1
                ? CreateExpression(index + 1)
                : expressions[index + 1];
            return expressionConstructor(expressions[index], nextExpression, operators[index]);
        }
    }
    private IExpression? TryParseSingleExpression(TokenType[] acceptableOperators, CreateExpression expressionConstructor, Func<IExpression?> childExpressionParser)
    {
        var leftExpression = childExpressionParser();
        if (leftExpression == null)
        {
            return null;
        }

        if (!acceptableOperators.Contains(_tokenBuffer.Current.Type))
        {
            return leftExpression;
        }
        var operatorToken = _tokenBuffer.Take();
        
        var rightExpression = childExpressionParser();
        if (rightExpression == null)
        {
            throw new TokenException(_tokenBuffer.Current.Position, $"Expected factor, but received, {_tokenBuffer.Current.Value}");
        }
        
        return expressionConstructor(leftExpression, rightExpression, operatorToken);
    }
    
    #endregion

    #region Factors

    private IFactor? TryParseFactor() => 
        TryParseConstFactor() 
        ?? TryParseBlock()
        ?? TryParseFunctionCall()
        ?? TryParseVariableFactor();

    private IFactor? TryParseConstFactor()
    {
        if (_tokenBuffer.Current.Type is not (TokenType.String or TokenType.Boolean or TokenType.Null or TokenType.Integer))
        {
            return null;
        }

        var token = _tokenBuffer.Take();
        return new ConstFactor(token.Value, token.Position);
    }
    
    private IFactor? TryParseBlock()
    {
        if (!TryTakeToken(TokenType.BraceOpen, out var startToken))
        {
            return null;
        }
        
        var statements = new List<IStatement>();

        var newStatement = TryParseStatement();
        while (newStatement != null)
        {
            statements.Add(newStatement);

            if (_tokenBuffer.Current.Type == TokenType.BraceClose)
            {
                break;
            }
            
            TakeTokenOrThrow(TokenType.EndOfStatement, $"Statements should be separated by {TokenType.EndOfStatement.TextValue()}");
            newStatement = TryParseStatement();
        }

        TakeTokenOrThrow(TokenType.BraceClose, "Block should be closed.");
        
        return new Block(statements, startToken.Position);
    }

    private IFactor? TryParseFunctionCall()
    {
        if (_tokenBuffer.Current.Type != TokenType.Identifier || _tokenBuffer.Next.Type != TokenType.ParenthesesOpen)
        {
            return null;
        }
        
        var errorMessage = "Function call should have \"name(args)\" syntax";
        var identifierToken = TakeTokenOrThrow(TokenType.Identifier, errorMessage);
        
        TakeTokenOrThrow(TokenType.ParenthesesOpen, errorMessage);
        
        var arguments = new List<IExpression>();
        while (true)
        {
            var expression = TryParseExpression();
            if (expression == null)
            {
                break;
            }
            arguments.Add(expression);
            
            if (!TryTakeToken(TokenType.Comma, out _))
            {
                break;
            }
        }
        
        TakeTokenOrThrow(TokenType.ParenthesesClose, errorMessage);
        
        return new FunctionCall((string)(StringFactor)identifierToken.Value, arguments, identifierToken.Position);
    }

    private IFactor? TryParseVariableFactor()
    {
        if (!TryTakeToken(TokenType.Identifier, out var token))
        {
            return null;
        }
        return new VariableFactor((string)(StringFactor)token.Value, token.Position);
    }
    
    #endregion
    
    private TokenData TakeTokenOrThrow(TokenType tokenType, string message)
    {
        if (_tokenBuffer.Current.Type != tokenType)
        {
            throw new UnexpectedTokenException(_tokenBuffer.Current, tokenType, message);
        }
        return _tokenBuffer.Take();
    }
    private bool TryTakeToken(TokenType tokenType, out TokenData token)
    {
        if (_tokenBuffer.Current.Type != tokenType)
        {
            token = default;
            return false;
        }

        token = _tokenBuffer.Take();
        return true;
    }
}