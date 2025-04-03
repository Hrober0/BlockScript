using System.Text;

namespace BlockScript.Lexer.TokenParsers;

public delegate object SequenceParsingMethod(string sequence);

public class SequenceTokenParser(TokenType tokenType, string targetSequence, SequenceParsingMethod? parsingMethod=null) : TokenParser
{
    private readonly StringBuilder _stringBuilder = new();

    public override AcceptStatus AcceptChar(char c)
    {
        if (c != targetSequence[_stringBuilder.Length])
        {
            return AcceptStatus.Deny;
        }

        _stringBuilder.Append(c);
        return IsValid()
            ? AcceptStatus.Completed
            : AcceptStatus.Possible;
    }

    public override bool IsValid() => _stringBuilder.Length == targetSequence.Length;

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
    }
}