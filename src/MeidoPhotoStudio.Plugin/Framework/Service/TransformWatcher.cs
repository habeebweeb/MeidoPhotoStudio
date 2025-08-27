using MeidoPhotoStudio.Plugin.Framework.Extensions;

using TransformType = MeidoPhotoStudio.Plugin.Framework.Service.TransformChangeEventArgs.TransformType;

namespace MeidoPhotoStudio.Plugin.Framework.Service;

public class TransformWatcher : MonoBehaviour
{
    private readonly Dictionary<Transform, Action<TransformChangeEventArgs>> subscribedTransforms = [];
    private readonly Dictionary<Transform, TransformBackup> transformBackups = [];

    public void Subscribe(Transform transform, Action<TransformChangeEventArgs> callback)
    {
        _ = transform ? transform : throw new ArgumentNullException(nameof(transform));
        _ = callback ?? throw new ArgumentNullException(nameof(callback));

        if (subscribedTransforms.ContainsKey(transform))
            return;

        subscribedTransforms.Add(transform, callback);
        transformBackups.Add(transform, new(transform));
        transform.hasChanged = true;
    }

    public void Unsubscribe(Transform transform)
    {
        _ = transform ? transform : throw new ArgumentNullException(nameof(transform));

        if (!subscribedTransforms.ContainsKey(transform))
            return;

        subscribedTransforms.Remove(transform);
        transformBackups.Remove(transform);
    }

    internal void Clear()
    {
        subscribedTransforms.Clear();
        transformBackups.Clear();
    }

    private void LateUpdate()
    {
        foreach (var (transform, callback) in subscribedTransforms)
        {
            if (!transform)
                continue;

            if (!transform.hasChanged)
                continue;

            var newBackup = new TransformBackup(transform);

            var (_, oldPosition, oldRotation, oldScale) = transformBackups[transform];
            var (_, newPosition, newRotation, newScale) = newBackup;

            var changeType = TransformType.None;

            if (oldPosition != newPosition)
                changeType |= TransformType.Position;

            if (oldRotation != newRotation)
                changeType |= TransformType.Rotation;

            if (oldScale != newScale)
                changeType |= TransformType.Scale;

            callback(new(changeType));

            transformBackups[transform] = newBackup;

            transform.hasChanged = false;
        }
    }
}
