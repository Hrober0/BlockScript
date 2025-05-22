using BlockScript.Exceptions;
using BlockScript.Parser.Statements;
using BlockScript.Reader;

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
    
    public object? GetContextData(string identifier, Position position)
    {
        if (!TryGetData(identifier, out var dataValue))
        {
            throw new RuntimeException(position, $"Variable of name {identifier} was not defined!");
        }

        return dataValue;
    }
    
    public void AddData(string identifier, object? statement) => _data[identifier] = statement;
}