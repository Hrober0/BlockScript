using BlockScript.Exceptions;
using BlockScript.Lexer;
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
            Console.WriteLine(token);
            return token.Type is TokenType.Comment ? FilterToken() : token;
        }
    }
    
    public Block ParserProgram()
    {
        var statements = new List<IStatement>();

        var newStatement = TryParseStatement();
        while (newStatement != null)
        {
            statements.Add(newStatement);
            TakeTokenOrThrow(TokenType.EndOfStatement);
            
            newStatement = TryParseStatement();
        }
        
        return new Block(statements);
    }

    #region Statements
    
    private IStatement? TryParseStatement()
    {
        return TryParseAssign() ?? TryParseLambda() ?? TryParseExpression();
    }

    private IStatement? TryParseAssign()
    {
        if (_tokenBuffer.Current.Type is not TokenType.Identifier || _tokenBuffer.Next.Type is not (TokenType.OperatorAssign or TokenType.OperatorNullAssign))
        {
            return null;
        }
        var identifierToken = TakeTokenOrThrow(TokenType.Identifier);
        var nullAssign = _tokenBuffer.Current.Type is TokenType.OperatorNullAssign;
        _tokenBuffer.Take();
        var expression = TryParseExpression();
        if (expression == null)
        {
            throw new TokenException(_tokenBuffer.Current.Line, _tokenBuffer.Current.Column, $"Unexpected token '{_tokenBuffer.Current.Value}', expected expression.");
        }
        return new Assign((string)identifierToken.Value, expression, nullAssign);
    }
    
    private IStatement? TryParseLambda()
    {
        if (!TryTakeToken(TokenType.ParenhticesOpen, out _))
        {
            return null;
        }

        var arguments = ParseArguments();

        TakeTokenOrThrow(TokenType.ParenhticesClose);
        TakeTokenOrThrow(TokenType.OperatorArrow);

        var body = TryParseExpression();
        if (body == null)
        {
            throw new TokenException(_tokenBuffer.Current.Line, _tokenBuffer.Current.Column, $"Unexpected token '{_tokenBuffer.Current.Value}', expected expression.");
        }
        
        return new Lambda(arguments, body);
    }
    
    #endregion

    #region Expressions

    private IExpression? TryParseExpression() => TryParseOrExpression();
    
    private IExpression? TryParseOrExpression() => TryParseExpression([TokenType.OperatorOr],
        (expressions, _) => new LogicExpression(expressions, TokenType.OperatorOr),
        TryParseAndExpression);

    private IExpression? TryParseAndExpression() => TryParseExpression([TokenType.OperatorAnd],
        (expressions, _) => new LogicExpression(expressions, TokenType.OperatorAnd),
        TryParseCompereExpression);

    private IExpression? TryParseCompereExpression() => TryParseExpression([TokenType.OperatorEqual, TokenType.OperatorNotEqual, TokenType.OperatorLess,  TokenType.OperatorLessEqual, TokenType.OperatorGreater, TokenType.OperatorGreaterEqual],
        (expressions, operators) => new CompereExpression(expressions, operators),
        TryParseNullColExpression, 2);
    
    private IExpression? TryParseNullColExpression() => TryParseExpression([TokenType.OperatorNullCoalescing],
        (expressions, _) => new NullCoalescingExpression(expressions),
        TryParseAddExpression);
    
    private IExpression? TryParseAddExpression() => TryParseExpression([TokenType.OperatorAdd, TokenType.OperatorSubtract],
        (expressions, operators) => new ArithmeticalExpression(expressions, operators),
        TryParseMultiplicationExpression);
    
    private IExpression? TryParseMultiplicationExpression() => TryParseExpression([TokenType.OperatorMultiply, TokenType.OperatorDivide],
        (expressions, operators) => new ArithmeticalExpression(expressions, operators),
        TryParseNotExpression);
    
    private NotExpression? TryParseNotExpression()
    {
        var negate = false;
        if (_tokenBuffer.Current.Type == TokenType.OperatorNot)
        {
            _tokenBuffer.Take();
            negate = true;
        }
        var factor = TryParseFactor();
        return factor != null ? new NotExpression(factor, negate) : null;
    }

    private delegate IExpression CreateExpression(List<IExpression> expressions, List<TokenType> operators);
    private IExpression? TryParseExpression(TokenType[] acceptableOperators, CreateExpression expressionConstructor, Func<IExpression?> childExpressionParser, int limit=255)
    {
        var expression = childExpressionParser();
        if (expression == null)
        {
            return null;
        }
        var expressions = new List<IExpression>();
        var operators = new List<TokenType>();
        expressions.Add(expression);
        while (acceptableOperators.Contains(_tokenBuffer.Current.Type) && limit > 1)
        {
            operators.Add(_tokenBuffer.Take().Type);
            expression = childExpressionParser();
            if (expression == null)
            {
                throw new TokenException(_tokenBuffer.Current.Line, _tokenBuffer.Current.Column, $"Expected factor, but received, {_tokenBuffer.Current.Value}");
            }
            expressions.Add(expression);
            limit--;
        }
        
        return expressionConstructor(expressions, operators);
    }
    
    #endregion

    #region Factors

    private IFactor? TryParseFactor()
    {
        return TryParseConstFactor() ?? TryParseBlock() ?? TryParseFunctionCall() ?? TryParseVariableFactor();
    }

    private IFactor? TryParseConstFactor()
    {
        if (_tokenBuffer.Current.Type is not (TokenType.String or TokenType.Boolean or TokenType.Null or TokenType.Integer))
        {
            return null;
        }

        var token = _tokenBuffer.Take();
        return new ConstFactor(token.Value);
    }
    
    private IFactor? TryParseBlock()
    {
        if (!TryTakeToken(TokenType.BraceOpen, out _))
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
            
            TakeTokenOrThrow(TokenType.EndOfStatement);
            newStatement = TryParseStatement();
        }

        TakeTokenOrThrow(TokenType.BraceClose);
        
        return new Block(statements);
    }

    private IFactor? TryParseFunctionCall()
    {
        if (_tokenBuffer.Current.Type != TokenType.Identifier || _tokenBuffer.Next.Type != TokenType.ParenhticesOpen)
        {
            return null;
        }
        var identifierToken = TakeTokenOrThrow(TokenType.Identifier);
        TakeTokenOrThrow(TokenType.ParenhticesOpen);

        var arguments = ParseArguments();
        
        TakeTokenOrThrow(TokenType.ParenhticesClose);

        return new FunctionCall((string)identifierToken.Value, arguments);
    }

    private IFactor? TryParseVariableFactor()
    {
        if (!TryTakeToken(TokenType.Identifier, out var token))
        {
            return null;
        }
        return new VariableFactor((string)token.Value);
    }
    
    #endregion
    
    private List<IExpression> ParseArguments()
    {
        var expressions = new List<IExpression>();
        var expression = TryParseExpression();
        while (expression != null)
        {
            expressions.Add(expression);
            TakeTokenOrThrow(TokenType.Comma);
            expression = TryParseExpression();
        }

        return expressions;
    }
    
    private TokenData TakeTokenOrThrow(TokenType tokenType)
    {
        if (_tokenBuffer.Current.Type != tokenType)
        {
            throw new UnexpectedTokenException(_tokenBuffer.Current, tokenType);
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