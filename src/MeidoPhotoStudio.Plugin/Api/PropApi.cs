using MeidoPhotoStudio.Plugin.Core;
using MeidoPhotoStudio.Plugin.Core.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Props;

namespace MeidoPhotoStudio.Plugin.Api;

public class PropApi : ApiBase
{
    private readonly PropService propService;
    private readonly PropAttachmentService propAttachmentService;

    private readonly SelectionController<PropController> propSelectionController;

    public PropApi(
        PluginCore pluginCore,
        PropService propService,
        PropAttachmentService propAttachmentService,
        SelectionController<PropController> propSelectionController)
        : base(pluginCore)
    {
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        this.propAttachmentService = propAttachmentService ?? throw new ArgumentNullException(nameof(propAttachmentService));
        this.propSelectionController = propSelectionController ?? throw new ArgumentNullException(nameof(propSelectionController));

        this.propService.AddedProp += OnPropAdded;
        this.propService.RemovingProp += OnPropRemoving;
        this.propService.RemovedProp += OnPropRemoved;
    }

    public event EventHandler<PropServiceEventArgs> AddedProp;

    public event EventHandler<PropServiceEventArgs> RemovingProp;

    public event EventHandler<PropServiceEventArgs> RemovedProp;

    public IEnumerable<PropController> Props
    {
        get
        {
            Valid();

            return propService;
        }
    }

    public PropController SelectedProp
    {
        get
        {
            Valid();

            return propSelectionController.Current;
        }

        set
        {
            Valid();

            _ = value ?? throw new ArgumentNullException(nameof(value));

            propSelectionController.Select(value);
        }
    }

    public int SelectedPropIndex
    {
        get
        {
            Valid();

            return propSelectionController.CurrentIndex;
        }

        set
        {
            Valid();

            if ((uint)value >= propService.Count)
                throw new ArgumentOutOfRangeException(nameof(value));

            propSelectionController.Select(value);
        }
    }

    public int PropCount
    {
        get
        {
            Valid();

            return propService.Count;
        }
    }

    public PropController this[int index]
    {
        get
        {
            Valid();

            return (uint)index >= propService.Count
                ? throw new ArgumentOutOfRangeException(nameof(index))
                : propService[index];
        }
    }

    public bool TryGetAttachmentInfo(PropController propController, out AttachPointInfo info)
    {
        Valid();

        _ = propController ?? throw new ArgumentNullException(nameof(propController));

        return propAttachmentService.TryGetAttachPointInfo(propController, out info);
    }

    public PropController AddProp(IPropModel model)
    {
        Valid();

        _ = model ?? throw new ArgumentNullException(nameof(model));

        PropController prop = null;

        try
        {
            propService.AddedProp += OnPropAdded;
            propService.Add(model);
        }
        catch
        {
            throw;
        }
        finally
        {
            propService.AddedProp -= OnPropAdded;
        }

        return prop;

        void OnPropAdded(object sender, PropServiceEventArgs e) =>
            prop = e.PropController;
    }

    public void RemoveProp(int index)
    {
        Valid();

        if ((uint)index >= propService.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        propService.Remove(index);
    }

    public void RemoveProp(PropController prop)
    {
        Valid();

        _ = prop ?? throw new ArgumentNullException(nameof(prop));

        propService.Remove(prop);
    }

    public void RemoveAllProps()
    {
        Valid();

        propService.Clear();
    }

    public void AttachPropTo(PropController prop, CharacterController character, AttachPoint attachPoint, bool keepPosition)
    {
        Valid();

        _ = prop ?? throw new ArgumentNullException(nameof(prop));
        _ = character ?? throw new ArgumentNullException(nameof(character));

        if (!Enum.IsDefined(typeof(AttachPoint), attachPoint))
            throw new ArgumentOutOfRangeException(nameof(attachPoint));

        propAttachmentService.AttachPropTo(prop, character, attachPoint, keepPosition);
    }

    public void DetachProp(PropController prop)
    {
        Valid();

        _ = prop ?? throw new ArgumentNullException(nameof(prop));

        propAttachmentService.DetachProp(prop);
    }

    private void OnPropAdded(object sender, PropServiceEventArgs e) =>
        AddedProp?.Invoke(this, e);

    private void OnPropRemoving(object sender, PropServiceEventArgs e) =>
        RemovingProp?.Invoke(this, e);

    private void OnPropRemoved(object sender, PropServiceEventArgs e) =>
        RemovedProp?.Invoke(this, e);
}
