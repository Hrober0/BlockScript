using System.Text;

namespace BlockScript.Lexer.TokenParsers;

public class StringTokenParser() : TokenParser
{
    private const int MAX_STRING_LENGTH = 255;
    
    private readonly StringBuilder _stringBuilder = new();
    private bool _wasOpen = false;
    private bool _wasClose = false;
    
    public override AcceptStatus AcceptChar(char c)
    {
        if (c == '\"')
        {
            if (_wasOpen)
            {
                _wasClose = true;
                return AcceptStatus.Completed;
            }

            _wasOpen = true;
            return AcceptStatus.Possible;
        }

        if (!_wasOpen)
        {
            return AcceptStatus.Deny;
        }
        
        if (_stringBuilder.Length >= MAX_STRING_LENGTH)
        {
            throw new OverflowException($"String exceeds {MAX_STRING_LENGTH} characters.");
        }
        
        _stringBuilder.Append(c);
        return AcceptStatus.Possible;
    }
    
    public override bool IsValid() => _wasClose;

    public override TokenData CreateToken(int line, int column)
    {
        return new()
        {
            Line = line,
            Column = column,
            Type = TokenType.String,
            Value = _stringBuilder.ToString(),
            CharacterLength = _stringBuilder.Length + 2,
        };
    }

    public override void Reset()
    {
        _stringBuilder.Clear();
        _wasClose = false;
        _wasOpen = false;
    }

    public override string? GetErrorHint() => "Expected '\"' at the end of string";
}