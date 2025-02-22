using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.UI.Legacy;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

using HandleType = MeidoPhotoStudio.Plugin.Core.Character.Pose.IKDragHandleController.HandleType;

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
    private bool autoSelect;

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

    public bool AutoSelect
    {
        get => autoSelect;
        set
        {
            if (value == autoSelect)
                return;

            autoSelect = value;

            foreach (var controller in controllers.Values)
                controller.AutoSelect = autoSelect;
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

        if (!e.ChangingSlots.Contains(SafeMpn.GetValue(nameof(MPN.body))))
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

        var ikDragHandleController = new IKDragHandleController()
        {
            [HandleType.Cube] = MakeCube(character, selectionController, tabSelectionController, CubeEnabled),
            [HandleType.Body] = MakeBody(character, selectionController, tabSelectionController),
            [HandleType.Head] = MakeHead(character, undoRedoController, selectionController, tabSelectionController),
            [HandleType.EyeL] = MakeEye(character, undoRedoController, selectionController, left: true),
            [HandleType.EyeR] = MakeEye(character, undoRedoController, selectionController, left: false),
            [HandleType.UpperArmL] = MakeUpperLimb(character, undoRedoController, selectionController, "Bip01 L UpperArm"),
            [HandleType.UpperArmR] = MakeUpperLimb(character, undoRedoController, selectionController, "Bip01 R UpperArm"),
            [HandleType.ForearmL] = MakeMiddleLimb(character, undoRedoController, selectionController, "Bip01 L Forearm"),
            [HandleType.ForearmR] = MakeMiddleLimb(character, undoRedoController, selectionController, "Bip01 R Forearm"),
            [HandleType.HandL] = MakeLowerLimb(character, undoRedoController, selectionController, "Bip01 L Hand"),
            [HandleType.HandR] = MakeLowerLimb(character, undoRedoController, selectionController, "Bip01 R Hand"),
            [HandleType.ChestL] = MakeChest(character, undoRedoController, selectionController, "Mune_L"),
            [HandleType.ChestR] = MakeChest(character, undoRedoController, selectionController, "Mune_R"),
            [HandleType.ChestSubL] = MakeChestSub(character, undoRedoController, selectionController, "Mune_L_sub"),
            [HandleType.ChestSubR] = MakeChestSub(character, undoRedoController, selectionController, "Mune_R_sub"),
            [HandleType.Torso] = MakeTorso(character, undoRedoController, selectionController),
            [HandleType.HeadBase] = MakeSpine(character, undoRedoController, selectionController, "Bip01 Head"),
            [HandleType.Neck] = MakeSpine(character, undoRedoController, selectionController, "Bip01 Neck"),
            [HandleType.Spine] = MakeSpine(character, undoRedoController, selectionController, "Bip01 Spine"),
            [HandleType.Spine0a] = MakeSpine(character, undoRedoController, selectionController, "Bip01 Spine0a"),
            [HandleType.Spine1] = MakeSpine(character, undoRedoController, selectionController, "Bip01 Spine1"),
            [HandleType.Spine1a] = MakeSpine(character, undoRedoController, selectionController, "Bip01 Spine1a"),
            [HandleType.Hip] = MakePelvis(character, undoRedoController, selectionController),
            [HandleType.ThighL] = MakeThigh(character, undoRedoController, selectionController, "Bip01 L Thigh"),
            [HandleType.ThighR] = MakeThigh(character, undoRedoController, selectionController, "Bip01 R Thigh"),
            [HandleType.CalfL] = MakeMiddleLimb(character, undoRedoController, selectionController, "Bip01 L Calf"),
            [HandleType.CalfR] = MakeMiddleLimb(character, undoRedoController, selectionController, "Bip01 R Calf"),
            [HandleType.FootL] = MakeLowerLimb(character, undoRedoController, selectionController, "Bip01 L Foot"),
            [HandleType.FootR] = MakeLowerLimb(character, undoRedoController, selectionController, "Bip01 R Foot"),
            [HandleType.Root] = MakeHip(character, undoRedoController, selectionController),
        };

        InitializeHandsAndFeet(ikDragHandleController, character, undoRedoController, selectionController);

        foreach (var dragHandleController in ikDragHandleController)
            characterDragHandleInputService.AddController(dragHandleController);

        ikDragHandleController.SmallHandle = SmallHandle;
        ikDragHandleController.CubeEnabled = CubeEnabled;
        ikDragHandleController.AutoSelect = AutoSelect;

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

            var gizmo = new CustomGizmo.Builder()
            {
                Name = $"[Cube Body Gizmo ({character})]",
                Target = characterTransform,
                Size = 0.45f,
                Mode = CustomGizmo.GizmoMode.World,
            }.Build();

            return new(dragHandle, gizmo, characterTransform, character, selectionController, tabSelectionController)
            {
                DragHandleEnabled = cubeEnabled,
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
            CharacterController character, CharacterUndoRedoController undoRedoController, SelectionController<CharacterController> selectionController, string boneName)
        {
            var bone = character.IK.GetBone(boneName);

            var (dragHandle, gizmo, ikTarget) = BuildIKDragHandleAndGizmo(character, bone);

            return new(dragHandle, gizmo, character, undoRedoController, selectionController, bone, ikTarget);
        }

        static MiddleLimbDragHandleController MakeMiddleLimb(
            CharacterController character, CharacterUndoRedoController undoRedoController, SelectionController<CharacterController> selectionController, string boneName)
        {
            var bone = character.IK.GetBone(boneName);

            var (dragHandle, gizmo, ikTarget) = BuildIKDragHandleAndGizmo(character, bone);

            gizmo.VisibleRotateX = false;
            gizmo.VisibleRotateY = false;
            gizmo.VisibleRotateZ = true;

            return new(dragHandle, gizmo, character, undoRedoController, selectionController, bone, ikTarget);
        }

        static LowerLimbDragHandleController MakeLowerLimb(
            CharacterController character, CharacterUndoRedoController undoRedoController, SelectionController<CharacterController> selectionController, string boneName)
        {
            var bone = character.IK.GetBone(boneName);

            var (dragHandle, gizmo, ikTarget) = BuildIKDragHandleAndGizmo(character, bone);

            return new(dragHandle, gizmo, character, undoRedoController, selectionController, bone, ikTarget);
        }

        static TorsoDragHandleController MakeTorso(
            CharacterController character, CharacterUndoRedoController undoRedoController, SelectionController<CharacterController> selectionController)
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

            return new(dragHandle, character, undoRedoController, selectionController, spine1a, spine1, spine0a, spine);
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

            return new(dragHandle, character, undoRedoController, selectionController, tabSelectionController, neck);
        }

        static PelvisDragHandleController MakePelvis(
            CharacterController character, CharacterUndoRedoController undoRedoController, SelectionController<CharacterController> selectionController)
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

            return new(dragHandle, gizmo, character, undoRedoController, selectionController, pelvis);
        }

        static SpineDragHandleController MakeSpine(
            CharacterController character, CharacterUndoRedoController undoRedoController, SelectionController<CharacterController> selectionController, string boneName)
        {
            var bone = character.IK.GetBone(boneName);

            var dragHandle = new DragHandle.Builder()
            {
                Shape = PrimitiveType.Sphere,
                Scale = Vector3.one * 0.04f,
                Target = bone,
                Name = DragHandleName(character, bone),
                PositionDelegate = () => bone.position,
            }.Build();

            var gizmo = new CustomGizmo.Builder()
            {
                Size = 0.25f,
                Mode = CustomGizmo.GizmoMode.Local,
                Target = bone,
                Name = GizmoName(character, bone),
            }.Build();

            return new(dragHandle, gizmo, character, undoRedoController, selectionController, bone);
        }

        static HipDragHandleController MakeHip(CharacterController character, CharacterUndoRedoController undoRedoController, SelectionController<CharacterController> selectionController)
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

            return new(dragHandle, gizmo, character, undoRedoController, selectionController, bone);
        }

        static ThighGizmoController MakeThigh(
            CharacterController character, CharacterUndoRedoController undoRedoController, SelectionController<CharacterController> selectionController, string boneName)
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

            return new(gizmo, character, undoRedoController, selectionController, bone);
        }

        static ChestDragHandleController MakeChest(
            CharacterController character, CharacterUndoRedoController undoRedoController, SelectionController<CharacterController> selectionController, string boneName)
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

            return new(dragHandle, gizmo, character, undoRedoController, selectionController, subBone, ikTarget);
        }

        static ChestSubGizmoController MakeChestSub(
            CharacterController character, CharacterUndoRedoController undoRedoController, SelectionController<CharacterController> selectionController, string boneName)
        {
            var bone = character.IK.GetBone(boneName);

            var gizmo = new CustomGizmo.Builder()
            {
                Name = GizmoName(character, bone),
                Target = bone,
                Size = 0.2f,
                Mode = CustomGizmo.GizmoMode.Local,
            }.Build();

            return new(gizmo, character, undoRedoController, selectionController, bone);
        }

        static void InitializeHandsAndFeet(
            IKDragHandleController controller, CharacterController character, CharacterUndoRedoController undoRedoController, SelectionController<CharacterController> selectionController)
        {
            var handleToBoneMap = new Dictionary<HandleType, string>()
            {
                [HandleType.Finger0L] = "Bip01 L Finger0",
                [HandleType.Finger01L] = "Bip01 L Finger01",
                [HandleType.Finger02L] = "Bip01 L Finger02",
                [HandleType.Finger0NubL] = "Bip01 L Finger0Nub",
                [HandleType.Finger1L] = "Bip01 L Finger1",
                [HandleType.Finger11L] = "Bip01 L Finger11",
                [HandleType.Finger12L] = "Bip01 L Finger12",
                [HandleType.Finger1NubL] = "Bip01 L Finger1Nub",
                [HandleType.Finger2L] = "Bip01 L Finger2",
                [HandleType.Finger21L] = "Bip01 L Finger21",
                [HandleType.Finger22L] = "Bip01 L Finger22",
                [HandleType.Finger2NubL] = "Bip01 L Finger2Nub",
                [HandleType.Finger3L] = "Bip01 L Finger3",
                [HandleType.Finger31L] = "Bip01 L Finger31",
                [HandleType.Finger32L] = "Bip01 L Finger32",
                [HandleType.Finger3NubL] = "Bip01 L Finger3Nub",
                [HandleType.Finger4L] = "Bip01 L Finger4",
                [HandleType.Finger41L] = "Bip01 L Finger41",
                [HandleType.Finger42L] = "Bip01 L Finger42",
                [HandleType.Finger4NubL] = "Bip01 L Finger4Nub",
                [HandleType.Finger0R] = "Bip01 R Finger0",
                [HandleType.Finger01R] = "Bip01 R Finger01",
                [HandleType.Finger02R] = "Bip01 R Finger02",
                [HandleType.Finger0NubR] = "Bip01 R Finger0Nub",
                [HandleType.Finger1R] = "Bip01 R Finger1",
                [HandleType.Finger11R] = "Bip01 R Finger11",
                [HandleType.Finger12R] = "Bip01 R Finger12",
                [HandleType.Finger1NubR] = "Bip01 R Finger1Nub",
                [HandleType.Finger2R] = "Bip01 R Finger2",
                [HandleType.Finger21R] = "Bip01 R Finger21",
                [HandleType.Finger22R] = "Bip01 R Finger22",
                [HandleType.Finger2NubR] = "Bip01 R Finger2Nub",
                [HandleType.Finger3R] = "Bip01 R Finger3",
                [HandleType.Finger31R] = "Bip01 R Finger31",
                [HandleType.Finger32R] = "Bip01 R Finger32",
                [HandleType.Finger3NubR] = "Bip01 R Finger3Nub",
                [HandleType.Finger4R] = "Bip01 R Finger4",
                [HandleType.Finger41R] = "Bip01 R Finger41",
                [HandleType.Finger42R] = "Bip01 R Finger42",
                [HandleType.Finger4NubR] = "Bip01 R Finger4Nub",
                [HandleType.Toe0L] = "Bip01 L Toe0",
                [HandleType.Toe01L] = "Bip01 L Toe01",
                [HandleType.Toe0NubL] = "Bip01 L Toe0Nub",
                [HandleType.Toe1L] = "Bip01 L Toe1",
                [HandleType.Toe11L] = "Bip01 L Toe11",
                [HandleType.Toe1NubL] = "Bip01 L Toe1Nub",
                [HandleType.Toe2L] = "Bip01 L Toe2",
                [HandleType.Toe21L] = "Bip01 L Toe21",
                [HandleType.Toe2NubL] = "Bip01 L Toe2Nub",
                [HandleType.Toe0R] = "Bip01 R Toe0",
                [HandleType.Toe01R] = "Bip01 R Toe01",
                [HandleType.Toe0NubR] = "Bip01 R Toe0Nub",
                [HandleType.Toe1R] = "Bip01 R Toe1",
                [HandleType.Toe11R] = "Bip01 R Toe11",
                [HandleType.Toe1NubR] = "Bip01 R Toe1Nub",
                [HandleType.Toe2R] = "Bip01 R Toe2",
                [HandleType.Toe21R] = "Bip01 R Toe21",
                [HandleType.Toe2NubR] = "Bip01 R Toe2Nub",
            };

            foreach (var (type, bone) in handleToBoneMap)
            {
                var finger = bone.Contains("Finger", StringComparison.OrdinalIgnoreCase);
                var joint = bone.Split(' ')[2];
                var baseJoint = finger ? joint.Length is 7 : joint.Length is 4;

                controller[type] = baseJoint ? MakeDigitBase(bone) : MakeNoLimitDigit(bone);
            }

            DigitBaseDragHandleController MakeDigitBase(string boneName)
            {
                var bone = character.IK.GetBone(boneName);
                var realBone = character.IK.GetBone($"{boneName}1");
                var positionNode = character.IK.GetMeshNode(boneName);

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

                return new(dragHandle, gizmo, character, undoRedoController, selectionController, realBone, ikTarget);
            }

            DigitDragHandleController MakeNoLimitDigit(string boneName)
            {
                var bone = character.IK.GetBone(boneName);
                var realJoint = bone.parent;
                var positionNode = character.IK.GetMeshNode(realJoint.name);

                if (!positionNode)
                    positionNode = realJoint;

                var ikTarget = character.IK.CreateIKSolverTarget();

                var dragHandle = new DragHandle.Builder()
                {
                    Name = DragHandleName(character, bone),
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

                return new(dragHandle, gizmo, character, undoRedoController, selectionController, bone, ikTarget);
            }
        }

        static EyeDragHandleController MakeEye(
            CharacterController character, CharacterUndoRedoController undoRedoController, SelectionController<CharacterController> selectionController, bool left)
        {
            var dragHandle = new DragHandle.Builder()
            {
                Name = DragHandleName(character, $"{(left ? "Left" : "Right")} Eye"),
                Shape = PrimitiveType.Sphere,
                Scale = Vector3.one * 0.1f,
                PositionDelegate = EyePosition(character, left),
            }.Build();

            return new(dragHandle, character, undoRedoController, selectionController, left);

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
