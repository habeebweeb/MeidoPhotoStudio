using System.ComponentModel;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class IKDragHandleController : IEnumerable<ICharacterDragHandleController>, INotifyPropertyChanged
{
    private readonly (float Small, float Normal) handleSize = (0.5f, 1f);
    private readonly (float Small, float Normal) gizmoSize = (0.225f, 0.45f);
    private readonly Dictionary<HandleType, ICharacterDragHandleController> controllers = [];

    private bool smallHandle;
    private bool ikEnabled = true;
    private bool boneModeEnabled;
    private bool autoSelect;

    internal IKDragHandleController()
    {
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public enum HandleType
    {
        Cube,
        Body,

        Head,

        EyeL,
        EyeR,

        UpperArmL,
        UpperArmR,

        ForearmL,
        ForearmR,

        HandL,
        HandR,

        Finger0L,
        Finger02L,
        Finger0NubL,
        Finger1L,
        Finger12L,
        Finger1NubL,
        Finger2L,
        Finger22L,
        Finger2NubL,
        Finger3L,
        Finger32L,
        Finger3NubL,
        Finger4L,
        Finger42L,
        Finger4NubL,
        Finger0R,
        Finger02R,
        Finger0NubR,
        Finger1R,
        Finger12R,
        Finger1NubR,
        Finger2R,
        Finger22R,
        Finger2NubR,
        Finger3R,
        Finger32R,
        Finger3NubR,
        Finger4R,
        Finger42R,
        Finger4NubR,

        ChestL,
        ChestR,

        ChestSubL,
        ChestSubR,

        // all spine bones
        Torso,

        // Spine
        HeadBase,
        Neck,
        Spine,
        Spine0a,
        Spine1,
        Spine1a,

        Hip,

        ThighL,
        ThighR,

        CalfL,
        CalfR,

        FootL,
        FootR,

        Toe0L,
        Toe0NubL,
        Toe1L,
        Toe1NubL,
        Toe2L,
        Toe2NubL,
        Toe0R,
        Toe0NubR,
        Toe1R,
        Toe1NubR,
        Toe2R,
        Toe2NubR,

        Root,
    }

    public bool SmallHandle
    {
        get => smallHandle;
        set
        {
            if (value == smallHandle)
                return;

            smallHandle = value;

            Cube.HandleSize = smallHandle ? handleSize.Small : handleSize.Normal;
            Cube.GizmoSize = smallHandle ? gizmoSize.Small : gizmoSize.Normal;

            RaisePropertyChanged(nameof(SmallHandle));
        }
    }

    public bool CubeEnabled
    {
        get => Cube.DragHandleEnabled;
        set
        {
            if (value == Cube.DragHandleEnabled)
                return;

            Cube.DragHandleEnabled = value;
            Cube.GizmoEnabled = value;

            RaisePropertyChanged(nameof(CubeEnabled));
        }
    }

    public bool IKEnabled
    {
        get => ikEnabled;
        set
        {
            if (value == ikEnabled)
                return;

            ikEnabled = value;

            foreach (var controller in this.Except(new[] { Cube }))
            {
                controller.IKEnabled = ikEnabled;
                controller.DragHandleEnabled = ikEnabled;
                controller.GizmoEnabled = ikEnabled;
            }

            Cube.IKEnabled = ikEnabled;

            RaisePropertyChanged(nameof(IKEnabled));
        }
    }

    public bool BoneMode
    {
        get => boneModeEnabled;
        set
        {
            if (value == boneModeEnabled)
                return;

            boneModeEnabled = value;

            foreach (var controller in this)
                controller.BoneMode = boneModeEnabled;

            RaisePropertyChanged(nameof(BoneMode));
        }
    }

    public bool AutoSelect
    {
        get => autoSelect;
        set
        {
            if (value == autoSelect)
                return;

            autoSelect = value;

            foreach (var controller in this)
                controller.AutoSelect = autoSelect;
        }
    }

    private CharacterGeneralDragHandleController Cube { get; set; }

    public ICharacterDragHandleController this[HandleType type]
    {
        get => controllers[type];
        internal set
        {
            controllers[type] = value;

            if (type is HandleType.Cube && value is CharacterGeneralDragHandleController { IsCube: true } cube)
                Cube = cube;
        }
    }

    public IEnumerator<ICharacterDragHandleController> GetEnumerator() =>
        controllers.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    private void RaisePropertyChanged(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

        PropertyChanged?.Invoke(this, new(name));
    }
}
