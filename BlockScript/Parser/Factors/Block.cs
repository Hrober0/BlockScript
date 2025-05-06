using System.Text;
using BlockScript.Parser.Statements;
using BlockScript.Utilities;

namespace BlockScript.Parser.Factors;

public class Block : IFactor
{
    public List<IStatement> Statements { get; }

    public Block(List<IStatement> statements)
    {
        Statements = statements;
    }
    
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