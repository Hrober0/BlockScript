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
            TakeTokenOrThrow(TokenType.EndOfStatement, $"Statements should be separated by {TokenType.EndOfStatement.TextValue()}");
            
            newStatement = TryParseStatement();
        }
        
        TakeTokenOrThrow(TokenType.EndOfText, $"Expected end of program");
        
        return new Block(statements);
    }

    #region Statements

    private IStatement ParseStatement(string message)
    {
        var statement = TryParseStatement();
        if (statement == null)
        {
            throw new TokenException(_tokenBuffer.Current.Line, _tokenBuffer.Current.Column, $"Unexpected token '{_tokenBuffer.Current.Value}', expected statement. {message}");
        }

        return statement;
    }
    
    private IStatement? TryParseStatement() =>
        TryParseAssign()
        ?? TryParseLambda()
        ?? TryParseCondition()
        ?? TryParseLoop()
        ?? TryParsePrint()
        ??  TryParseExpression();

    private IStatement? TryParseAssign()
    {
        if (_tokenBuffer.Current.Type is not TokenType.Identifier || _tokenBuffer.Next.Type is not (TokenType.OperatorAssign or TokenType.OperatorNullAssign))
        {
            return null;
        }
        var identifierToken = TakeTokenOrThrow(TokenType.Identifier, "Assigment require name.");
        var nullAssign = _tokenBuffer.Current.Type is TokenType.OperatorNullAssign;
        _tokenBuffer.Take();
        var statement = ParseStatement("Assigment require value");
        return new Assign((string)identifierToken.Value, statement, nullAssign);
    }
    
    private IStatement? TryParseLambda()
    {
        if (!TryTakeToken(TokenType.ParenhticesOpen, out _))
        {
            return null;
        }

        var arguments = ParseArguments();

        var errorMessage = "Lambda should have \"(args) => expression\" syntax";
        
        TakeTokenOrThrow(TokenType.ParenhticesClose, errorMessage);
        TakeTokenOrThrow(TokenType.OperatorArrow, errorMessage);

        var body = ParseStatement(errorMessage);
        
        return new Lambda(arguments, body);
    }

    private IStatement? TryParseCondition()
    {
        if (!TryTakeToken(TokenType.If, out _))
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
            return new Condition(conditionaryItems, elseBody);            
        }
        
        return new Condition(conditionaryItems, null);
    }
    
    private IStatement? TryParseLoop()
    {
        if (!TryTakeToken(TokenType.Loop, out _))
        {
            return null;
        }

        var errorMessage = $"Loop should have \"{TokenType.Loop.TextValue()} expression expression\" syntax";
        
        var condition = ParseExpression(errorMessage);
        var body = ParseStatement(errorMessage);
        
        return new Loop(condition, body);
    }
    
    private IStatement? TryParsePrint()
    {
        if (!TryTakeToken(TokenType.Print, out _))
        {
            return null;
        }
        
        var errorMessage = $"Print should have \"{TokenType.Print.TextValue()}(expression)\" syntax";

        TakeTokenOrThrow(TokenType.ParenhticesOpen, errorMessage);
        var body = ParseStatement(errorMessage);
        TakeTokenOrThrow(TokenType.ParenhticesClose, errorMessage);
        
        return new Print(body);
    }
    
    #endregion

    #region Expressions

    private IExpression ParseExpression(string message)
    {
        var expression = TryParseExpression();
        if (expression == null)
        {
            throw new TokenException(_tokenBuffer.Current.Line, _tokenBuffer.Current.Column, $"Unexpected token '{_tokenBuffer.Current.Value}'. {message}");
        }

        return expression;
    }
    
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
        if (_tokenBuffer.Current.Type is TokenType.OperatorSubtract)
        {
            _tokenBuffer.Take();
            negate = true;
        }
        var factor = TryParseFactor();
        if (factor == null && negate)
        {
            throw new TokenException(_tokenBuffer.Current.Line, _tokenBuffer.Current.Column, $"Unexpected token '{_tokenBuffer.Current.Value}', after {TokenType.OperatorSubtract.TextValue()} should be factor.");
        }
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
            
            TakeTokenOrThrow(TokenType.EndOfStatement, $"Statements should be separated by {TokenType.EndOfStatement.TextValue()}");
            newStatement = TryParseStatement();
        }

        TakeTokenOrThrow(TokenType.BraceClose, "Block should be closed.");
        
        return new Block(statements);
    }

    private IFactor? TryParseFunctionCall()
    {
        if (_tokenBuffer.Current.Type != TokenType.Identifier || _tokenBuffer.Next.Type != TokenType.ParenhticesOpen)
        {
            return null;
        }
        
        var errorMessage = "Function call should have \"name(args)\" syntax";
        var identifierToken = TakeTokenOrThrow(TokenType.Identifier, errorMessage);
        
        TakeTokenOrThrow(TokenType.ParenhticesOpen, errorMessage);
        
        var arguments = ParseArguments();
        
        TakeTokenOrThrow(TokenType.ParenhticesClose, errorMessage);
        
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
            
            if (!TryTakeToken(TokenType.Comma, out _))
            {
                break;
            }
            expression = TryParseExpression();
        }

        return expressions;
    }
    
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