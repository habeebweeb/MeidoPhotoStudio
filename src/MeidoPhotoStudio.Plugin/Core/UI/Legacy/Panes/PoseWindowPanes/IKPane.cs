using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Character.Pose;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework.Service;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class IKPane : BasePane
{
    private readonly IKDragHandleService ikDragHandleService;
    private readonly CharacterUndoRedoService characterUndoRedoService;
    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly Toggle ikEnabledToggle;
    private readonly Toggle boneModeEnabledToggle;
    private readonly Toggle limitLimbRotationsToggle;
    private readonly Toggle limitDigitRotationsToggle;
    private readonly Button flipButton;
    private readonly SubPaneHeader transformInputToggle;
    private readonly TransformInputPane transformInputPane;
    private readonly List<BoneBackup> boneBackup = [];

    public IKPane(
        Translation translation,
        IKDragHandleService ikDragHandleService,
        CharacterUndoRedoService characterUndoRedoService,
        SelectionController<CharacterController> characterSelectionController,
        TransformClipboard transformClipboard)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));
        this.ikDragHandleService = ikDragHandleService ?? throw new ArgumentNullException(nameof(ikDragHandleService));
        this.characterUndoRedoService = characterUndoRedoService ?? throw new ArgumentNullException(nameof(characterUndoRedoService));
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));
        _ = transformClipboard ?? throw new ArgumentNullException(nameof(transformClipboard));

        this.characterSelectionController.Selecting += OnCharacterSelectionChanging;
        this.characterSelectionController.Selected += OnCharacterSelectionChanged;

        ikEnabledToggle = new(new LocalizableGUIContent(translation, "maidPoseWindow", "enabledToggle"), true);
        ikEnabledToggle.ControlEvent += OnIKEnabledChanged;

        boneModeEnabledToggle = new(new LocalizableGUIContent(translation, "maidPoseWindow", "boneToggle"), false);
        boneModeEnabledToggle.ControlEvent += OnBoneModeEnabledChanged;

        limitLimbRotationsToggle = new(new LocalizableGUIContent(translation, "maidPoseWindow", "limitJointsToggle"));
        limitLimbRotationsToggle.ControlEvent += OnLimitLimbRotationsChanged;

        limitDigitRotationsToggle = new(new LocalizableGUIContent(translation, "maidPoseWindow", "limitDigitsToggle"));
        limitDigitRotationsToggle.ControlEvent += OnLimitDigitRotationsChanged;

        flipButton = new(new LocalizableGUIContent(translation, "maidPoseWindow", "flipPoseToggle"));
        flipButton.ControlEvent += OnFlipButtonPushed;

        transformInputToggle = new(new LocalizableGUIContent(translation, "maidPoseWindow", "preciseTransformToggle"), false);

        transformInputPane = new(translation, transformClipboard)
        {
            LinkScale = true,
        };

        transformInputPane.Transforming += OnTransforming;
        transformInputPane.CancelledTransformation += OnCancelledTransformation;

        Add(transformInputPane);
    }

    private CharacterUndoRedoController CharacterUndoRedo =>
        CurrentCharacter is null ? null : characterUndoRedoService[CurrentCharacter];

    private CharacterController CurrentCharacter =>
        characterSelectionController.Current;

    public override void Draw()
    {
        var enabled = Parent.Enabled && characterSelectionController.Current is not null;

        GUI.enabled = enabled;

        DrawIK(enabled);

        UIUtility.DrawBlackLine();

        DrawFlip(enabled);

        UIUtility.DrawBlackLine();

        transformInputToggle.Draw();

        if (transformInputToggle.Enabled)
            transformInputPane.Draw();

        void DrawIK(bool enabled)
        {
            GUILayout.BeginHorizontal();

            ikEnabledToggle.Draw();

            GUI.enabled = enabled && ikEnabledToggle.Value;

            boneModeEnabledToggle.Draw();

            GUILayout.EndHorizontal();

            UIUtility.DrawBlackLine();

            GUILayout.BeginHorizontal();

            limitLimbRotationsToggle.Draw();

            limitDigitRotationsToggle.Draw();

            GUILayout.EndHorizontal();
        }

        void DrawFlip(bool enabled)
        {
            GUI.enabled = enabled;

            flipButton.Draw(GUILayout.ExpandWidth(false));
        }
    }

    private void OnCharacterSelectionChanging(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (e.Selected is null)
            return;

        var dragHandleController = ikDragHandleService[e.Selected];
        var ik = e.Selected.IK;
        var clothing = e.Selected.Clothing;

        dragHandleController.PropertyChanged -= OnIKDragHandleControllerPropertyChanged;
        ik.PropertyChanged -= OnIKControllerPropertyChanged;
    }

    private void OnCharacterSelectionChanged(object sender, SelectionEventArgs<CharacterController> e)
    {
        transformInputPane.Target = e.Selected;

        if (e.Selected is null)
            return;

        var dragHandleController = ikDragHandleService[e.Selected];
        var ik = e.Selected.IK;
        var clothing = e.Selected.Clothing;

        dragHandleController.PropertyChanged += OnIKDragHandleControllerPropertyChanged;
        ik.PropertyChanged += OnIKControllerPropertyChanged;

        ikEnabledToggle.SetEnabledWithoutNotify(dragHandleController.IKEnabled);
        boneModeEnabledToggle.SetEnabledWithoutNotify(dragHandleController.BoneMode);
        limitLimbRotationsToggle.SetEnabledWithoutNotify(ik.LimitLimbRotations);
        limitDigitRotationsToggle.SetEnabledWithoutNotify(ik.LimitDigitRotations);
    }

    private void OnTransforming(object sender, EventArgs e)
    {
        if (CurrentCharacter is not CharacterController character)
            return;

        boneBackup.Clear();

        if (character.IK.LeftHandLock is { LockPosition: true } leftHand)
            boneBackup.AddRange(leftHand.Chain.Select(BoneBackup.Create));

        if (character.IK.RightHandLock is { LockPosition: true } rightHand)
            boneBackup.AddRange(rightHand.Chain.Select(BoneBackup.Create));

        if (character.IK.LeftFootLock is { LockPosition: true } leftFoot)
            boneBackup.AddRange(leftFoot.Chain.Select(BoneBackup.Create));

        if (character.IK.RightFootLock is { LockPosition: true } rightFoot)
            boneBackup.AddRange(rightFoot.Chain.Select(BoneBackup.Create));
    }

    private void OnCancelledTransformation(object sender, EventArgs e)
    {
        if (CurrentCharacter is null)
            return;

        foreach (var backup in boneBackup)
            backup.Apply();
    }

    private void OnIKDragHandleControllerPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var dragHandleController = (IKDragHandleController)sender;

        if (e.PropertyName is nameof(IKDragHandleController.IKEnabled))
            ikEnabledToggle.SetEnabledWithoutNotify(dragHandleController.IKEnabled);
        else if (e.PropertyName is nameof(IKDragHandleController.BoneMode))
            boneModeEnabledToggle.SetEnabledWithoutNotify(dragHandleController.BoneMode);
    }

    private void OnIKControllerPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var ikController = (IKController)sender;

        if (e.PropertyName is nameof(IKController.LimitLimbRotations))
            limitLimbRotationsToggle.SetEnabledWithoutNotify(ikController.LimitLimbRotations);
        else if (e.PropertyName is nameof(IKController.LimitDigitRotations))
            limitDigitRotationsToggle.SetEnabledWithoutNotify(ikController.LimitDigitRotations);
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
        if (CurrentCharacter is not CharacterController character)
            return;

        CharacterUndoRedo.StartPoseChange();

        if (character.IK.SetLimbRotationLimitsEnabled(limitLimbRotationsToggle.Value))
            CharacterUndoRedo.EndPoseChange();
        else
            CharacterUndoRedo.CancelPoseChange();
    }

    private void OnLimitDigitRotationsChanged(object sender, EventArgs e)
    {
        if (CurrentCharacter is not CharacterController character)
            return;

        CharacterUndoRedo.StartPoseChange();

        if (character.IK.SetDigitRotationLimitsEnabled(limitDigitRotationsToggle.Value))
            CharacterUndoRedo.EndPoseChange();
        else
            CharacterUndoRedo.CancelPoseChange();
    }

    private void OnFlipButtonPushed(object sender, EventArgs e)
    {
        if (CurrentCharacter is null)
            return;

        characterUndoRedoService[CurrentCharacter].StartPoseChange();
        CurrentCharacter.IK.Flip();
        characterUndoRedoService[CurrentCharacter].EndPoseChange();
    }
}
