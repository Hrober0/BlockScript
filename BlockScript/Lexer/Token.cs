public enum TokenType
{
    EOT,
    Operator,
}

public class Token
{
    public TokenType Type {  get; set; }
    public string Value { get; set; }
    public int Line { get; set; }
    public int Column { get; set; }

    public override string ToString()
    {
        return $"[{Line},{Column}]: {Type} \'{Value}\'";
    }
}