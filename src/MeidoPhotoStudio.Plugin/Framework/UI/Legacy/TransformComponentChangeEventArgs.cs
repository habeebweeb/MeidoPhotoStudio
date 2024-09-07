namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public class TransformComponentChangeEventArgs(
    TransformComponentChangeEventArgs.TransformComponent component, float componentValue, Vector3 value)
    : EventArgs
{
    public enum TransformComponent
    {
        X,
        Y,
        Z,
    }

    public TransformComponent Component { get; } = component;

    public float ComponentValue { get; } = componentValue;

    public Vector3 Value { get; } = value;

    public void Deconstruct(out TransformComponent component, out float componentValue, out Vector3 value)
    {
        component = Component;
        componentValue = ComponentValue;
        value = Value;
    }
}
