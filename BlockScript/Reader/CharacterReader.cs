using BlockScript.Utilities;

namespace BlockScript.Reader;

public class CharacterReader(TextReader textReader)
{
    private int _line = 1;
    private int _column = 0;
    
    private char _currentCharacter = '\0';
    
    public Character GetCharacter()
    {
        if (_currentCharacter == UnifiedCharacters.EndOfText)
        {
            return new(UnifiedCharacters.EndOfText, _line, _column);
        }
        
        if (_currentCharacter == UnifiedCharacters.NewLine)
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

            _currentCharacter = UnifiedCharacters.NewLine;
        }

        // unify white space
        else if (char.IsWhiteSpace(_currentCharacter))
        {
            _currentCharacter = UnifiedCharacters.WhiteSpace;
        }

        _column++;
        return new(_currentCharacter, _line, _column);
    }

    private static char ParseChar(int streamChar)
    {
        if (streamChar < 0)
        {
            return UnifiedCharacters.EndOfText;
        }
        return (char)streamChar;
    }
}