using BlockScript.Exceptions;
using BlockScript.Lexer;
using BlockScript.Parser.Expressions;
using BlockScript.Parser.Factors;
using BlockScript.Parser.Statements;
using BlockScript.Utilities;

namespace BlockScript.Parser;

public class LanguageParser(StreamBuffer<TokenData> tokenBuffer)
{
    public Block ParserProgram()
    {
        var statements = new List<IStatement>();

        var newStatement = TryParseStatement();
        while (newStatement != null)
        {
            statements.Add(newStatement);
            newStatement = TryParseStatement();
        }
        
        return new Block(statements);
    }

    #region Statements
    
    private IStatement? TryParseStatement()
    {
        return TryParseLambda() ?? TryParseExpression();
    }

    private IStatement? TryParseLambda()
    {
        if (tokenBuffer.Current.Type != TokenType.Identifier)
        {
            return null;
        }
        var identifierToken = tokenBuffer.Take();

        TakeTokenOrThrow(TokenType.BraceOpen);
        
        
        
        TakeTokenOrThrow(TokenType.BraceClose);

        return new Lambda();
    }

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
    
    #endregion

    #region Expressions

    private IExpression ParseExpression()
    {
        return ParseMultipleExpressionFrom([TokenType.OperatorOr], 
            () => ParseMultipleExpressionFrom([TokenType.OperatorAnd],
                () => ParseSingleExpressionFrom([TokenType.OperatorEqual, TokenType.OperatorNotEqual, TokenType.OperatorLess,  TokenType.OperatorLessEqual, TokenType.OperatorGreater, TokenType.OperatorGreaterEqual],
                    () => ParseMultipleExpressionFrom([TokenType.OperatorNullCoalescing],
                        () => ParseMultipleExpressionFrom([TokenType.OperatorAdd, TokenType.OperatorSubtract],
                ));
    }

    private IExpression ParseMultipleExpressionFrom(TokenType[] operators, Func<IExpression> nextExpression)
    {
        
    }

    private IExpression ParseSingleExpressionFrom(TokenType[] operators, Func<IExpression> nextExpression)
    {
       
    }
    
    private AddExpression ParseMultiplicativeExpressionFrom()
    {
        var expressions = new List<IExpression>();
        var mul = new List<bool>();
        expressions.Add(ParseNotExpression());
        while (tokenBuffer.Current.Type is TokenType.OperatorMultiply or TokenType.OperatorDivide)
        {
            mul.Add(tokenBuffer.Current.Type is TokenType.OperatorMultiply);
            tokenBuffer.Take();
            expressions.Add(ParseNotExpression());
        }
        
        return new ArithmeticalExpression(expressions, mul);
    }

    private ArithmeticalExpression ParseArithmeticalExpression(TokenType[] acceptableOperators)
    {
        var expressions = new List<IExpression>();
        var operators = new List<TokenType>();
        expressions.Add(ParseNotExpression());
        while (operators.Contains(tokenBuffer.Current.Type))
        {
            operators.Add(tokenBuffer.Take().Type);
            expressions.Add(ParseNotExpression());
        }
        
        return new ArithmeticalExpression(expressions, operators);
    }

    private NotExpression ParseNotExpression()
    {
        var negate = false;
        if (tokenBuffer.Current.Type == TokenType.OperatorNot)
        {
            tokenBuffer.Take();
            negate = true;
        }
        var factor = TryParseFactor();
        if (factor != null)
        {
            return new NotExpression(factor, negate);
        }
        
        throw new TokenException(tokenBuffer.Current.Line, tokenBuffer.Current.Column, $"Expected factor, but received, {tokenBuffer.Current.Value}");
    }
    
    #endregion

    #region Factors

    private IFactor? TryParseFactor()
    {
        if (tokenBuffer.Current.Type is TokenType.String or TokenType.Boolean or TokenType.Null or TokenType.Integer)
        {
            var token = tokenBuffer.Take();
            return new ConstFactor(token.Value);
        }

        return TryParseBlock();
    }
    
    private IFactor? TryParseBlock()
    {
        if (tokenBuffer.Current.Type != TokenType.ParenhticesOpen)
        {
            return null;
        }
        tokenBuffer.Take();
        
        var statements = new List<IStatement>();

        var newStatement = TryParseStatement();
        while (newStatement != null)
        {
            statements.Add(newStatement);
            newStatement = TryParseStatement();
        }

        TakeTokenOrThrow(TokenType.ParenhticesClose);
        
        return new Block(statements);
    }

    #endregion
    
    private TokenData TakeTokenOrThrow(TokenType tokenType)
    {
        if (tokenBuffer.Current.Type != tokenType)
        {
            throw new UnexpectedTokenException(tokenBuffer.Current, tokenType);
        }
        return tokenBuffer.Take();
    }
}