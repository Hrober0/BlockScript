using System.Text;

namespace BlockScript.Lexer.TokenParsers;

public class CommentTokenParser() : TokenParser
{
    private const int MAX_COMMENT_LENGTH = 255;
    
    private readonly StringBuilder _stringBuilder = new();
    private bool _wasOpen = false;
    
    public override AcceptStatus AcceptChar(char c)
    {
        if (!_wasOpen)
        {
            if (c != '#')
            {
                return AcceptStatus.Deny;
            }

            _wasOpen = true;
            return AcceptStatus.Possible;

        }

        if (_stringBuilder.Length >= MAX_COMMENT_LENGTH)
        {
            throw new OverflowException($"Comment exceeds {MAX_COMMENT_LENGTH} characters.");
        }
        
        _stringBuilder.Append(c);
        return AcceptStatus.Possible;
    }
    
    public override bool IsValid() => _wasOpen;

    public override TokenData CreateToken(int line, int column)
    {
        return new()
        {
            Line = line,
            Column = column,
            Type = TokenType.Comment,
            Value = _stringBuilder.ToString(),
            CharacterLength = _stringBuilder.Length + 1,
        };
    }

    public override void Reset()
    {
        _stringBuilder.Clear();
        _wasOpen = false;
    }
}