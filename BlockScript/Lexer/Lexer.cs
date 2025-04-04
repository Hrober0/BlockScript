using BlockScript.Lexer.TokenParsers;
using BlockScript.Reader;
using BlockScript.Utilities;

namespace BlockScript.Lexer;

public class Lexer : ILexer
{
    private readonly Buffer<Character> _buffer;
    
    private readonly List<TokenParser> _tokenParsers =
    [
        new SequenceTokenParser(TokenType.EndOfText, UnifiedCharacters.EndOfText.ToString()),

        // syntax
        new SequenceTokenParser(TokenType.EndOfStatement, ";"),
        new SequenceTokenParser(TokenType.Operator, "("),
        new SequenceTokenParser(TokenType.Operator, ")"),
        new SequenceTokenParser(TokenType.Operator, "{"),
        new SequenceTokenParser(TokenType.Operator, "}"),
        new CommentTokenParser(),
        
        // key-words
        new WordTokenParser(TokenType.Loop, "loop"),

        // data
        new IntTokenParser(),
        new StringTokenParser(),
        new WordTokenParser(TokenType.Null, "null"),
        new WordTokenParser(TokenType.Boolean, "true", _ => true),
        new WordTokenParser(TokenType.Boolean, "false", _ => false),
        
        // logical
        new SequenceTokenParser(TokenType.Operator, "=="),
        new SequenceTokenParser(TokenType.Operator, "=>"),
        new SequenceTokenParser(TokenType.Operator, "="),
        new SequenceTokenParser(TokenType.Operator, "<="),
        new SequenceTokenParser(TokenType.Operator, "<"),
        new SequenceTokenParser(TokenType.Operator, ">="),
        new SequenceTokenParser(TokenType.Operator, ">"),
        new SequenceTokenParser(TokenType.Operator, "!="),
        new SequenceTokenParser(TokenType.Operator, "!"),
        new SequenceTokenParser(TokenType.Operator, "||"),
        new SequenceTokenParser(TokenType.Operator, "&&"),
        new SequenceTokenParser(TokenType.Operator, "??"),
        new SequenceTokenParser(TokenType.Operator, "?="),
        new SequenceTokenParser(TokenType.Operator, "?"),
        new SequenceTokenParser(TokenType.Operator, ":"),

        // arithmetical
        new SequenceTokenParser(TokenType.Operator, "+"),
        new SequenceTokenParser(TokenType.Operator, "-"),
        new SequenceTokenParser(TokenType.Operator, "*"),
        new SequenceTokenParser(TokenType.Operator, "/"),
        new SequenceTokenParser(TokenType.Operator, "+"),
        
        new IdentifierTokenParser(),
    ];

    public Lexer(TextReader textReader)
    {
        var reader = new CharacterReader(textReader);
         _buffer = new(reader.GetCharacter);
    }

    public TokenData GetToken()
    {
        _buffer.Return();
        var firstCharacter = _buffer.PeekNext();
        if (firstCharacter.Char is UnifiedCharacters.WhiteSpace or UnifiedCharacters.NewLine)
        {
            _buffer.Take(1);
            return GetToken();
        }
        
        foreach (var parser in _tokenParsers)
        {
            parser.Reset();
        }
        
        var possibleParsers = new HashSet<TokenParser>();
        foreach (var parser in _tokenParsers)
        {
            var status = parser.AcceptChar(firstCharacter.Char);
            if (status == AcceptStatus.Deny)
            {
                continue;
            }
            
            while (status is AcceptStatus.Possible)
            {
                var currentChar = _buffer.PeekNext().Char;
                if (currentChar is UnifiedCharacters.NewLine or UnifiedCharacters.EndOfText)
                {
                    break;
                }
                status = parser.AcceptChar(currentChar);
            }
                
            if (status == AcceptStatus.Completed)
            {
                var token = parser.CreateToken(firstCharacter.Line, firstCharacter.Column);
                _buffer.Take(token.CharacterLength);
                return token;
            }
            
            _buffer.Return();
            _buffer.PeekNext(); // to fix pointer
            possibleParsers.Add(parser);
        }

        foreach (var parser in possibleParsers)
        {
            if (parser.IsValid())
            {
                var token = parser.CreateToken(firstCharacter.Line, firstCharacter.Column);
                _buffer.Take(token.CharacterLength);
                return token;
            }
        }

        throw PrepareException(firstCharacter, possibleParsers);
    }

    private Exception PrepareException(Character invalidTokenFirstCharacter, IEnumerable<TokenParser> possibleParsers)
    {
        // error position
        var errorPosition = $"[{invalidTokenFirstCharacter.Line,2}, {invalidTokenFirstCharacter.Column,2}]";
        
        // error line
        var errorLine = invalidTokenFirstCharacter.Char.ToString();
        while (true)
        {
            var c = _buffer.PeekNext().Char;
            if (c is UnifiedCharacters.EndOfText or UnifiedCharacters.NewLine or UnifiedCharacters.WhiteSpace)
            {
                break;
            }
            errorLine += c;
        }
        
        // hints
        var hints = possibleParsers
                    .Select(parser => parser.GetErrorHint())
                    .OfType<string>()
                    .Aggregate("", (current, hint) => current + "\n" + hint);

        var errorMessage = $"{errorPosition}: Invalid token \'{errorLine}\'. {hints}";
        return new Exception(errorMessage);
    }
}