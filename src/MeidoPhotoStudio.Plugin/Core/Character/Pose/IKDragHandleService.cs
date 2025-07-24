using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.UI.Legacy;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;
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
    private bool autoSelectTab;
    private Color upperBoneColour;
    private Color middleBoneColour;
    private Color lowerBoneColour;
    private Color spineColour;
    private Color rootColour;
    private Color baseDigitJointColour;
    private Color middleDigitJointColour;
    private Color tipDigitJointColour;

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

        this.characterService.PreCalledCharacters += OnCharactersCalled;
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

    public bool AutoSelectTab
    {
        get => autoSelectTab;
        set
        {
            if (value == autoSelectTab)
                return;

            autoSelectTab = value;

            foreach (var controller in controllers.Values)
                controller.AutoSelectTab = autoSelectTab;
        }
    }

    public Color UpperBoneColour
    {
        get => upperBoneColour;
        set
        {
            if (upperBoneColour == value)
                return;

            upperBoneColour = value;

            foreach (var controller in controllers.Values.SelectMany(static controller => controller).OfType<UpperLimbDragHandleController>())
                controller.DragHandleColour = upperBoneColour;
        }
    }

    public Color MiddleBoneColour
    {
        get => middleBoneColour;
        set
        {
            if (middleBoneColour == value)
                return;

            middleBoneColour = value;

            foreach (var controller in controllers.Values.SelectMany(static controller => controller).OfType<MiddleLimbDragHandleController>())
                controller.DragHandleColour = middleBoneColour;
        }
    }

    public Color LowerBoneColour
    {
        get => lowerBoneColour;
        set
        {
            if (lowerBoneColour == value)
                return;

            lowerBoneColour = value;

            foreach (var controller in controllers.Values.SelectMany(static controller => controller).OfType<LowerLimbDragHandleController>())
                controller.DragHandleColour = lowerBoneColour;
        }
    }

    public Color SpineColour
    {
        get => spineColour;
        set
        {
            if (spineColour == value)
                return;

            spineColour = value;

            foreach (var controller in controllers.Values.SelectMany(static controller => controller).OfType<SpineDragHandleController>())
                controller.DragHandleColour = spineColour;
        }
    }

    public Color RootColour
    {
        get => rootColour;
        set
        {
            if (rootColour == value)
                return;

            rootColour = value;

            foreach (var controller in controllers.Values.Select(static controller => controller[HandleType.Root]).OfType<HipDragHandleController>())
                controller.DragHandleColour = rootColour;
        }
    }

    public Color BaseDigitJointColour
    {
        get => baseDigitJointColour;
        set
        {
            if (baseDigitJointColour == value)
                return;

            baseDigitJointColour = value;

            UpdateDigitColours();
        }
    }

    public Color MiddleDigitJointColour
    {
        get => middleDigitJointColour;
        set
        {
            if (middleDigitJointColour == value)
                return;

            middleDigitJointColour = value;

            UpdateDigitColours();
        }
    }

    public Color TipDigitJointColour
    {
        get => tipDigitJointColour;
        set
        {
            if (tipDigitJointColour == value)
                return;

            tipDigitJointColour = value;

            UpdateDigitColours();
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

        if (!e.ChangingSlots.Contains(SafeMpn.body))
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
            [HandleType.EyeL] = MakeEye(
                character, undoRedoController, selectionController, tabSelectionController, left: true),
            [HandleType.EyeR] = MakeEye(
                character, undoRedoController, selectionController, tabSelectionController, left: false),
            [HandleType.UpperArmL] = MakeUpperLimb(
                character, undoRedoController, selectionController, tabSelectionController, "Bip01 L UpperArm"),
            [HandleType.UpperArmR] = MakeUpperLimb(
                character, undoRedoController, selectionController, tabSelectionController, "Bip01 R UpperArm"),
            [HandleType.ForearmL] = MakeMiddleLimb(
                character, undoRedoController, selectionController, tabSelectionController, "Bip01 L Forearm"),
            [HandleType.ForearmR] = MakeMiddleLimb(
                character, undoRedoController, selectionController, tabSelectionController, "Bip01 R Forearm"),
            [HandleType.HandL] = MakeLowerLimb(
                character, undoRedoController, selectionController, tabSelectionController, "Bip01 L Hand"),
            [HandleType.HandR] = MakeLowerLimb(
                character, undoRedoController, selectionController, tabSelectionController, "Bip01 R Hand"),
            [HandleType.ChestL] = MakeChest(
                character, undoRedoController, selectionController, tabSelectionController, "Mune_L"),
            [HandleType.ChestR] = MakeChest(
                character, undoRedoController, selectionController, tabSelectionController, "Mune_R"),
            [HandleType.ChestSubL] = MakeChestSub(
                character, undoRedoController, selectionController, tabSelectionController, "Mune_L_sub"),
            [HandleType.ChestSubR] = MakeChestSub(
                character, undoRedoController, selectionController, tabSelectionController, "Mune_R_sub"),
            [HandleType.Torso] = MakeTorso(character, undoRedoController, selectionController, tabSelectionController),
            [HandleType.HeadBase] = MakeSpine(
                character, undoRedoController, selectionController, tabSelectionController, "Bip01 Head"),
            [HandleType.Neck] = MakeSpine(
                character, undoRedoController, selectionController, tabSelectionController, "Bip01 Neck"),
            [HandleType.Spine] = MakeSpine(
                character, undoRedoController, selectionController, tabSelectionController, "Bip01 Spine"),
            [HandleType.Spine0a] = MakeSpine(
                character, undoRedoController, selectionController, tabSelectionController, "Bip01 Spine0a"),
            [HandleType.Spine1] = MakeSpine(
                character, undoRedoController, selectionController, tabSelectionController, "Bip01 Spine1"),
            [HandleType.Spine1a] = MakeSpine(
                character, undoRedoController, selectionController, tabSelectionController, "Bip01 Spine1a"),
            [HandleType.Hip] = MakePelvis(
                character, undoRedoController, selectionController, tabSelectionController),
            [HandleType.ThighL] = MakeThigh(
                character, undoRedoController, selectionController, tabSelectionController, "Bip01 L Thigh"),
            [HandleType.ThighR] = MakeThigh(
                character, undoRedoController, selectionController, tabSelectionController, "Bip01 R Thigh"),
            [HandleType.CalfL] = MakeMiddleLimb(
                character, undoRedoController, selectionController, tabSelectionController, "Bip01 L Calf"),
            [HandleType.CalfR] = MakeMiddleLimb(
                character, undoRedoController, selectionController, tabSelectionController, "Bip01 R Calf"),
            [HandleType.FootL] = MakeLowerLimb(
                character, undoRedoController, selectionController, tabSelectionController, "Bip01 L Foot"),
            [HandleType.FootR] = MakeLowerLimb(
                character, undoRedoController, selectionController, tabSelectionController, "Bip01 R Foot"),
            [HandleType.Root] = MakeHip(
                character, undoRedoController, selectionController, tabSelectionController),
        };

        InitializeHandsAndFeet(
            ikDragHandleController, character, undoRedoController, selectionController, tabSelectionController);

        foreach (var dragHandleController in ikDragHandleController)
            characterDragHandleInputService.AddController(dragHandleController);

        ikDragHandleController.SmallHandle = SmallHandle;
        ikDragHandleController.CubeEnabled = CubeEnabled;
        ikDragHandleController.AutoSelect = AutoSelect;
        ikDragHandleController.AutoSelectTab = AutoSelectTab;

        UpdateDigitColours(ikDragHandleController);

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

        UpperLimbDragHandleController MakeUpperLimb(
            CharacterController character,
            CharacterUndoRedoController undoRedoController,
            SelectionController<CharacterController> selectionController,
            TabSelectionController tabSelectionController,
            string boneName)
        {
            var bone = character.IK.GetBone(boneName);

            var (dragHandle, gizmo, ikTarget) = BuildIKDragHandleAndGizmo(character, bone);

            return new(
                dragHandle,
                gizmo,
                character,
                undoRedoController,
                selectionController,
                tabSelectionController,
                bone,
                ikTarget)
            {
                DragHandleColour = UpperBoneColour,
            };
        }

        MiddleLimbDragHandleController MakeMiddleLimb(
            CharacterController character,
            CharacterUndoRedoController undoRedoController,
            SelectionController<CharacterController> selectionController,
            TabSelectionController tabSelectionController,
            string boneName)
        {
            var bone = character.IK.GetBone(boneName);

            var (dragHandle, gizmo, ikTarget) = BuildIKDragHandleAndGizmo(character, bone);

            gizmo.VisibleRotateX = false;
            gizmo.VisibleRotateY = false;
            gizmo.VisibleRotateZ = true;

            return new(
                dragHandle,
                gizmo,
                character,
                undoRedoController,
                selectionController,
                tabSelectionController,
                bone,
                ikTarget)
            {
                DragHandleColour = MiddleBoneColour,
            };
        }

        LowerLimbDragHandleController MakeLowerLimb(
            CharacterController character,
            CharacterUndoRedoController undoRedoController,
            SelectionController<CharacterController> selectionController,
            TabSelectionController tabSelectionController,
            string boneName)
        {
            var bone = character.IK.GetBone(boneName);

            var (dragHandle, gizmo, ikTarget) = BuildIKDragHandleAndGizmo(character, bone);

            return new(
                dragHandle,
                gizmo,
                character,
                undoRedoController,
                selectionController,
                tabSelectionController,
                bone,
                ikTarget)
            {
                DragHandleColour = LowerBoneColour,
            };
        }

        static TorsoDragHandleController MakeTorso(
            CharacterController character,
            CharacterUndoRedoController undoRedoController,
            SelectionController<CharacterController> selectionController,
            TabSelectionController tabSelectionController)
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

            return new(
                dragHandle,
                character,
                undoRedoController,
                selectionController,
                tabSelectionController,
                spine1a,
                spine1,
                spine0a,
                spine);
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
            CharacterController character,
            CharacterUndoRedoController undoRedoController,
            SelectionController<CharacterController> selectionController,
            TabSelectionController tabSelectionController)
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

            return new(
                dragHandle, gizmo, character, undoRedoController, selectionController, tabSelectionController, pelvis);
        }

        SpineDragHandleController MakeSpine(
            CharacterController character,
            CharacterUndoRedoController undoRedoController,
            SelectionController<CharacterController> selectionController,
            TabSelectionController tabSelectionController,
            string boneName)
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

            return new(
                dragHandle, gizmo, character, undoRedoController, selectionController, tabSelectionController, bone)
            {
                DragHandleColour = SpineColour,
            };
        }

        HipDragHandleController MakeHip(
            CharacterController character,
            CharacterUndoRedoController undoRedoController,
            SelectionController<CharacterController> selectionController,
            TabSelectionController tabSelectionController)
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

            return new(
                dragHandle, gizmo, character, undoRedoController, selectionController, tabSelectionController, bone)
            {
                DragHandleColour = RootColour,
            };
        }

        static ThighGizmoController MakeThigh(
            CharacterController character,
            CharacterUndoRedoController undoRedoController,
            SelectionController<CharacterController> selectionController,
            TabSelectionController tabSelectionController,
            string boneName)
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

            return new(gizmo, character, undoRedoController, selectionController, tabSelectionController, bone);
        }

        static ChestDragHandleController MakeChest(
            CharacterController character,
            CharacterUndoRedoController undoRedoController,
            SelectionController<CharacterController> selectionController,
            TabSelectionController tabSelectionController,
            string boneName)
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

            return new(
                dragHandle,
                gizmo,
                character,
                undoRedoController,
                selectionController,
                tabSelectionController,
                subBone,
                ikTarget);
        }

        static ChestSubGizmoController MakeChestSub(
            CharacterController character,
            CharacterUndoRedoController undoRedoController,
            SelectionController<CharacterController> selectionController,
            TabSelectionController tabSelectionController,
            string boneName)
        {
            var bone = character.IK.GetBone(boneName);

            var gizmo = new CustomGizmo.Builder()
            {
                Name = GizmoName(character, bone),
                Target = bone,
                Size = 0.2f,
                Mode = CustomGizmo.GizmoMode.Local,
            }.Build();

            return new(gizmo, character, undoRedoController, selectionController, tabSelectionController, bone);
        }

        static void InitializeHandsAndFeet(
            IKDragHandleController controller,
            CharacterController character,
            CharacterUndoRedoController undoRedoController,
            SelectionController<CharacterController> selectionController,
            TabSelectionController tabSelectionController)
        {
            var handleToBoneMap = new Dictionary<HandleType, string>()
            {
                [HandleType.Finger0L] = "Bip01 L Finger0",
                [HandleType.Finger02L] = "Bip01 L Finger02",
                [HandleType.Finger0NubL] = "Bip01 L Finger0Nub",
                [HandleType.Finger1L] = "Bip01 L Finger1",
                [HandleType.Finger12L] = "Bip01 L Finger12",
                [HandleType.Finger1NubL] = "Bip01 L Finger1Nub",
                [HandleType.Finger2L] = "Bip01 L Finger2",
                [HandleType.Finger22L] = "Bip01 L Finger22",
                [HandleType.Finger2NubL] = "Bip01 L Finger2Nub",
                [HandleType.Finger3L] = "Bip01 L Finger3",
                [HandleType.Finger32L] = "Bip01 L Finger32",
                [HandleType.Finger3NubL] = "Bip01 L Finger3Nub",
                [HandleType.Finger4L] = "Bip01 L Finger4",
                [HandleType.Finger42L] = "Bip01 L Finger42",
                [HandleType.Finger4NubL] = "Bip01 L Finger4Nub",
                [HandleType.Finger0R] = "Bip01 R Finger0",
                [HandleType.Finger02R] = "Bip01 R Finger02",
                [HandleType.Finger0NubR] = "Bip01 R Finger0Nub",
                [HandleType.Finger1R] = "Bip01 R Finger1",
                [HandleType.Finger12R] = "Bip01 R Finger12",
                [HandleType.Finger1NubR] = "Bip01 R Finger1Nub",
                [HandleType.Finger2R] = "Bip01 R Finger2",
                [HandleType.Finger22R] = "Bip01 R Finger22",
                [HandleType.Finger2NubR] = "Bip01 R Finger2Nub",
                [HandleType.Finger3R] = "Bip01 R Finger3",
                [HandleType.Finger32R] = "Bip01 R Finger32",
                [HandleType.Finger3NubR] = "Bip01 R Finger3Nub",
                [HandleType.Finger4R] = "Bip01 R Finger4",
                [HandleType.Finger42R] = "Bip01 R Finger42",
                [HandleType.Finger4NubR] = "Bip01 R Finger4Nub",
                [HandleType.Toe0L] = "Bip01 L Toe0",
                [HandleType.Toe0NubL] = "Bip01 L Toe0Nub",
                [HandleType.Toe1L] = "Bip01 L Toe1",
                [HandleType.Toe1NubL] = "Bip01 L Toe1Nub",
                [HandleType.Toe2L] = "Bip01 L Toe2",
                [HandleType.Toe2NubL] = "Bip01 L Toe2Nub",
                [HandleType.Toe0R] = "Bip01 R Toe0",
                [HandleType.Toe0NubR] = "Bip01 R Toe0Nub",
                [HandleType.Toe1R] = "Bip01 R Toe1",
                [HandleType.Toe1NubR] = "Bip01 R Toe1Nub",
                [HandleType.Toe2R] = "Bip01 R Toe2",
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
                var targetBone = character.IK.GetBone(boneName);
                var ikBone = character.IK.GetBone($"{boneName}1");
                var positionNode = character.IK.GetMeshNode(boneName);

                if (!positionNode)
                    positionNode = targetBone;

                var childPositionNode = character.IK.GetMeshNode($"{boneName}1");

                if (!childPositionNode)
                    childPositionNode = ikBone;

                var ikTarget = character.IK.CreateIKSolverTarget();
                var distance = Vector3.Distance(positionNode.position, childPositionNode.position);

                var dragHandle = new DragHandle.Builder()
                {
                    Name = DragHandleName(character, targetBone),
                    Shape = PrimitiveType.Cylinder,
                    Target = ikTarget,
                    Scale = new(0.0075f, distance / 2f, 0.0075f),
                    PositionDelegate = () => positionNode.position - positionNode.right * (distance / 2f),
                    RotationDelegate = AxisRotation(targetBone, 90f, Vector3.forward),
                }.Build();

                var gizmo = new CustomGizmo.Builder()
                {
                    Name = GizmoName(character, targetBone),
                    Size = 0.15f,
                    Target = targetBone,
                    Mode = CustomGizmo.GizmoMode.Local,
                    PositionTarget = positionNode,
                }.Build();

                return new(
                    dragHandle,
                    gizmo,
                    character,
                    undoRedoController,
                    selectionController,
                    tabSelectionController,
                    ikBone,
                    ikTarget);
            }

            DigitDragHandleController MakeNoLimitDigit(string boneName)
            {
                var ikBone = character.IK.GetBone(boneName);
                var targetBone = ikBone.parent;
                var targetNode = character.IK.GetMeshNode(targetBone.name);

                if (!targetNode)
                    targetNode = targetBone;

                var ikNode = character.IK.GetMeshNode(boneName);

                if (!ikNode)
                    ikNode = ikBone;

                var ikTarget = character.IK.CreateIKSolverTarget();
                var distance = Vector3.Distance(ikNode.position, targetNode.position);

                var dragHandle = new DragHandle.Builder()
                {
                    Name = DragHandleName(character, ikBone),
                    Shape = PrimitiveType.Cylinder,
                    Visible = true,
                    Target = ikTarget,
                    Scale = new(0.0075f, distance / 2f, 0.0075f),
                    PositionDelegate = () => targetNode.position - targetNode.right * (distance / 2f),
                    RotationDelegate = AxisRotation(targetBone, 90f, Vector3.forward),
                }.Build();

                var gizmo = new CustomGizmo.Builder()
                {
                    Name = GizmoName(character, targetBone),
                    Size = 0.15f,
                    Target = targetBone,
                    Mode = CustomGizmo.GizmoMode.Local,
                    PositionTarget = targetNode,
                }.Build();

                return new(
                    dragHandle,
                    gizmo,
                    character,
                    undoRedoController,
                    selectionController,
                    tabSelectionController,
                    ikBone,
                    ikTarget);
            }
        }

        static EyeDragHandleController MakeEye(
            CharacterController character,
            CharacterUndoRedoController undoRedoController,
            SelectionController<CharacterController> selectionController,
            TabSelectionController tabSelectionController,
            bool left)
        {
            var dragHandle = new DragHandle.Builder()
            {
                Name = DragHandleName(character, $"{(left ? "Left" : "Right")} Eye"),
                Shape = PrimitiveType.Sphere,
                Scale = Vector3.one * 0.1f,
                PositionDelegate = EyePosition(character, left),
            }.Build();

            return new(dragHandle, character, undoRedoController, selectionController, tabSelectionController, left);

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

    private void UpdateDigitColours()
    {
        foreach (var controller in controllers.Values)
            UpdateDigitColours(controller);
    }

    private void UpdateDigitColours(IKDragHandleController controller)
    {
        var colourMap = new Color[3]
        {
            BaseDigitJointColour,
            MiddleDigitJointColour,
            TipDigitJointColour,
        };

        var fingerOffset = HandleType.Finger0R - HandleType.Finger0L;

        for (var type = HandleType.Finger0L; type <= HandleType.Finger4NubL; type++)
        {
            var colour = colourMap[(type - HandleType.Finger0L) % 3];

            if (controller[type] is IColourableDragHandle leftDigit)
                leftDigit.DragHandleColour = colour;

            if (controller[type + fingerOffset] is IColourableDragHandle rightDigit)
                rightDigit.DragHandleColour = colour;
        }

        var toeOffset = HandleType.Toe0R - HandleType.Toe0L;

        for (var type = HandleType.Toe0L; type <= HandleType.Toe2NubL; type++)
        {
            var colour = colourMap[(type - HandleType.Toe0L) % 2];

            if (controller[type] is IColourableDragHandle leftDigit)
                leftDigit.DragHandleColour = colour;

            if (controller[type + toeOffset] is IColourableDragHandle rightDigit)
                rightDigit.DragHandleColour = colour;
        }
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
