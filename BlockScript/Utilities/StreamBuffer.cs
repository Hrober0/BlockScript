namespace BlockScript.Utilities;

public class StreamBuffer<T>(Func<T> getNew)
{
    public T Current { get; private set; } = getNew();
    public T Next { get; private set; } = getNew();

    public T Take()
    {
        Current = Next;
        Next = getNew();
        return Current;
    }
}