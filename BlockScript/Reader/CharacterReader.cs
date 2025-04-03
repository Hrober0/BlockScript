namespace BlockScript.Reader;

public class CharacterReader(TextReader textReader)
{
    public const char EndOfText = '\u0004';
    public const char NewLine = '\n';
    public const char WhiteSpace = ' ';
    
    private int _line = 1;
    private int _column = 0;
    
    private char _currentCharacter = '\0';
    
    public Character GetCharacter()
    {
        if (_currentCharacter == EndOfText)
        {
            return new(EndOfText, _line, _column);
        }
        
        if (_currentCharacter == NewLine)
        {
            _column = 0;
            _line++;   
        }

        _currentCharacter = ParseChar(textReader.Read());

        // new line
        if (_currentCharacter is '\r' or '\n')
        {
            // check windows CR LF sequence
            char nextChar = ParseChar(textReader.Peek());
            if (_currentCharacter is '\r' && nextChar is '\n')
            {
                textReader.Read();
            }

            _currentCharacter = NewLine;
        }

        // unify white space
        else if (char.IsWhiteSpace(_currentCharacter))
        {
            _currentCharacter = WhiteSpace;
        }

        _column++;
        return new(_currentCharacter, _line, _column);
    }

    private static char ParseChar(int streamChar)
    {
        if (streamChar < 0)
        {
            return EndOfText;
        }
        return (char)streamChar;
    }
}