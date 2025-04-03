namespace BlockScript.Lexer;

public enum TokenType
{
    EndOfText,
    EndOfStatement,
    Operator,
    
    Loop,
    
    Integer,
    String,
    Boolean,
    Null,
    
    Comment,
    
    Identifier,
}
