using BlockScript.Parser.Statements;

namespace BlockScript.Interpreter;

public class Context(Context? _parent)
{
    private readonly Dictionary<string, object?> _data = new();

    public bool TryGetData(string identifier, out object? statement)
    {
        if (_data.TryGetValue(identifier, out statement!))
        {
            return true;
        }
        return _parent?.TryGetData(identifier, out statement) ?? false;
    }
    
    public void AddData(string identifier, object? statement) => _data[identifier] = statement;
}