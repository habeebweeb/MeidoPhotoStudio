using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

using TransformType = MeidoPhotoStudio.Plugin.Core.TransformChangeEventArgs.TransformType;

namespace MeidoPhotoStudio.Plugin.Core;

public class TransformWatcher : MonoBehaviour
{
    private readonly Dictionary<Transform, Action<TransformType>> subscribedTransforms = [];
    private readonly Dictionary<Transform, TransformBackup> transformBackups = [];

    public void Subscribe(Transform transform, Action<TransformType> callback)
    {
        _ = transform ? transform : throw new ArgumentNullException(nameof(transform));
        _ = callback ?? throw new ArgumentNullException(nameof(callback));

        if (subscribedTransforms.ContainsKey(transform))
            return;

        subscribedTransforms.Add(transform, callback);
        transformBackups.Add(transform, new(transform));
        transform.hasChanged = false;
    }

    public void Unsubscribe(Transform transform)
    {
        _ = transform ? transform : throw new ArgumentNullException(nameof(transform));

        if (!subscribedTransforms.ContainsKey(transform))
            return;

        subscribedTransforms.Remove(transform);
        transformBackups.Remove(transform);

        if (!subscribedTransforms.Any())
            enabled = false;
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

            callback(changeType);

            transformBackups[transform] = newBackup;

            transform.hasChanged = false;
        }
    }
}
