
/// <summary>
/// Opaque handle returned by PushContext. Pass it back to PopContext to remove
/// exactly the entry you pushed.
/// </summary>
public readonly struct InputContextHandle
{
    internal readonly int Id;
    internal InputContextHandle(int id) => Id = id;

    public static readonly InputContextHandle Invalid = new InputContextHandle(-1);
    public bool IsValid => Id >= 0;
}
