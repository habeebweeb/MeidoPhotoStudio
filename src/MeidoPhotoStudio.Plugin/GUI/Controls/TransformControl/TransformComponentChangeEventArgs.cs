using System;

namespace MeidoPhotoStudio.Plugin;

public class TransformComponentChangeEventArgs : EventArgs
{
    public TransformComponentChangeEventArgs(TransformComponent component, float value)
    {
        Component = component;
        Value = value;
    }

    public enum TransformComponent
    {
        X,
        Y,
        Z,
    }

    public TransformComponent Component { get; }

    public float Value { get; }

    public void Deconstruct(out TransformComponent component, out float value)
    {
        component = Component;
        value = Value;
    }
}
