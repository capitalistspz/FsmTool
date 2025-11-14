namespace FsmTool;

public sealed class Ref<T>(T value)
{
    public T Value { get; set; } = value;
    public static implicit operator T(Ref<T> value) => value.Value;
}
