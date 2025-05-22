using BlockScript.Exceptions;
using BlockScript.Lexer.FactorValues;
using BlockScript.Parser.Statements;
using BlockScript.Reader;

namespace BlockScript.Interpreter;

public class Context(Context? _parent)
{
    private readonly Dictionary<string, IFactorValue> _data = new();

    public bool TryGetData(string identifier, out IFactorValue value)
    {
        if (_data.TryGetValue(identifier, out value!))
        {
            return true;
        }
        return _parent?.TryGetData(identifier, out value) ?? false;
    }
    
    public IFactorValue GetContextData(string identifier, Position position)
    {
        if (!TryGetData(identifier, out var dataValue))
        {
            throw new RuntimeException(position, $"Variable of name {identifier} was not defined!");
        }

        return dataValue;
    }
    
    public void AddData(string identifier, IFactorValue value) => _data[identifier] = value;
}