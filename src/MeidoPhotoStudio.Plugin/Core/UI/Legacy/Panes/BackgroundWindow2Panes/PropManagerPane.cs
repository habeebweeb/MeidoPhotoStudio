using System.ComponentModel;

using MeidoPhotoStudio.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class PropManagerPane : BasePane
{
    private static readonly string[] GizmoSpaceTranslationKeys = ["gizmoSpaceLocal", "gizmoSpaceWorld"];

    private readonly PropService propService;
    private readonly PropDragHandleService propDragHandleService;
    private readonly SelectionController<PropController> propSelectionController;
    private readonly TransformClipboard transformClipboard;
    private readonly Dropdown<PropController> propDropdown;
    private readonly Dictionary<PropController, string> propNames = [];
    private readonly Toggle dragPointToggle;
    private readonly Toggle gizmoToggle;
    private readonly Toggle shadowCastingToggle;
    private readonly Toggle visibleToggle;
    private readonly Button deletePropButton;
    private readonly Button copyPropButton;
    private readonly SelectionGrid gizmoMode;
    private readonly TransformControl positionTransformControl;
    private readonly TransformControl rotationTransformControl;
    private readonly TransformControl scaleTransformControl;
    private readonly Button focusButton;
    private readonly PaneHeader paneHeader;
    private readonly Toggle toggleAllDragHandles;
    private readonly Toggle toggleAllGizmos;
    private readonly Label gizmoSpaceLabel;
    private readonly Header toggleAllHandlesHeader;
    private readonly Label noPropsLabel;

    private bool transformControlChangedTransform;

    public PropManagerPane(
        PropService propService,
        PropDragHandleService propDragHandleService,
        SelectionController<PropController> propSelectionController,
        TransformClipboard transformClipboard)
    {
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        this.propDragHandleService = propDragHandleService ?? throw new ArgumentNullException(nameof(propDragHandleService));
        this.propSelectionController = propSelectionController ?? throw new ArgumentNullException(nameof(propSelectionController));
        this.transformClipboard = transformClipboard ?? throw new ArgumentNullException(nameof(transformClipboard));

        this.propService.AddedProp += OnAddedProp;
        this.propService.RemovedProp += OnRemovedProp;
        this.propSelectionController.Selecting += OnSelectingProp;
        this.propSelectionController.Selected += OnSelectedProp;

        propDropdown = new(formatter: PropNameFormatter);
        propDropdown.SelectionChanged += OnPropDropdownSelectionChange;

        dragPointToggle = new(Translation.Get("propManagerPane", "dragPointToggle"));
        dragPointToggle.ControlEvent += OnDragPointToggleChanged;

        gizmoToggle = new(Translation.Get("propManagerPane", "gizmoToggle"));
        gizmoToggle.ControlEvent += OnGizmoToggleChanged;

        shadowCastingToggle = new(Translation.Get("propManagerPane", "shadowCastingToggle"));
        shadowCastingToggle.ControlEvent += OnShadowCastingToggleChanged;

        visibleToggle = new(Translation.Get("propManagerPane", "visibleToggle"), true);
        visibleToggle.ControlEvent += OnVisibleToggleChanged;

        copyPropButton = new(Translation.Get("propManagerPane", "copyButton"));
        copyPropButton.ControlEvent += OnCopyButtonPressed;

        deletePropButton = new(Translation.Get("propManagerPane", "deleteButton"));
        deletePropButton.ControlEvent += OnDeleteButtonPressed;

        gizmoMode = new(Translation.GetArray("propManagerPane", GizmoSpaceTranslationKeys));
        gizmoMode.ControlEvent += OnGizmoModeToggleChanged;

        focusButton = new(Translation.Get("propManagerPane", "focusPropButton"));
        focusButton.ControlEvent += OnFocusButtonPushed;

        toggleAllDragHandles = new(Translation.Get("propManagerPane", "allDragHandleToggle"), true);
        toggleAllDragHandles.ControlEvent += OnToggleAllDragHandlesChanged;

        toggleAllGizmos = new(Translation.Get("propManagerPane", "allGizmoToggle"), true);
        toggleAllGizmos.ControlEvent += OnToggleAllGizmosChanged;

        gizmoSpaceLabel = new(Translation.Get("propManagerPane", "gizmoSpaceToggle"));

        positionTransformControl = new(Translation.Get("propManagerPane", "positionControl"), Vector3.zero)
        {
            TransformType = TransformClipboard.TransformType.Position,
            Clipboard = this.transformClipboard,
        };

        positionTransformControl.ControlEvent += OnTransformControlChanged;

        rotationTransformControl = new(Translation.Get("propManagerPane", "rotationControl"), Vector3.zero)
        {
            TransformType = TransformClipboard.TransformType.Rotation,
            Clipboard = this.transformClipboard,
        };

        rotationTransformControl.ControlEvent += OnTransformControlChanged;

        scaleTransformControl = new(Translation.Get("propManagerPane", "scaleControl"), Vector3.one)
        {
            TransformType = TransformClipboard.TransformType.Scale,
            Clipboard = this.transformClipboard,
        };

        scaleTransformControl.ControlEvent += OnTransformControlChanged;

        var copyButtonLabel = Translation.Get("transformControl", "copyButton");
        var pasteButtonLabel = Translation.Get("transformControl", "pasteButton");
        var resetButtonLabel = Translation.Get("transformControl", "resetButton");

        positionTransformControl.SetButtonLabels(copyButtonLabel, pasteButtonLabel, resetButtonLabel);
        rotationTransformControl.SetButtonLabels(copyButtonLabel, pasteButtonLabel, resetButtonLabel);
        scaleTransformControl.SetButtonLabels(copyButtonLabel, pasteButtonLabel, resetButtonLabel);

        toggleAllHandlesHeader = new(Translation.Get("propManagerPane", "toggleAllHandlesHeader"));
        paneHeader = new(Translation.Get("propManagerPane", "header"), true);

        noPropsLabel = new(Translation.Get("propManagerPane", "noProps"));

        string PropNameFormatter(PropController prop, int index) =>
            propNames[prop];
    }

    private PropController CurrentProp =>
        propSelectionController.Current;

    public override void Draw()
    {
        paneHeader.Draw();

        if (!paneHeader.Enabled)
            return;

        if (propService.Count is 0)
        {
            noPropsLabel.Draw();

            return;
        }

        GUILayout.BeginHorizontal();

        propDropdown.Draw(GUILayout.Width(185f));

        var arrowLayoutOptions = new[]
        {
            GUILayout.ExpandWidth(false),
            GUILayout.ExpandHeight(false),
        };

        if (GUILayout.Button("<", arrowLayoutOptions))
            propDropdown.CyclePrevious();

        if (GUILayout.Button(">", arrowLayoutOptions))
            propDropdown.CycleNext();

        GUILayout.EndHorizontal();

        MpsGui.BlackLine();

        var noExpandWidth = GUILayout.ExpandWidth(false);

        GUILayout.BeginHorizontal();
        dragPointToggle.Draw(noExpandWidth);
        GUILayout.FlexibleSpace();
        focusButton.Draw(noExpandWidth);
        copyPropButton.Draw(noExpandWidth);
        deletePropButton.Draw(noExpandWidth);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        gizmoToggle.Draw(noExpandWidth);
        GUILayout.FlexibleSpace();

        var guiEnabled = GUI.enabled;

        GUI.enabled = guiEnabled && gizmoToggle.Value;

        gizmoSpaceLabel.Draw();
        gizmoMode.Draw();

        GUI.enabled = guiEnabled;

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        visibleToggle.Draw(noExpandWidth);

        guiEnabled = GUI.enabled;

        GUI.enabled = guiEnabled && visibleToggle.Value;
        shadowCastingToggle.Draw(noExpandWidth);
        GUI.enabled = guiEnabled;

        GUILayout.EndHorizontal();

        toggleAllHandlesHeader.Draw();
        MpsGui.WhiteLine();

        GUILayout.BeginHorizontal();

        toggleAllDragHandles.Draw();
        toggleAllGizmos.Draw();

        GUILayout.EndHorizontal();

        MpsGui.BlackLine();

        positionTransformControl.Draw();
        rotationTransformControl.Draw();
        scaleTransformControl.Draw();

        GUI.enabled = true;
    }

    protected override void ReloadTranslation()
    {
        dragPointToggle.Label = Translation.Get("propManagerPane", "dragPointToggle");
        gizmoToggle.Label = Translation.Get("propManagerPane", "gizmoToggle");
        shadowCastingToggle.Label = Translation.Get("propManagerPane", "shadowCastingToggle");
        visibleToggle.Label = Translation.Get("propManagerPane", "visibleToggle");
        copyPropButton.Label = Translation.Get("propManagerPane", "copyButton");
        deletePropButton.Label = Translation.Get("propManagerPane", "deleteButton");
        gizmoMode.SetItemsWithoutNotify(Translation.GetArray("propManagerPane", GizmoSpaceTranslationKeys));
        focusButton.Label = Translation.Get("propManagerPane", "focusPropButton");
        toggleAllDragHandles.Label = Translation.Get("propManagerPane", "allDragHandleToggle");
        toggleAllGizmos.Label = Translation.Get("propManagerPane", "allGizmoToggle");
        gizmoSpaceLabel.Text = Translation.Get("propManagerPane", "gizmoSpaceToggle");

        var copyButtonLabel = Translation.Get("transformControl", "copyButton");
        var pasteButtonLabel = Translation.Get("transformControl", "pasteButton");
        var resetButtonLabel = Translation.Get("transformControl", "resetButton");

        positionTransformControl.Header = Translation.Get("propManagerPane", "positionControl");
        positionTransformControl.SetButtonLabels(copyButtonLabel, pasteButtonLabel, resetButtonLabel);

        rotationTransformControl.Header = Translation.Get("propManagerPane", "rotationControl");
        rotationTransformControl.SetButtonLabels(copyButtonLabel, pasteButtonLabel, resetButtonLabel);

        scaleTransformControl.Header = Translation.Get("propManagerPane", "scaleControl");
        scaleTransformControl.SetButtonLabels(copyButtonLabel, pasteButtonLabel, resetButtonLabel);

        toggleAllHandlesHeader.Text = Translation.Get("propManagerPane", "toggleAllHandlesHeader");
        paneHeader.Label = Translation.Get("propManagerPane", "header");
        noPropsLabel.Text = Translation.Get("propManagerPane", "noProps");
    }

    private void UpdateControls()
    {
        if (CurrentProp is null)
            return;

        shadowCastingToggle.SetEnabledWithoutNotify(CurrentProp.ShadowCasting);
        visibleToggle.SetEnabledWithoutNotify(CurrentProp.Visible);

        var dragHandleController = propDragHandleService[CurrentProp];

        dragPointToggle.SetEnabledWithoutNotify(dragHandleController.Enabled);
        gizmoToggle.SetEnabledWithoutNotify(dragHandleController.GizmoEnabled);
        gizmoMode.SetValueWithoutNotify((int)dragHandleController.GizmoMode);

        var propTransform = CurrentProp.GameObject.transform;

        positionTransformControl.SetValueWithoutNotify(propTransform.position);
        positionTransformControl.DefaultValue = CurrentProp.InitialTransform.Position;

        rotationTransformControl.SetValueWithoutNotify(propTransform.eulerAngles);
        rotationTransformControl.DefaultValue = CurrentProp.InitialTransform.Rotation.eulerAngles;

        scaleTransformControl.SetValueWithoutNotify(propTransform.localScale);
        scaleTransformControl.DefaultValue = CurrentProp.InitialTransform.LocalScale;
    }

    private void OnToggleAllDragHandlesChanged(object sender, EventArgs e)
    {
        foreach (var controller in propDragHandleService)
            controller.Enabled = toggleAllDragHandles.Value;
    }

    private void OnToggleAllGizmosChanged(object sender, EventArgs e)
    {
        foreach (var controller in propDragHandleService)
            controller.GizmoEnabled = toggleAllGizmos.Value;
    }

    private void OnAddedProp(object sender, PropServiceEventArgs e)
    {
        propNames[e.PropController] = UniquePropName(new(propNames.Values), e.PropController.PropModel);
        propDropdown.SetItems(propService, propService.Count - 1);

        static string UniquePropName(HashSet<string> currentNames, IPropModel propModel)
        {
            var propName = PropName(propModel);
            var newPropName = propName;
            var index = 1;

            while (currentNames.Contains(newPropName))
            {
                index++;
                newPropName = $"{propName} ({index})";
            }

            return newPropName;
        }

        static string PropName(IPropModel propModel) =>
            propModel.Name;
    }

    private void OnRemovedProp(object sender, PropServiceEventArgs e)
    {
        if (propService.Count is 0)
        {
            propDropdown.Clear();
            propNames.Clear();

            return;
        }

        var propIndex = propDropdown.SelectedItemIndex >= propService.Count
            ? propService.Count - 1
            : propDropdown.SelectedItemIndex;

        propNames.Remove(e.PropController);
        propDropdown.SetItems(propService, propIndex);
    }

    private void OnPropTransformChanged(object sender, EventArgs e)
    {
        var prop = (PropController)sender;

        if (transformControlChangedTransform)
        {
            transformControlChangedTransform = false;

            return;
        }

        var transform = prop.GameObject.transform;

        positionTransformControl.SetValueWithoutNotify(transform.position);
        rotationTransformControl.SetValueWithoutNotify(transform.eulerAngles);
        scaleTransformControl.SetValueWithoutNotify(transform.localScale);
    }

    private void OnSelectingProp(object sender, SelectionEventArgs<PropController> e)
    {
        if (e.Selected is not PropController prop)
            return;

        prop.TransformChanged -= OnPropTransformChanged;
        prop.PropertyChanged -= OnPropPropertyChanged;

        var dragHandleController = propDragHandleService[prop];

        dragHandleController.PropertyChanged -= OnDragHandlePropertyChanged;
    }

    private void OnSelectedProp(object sender, SelectionEventArgs<PropController> e)
    {
        if (e.Selected is not PropController prop)
            return;

        prop.TransformChanged += OnPropTransformChanged;
        prop.PropertyChanged += OnPropPropertyChanged;

        var dragHandleController = propDragHandleService[prop];

        dragHandleController.PropertyChanged += OnDragHandlePropertyChanged;

        propDropdown.SetSelectedIndexWithoutNotify(e.Index);

        UpdateControls();
    }

    private void OnPropPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var prop = (PropController)sender;

        if (e.PropertyName is nameof(PropController.ShadowCasting))
            shadowCastingToggle.SetEnabledWithoutNotify(prop.ShadowCasting);
        else if (e.PropertyName is nameof(PropController.Visible))
            visibleToggle.SetEnabledWithoutNotify(prop.Visible);
    }

    private void OnDragHandlePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var controller = (PropDragHandleController)sender;

        if (e.PropertyName is nameof(PropDragHandleController.Enabled))
            dragPointToggle.SetEnabledWithoutNotify(controller.Enabled);
        else if (e.PropertyName is nameof(PropDragHandleController.GizmoMode))
            gizmoMode.SetValueWithoutNotify((int)controller.GizmoMode);
        else if (e.PropertyName is nameof(PropDragHandleController.GizmoEnabled))
            gizmoToggle.SetEnabledWithoutNotify(controller.GizmoEnabled);
    }

    private void OnPropDropdownSelectionChange(object sender, EventArgs e)
    {
        if (propService.Count is 0)
            return;

        propSelectionController.Select(propDropdown.SelectedItem);
    }

    private void OnDragPointToggleChanged(object sender, EventArgs e)
    {
        var controller = propDragHandleService[CurrentProp];

        controller.Enabled = dragPointToggle.Value;
    }

    private void OnGizmoToggleChanged(object sender, EventArgs e)
    {
        var controller = propDragHandleService[CurrentProp];

        controller.GizmoEnabled = gizmoToggle.Value;
    }

    private void OnShadowCastingToggleChanged(object sender, EventArgs e)
    {
        if (CurrentProp is null)
            return;

        CurrentProp.ShadowCasting = shadowCastingToggle.Value;
    }

    private void OnVisibleToggleChanged(object sender, EventArgs e)
    {
        if (CurrentProp is null)
            return;

        CurrentProp.Visible = visibleToggle.Value;
    }

    private void OnCopyButtonPressed(object sender, EventArgs e)
    {
        if (CurrentProp is null)
            return;

        propService.Clone(propService.IndexOf(CurrentProp));
    }

    private void OnDeleteButtonPressed(object sender, EventArgs e)
    {
        if (CurrentProp is null)
            return;

        propService.Remove(propService.IndexOf(CurrentProp));
    }

    private void OnGizmoModeToggleChanged(object sender, EventArgs e)
    {
        if (CurrentProp is null)
            return;

        var controller = propDragHandleService[CurrentProp];

        controller.GizmoMode = (CustomGizmo.GizmoMode)gizmoMode.SelectedItemIndex;
    }

    private void OnFocusButtonPushed(object sender, EventArgs e)
    {
        if (CurrentProp is null)
            return;

        CurrentProp.Focus();
    }

    private void OnTransformControlChanged(object sender, TransformComponentChangeEventArgs e)
    {
        if (CurrentProp is null)
            return;

        transformControlChangedTransform = true;

        var control = (TransformControl)sender;

        if (control.TransformType is TransformClipboard.TransformType.Position)
        {
            CurrentProp.GameObject.transform.position = e.Value;
        }
        else if (control.TransformType is TransformClipboard.TransformType.Rotation)
        {
            CurrentProp.GameObject.transform.eulerAngles = e.Value;
        }
        else if (control.TransformType is TransformClipboard.TransformType.Scale)
        {
            if (e.Value.x < 0f || e.Value.y < 0f || e.Value.z < 0f)
                return;

            CurrentProp.GameObject.transform.localScale = e.Value;
        }
    }
}
