﻿using System.Text;
using BlockScript.Lexer;
using BlockScript.Parser.Statements;
using BlockScript.Reader;
using BlockScript.Utilities;

namespace BlockScript.Parser.Factors;

public record Block(List<IStatement> Statements, Position Position) : IFactor
{
    public Block(List<IStatement> Statements) : this(Statements, Position.Default) { }
    
    public override string ToString()
    {
        var sb = new StringBuilder("Block {");
        foreach (var statement in Statements)
        {
            sb.AppendLine(statement.ToString());
        }

        sb.AppendLine("}");
        return $"Block {{\n{Statements.Stringify("\n")}\n}}";
    }
}