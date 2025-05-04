using System.Text;
using BlockScript.Parser.Statements;
using BlockScript.Utilities;

namespace BlockScript.Parser.Factors;

public class Block(List<IStatement> statements) : IFactor
{
    public override string ToString()
    {
        var sb = new StringBuilder("Block {");
        foreach (var statement in statements)
        {
            sb.AppendLine(statement.ToString());
        }

        sb.AppendLine("}");
        return $"Block {{\n{statements.Stringify("\n")}\n}}";
    }
}