using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class PropManagerPane : BasePane
{
    private static readonly string[] GizmoSpaceTranslationKeys = new[] { "gizmoSpaceLocal", "gizmoSpaceWorld" };

    private readonly PropManager propManager;
    private readonly Dropdown propDropdown;
    private readonly Button previousPropButton;
    private readonly Button nextPropButton;
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

    private string propManagerHeader;
    private string gizmoSpaceLabel;
    private TransformClipboard clipboard;

    public PropManagerPane(PropManager propManager)
    {
        this.propManager = propManager;
        this.propManager.PropSelectionChange += (_, _) =>
        {
            UpdateToggles();
            UpdatePropGizmoMode();
            UpdateTransformControls();
        };

        this.propManager.PropListChange += (_, _) =>
        {
            UpdatePropList();
        };

        this.propManager.FromPropSelect += (_, _) =>
        {
            updating = true;
            propDropdown.SelectedItemIndex = CurrentDoguIndex;
            updating = false;
        };

        this.propManager.PropAdded += (_, e) =>
        {
            var prop = e.Prop;

            prop.Move += MoveEventHandler;
            prop.Rotate += RotateEventHandler;
            prop.Scale += ScaleEventHandler;

            if (this.propManager.PropCount >= 1)
                this.propManager.CurrentPropIndex = this.propManager.PropCount - 1;
        };

        this.propManager.DestroyingProp += (_, e) =>
        {
            var prop = e.Prop;

            prop.Move -= MoveEventHandler;
            prop.Rotate -= RotateEventHandler;
            prop.Scale -= ScaleEventHandler;
        };

        propDropdown = new(this.propManager.PropNameList);
        propDropdown.SelectionChange += (_, _) =>
        {
            if (updating)
                return;

            this.propManager.CurrentPropIndex = propDropdown.SelectedItemIndex;

            UpdateToggles();
            UpdatePropGizmoMode();
            UpdateTransformControls();
        };

        previousPropButton = new("<");
        previousPropButton.ControlEvent += (_, _) =>
            propDropdown.Step(-1);

        nextPropButton = new(">");
        nextPropButton.ControlEvent += (_, _) =>
            propDropdown.Step(1);

        dragPointToggle = new(Translation.Get("propManagerPane", "dragPointToggle"));
        dragPointToggle.ControlEvent += (_, _) =>
        {
            if (updating || this.propManager.PropCount is 0)
                return;

            this.propManager.CurrentProp.DragPointEnabled = dragPointToggle.Value;
        };

        gizmoToggle = new(Translation.Get("propManagerPane", "gizmoToggle"));
        gizmoToggle.ControlEvent += (_, _) =>
        {
            if (updating || this.propManager.PropCount is 0)
                return;

            this.propManager.CurrentProp.GizmoEnabled = gizmoToggle.Value;
        };

        shadowCastingToggle = new(Translation.Get("propManagerPane", "shadowCastingToggle"));
        shadowCastingToggle.ControlEvent += (_, _) =>
        {
            if (updating || this.propManager.PropCount is 0)
                return;

            this.propManager.CurrentProp.ShadowCasting = shadowCastingToggle.Value;
        };

        visibleToggle = new(Translation.Get("propManagerPane", "visibleToggle"), true);
        visibleToggle.ControlEvent += (_, _) =>
        {
            if (updating || this.propManager.PropCount is 0)
                return;

            this.propManager.CurrentProp.Visible = visibleToggle.Value;
        };

        copyPropButton = new(Translation.Get("propManagerPane", "copyButton"));
        copyPropButton.ControlEvent += (_, _) =>
            this.propManager.CopyProp(CurrentDoguIndex);

        deletePropButton = new(Translation.Get("propManagerPane", "deleteButton"));
        deletePropButton.ControlEvent += (_, _) =>
            this.propManager.RemoveProp(CurrentDoguIndex);

        gizmoMode = new(Translation.GetArray("propManagerPane", GizmoSpaceTranslationKeys));
        gizmoMode.ControlEvent += (_, _) =>
        {
            var newMode = (CustomGizmo.GizmoMode)gizmoMode.SelectedItemIndex;

            SetGizmoMode(newMode);
        };

        focusButton = new(Translation.Get("propManagerPane", "focusPropButton"));
        focusButton.ControlEvent += FocusButtonClickedEventHandler;

        gizmoSpaceLabel = Translation.Get("propManagerPane", "gizmoSpaceToggle");

        positionTransformControl = new(Translation.Get("propManagerPane", "positionControl"), Vector3.zero)
        {
            TransformType = TransformClipboard.TransformType.Position,
        };

        positionTransformControl.ControlEvent += PositionControlChangedEventHandler;

        rotationTransformControl = new(Translation.Get("propManagerPane", "rotationControl"), Vector3.zero)
        {
            TransformType = TransformClipboard.TransformType.Rotation,
        };

        rotationTransformControl.ControlEvent += RotationControlChangedEventHandler;

        scaleTransformControl = new(Translation.Get("propManagerPane", "scaleControl"), Vector3.one)
        {
            TransformType = TransformClipboard.TransformType.Scale,
        };

        scaleTransformControl.ControlEvent += ScaleControlChangedEventHandler;

        var copyButtonLabel = Translation.Get("transformControl", "copyButton");
        var pasteButtonLabel = Translation.Get("transformControl", "pasteButton");
        var resetButtonLabel = Translation.Get("transformControl", "resetButton");

        positionTransformControl.SetButtonLabels(copyButtonLabel, pasteButtonLabel, resetButtonLabel);
        rotationTransformControl.SetButtonLabels(copyButtonLabel, pasteButtonLabel, resetButtonLabel);
        scaleTransformControl.SetButtonLabels(copyButtonLabel, pasteButtonLabel, resetButtonLabel);

        // TODO: Move creation of the clipboard outside so Meido can use it too.
        Clipboard = new TransformClipboard();

        propManagerHeader = Translation.Get("propManagerPane", "header");
    }

    public TransformClipboard Clipboard
    {
        get => clipboard;
        set
        {
            clipboard = value;

            positionTransformControl.Clipboard = clipboard;
            rotationTransformControl.Clipboard = clipboard;
            scaleTransformControl.Clipboard = clipboard;
        }
    }

    private int CurrentDoguIndex =>
        propManager.CurrentPropIndex;

    public override void Draw()
    {
        const float buttonHeight = 30;

        var arrowLayoutOptions = new[]
        {
            GUILayout.Width(buttonHeight),
            GUILayout.Height(buttonHeight),
        };

        const float dropdownButtonWidth = 140f;

        var dropdownLayoutOptions = new[]
        {
            GUILayout.Height(buttonHeight),
            GUILayout.Width(dropdownButtonWidth),
        };

        MpsGui.Header(propManagerHeader);
        MpsGui.WhiteLine();

        GUI.enabled = propManager.PropCount > 0;

        GUILayout.BeginHorizontal();
        propDropdown.Draw(dropdownLayoutOptions);
        previousPropButton.Draw(arrowLayoutOptions);
        nextPropButton.Draw(arrowLayoutOptions);
        GUILayout.EndHorizontal();

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

        MpsGui.BlackLine();

        positionTransformControl.Draw();
        rotationTransformControl.Draw();
        scaleTransformControl.Draw();

        GUI.enabled = true;
    }

    public override void UpdatePane()
    {
        UpdateToggles();
        UpdatePropGizmoMode();
        UpdateTransformControls();
    }

    protected override void ReloadTranslation()
    {
        dragPointToggle.Label = Translation.Get("propManagerPane", "dragPointToggle");
        gizmoToggle.Label = Translation.Get("propManagerPane", "gizmoToggle");
        shadowCastingToggle.Label = Translation.Get("propManagerPane", "shadowCastingToggle");
        visibleToggle.Label = Translation.Get("propManagerPane", "visibleToggle");
        copyPropButton.Label = Translation.Get("propManagerPane", "copyButton");
        deletePropButton.Label = Translation.Get("propManagerPane", "deleteButton");
        propManagerHeader = Translation.Get("propManagerPane", "header");
        focusButton.Label = Translation.Get("propManagerPane", "focusPropButton");
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
    }

    private void FocusButtonClickedEventHandler(object sender, System.EventArgs e)
    {
        var prop = propManager.CurrentProp;

        if (!prop)
            return;

        prop.Focus();
    }

    private void PositionControlChangedEventHandler(object sender, TransformComponentChangeEventArgs e)
    {
        var prop = propManager.CurrentProp;

        if (!prop)
            return;

        var (component, value) = e;
        var position = prop.MyObject.localPosition;

        position[(int)component] = value;

        prop.MyObject.localPosition = position;
    }

    private void RotationControlChangedEventHandler(object sender, TransformComponentChangeEventArgs e)
    {
        var prop = propManager.CurrentProp;

        if (!prop)
            return;

        var (component, value) = e;
        var rotation = prop.MyObject.eulerAngles;

        rotation[(int)component] = value;

        prop.MyObject.eulerAngles = rotation;
    }

    private void ScaleControlChangedEventHandler(object sender, TransformComponentChangeEventArgs e)
    {
        var prop = propManager.CurrentProp;

        if (!prop)
            return;

        var (component, value) = e;

        if (value < 0f)
            return;

        var scale = prop.MyObject.localScale;

        scale[(int)component] = value;

        prop.MyObject.localScale = scale;
    }

    private void UpdatePropList()
    {
        updating = true;
        propDropdown.SetDropdownItems(propManager.PropNameList, CurrentDoguIndex);
        updating = false;
    }

    private void UpdateToggles()
    {
        var prop = propManager.CurrentProp;

        if (!prop)
            return;

        updating = true;
        dragPointToggle.Value = prop.DragPointEnabled;
        gizmoToggle.Value = prop.GizmoEnabled;
        shadowCastingToggle.Value = prop.ShadowCasting;
        visibleToggle.Value = prop.Visible;
        updating = false;
    }

    private void UpdatePropGizmoMode()
    {
        var prop = propManager.CurrentProp;

        if (!prop)
            return;

        var value = (int)prop.Gizmo.Mode;

        gizmoMode.SetValueWithoutNotify(value);
    }

    private void SetGizmoMode(CustomGizmo.GizmoMode mode)
    {
        var prop = propManager.CurrentProp;

        if (!prop)
            return;

        prop.Gizmo.Mode = mode;
    }

    private void UpdateTransformControls()
    {
        var position = Vector3.zero;
        var rotation = Vector3.zero;
        var localScale = Vector3.one;
        var initialPosition = Vector3.zero;
        var initialRotation = Vector3.zero;
        var initialLocalScale = Vector3.one;

        if (propManager.CurrentProp)
        {
            var prop = propManager.CurrentProp;
            var propTransform = propManager.CurrentProp.MyObject;

            position = propTransform.localPosition;
            rotation = propTransform.eulerAngles;
            localScale = propTransform.localScale;

            initialPosition = prop.DefaultPosition;
            initialRotation = prop.DefaultRotation.eulerAngles;
            initialLocalScale = prop.DefaultScale;
        }

        positionTransformControl.SetValueWithoutNotify(position);
        positionTransformControl.DefaultValue = initialPosition;

        rotationTransformControl.SetValueWithoutNotify(rotation);
        rotationTransformControl.DefaultValue = initialRotation;

        scaleTransformControl.SetValueWithoutNotify(localScale);
        scaleTransformControl.DefaultValue = initialLocalScale;
    }

    private void MoveEventHandler(object sender, System.EventArgs e)
    {
        var prop = (DragPointProp)sender;

        if (prop != propManager.CurrentProp)
            return;

        var position = prop.MyObject.localPosition;

        positionTransformControl.SetValueWithoutNotify(position);
    }

    private void RotateEventHandler(object sender, System.EventArgs e)
    {
        var prop = (DragPointProp)sender;

        if (prop != propManager.CurrentProp)
            return;

        var rotation = prop.MyObject.eulerAngles;

        rotationTransformControl.SetValueWithoutNotify(rotation);
    }

    private void ScaleEventHandler(object sender, System.EventArgs e)
    {
        var prop = (DragPointProp)sender;

        if (prop != propManager.CurrentProp)
            return;

        var localScale = prop.MyObject.localScale;

        scaleTransformControl.SetValueWithoutNotify(localScale);
    }
}
