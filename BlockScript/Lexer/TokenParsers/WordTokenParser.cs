using System.Text;

namespace BlockScript.Lexer.TokenParsers;

public delegate object CustomParsingMethod(string sequence);

public class WordTokenParser(TokenType tokenType, string targetSequence, CustomParsingMethod? parsingMethod=null) : TokenParser
{
    private readonly StringBuilder _stringBuilder = new();
    private bool _isFollowByIncorrectCharacter = false;
    public override AcceptStatus AcceptChar(char c)
    {
        if (_stringBuilder.Length == targetSequence.Length)
        {
            if (!char.IsLetterOrDigit(c))
            {
                return AcceptStatus.Completed;
            }

            _isFollowByIncorrectCharacter = true;
            return AcceptStatus.Deny;
        }
        
        if (c != targetSequence[_stringBuilder.Length])
        {
            return AcceptStatus.Deny;
        }

        _stringBuilder.Append(c);
        return AcceptStatus.Possible;
    }

    public override bool IsValid() => _stringBuilder.Length == targetSequence.Length && !_isFollowByIncorrectCharacter;
    
    public override TokenData CreateToken(int line, int column)
    {
        return new()
        {
            Line = line,
            Column = column,
            Type = tokenType,
            Value = parsingMethod?.Invoke(_stringBuilder.ToString()) ??  _stringBuilder.ToString(),
            CharacterLength = _stringBuilder.Length,
        };
    }

    public override void Reset()
    {
        _stringBuilder.Clear();
        _isFollowByIncorrectCharacter = false;
    }
}