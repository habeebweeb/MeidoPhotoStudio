using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class IKDragHandleService : INotifyPropertyChanged
{
    private readonly CharacterDragHandleInputService characterDragHandleInputService;
    private readonly CharacterService characterService;
    private readonly CharacterUndoRedoService characterUndoRedoService;
    private readonly SelectionController<CharacterController> selectionController;
    private readonly TabSelectionController tabSelectionController;
    private readonly Dictionary<CharacterController, IKDragHandleController> controllers = [];

    private bool cubeEnabled;
    private bool smallHandle;

    public IKDragHandleService(
        CharacterDragHandleInputService characterDragHandleInputService,
        CharacterService characterService,
        CharacterUndoRedoService characterUndoRedoService,
        SelectionController<CharacterController> selectionController,
        TabSelectionController tabSelectionController)
    {
        this.characterDragHandleInputService = characterDragHandleInputService ?? throw new ArgumentNullException(nameof(characterDragHandleInputService));
        this.characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));
        this.characterUndoRedoService = characterUndoRedoService ?? throw new ArgumentNullException(nameof(characterUndoRedoService));
        this.selectionController = selectionController ?? throw new ArgumentNullException(nameof(selectionController));
        this.tabSelectionController = tabSelectionController ?? throw new ArgumentNullException(nameof(tabSelectionController));

        this.characterService.CalledCharacters += OnCharactersCalled;
        this.characterService.Deactivating += OnDeactivating;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public bool CubeEnabled
    {
        get => cubeEnabled;
        set
        {
            if (cubeEnabled == value)
                return;

            cubeEnabled = value;

            foreach (var controller in controllers.Values)
                controller.CubeEnabled = cubeEnabled;

            RaisePropertyChanged(nameof(CubeEnabled));
        }
    }

    public bool SmallHandle
    {
        get => smallHandle;
        set
        {
            if (value == smallHandle)
                return;

            smallHandle = value;

            foreach (var controller in controllers.Values)
                controller.SmallHandle = smallHandle;

            RaisePropertyChanged(nameof(SmallHandle));
        }
    }

    public IKDragHandleController this[CharacterController characterController] =>
        characterController is null
            ? throw new ArgumentNullException(nameof(characterController))
            : controllers[characterController];

    private void OnDeactivating(object sender, EventArgs e)
    {
        foreach (var controller in controllers.Values)
            DestroyController(controller);

        controllers.Clear();
    }

    private void OnCharactersCalled(object sender, CharacterServiceEventArgs e)
    {
        var oldCharacters = controllers.Keys.ToArray();

        foreach (var character in oldCharacters.Except(e.LoadedCharacters))
        {
            DestroyController(controllers[character]);
            character.ProcessingCharacterProps -= OnCharacterProcessing;
            controllers.Remove(character);
        }

        foreach (var character in e.LoadedCharacters.Except(oldCharacters))
        {
            controllers[character] = InitializeDragHandles(character);
            character.ProcessingCharacterProps += OnCharacterProcessing;
        }
    }

    private void OnCharacterProcessing(object sender, CharacterProcessingEventArgs e)
    {
        var character = sender as CharacterController;

        if (!controllers.ContainsKey(character))
        {
            character.ProcessingCharacterProps -= OnCharacterProcessing;

            return;
        }

        var bodyMpn = (MPN)Enum.Parse(typeof(MPN), nameof(MPN.body));

        if (!e.ChangingSlots.Contains(bodyMpn))
            return;

        DestroyController(controllers[character]);

        character.ProcessedCharacterProps += OnCharacterProcessed;

        void OnCharacterProcessed(object sender, CharacterProcessingEventArgs e)
        {
            controllers[character] = InitializeDragHandles(character);

            character.ProcessedCharacterProps -= OnCharacterProcessed;
        }
    }

    private IKDragHandleController InitializeDragHandles(CharacterController character)
    {
        var undoRedoController = characterUndoRedoService[character];

        var ikDragHandleController = new IKDragHandleController.Builder()
        {
            Cube = MakeCube(character, selectionController, tabSelectionController, CubeEnabled),
            Body = MakeBody(character, selectionController, tabSelectionController),
            UpperArmLeft = MakeUpperLimb(character, undoRedoController, "Bip01 L UpperArm"),
            UpperArmRight = MakeUpperLimb(character, undoRedoController, "Bip01 R UpperArm"),
            ForearmLeft = MakeMiddleLimb(character, undoRedoController, "Bip01 L Forearm"),
            ForearmRight = MakeMiddleLimb(character, undoRedoController, "Bip01 R Forearm"),
            CalfLeft = MakeMiddleLimb(character, undoRedoController, "Bip01 L Calf"),
            CalfRight = MakeMiddleLimb(character, undoRedoController, "Bip01 R Calf"),
            HandLeft = MakeLowerLimb(character, undoRedoController, "Bip01 L Hand"),
            HandRight = MakeLowerLimb(character, undoRedoController, "Bip01 R Hand"),
            FootLeft = MakeLowerLimb(character, undoRedoController, "Bip01 L Foot"),
            FootRight = MakeLowerLimb(character, undoRedoController, "Bip01 R Foot"),
            Torso = MakeTorso(character, undoRedoController),
            Head = MakeHead(character, undoRedoController, selectionController, tabSelectionController),
            Pelvis = MakePelvis(character, undoRedoController),
            Spine = MakeSpine(
                character,
                undoRedoController,
                "Bip01 Head",
                "Bip01 Neck",
                "Bip01 Spine",
                "Bip01 Spine0a",
                "Bip01 Spine1",
                "Bip01 Spine1a"),
            Hip = MakeHip(character, undoRedoController),
            ThighLeft = MakeThigh(character, undoRedoController, "Bip01 L Thigh"),
            ThighRight = MakeThigh(character, undoRedoController, "Bip01 R Thigh"),
            ChestLeft = MakeChest(character, undoRedoController, "Mune_L"),
            ChestRight = MakeChest(character, undoRedoController, "Mune_R"),
            ChestSubLeft = MakeChestSub(character, undoRedoController, "Mune_L_sub"),
            ChestSubRight = MakeChestSub(character, undoRedoController, "Mune_R_sub"),
            DigitBases = MakeDigitBases(character, undoRedoController),
            Digits = MakeNoLimitDigits(character, undoRedoController),
            LeftEye = MakeEye(character, undoRedoController, left: true),
            RightEye = MakeEye(character, undoRedoController, left: false),
        }.Build();

        foreach (var dragHandleController in ikDragHandleController)
            characterDragHandleInputService.AddController(dragHandleController);

        return ikDragHandleController;

        static (DragHandle DragHandle, CustomGizmo Gizmo, Transform IKTarget) BuildIKDragHandleAndGizmo(
            CharacterController character, Transform bone)
        {
            var positionNode = character.IK.GetMeshNode(bone.name);

            if (!positionNode)
                positionNode = bone;

            var ikTarget = character.IK.CreateIKSolverTarget();

            var dragHandle = new DragHandle.Builder()
            {
                Name = DragHandleName(character, bone),
                Shape = PrimitiveType.Sphere,
                Target = ikTarget,
                Scale = Vector3.one * 0.1f,
                PositionDelegate = () => positionNode.position,
            }.Build();

            var gizmo = new CustomGizmo.Builder()
            {
                Name = GizmoName(character, bone),
                Size = 0.25f,
                Target = bone,
                Mode = CustomGizmo.GizmoMode.Local,
                PositionTarget = positionNode,
            }.Build();

            return (dragHandle, gizmo, ikTarget);
        }

        static CharacterGeneralDragHandleController MakeCube(
            CharacterController character,
            SelectionController<CharacterController> selectionController,
            TabSelectionController tabSelectionController,
            bool cubeEnabled)
        {
            var characterTransform = character.GameObject.transform;

            var dragHandle = new DragHandle.Builder()
            {
                Name = $"[Cube Body Drag Handle ({character})]",
                Target = characterTransform,
                ConstantSize = true,
                Scale = Vector3.one * 0.12f,
                PositionDelegate = () => characterTransform.position,
            }.Build();

            return new(dragHandle, characterTransform, character, selectionController, tabSelectionController)
            {
                Enabled = cubeEnabled,
                IsCube = true,
            };
        }

        static CharacterGeneralDragHandleController MakeBody(
            CharacterController character,
            SelectionController<CharacterController> selectionController,
            TabSelectionController tabSelectionController)
        {
            var characterTransform = character.GameObject.transform;

            var dragHandle = new DragHandle.Builder()
            {
                Name = $"[Body Drag Handle ({character})]",
                Target = characterTransform,
                Shape = PrimitiveType.Capsule,
                ConstantSize = false,
                Visible = false,
                Scale = new(0.2f, 0.3f, 0.2f),
                PositionDelegate = PositionBetweenTransforms(
                    character.IK.GetBone("Bip01 Spine1"), character.IK.GetBone("Bip01 Spine0a")),
                RotationDelegate = AxisRotation(character.IK.GetBone("Bip01 Spine0a"), 90f, Vector3.forward),
            }.Build();

            return new(dragHandle, characterTransform, character, selectionController, tabSelectionController)
            {
                ScalesWithCharacter = true,
                IsCube = false,
            };
        }

        static UpperLimbDragHandleController MakeUpperLimb(
            CharacterController character, CharacterUndoRedoController undoRedoController, string boneName)
        {
            var bone = character.IK.GetBone(boneName);

            var (dragHandle, gizmo, ikTarget) = BuildIKDragHandleAndGizmo(character, bone);

            return new(dragHandle, gizmo, character, undoRedoController, bone, ikTarget);
        }

        static MiddleLimbDragHandleController MakeMiddleLimb(
            CharacterController character, CharacterUndoRedoController undoRedoController, string boneName)
        {
            var bone = character.IK.GetBone(boneName);

            var (dragHandle, gizmo, ikTarget) = BuildIKDragHandleAndGizmo(character, bone);

            gizmo.VisibleRotateX = false;
            gizmo.VisibleRotateY = false;
            gizmo.VisibleRotateZ = true;

            return new(dragHandle, gizmo, character, undoRedoController, bone, ikTarget);
        }

        static LowerLimbDragHandleController MakeLowerLimb(
            CharacterController character, CharacterUndoRedoController undoRedoController, string boneName)
        {
            var bone = character.IK.GetBone(boneName);

            var (dragHandle, gizmo, ikTarget) = BuildIKDragHandleAndGizmo(character, bone);

            return new(dragHandle, gizmo, character, undoRedoController, bone, ikTarget);
        }

        static TorsoDragHandleController MakeTorso(
            CharacterController character, CharacterUndoRedoController undoRedoController)
        {
            var spine1a = character.IK.GetBone("Bip01 Spine1a");
            var spine1 = spine1a.parent;
            var spine0a = spine1.parent;
            var spine = spine0a.parent;

            var dragHandle = new DragHandle.Builder()
            {
                Name = $"[Torso Drag Handle ({character})]",
                Shape = PrimitiveType.Capsule,
                Scale = new(0.2f, 0.2f, 0.2f),
                PositionDelegate = PositionBetweenTransforms(spine1, spine1a),
                RotationDelegate = AxisRotation(spine1, 90f, Vector3.forward),
            }.Build();

            return new(dragHandle, character, undoRedoController, spine1a, spine1, spine0a, spine);
        }

        static HeadDragHandleController MakeHead(
            CharacterController character,
            CharacterUndoRedoController undoRedoController,
            SelectionController<CharacterController> selectionController,
            TabSelectionController tabSelectionController)
        {
            var head = character.IK.GetBone("Bip01 Head");
            var neck = character.IK.GetBone("Bip01 Neck");
            var headNub = character.IK.GetBone("Bip01 HeadNub");

            var dragHandle = new DragHandle.Builder()
            {
                Name = DragHandleName(character, head),
                Shape = PrimitiveType.Sphere,
                Scale = new(0.2f, 0.24f, 0.2f),
                PositionDelegate = PositionBetweenTransforms(head, headNub),
                RotationDelegate = AxisRotation(head, 90f, Vector3.forward),
            }.Build();

            return new(dragHandle, character, undoRedoController, neck, selectionController, tabSelectionController);
        }

        static PelvisDragHandleController MakePelvis(
            CharacterController character, CharacterUndoRedoController undoRedoController)
        {
            var spine = character.IK.GetBone("Bip01 Spine");
            var pelvis = character.IK.GetBone("Bip01 Pelvis");

            var dragHandle = new DragHandle.Builder()
            {
                Name = DragHandleName(character, pelvis),
                Shape = PrimitiveType.Capsule,
                Scale = new(0.2f, 0.15f, 0.2f),
                PositionDelegate = PositionBetweenTransforms(spine, pelvis),
                RotationDelegate = AxisRotation(pelvis, 90f, new(0f, 1f, 1f)),
            }.Build();

            var gizmo = new CustomGizmo.Builder()
            {
                Name = GizmoName(character, pelvis),
                Target = pelvis,
                Size = 0.25f,
                Mode = CustomGizmo.GizmoMode.Local,
            }.Build();

            return new(dragHandle, gizmo, character, undoRedoController, pelvis);
        }

        static SpineDragHandleController[] MakeSpine(
            CharacterController character, CharacterUndoRedoController undoRedoController, params string[] boneNames)
        {
            var dragHandleBuilder = new DragHandle.Builder()
            {
                Shape = PrimitiveType.Sphere,
                Scale = Vector3.one * 0.04f,
            };

            var gizmoBuilder = new CustomGizmo.Builder()
            {
                Size = 0.25f,
                Mode = CustomGizmo.GizmoMode.Local,
            };

            var controllers = new SpineDragHandleController[boneNames.Length];

            foreach (var (index, boneName) in boneNames.WithIndex())
            {
                var bone = character.IK.GetBone(boneName);

                var dragHandle = dragHandleBuilder
                    .WithTarget(bone)
                    .WithName(DragHandleName(character, bone))
                    .WithPositionDelegate(() => bone.position)
                    .Build();

                var gizmo = gizmoBuilder
                    .WithName(GizmoName(character, bone))
                    .WithTarget(bone)
                    .Build();

                controllers[index] = new(dragHandle, gizmo, character, undoRedoController, bone);
            }

            return controllers;
        }

        static HipDragHandleController MakeHip(CharacterController character, CharacterUndoRedoController undoRedoController)
        {
            var bone = character.IK.GetBone("Bip01");

            var dragHandle = new DragHandle.Builder()
            {
                Name = DragHandleName(character, bone),
                Shape = PrimitiveType.Cube,
                Scale = Vector3.one * 0.04f,
                Target = bone,
                PositionDelegate = () => bone.position,
                RotationDelegate = () => bone.localRotation,
            }.Build();

            var gizmo = new CustomGizmo.Builder()
            {
                Name = GizmoName(character, bone),
                Target = bone,
                Size = 0.25f,
                Mode = CustomGizmo.GizmoMode.Local,
            }.Build();

            return new(dragHandle, gizmo, character, undoRedoController, bone);
        }

        static ThighGizmoController MakeThigh(
            CharacterController character, CharacterUndoRedoController undoRedoController, string boneName)
        {
            var bone = character.IK.GetBone(boneName);

            var positionBone = character.IK.GetMeshNode(boneName);

            if (!positionBone)
                positionBone = bone;

            var gizmo = new CustomGizmo.Builder()
            {
                Name = GizmoName(character, bone),
                Target = bone,
                Size = 0.25f,
                Mode = CustomGizmo.GizmoMode.Local,
                PositionTarget = positionBone,
            }.Build();

            return new(gizmo, character, undoRedoController, bone);
        }

        static ChestDragHandleController MakeChest(
            CharacterController character, CharacterUndoRedoController undoRedoController, string boneName)
        {
            var bone = character.IK.GetBone(boneName);
            var subBone = character.IK.GetBone($"{boneName}_sub");

            var ikTarget = character.IK.CreateIKSolverTarget();

            var dragHandle = new DragHandle.Builder()
            {
                Name = DragHandleName(character, bone),
                Target = ikTarget,
                Shape = PrimitiveType.Sphere,
                Scale = Vector3.one * 0.12f,
                PositionDelegate = PositionBetweenTransforms(bone, subBone),
            }.Build();

            var gizmo = new CustomGizmo.Builder()
            {
                Name = GizmoName(character, bone),
                Target = bone,
                Size = 0.25f,
                Mode = CustomGizmo.GizmoMode.Local,
            }.Build();

            return new(dragHandle, gizmo, character, undoRedoController, subBone, ikTarget);
        }

        static ChestSubGizmoController MakeChestSub(
            CharacterController character, CharacterUndoRedoController undoRedoController, string boneName)
        {
            var bone = character.IK.GetBone(boneName);

            var gizmo = new CustomGizmo.Builder()
            {
                Name = GizmoName(character, bone),
                Target = bone,
                Size = 0.2f,
                Mode = CustomGizmo.GizmoMode.Local,
            }.Build();

            return new(gizmo, character, undoRedoController, bone);
        }

        static DigitBaseDragHandleController[] MakeDigitBases(CharacterController character, CharacterUndoRedoController undoRedoController)
        {
            return new[]
            {
                "Bip01 ? Finger0", "Bip01 ? Finger1", "Bip01 ? Finger2", "Bip01 ? Finger3", "Bip01 ? Finger4",
                "Bip01 ? Toe0", "Bip01 ? Toe1", "Bip01 ? Toe2",
            }
                .SelectMany(digit => new[] { digit.Replace('?', 'L'), digit.Replace('?', 'R') })
                .Select(boneName => MakeDigitBase(character, undoRedoController, boneName))
                .ToArray();

            static DigitBaseDragHandleController MakeDigitBase(
                CharacterController character, CharacterUndoRedoController undoRedoController, string boneName)
            {
                var bone = character.IK.GetBone(boneName);
                var realBone = character.IK.GetBone($"{boneName}1");
                var positionNode = character.IK.GetMeshNode(bone.name);

                if (!positionNode)
                    positionNode = bone;

                var ikTarget = character.IK.CreateIKSolverTarget();

                var dragHandle = new DragHandle.Builder()
                {
                    Name = DragHandleName(character, bone),
                    Shape = PrimitiveType.Sphere,
                    Target = ikTarget,
                    Scale = Vector3.one * 0.01f,
                    PositionDelegate = () => positionNode.position,
                }.Build();

                var gizmo = new CustomGizmo.Builder()
                {
                    Name = GizmoName(character, bone),
                    Size = 0.15f,
                    Target = bone,
                    Mode = CustomGizmo.GizmoMode.Local,
                    PositionTarget = positionNode,
                }.Build();

                return new(dragHandle, gizmo, character, undoRedoController, realBone, ikTarget);
            }
        }

        static DigitDragHandleController[] MakeNoLimitDigits(CharacterController character, CharacterUndoRedoController undoRedoController)
        {
            var digits = new[]
            {
                "Bip01 ? Finger0Nub", "Bip01 ? Finger1Nub", "Bip01 ? Finger2Nub", "Bip01 ? Finger3Nub", "Bip01 ? Finger4Nub",
                "Bip01 ? Toe0Nub", "Bip01 ? Toe1Nub", "Bip01 ? Toe2Nub",
            };

            var digitControllers = new List<DigitDragHandleController>();

            foreach (var digit in digits)
            {
                var leftJoint = character.IK.GetBone(digit.Replace('?', 'L'));
                var rightJoint = character.IK.GetBone(digit.Replace('?', 'R'));
                var jointCount = digit.Contains("Finger") ? 2 : 1;

                for (var i = jointCount; i > 0; --i)
                {
                    digitControllers.Add(MakeDigit(character, undoRedoController, leftJoint));
                    digitControllers.Add(MakeDigit(character, undoRedoController, rightJoint));

                    leftJoint = leftJoint.parent;
                    rightJoint = rightJoint.parent;
                }
            }

            return [.. digitControllers];

            static DigitDragHandleController MakeDigit(
                CharacterController character, CharacterUndoRedoController undoRedoController, Transform digit)
            {
                var realJoint = digit.parent;
                var positionNode = character.IK.GetMeshNode(realJoint.name);

                if (!positionNode)
                    positionNode = realJoint;

                var ikTarget = character.IK.CreateIKSolverTarget();

                var dragHandle = new DragHandle.Builder()
                {
                    Name = DragHandleName(character, digit),
                    Shape = PrimitiveType.Sphere,
                    Visible = true,
                    Target = ikTarget,
                    Scale = Vector3.one * 0.01f,
                    PositionDelegate = () => positionNode.position,
                }.Build();

                var gizmo = new CustomGizmo.Builder()
                {
                    Name = GizmoName(character, realJoint),
                    Size = 0.15f,
                    Target = realJoint,
                    Mode = CustomGizmo.GizmoMode.Local,
                    PositionTarget = positionNode,
                }.Build();

                return new(dragHandle, gizmo, character, undoRedoController, digit, ikTarget);
            }
        }

        static EyeDragHandleController MakeEye(
            CharacterController character, CharacterUndoRedoController undoRedoController, bool left)
        {
            var dragHandle = new DragHandle.Builder()
            {
                Name = DragHandleName(character, $"{(left ? "Left" : "Right")} Eye"),
                Shape = PrimitiveType.Sphere,
                Scale = Vector3.one * 0.1f,
                PositionDelegate = EyePosition(character, left),
            }.Build();

            return new(dragHandle, character, undoRedoController, left);

            static Func<Vector3> EyePosition(CharacterController character, bool left)
            {
                var head = character.IK.GetBone("Bip01 Head");
                var headNub = character.IK.GetBone("Bip01 HeadNub");

                var inverse = left ? 1 : -1;

                var characterTransform = character.Transform;

                return () =>
                {
                    var scale = characterTransform.localScale;

                    return (head.position + headNub.position) * 0.5f
                        + 0.05f * scale.x * head.right
                        + 0.05f * scale.y * head.up
                        + 0.04f * scale.z * inverse * head.forward;
                };
            }
        }

        static Func<Vector3> PositionBetweenTransforms(Transform a, Transform b) =>
            () =>
            {
                var (ax, ay, az) = a.position;
                var (bx, by, bz) = b.position;

                return new(
                    (ax + bx) / 2f,
                    (ay + by) / 2f,
                    (az + bz) / 2f);
            };

        static Func<Quaternion> AxisRotation(Transform target, float angle, Vector3 axis)
        {
            var rotation = Quaternion.Euler(axis * angle);

            return () => target.rotation * rotation;
        }

        static string GizmoName(CharacterController character, object @object) =>
            $"[{@object} Gizmo ({character})]";

        static string DragHandleName(CharacterController character, object @object) =>
            $"[{@object} Drag Handle ({character})]";
    }

    private void DestroyController(IKDragHandleController controller)
    {
        foreach (var dragHandle in controller)
        {
            dragHandle?.Destroy();
            characterDragHandleInputService.RemoveController(dragHandle);
        }
    }

    private void RaisePropertyChanged(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

        PropertyChanged?.Invoke(this, new(name));
    }
}
