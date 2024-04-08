using System;
using System.Collections;
using System.Collections.Generic;

using MeidoPhotoStudio.Database.Props;
using MeidoPhotoStudio.Database.Props.Menu;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Core.Props;

public class PropService : IEnumerable<PropController>, IIndexableCollection<PropController>
{
    private readonly List<PropController> propControllers = new();

    public event EventHandler<PropServiceEventArgs> AddedProp;

    public event EventHandler<PropServiceEventArgs> RemovingProp;

    public event EventHandler<PropServiceEventArgs> RemovedProp;

    public int Count =>
        propControllers.Count;

    public PropController this[int index] =>
        (uint)index >= propControllers.Count
            ? throw new ArgumentOutOfRangeException(nameof(index))
            : propControllers[index];

    public void Add(IPropModel propModel)
    {
        ShapeKeyController shapeKeyController = null;

        var propGameObject = propModel is MenuFilePropModel menuFilePropModel
            ? InstantiateMenuProp(menuFilePropModel, out shapeKeyController)
            : InstantiateProp(propModel);

        if (!propGameObject)
            return;

        Add(new PropController(propModel, propGameObject, shapeKeyController));
    }

    public void Clone(int index)
    {
        if ((uint)index >= propControllers.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        var originalProp = propControllers[index];

        ShapeKeyController shapeKeyController = null;

        var copiedPropGameObject = originalProp.PropModel is MenuFilePropModel menuFilePropModel
            ? InstantiateMenuProp(menuFilePropModel, out shapeKeyController)
            : InstantiateProp(originalProp.PropModel);

        if (!copiedPropGameObject)
            return;

        var copiedProp = new PropController(originalProp.PropModel, copiedPropGameObject, shapeKeyController);

        CopyProperties(originalProp, copiedProp);

        MoveProp(originalProp.GameObject.transform, copiedPropGameObject.transform);

        Add(copiedProp);

        static void CopyProperties(PropController original, PropController copy)
        {
            copy.ShadowCasting = original.ShadowCasting;

            var originalTransform = original.GameObject.transform;
            var copiedTransform = copy.GameObject.transform;

            copiedTransform.SetPositionAndRotation(originalTransform.position, originalTransform.rotation);
            copiedTransform.localScale = originalTransform.localScale;

            if (original.ShapeKeyController is not null && copy.ShapeKeyController is not null)
                foreach (var (shapeKey, value) in original.ShapeKeyController)
                    copy.ShapeKeyController[shapeKey] = value;
        }

        static void MoveProp(Transform original, Transform copy)
        {
            var propRenderer = original.GetComponentInChildren<Renderer>();
            var cameraTransform = GameMain.Instance.MainCamera.camera.transform;

            var distance = propRenderer.bounds.extents.x * 0.75f;

            copy.Translate(cameraTransform.right * distance, Space.World);
        }
    }

    public int IndexOf(PropController propController) =>
        propController is null
            ? throw new ArgumentNullException(nameof(propController))
            : propControllers.IndexOf(propController);

    public void Remove(int index)
    {
        if ((uint)index >= propControllers.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        var propController = propControllers[index];

        RemovingProp?.Invoke(this, new(propController, index));

        propControllers.RemoveAt(index);

        RemovedProp?.Invoke(this, new(propController, index));

        UnityEngine.Object.Destroy(propController.GameObject);
    }

    public void Remove(PropController propController)
    {
        var propIndex = propControllers.IndexOf(propController);

        if (propIndex is -1)
        {
            // TODO: Log prop not found.
            return;
        }

        Remove(propIndex);
    }

    public void Clear()
    {
        for (var i = propControllers.Count - 1; i >= 0; i--)
            Remove(i);
    }

    public IEnumerator<PropController> GetEnumerator() =>
        propControllers.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    private GameObject InstantiateMenuProp(
        MenuFilePropModel menuFilePropModel, out ShapeKeyController shapeKeyController)
    {
        var propGameObject = new MenuFilePropInstantiator().Instantiate(menuFilePropModel, out shapeKeyController);

        return propGameObject;
    }

    private GameObject InstantiateProp(IPropModel propModel)
    {
        var propGameObject = new PropInstantiator().Instantiate(propModel);

        if (propGameObject)
            return propGameObject;

        Utility.LogDebug($"Could not instantiate prop: {propModel.Name}");

        return null;
    }

    private void Add(PropController propController)
    {
        propControllers.Add(propController);

        AddedProp?.Invoke(this, new PropServiceEventArgs(propController, propControllers.Count - 1));
    }
}
