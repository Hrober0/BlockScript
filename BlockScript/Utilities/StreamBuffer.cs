namespace BlockScript.Utilities;

public class StreamBuffer<T>(Func<T> getNew)
{
    public T Current { get; private set; } = getNew();
    public T Next { get; private set; } = getNew();

    public T Take()
    {
        var ret = Current;
        Current = Next;
        Next = getNew();
        return ret;
    }
}