using MeidoPhotoStudio.Database.Props;
using MeidoPhotoStudio.Plugin.Core;
using MeidoPhotoStudio.Plugin.Core.Props;

namespace MeidoPhotoStudio.Plugin;

public class PropManagerPane : BasePane
{
    private static readonly string[] GizmoSpaceTranslationKeys = ["gizmoSpaceLocal", "gizmoSpaceWorld"];

    private readonly PropService propService;
    private readonly PropDragHandleService propDragHandleService;
    private readonly SelectionController<PropController> propSelectionController;
    private readonly TransformClipboard transformClipboard;
    private readonly Dropdown propDropdown;
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
    private readonly Toggle paneHeader;
    private readonly Toggle toggleAllDragHandles;
    private readonly Toggle toggleAllGizmos;

    private string toggleAllHandlesHeader;
    private string gizmoSpaceLabel;

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

        propDropdown = new([Translation.Get("propManagerPane", "noProps")]);
        propDropdown.SelectionChange += OnPropDropdownSelectionChange;

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

        gizmoSpaceLabel = Translation.Get("propManagerPane", "gizmoSpaceToggle");

        positionTransformControl = new(Translation.Get("propManagerPane", "positionControl"), Vector3.zero)
        {
            TransformType = TransformClipboard.TransformType.Position,
            Clipboard = this.transformClipboard,
        };

        positionTransformControl.ControlEvent += OnPositionTransformChanged;

        rotationTransformControl = new(Translation.Get("propManagerPane", "rotationControl"), Vector3.zero)
        {
            TransformType = TransformClipboard.TransformType.Rotation,
            Clipboard = this.transformClipboard,
        };

        rotationTransformControl.ControlEvent += OnRotationTransformChanged;

        scaleTransformControl = new(Translation.Get("propManagerPane", "scaleControl"), Vector3.one)
        {
            TransformType = TransformClipboard.TransformType.Scale,
            Clipboard = this.transformClipboard,
        };

        scaleTransformControl.ControlEvent += OnScaleTransformChanged;

        var copyButtonLabel = Translation.Get("transformControl", "copyButton");
        var pasteButtonLabel = Translation.Get("transformControl", "pasteButton");
        var resetButtonLabel = Translation.Get("transformControl", "resetButton");

        positionTransformControl.SetButtonLabels(copyButtonLabel, pasteButtonLabel, resetButtonLabel);
        rotationTransformControl.SetButtonLabels(copyButtonLabel, pasteButtonLabel, resetButtonLabel);
        scaleTransformControl.SetButtonLabels(copyButtonLabel, pasteButtonLabel, resetButtonLabel);

        toggleAllHandlesHeader = Translation.Get("propManagerPane", "toggleAllHandlesHeader");
        paneHeader = new(Translation.Get("propManagerPane", "header"), true);
    }

    private PropController CurrentProp =>
        propSelectionController.Current;

    public override void Draw()
    {
        paneHeader.Draw();
        MpsGui.WhiteLine();

        if (!paneHeader.Value)
            return;

        GUI.enabled = propService.Count > 0;

        GUILayout.BeginHorizontal();

        propDropdown.Draw(GUILayout.Width(185f));

        var arrowLayoutOptions = new[]
        {
            GUILayout.ExpandWidth(false),
            GUILayout.ExpandHeight(false),
        };

        if (GUILayout.Button("<", arrowLayoutOptions))
            propDropdown.Step(-1);

        if (GUILayout.Button(">", arrowLayoutOptions))
            propDropdown.Step(1);

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

        GUILayout.Label(gizmoSpaceLabel);
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

        MpsGui.Header(toggleAllHandlesHeader);
        MpsGui.BlackLine();

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
        if (propService.Count is 0)
            propDropdown.SetDropdownItem(0, Translation.Get("propManagerPane", "noProps"));

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
        gizmoSpaceLabel = Translation.Get("propManagerPane", "gizmoSpaceToggle");

        var copyButtonLabel = Translation.Get("transformControl", "copyButton");
        var pasteButtonLabel = Translation.Get("transformControl", "pasteButton");
        var resetButtonLabel = Translation.Get("transformControl", "resetButton");

        positionTransformControl.Header = Translation.Get("propManagerPane", "positionControl");
        positionTransformControl.SetButtonLabels(copyButtonLabel, pasteButtonLabel, resetButtonLabel);

        rotationTransformControl.Header = Translation.Get("propManagerPane", "rotationControl");
        rotationTransformControl.SetButtonLabels(copyButtonLabel, pasteButtonLabel, resetButtonLabel);

        scaleTransformControl.Header = Translation.Get("propManagerPane", "scaleControl");
        scaleTransformControl.SetButtonLabels(copyButtonLabel, pasteButtonLabel, resetButtonLabel);

        toggleAllHandlesHeader = Translation.Get("propManagerPane", "toggleAllHandlesHeader");
        paneHeader.Label = Translation.Get("propManagerPane", "header");
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

        dragPointToggle.SetEnabledWithoutNotify(toggleAllDragHandles.Value);
    }

    private void OnToggleAllGizmosChanged(object sender, EventArgs e)
    {
        foreach (var controller in propDragHandleService)
            controller.GizmoEnabled = toggleAllGizmos.Value;

        gizmoToggle.SetEnabledWithoutNotify(toggleAllGizmos.Value);
    }

    private void OnAddedProp(object sender, PropServiceEventArgs e)
    {
        if (propService.Count is 1)
        {
            propDropdown.SetDropdownItems([PropName(e.PropController.PropModel)], 0);

            return;
        }

        var currentNames = new HashSet<string>(propDropdown.DropdownList, StringComparer.InvariantCultureIgnoreCase);
        var propNameList = new List<string>(propDropdown.DropdownList);

        propNameList.Insert(e.PropIndex, UniquePropName(currentNames, e.PropController.PropModel));
        propDropdown.SetDropdownItems([.. propNameList], propService.Count - 1);

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
            propDropdown.SetDropdownItems([Translation.Get("propManagerPane", "noProps")], 0);

            return;
        }

        var propIndex = propDropdown.SelectedItemIndex >= propService.Count
            ? propService.Count - 1
            : propDropdown.SelectedItemIndex;

        var propNameList = new List<string>(propDropdown.DropdownList);

        propNameList.RemoveAt(e.PropIndex);
        propDropdown.SetDropdownItems([.. propNameList], propIndex);
    }

    private void OnPropTransformChanged(object sender, EventArgs e)
    {
        var prop = (PropController)sender;

        if (prop != CurrentProp)
            return;

        var transform = prop.GameObject.transform;

        positionTransformControl.SetValueWithoutNotify(transform.position);
        rotationTransformControl.SetValueWithoutNotify(transform.eulerAngles);
        scaleTransformControl.SetValueWithoutNotify(transform.localScale);
    }

    private void OnSelectingProp(object sender, SelectionEventArgs<PropController> e)
    {
        if (CurrentProp is null)
            return;

        CurrentProp.TransformChanged -= OnPropTransformChanged;
    }

    private void OnSelectedProp(object sender, SelectionEventArgs<PropController> e)
    {
        if (CurrentProp is null)
            return;

        CurrentProp.TransformChanged += OnPropTransformChanged;

        propDropdown.SetIndexWithoutNotify(e.Index);

        UpdateControls();
    }

    private void OnPropDropdownSelectionChange(object sender, EventArgs e)
    {
        if (propService.Count is 0)
            return;

        propSelectionController.Select(propDropdown.SelectedItemIndex);
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

    private void OnPositionTransformChanged(object sender, TransformComponentChangeEventArgs e)
    {
        if (CurrentProp is null)
            return;

        var (component, value) = e;
        var propTransform = CurrentProp.GameObject.transform;
        var position = propTransform.position;

        position[(int)component] = value;

        propTransform.localPosition = position;
    }

    private void OnRotationTransformChanged(object sender, TransformComponentChangeEventArgs e)
    {
        if (CurrentProp is null)
            return;

        var (component, value) = e;
        var propTransform = CurrentProp.GameObject.transform;
        var rotation = propTransform.eulerAngles;

        rotation[(int)component] = value;

        propTransform.eulerAngles = rotation;
    }

    private void OnScaleTransformChanged(object sender, TransformComponentChangeEventArgs e)
    {
        if (CurrentProp is null)
            return;

        var (component, value) = e;

        if (value < 0f)
            return;

        var propTransform = CurrentProp.GameObject.transform;
        var scale = propTransform.localScale;

        scale[(int)component] = value;

        propTransform.localScale = scale;
    }
}
