using MeidoPhotoStudio.Plugin.Core;
using MeidoPhotoStudio.Plugin.Core.Character.Pose;

namespace MeidoPhotoStudio.Plugin;

public class IKPane : BasePane
{
    private readonly IKDragHandleService ikDragHandleService;
    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly PaneHeader paneHeader;
    private readonly Toggle ikEnabledToggle;
    private readonly Toggle boneModeEnabledToggle;
    private readonly Toggle limitLimbRotationsToggle;
    private readonly Toggle limitDigitRotationsToggle;
    private readonly Toggle customFloorHeightToggle;
    private readonly NumericalTextField floorHeightTextfield;
    private readonly Button flipButton;

    private string customFloorHeightHeader;

    public IKPane(
        IKDragHandleService ikDragHandleService,
        SelectionController<CharacterController> characterSelectionController)
    {
        this.ikDragHandleService = ikDragHandleService ?? throw new ArgumentNullException(nameof(ikDragHandleService));
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));

        this.characterSelectionController.Selected += OnCharacterSelectionChanged;

        paneHeader = new(Translation.Get("maidPoseWindow", "header"), true);

        ikEnabledToggle = new(Translation.Get("maidPoseWindow", "enabledToggle"), true);
        ikEnabledToggle.ControlEvent += OnIKEnabledChanged;

        boneModeEnabledToggle = new(Translation.Get("maidPoseWindow", "boneToggle"), false);
        boneModeEnabledToggle.ControlEvent += OnBoneModeEnabledChanged;

        limitLimbRotationsToggle = new(Translation.Get("maidPoseWindow", "limitJointsToggle"));
        limitLimbRotationsToggle.ControlEvent += OnLimitLimbRotationsChanged;

        limitDigitRotationsToggle = new(Translation.Get("maidPoseWindow", "limitDigitsToggle"));
        limitDigitRotationsToggle.ControlEvent += OnLimitDigitRotationsChanged;

        customFloorHeightHeader = Translation.Get("maidPoseWindow", "customFloorHeightHeader");

        customFloorHeightToggle = new(Translation.Get("maidPoseWindow", "customFloorHeightEnabledToggle"), false);
        customFloorHeightToggle.ControlEvent += OnCustomFloorHeightToggleChanged;

        floorHeightTextfield = new(0f);
        floorHeightTextfield.ControlEvent += OnFloorHeightChanged;

        flipButton = new(Translation.Get("maidPoseWindow", "flipPoseToggle"));
        flipButton.ControlEvent += OnFlipButtonPushed;
    }

    private CharacterController CurrentCharacter =>
        characterSelectionController.Current;

    public override void Draw()
    {
        var enabled = characterSelectionController.Current is not null;

        GUI.enabled = enabled;

        paneHeader.Draw();

        if (!paneHeader.Enabled)
            return;

        DrawIK(enabled);

        MpsGui.BlackLine();

        DrawCustomFloorHeight(enabled);

        MpsGui.BlackLine();

        DrawFlip(enabled);

        void DrawIK(bool enabled)
        {
            GUILayout.BeginHorizontal();

            ikEnabledToggle.Draw();

            GUI.enabled = enabled && ikEnabledToggle.Value;

            boneModeEnabledToggle.Draw();

            GUILayout.EndHorizontal();

            MpsGui.BlackLine();

            GUILayout.BeginHorizontal();

            limitLimbRotationsToggle.Draw();

            limitDigitRotationsToggle.Draw();

            GUILayout.EndHorizontal();
        }

        void DrawCustomFloorHeight(bool enabled)
        {
            GUI.enabled = enabled;

            GUILayout.Label(customFloorHeightHeader);

            MpsGui.BlackLine();

            GUILayout.BeginHorizontal();

            var noExpandWidth = GUILayout.ExpandWidth(false);

            customFloorHeightToggle.Draw(noExpandWidth);

            GUI.enabled = enabled && customFloorHeightToggle.Value;

            if (GUILayout.Button("<", noExpandWidth))
                floorHeightTextfield.Value -= 0.01f;

            if (GUILayout.Button(">", noExpandWidth))
                floorHeightTextfield.Value += 0.01f;

            floorHeightTextfield.Draw(GUILayout.Width(70f));

            if (GUILayout.Button("|", noExpandWidth))
                floorHeightTextfield.Value = 0f;

            GUILayout.EndHorizontal();
        }

        void DrawFlip(bool enabled)
        {
            GUI.enabled = enabled;

            flipButton.Draw(GUILayout.ExpandWidth(false));
        }
    }

    protected override void ReloadTranslation()
    {
        paneHeader.Label = Translation.Get("maidPoseWindow", "header");
        ikEnabledToggle.Label = Translation.Get("maidPoseWindow", "enabledToggle");
        boneModeEnabledToggle.Label = Translation.Get("maidPoseWindow", "boneToggle");
        limitLimbRotationsToggle.Label = Translation.Get("maidPoseWindow", "limitJointsToggle");
        limitDigitRotationsToggle.Label = Translation.Get("maidPoseWindow", "limitDigitsToggle");
        customFloorHeightHeader = Translation.Get("maidPoseWindow", "customFloorHeightHeader");
        customFloorHeightToggle.Label = Translation.Get("maidPoseWindow", "customFloorHeightEnabledToggle");
        flipButton.Label = Translation.Get("maidPoseWindow", "flipPoseToggle");
    }

    private void OnCharacterSelectionChanged(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (e.Selected is null)
            return;

        var dragHandleController = ikDragHandleService[e.Selected];

        ikEnabledToggle.SetEnabledWithoutNotify(dragHandleController.IKEnabled);
        boneModeEnabledToggle.SetEnabledWithoutNotify(dragHandleController.BoneMode);
        limitLimbRotationsToggle.SetEnabledWithoutNotify(e.Selected.IK.LimitLimbRotations);
        limitDigitRotationsToggle.SetEnabledWithoutNotify(e.Selected.IK.LimitDigitRotations);
        customFloorHeightToggle.SetEnabledWithoutNotify(e.Selected.Clothing.CustomFloorHeight);
        floorHeightTextfield.SetValueWithoutNotify(e.Selected.Clothing.FloorHeight);
    }

    private void OnIKEnabledChanged(object sender, EventArgs e)
    {
        if (CurrentCharacter is null)
            return;

        var dragHandleController = ikDragHandleService[CurrentCharacter];

        dragHandleController.IKEnabled = ikEnabledToggle.Value;
    }

    private void OnBoneModeEnabledChanged(object sender, EventArgs e)
    {
        if (CurrentCharacter is null)
            return;

        var dragHandleController = ikDragHandleService[CurrentCharacter];

        dragHandleController.BoneMode = boneModeEnabledToggle.Value;
    }

    private void OnLimitLimbRotationsChanged(object sender, EventArgs e)
    {
        if (CurrentCharacter is null)
            return;

        CurrentCharacter.IK.LimitLimbRotations = limitLimbRotationsToggle.Value;
    }

    private void OnLimitDigitRotationsChanged(object sender, EventArgs e)
    {
        if (CurrentCharacter is null)
            return;

        CurrentCharacter.IK.LimitDigitRotations = limitDigitRotationsToggle.Value;
    }

    private void OnCustomFloorHeightToggleChanged(object sender, EventArgs e)
    {
        if (CurrentCharacter is null)
            return;

        CurrentCharacter.Clothing.CustomFloorHeight = customFloorHeightToggle.Value;
    }

    private void OnFloorHeightChanged(object sender, EventArgs e)
    {
        if (CurrentCharacter is null)
            return;

        CurrentCharacter.Clothing.FloorHeight = floorHeightTextfield.Value;
    }

    private void OnFlipButtonPushed(object sender, EventArgs e)
    {
        if (CurrentCharacter is null)
            return;

        CurrentCharacter.IK.Flip();
    }
}
