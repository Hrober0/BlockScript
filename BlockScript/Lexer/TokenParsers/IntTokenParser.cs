using System.Text;

namespace BlockScript.Lexer.TokenParsers;

public class IntTokenParser() : TokenParser
{
    private int _number;
    private int _characters;

    public override AcceptStatus AcceptChar(char c)
    {
        if (!int.TryParse(c.ToString(), out int digit))
        {
            return AcceptStatus.Deny;
        }

        if (_characters > 0 && _number == 0)
        {
            return AcceptStatus.Deny;
        }

        try
        {
            var newNumber = checked(_number * 10 + digit);
        }
        catch (OverflowException)
        {
            throw new OverflowException($"Int value {_number}{c} exceeds {int.MaxValue}");
        }
        
        _number = _number * 10 + digit;

        _characters++;
        return AcceptStatus.Possible;
    }
    
    public override bool IsValid() => _characters > 0;

    public override TokenData CreateToken(int line, int column)
    {
        return new()
        {
            Line = line,
            Column = column,
            Type = TokenType.Integer,
            Value = _number,
            CharacterLength = _characters,
        };
    }

    public override void Reset()
    {
        _number = 0;
        _characters = 0;
    }
}