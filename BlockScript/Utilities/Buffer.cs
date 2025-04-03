namespace BlockScript.Utilities;

public class Buffer<T>(Func<T> getNew)
{
    private readonly List<T> _buffer = [];
    private int _pointer;
    
    public T PeekNext()
    {
        if (_pointer >= _buffer.Count)
        {
            _buffer.Add(getNew());
        }
        return _buffer[_pointer++];
    }

    public void TakeAll()
    {
        _buffer.RemoveRange(0, _pointer);
        _pointer = 0;
    }
    
    public void Take(int elementsToTake)
    {
        if (elementsToTake > _buffer.Count)
        {
            throw new IndexOutOfRangeException();
        }
        _buffer.RemoveRange(0, elementsToTake);
        _pointer = Math.Max(_pointer - elementsToTake, 0);
    }

    public void Return()
    {
        _pointer = 0;
    }
}