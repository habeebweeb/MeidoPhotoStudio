namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class IKDragHandleController : IEnumerable<ICharacterDragHandleController>
{
    private readonly (float Small, float Normal) handleSize = (0.5f, 1f);
    private readonly (float Small, float Normal) gizmoSize = (0.225f, 0.45f);

    private bool smallHandle;
    private bool ikEnabled = true;
    private bool boneModeEnabled;

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
        }
    }

    public bool CubeEnabled
    {
        get => Cube.Enabled;
        set => Cube.Enabled = value;
    }

    public bool IKEnabled
    {
        get => ikEnabled;
        set
        {
            ikEnabled = value;

            foreach (var controller in this.Except(new[] { Cube }))
                controller.IKEnabled = ikEnabled;
        }
    }

    public bool BoneMode
    {
        get => boneModeEnabled;
        set
        {
            boneModeEnabled = value;

            foreach (var controller in this)
                controller.BoneMode = boneModeEnabled;
        }
    }

    private CharacterGeneralDragHandleController Cube { get; init; }

    private List<ICharacterDragHandleController> Controllers { get; init; }

    public IEnumerator<ICharacterDragHandleController> GetEnumerator() =>
        Controllers.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public class Builder
    {
        public CharacterGeneralDragHandleController Cube { get; init; }

        public CharacterGeneralDragHandleController Body { get; init; }

        public UpperLimbDragHandleController UpperArmLeft { get; init; }

        public UpperLimbDragHandleController UpperArmRight { get; init; }

        public MiddleLimbDragHandleController ForearmLeft { get; init; }

        public MiddleLimbDragHandleController ForearmRight { get; init; }

        public MiddleLimbDragHandleController CalfLeft { get; init; }

        public MiddleLimbDragHandleController CalfRight { get; init; }

        public LowerLimbDragHandleController HandLeft { get; init; }

        public LowerLimbDragHandleController HandRight { get; init; }

        public LowerLimbDragHandleController FootLeft { get; init; }

        public LowerLimbDragHandleController FootRight { get; init; }

        public TorsoDragHandleController Torso { get; init; }

        public HeadDragHandleController Head { get; init; }

        public PelvisDragHandleController Pelvis { get; init; }

        public IEnumerable<SpineDragHandleController> Spine { get; init; }

        public HipDragHandleController Hip { get; init; }

        public ThighGizmoController ThighLeft { get; init; }

        public ThighGizmoController ThighRight { get; init; }

        public ChestDragHandleController ChestLeft { get; init; }

        public ChestDragHandleController ChestRight { get; init; }

        public ChestSubGizmoController ChestSubLeft { get; init; }

        public ChestSubGizmoController ChestSubRight { get; init; }

        public IEnumerable<DigitBaseDragHandleController> DigitBases { get; init; }

        public IEnumerable<DigitDragHandleController> Digits { get; init; }

        public EyeDragHandleController LeftEye { get; init; }

        public EyeDragHandleController RightEye { get; init; }

        public IKDragHandleController Build() =>
            new()
            {
                Cube = Cube,
                Controllers = [
                    Cube,
                    Body,
                    UpperArmLeft,
                    UpperArmRight,
                    ForearmLeft,
                    ForearmRight,
                    CalfLeft,
                    CalfRight,
                    HandLeft,
                    HandRight,
                    FootLeft,
                    FootRight,
                    Torso,
                    Head,
                    Pelvis,
                    ..Spine,
                    Hip,
                    ThighLeft,
                    ThighRight,
                    ChestLeft,
                    ChestRight,
                    ChestSubLeft,
                    ChestSubRight,
                    ..DigitBases,
                    ..Digits,
                    LeftEye,
                    RightEye,
                ],
            };
    }
}