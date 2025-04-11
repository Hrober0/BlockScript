namespace BlockScript.Lexer;

public enum TokenType
{
    // Syntax
    EndOfText,
    EndOfStatement,
    ParenhticesOpen,
    ParenhticesClose,
    BraceOpen,
    BraceClose,
    
    // Key words
    Loop,
    
    // Data
    Integer,
    String,
    Boolean,
    Null,
    
    Comment,
    Identifier,

    // Logical Operators
    OperatorEqual,
    OperatorArrow,
    OperatorAssign,
    OperatorLessEqual,
    OperatorLess,
    OperatorGreaterEqual,
    OperatorGreater,
    OperatorNotEqual,
    OperatorNot,
    OperatorOr,
    OperatorAnd,
    OperatorNullCoalescing,
    OperatorNullAssign,
    OperatorTernaryIf,
    OperatorTernaryElse,

    // Arithmetical Operators
    OperatorAdd,
    OperatorSubtract,
    OperatorMultiply,
    OperatorDivide,
}
