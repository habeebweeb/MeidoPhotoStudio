using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Character.Pose;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class IKLockingPane : BasePane
{
    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly IKLockControlSet leftHandSet;
    private readonly IKLockControlSet rightHandSet;
    private readonly IKLockControlSet leftFootSet;
    private readonly IKLockControlSet rightFootSet;

    public IKLockingPane(
        Translation translation,
        SelectionController<CharacterController> characterSelectionController)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));
        this.characterSelectionController.Selected += OnCharacterSelected;

        var positionContent = new LocalizableGUIContent(translation, "ikLockPane", "positionLabel");
        var rotationContent = new LocalizableGUIContent(translation, "ikLockPane", "rotationLabel");
        var lockLabelContent = new LocalizableGUIContent(translation, "ikLockPane", "lockLabel");

        leftHandSet = new(
            new LocalizableGUIContent(translation, "ikLockPane", "leftHandHeader"),
            lockLabelContent,
            positionContent,
            rotationContent);

        rightHandSet = new(
            new LocalizableGUIContent(translation, "ikLockPane", "rightHandHeader"),
            lockLabelContent,
            positionContent,
            rotationContent);

        leftFootSet = new(
            new LocalizableGUIContent(translation, "ikLockPane", "leftFootHeader"),
            lockLabelContent,
            positionContent,
            rotationContent);

        rightFootSet = new(
            new LocalizableGUIContent(translation, "ikLockPane", "rightFootHeader"),
            lockLabelContent,
            positionContent,
            rotationContent);
    }

    private CharacterController CurrentCharacter =>
        characterSelectionController.Current;

    public override void Draw()
    {
        var enabled = Parent.Enabled && characterSelectionController.Current is not null;

        GUI.enabled = enabled;

        leftHandSet.Draw();
        rightHandSet.Draw();
        leftFootSet.Draw();
        rightFootSet.Draw();
    }

    private void OnCharacterSelected(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (CurrentCharacter?.IK is not IKController ik)
            return;

        leftHandSet.SetController(ik.LeftHandLock);
        rightHandSet.SetController(ik.RightHandLock);
        leftFootSet.SetController(ik.LeftFootLock);
        rightFootSet.SetController(ik.RightFootLock);
    }

    private class IKLockControlSet
    {
        private readonly Header header;
        private readonly Label lockLabel;
        private readonly Toggle positionLockToggle;
        private readonly Toggle rotationLockToggle;

        private IKLockController controller;

        public IKLockControlSet(
            GUIContent headerContent,
            GUIContent lockLabelContent,
            GUIContent positionContent,
            GUIContent rotationContent)
        {
            header = new(headerContent);
            lockLabel = new(lockLabelContent);
            positionLockToggle = new(positionContent);
            rotationLockToggle = new(rotationContent);

            positionLockToggle.ControlEvent += OnPositionLockToggleChanged;
            rotationLockToggle.ControlEvent += OnRotationLockToggleChanged;
        }

        public void Draw()
        {
            if (controller is null)
                return;

            header.Draw();

            GUILayout.BeginHorizontal();

            lockLabel.Draw(GUILayout.ExpandWidth(false));

            positionLockToggle.Draw();
            rotationLockToggle.Draw();

            GUILayout.EndHorizontal();
        }

        public void SetController(IKLockController newController)
        {
            if (newController == controller)
                return;

            if (controller is not null)
                controller.PropertyChanged -= OnIKLockPropertyChanged;

            controller = newController;

            if (controller is null)
                return;

            controller.PropertyChanged += OnIKLockPropertyChanged;

            positionLockToggle.SetEnabledWithoutNotify(controller.LockPosition);
            rotationLockToggle.SetEnabledWithoutNotify(controller.LockRotation);
        }

        private void OnPositionLockToggleChanged(object sender, EventArgs e)
        {
            if (controller is null)
                return;

            controller.LockPosition = positionLockToggle.Value;
        }

        private void OnRotationLockToggleChanged(object sender, EventArgs e)
        {
            if (controller is null)
                return;

            controller.LockRotation = rotationLockToggle.Value;
        }

        private void OnIKLockPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(IKLockController.LockPosition))
                positionLockToggle.SetEnabledWithoutNotify(controller.LockPosition);
            else if (e.PropertyName is nameof(IKLockController.LockRotation))
                rotationLockToggle.SetEnabledWithoutNotify(controller.LockRotation);
        }
    }
}
