using BlockScript.Lexer.TokenParsers;
using BlockScript.Reader;
using BlockScript.Utilities;

namespace BlockScript.Lexer;

public class Lexer : ILexer
{
    private readonly Buffer<Character> _buffer;
    
    private readonly List<TokenParser> _tokenParsers =
    [
        new SequenceTokenParser(TokenType.EndOfText, CharacterReader.EndOfText.ToString()),

        // syntax
        new SequenceTokenParser(TokenType.EndOfStatement, ";"),
        new SequenceTokenParser(TokenType.Operator, "("),
        new SequenceTokenParser(TokenType.Operator, ")"),
        new SequenceTokenParser(TokenType.Operator, "{"),
        new SequenceTokenParser(TokenType.Operator, "}"),
        new CommentTokenParser(),
        
        // key-words
        new SequenceTokenParser(TokenType.Loop, "loop"),

        // data
        new IntTokenParser(),
        new StringTokenParser(),
        new SequenceTokenParser(TokenType.Null, "null"),
        new SequenceTokenParser(TokenType.Boolean, "true", _ => true),
        new SequenceTokenParser(TokenType.Boolean, "false", _ => false),
        
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
        if (firstCharacter.Char is CharacterReader.WhiteSpace or CharacterReader.NewLine)
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
                if (currentChar is CharacterReader.NewLine or CharacterReader.EndOfText)
                {
                    break;
                }
                status = parser.AcceptChar(currentChar);
            }
                
            if (status == AcceptStatus.Completed)
            {
                _buffer.TakeAll();
                return parser.CreateToken(firstCharacter.Line, firstCharacter.Column);
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
            if (c is CharacterReader.EndOfText or CharacterReader.NewLine or CharacterReader.WhiteSpace)
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