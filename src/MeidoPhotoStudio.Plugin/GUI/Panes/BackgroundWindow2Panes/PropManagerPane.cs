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
    private readonly Button deletePropButton;
    private readonly Button copyPropButton;
    private readonly SelectionGrid gizmoMode;

    private string propManagerHeader;
    private string gizmoSpaceLabel;

    public PropManagerPane(PropManager propManager)
    {
        this.propManager = propManager;
        this.propManager.PropListChange += (_, _) =>
        {
            UpdatePropList();
            UpdateToggles();
            UpdatePropGizmoMode();
        };

        this.propManager.FromPropSelect += (_, _) =>
        {
            updating = true;
            propDropdown.SelectedItemIndex = CurrentDoguIndex;
            updating = false;
            UpdateToggles();
            UpdatePropGizmoMode();
        };

        propDropdown = new(this.propManager.PropNameList);
        propDropdown.SelectionChange += (_, _) =>
        {
            if (updating)
                return;

            this.propManager.CurrentPropIndex = propDropdown.SelectedItemIndex;

            UpdateToggles();
            UpdatePropGizmoMode();
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

        gizmoSpaceLabel = Translation.Get("propManagerPane", "gizmoSpaceToggle");

        propManagerHeader = Translation.Get("propManagerPane", "header");
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
        gizmoToggle.Draw(noExpandWidth);
        copyPropButton.Draw(noExpandWidth);
        deletePropButton.Draw(noExpandWidth);
        GUILayout.EndHorizontal();

        var guiEnabled = GUI.enabled;

        GUI.enabled = guiEnabled && gizmoToggle.Value;

        GUILayout.BeginHorizontal();
        GUILayout.Label(gizmoSpaceLabel);
        gizmoMode.Draw();
        GUILayout.EndHorizontal();

        GUI.enabled = guiEnabled;

        GUILayout.BeginHorizontal();
        shadowCastingToggle.Draw(noExpandWidth);
        GUILayout.EndHorizontal();

        GUI.enabled = true;
    }

    protected override void ReloadTranslation()
    {
        dragPointToggle.Label = Translation.Get("propManagerPane", "dragPointToggle");
        gizmoToggle.Label = Translation.Get("propManagerPane", "gizmoToggle");
        shadowCastingToggle.Label = Translation.Get("propManagerPane", "shadowCastingToggle");
        copyPropButton.Label = Translation.Get("propManagerPane", "copyButton");
        deletePropButton.Label = Translation.Get("propManagerPane", "deleteButton");
        propManagerHeader = Translation.Get("propManagerPane", "header");
        gizmoSpaceLabel = Translation.Get("propManagerPane", "gizmoSpaceToggle");
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
}
