namespace MeidoPhotoStudio.Plugin;

public class TransformComponentChangeEventArgs(
    TransformComponentChangeEventArgs.TransformComponent component, float value) : EventArgs
{
    public enum TransformComponent
    {
        X,
        Y,
        Z,
    }

    public TransformComponent Component { get; } = component;

    public float Value { get; } = value;

    public void Deconstruct(out TransformComponent component, out float value)
    {
        component = Component;
        value = Value;
    }
}
