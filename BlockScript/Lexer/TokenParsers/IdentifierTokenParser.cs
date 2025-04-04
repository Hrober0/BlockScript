using System.Text;

namespace BlockScript.Lexer.TokenParsers;

public class IdentifierTokenParser() : TokenParser
{
    private const int MAX_IDENTIFIER_LENGTH = 255;
    
    private readonly StringBuilder _stringBuilder = new();
    
    public override AcceptStatus AcceptChar(char c)
    {
        if (char.IsLetter(c) || char.IsDigit(c) && _stringBuilder.Length > 0)
        {
            if (_stringBuilder.Length == MAX_IDENTIFIER_LENGTH)
            {
                throw new OverflowException($"Identifier exceeds {MAX_IDENTIFIER_LENGTH} characters.");
            }
            
            _stringBuilder.Append(c);
            return AcceptStatus.Possible;
        }
        
        return AcceptStatus.Deny;
    }
    
    public override bool IsValid() => _stringBuilder.Length > 0;

    public override TokenData CreateToken(int line, int column)
    {
        return new()
        {
            Line = line,
            Column = column,
            Type = TokenType.Identifier,
            Value = _stringBuilder.ToString(),
            CharacterLength = _stringBuilder.Length,
        };
    }

    public override void Reset()
    {
        _stringBuilder.Clear();
    }
}