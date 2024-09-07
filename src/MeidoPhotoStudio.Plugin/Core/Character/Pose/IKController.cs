using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Database.Character;
using MeidoPhotoStudio.Plugin.Core.Serialization;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using RootMotion.FinalIK;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class IKController : INotifyPropertyChanged
{
    private static readonly Transform[] EmptyChain = [];

    private static GameObject ikTargetParent;

    private readonly CharacterController character;

    private Dictionary<string, Transform> boneCache = [];
    private ToggleableRotationLimitHinge[] limbRotationLimits = [];
    private ToggleableRotationLimitHinge[] digitRotationLimits = [];
    private Dictionary<string, RotationLimit> rotationLimitCache;
    private FABRIK fabrik;
    private HandController leftHand;
    private HandController rightHand;
    private HandController leftFoot;
    private HandController rightFoot;
    private bool limitLimbRotations = true;
    private bool limitDigitRotations = true;
    private bool dirty = false;

    public IKController(CharacterController character)
    {
        this.character = character ?? throw new ArgumentNullException(nameof(character));

        this.character.ProcessingCharacterProps += OnCharacterProcessing;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public bool Dirty
    {
        get => dirty;
        internal set
        {
            if (dirty == value)
                return;

            dirty = value;
        }
    }

    public bool LimitLimbRotations
    {
        get => limitLimbRotations;
        set => SetLimits(value, digits: false);
    }

    public bool LimitDigitRotations
    {
        get => limitDigitRotations;
        set => SetLimits(value, digits: true);
    }

    public bool MuneLEnabled
    {
        get => character.Maid.body0.GetMuneLEnabled();
        set
        {
            if (value == MuneLEnabled)
                return;

            character.Maid.body0.SetMuneYureLWithEnable(value);

            RaisePropertyChanged(nameof(MuneLEnabled));
        }
    }

    public bool MuneREnabled
    {
        get => character.Maid.body0.GetMuneREnabled();
        set
        {
            if (value == MuneREnabled)
                return;

            character.Maid.body0.SetMuneYureRWithEnable(value);

            RaisePropertyChanged(nameof(MuneREnabled));
        }
    }

    private static GameObject IKSolverTargetParent =>
        ikTargetParent ? ikTargetParent : ikTargetParent = new("[IK Solver Target Parent]");

    private HandController LeftHand =>
        leftHand ??= new(this, HandOrFootType.HandLeft);

    private HandController RightHand =>
        rightHand ??= new(this, HandOrFootType.HandRight);

    private HandController LeftFoot =>
        leftFoot ??= new HandController(this, HandOrFootType.FootLeft);

    private HandController RightFoot =>
        rightFoot ??= new HandController(this, HandOrFootType.FootRight);

    private FABRIK FABRIK
    {
        get
        {
            if (fabrik)
                return fabrik;

            fabrik = InitializeFABRIK();

            return fabrik;

            FABRIK InitializeFABRIK()
            {
                var root = GetBone("Bip01").transform;
                var fabrik = root.GetOrAddComponent<FABRIK>();

                fabrik.fixTransforms = false;

                var solver = fabrik.solver;

                solver.useRotationLimits = LimitLimbRotations || LimitDigitRotations;
                solver.maxIterations = 1;
                solver.Initiate(root);

                return fabrik;
            }
        }
    }

    private IKSolverFABRIK Solver =>
        FABRIK.solver;

    private CacheBoneDataArray CacheBoneData
    {
        get
        {
            var cache = character.Maid.GetOrAddComponent<CacheBoneDataArray>();

            if (cache.bone_data?.transform == null)
                cache.CreateCache(character.GetBone("Bip01"));

            return cache;
        }
    }

    public Transform CreateIKSolverTarget()
    {
        var ikTargetGameObject = new GameObject
        {
            name = $"[IK Target {character}]",
        };

        ikTargetGameObject.transform.SetParent(IKSolverTargetParent.transform, false);

        return ikTargetGameObject.transform;
    }

    public RotationLimit GetRotationLimit(string boneName)
    {
        InitializeRotationLimits();

        return rotationLimitCache.ContainsKey(boneName)
            ? rotationLimitCache[boneName]
            : null;
    }

    public Transform GetBone(string boneName)
    {
        if (boneCache.TryGetValue(boneName, out var bone))
            return bone;

        bone = character.GetBone(boneName);

        if (!bone)
        {
            Utility.LogWarning($"'{boneName}' does not exist");

            return null;
        }

        boneCache[boneName] = bone;

        return bone;
    }

    public Transform GetMeshNode(string boneName) =>
        CMT.SearchObjName(character.Maid.body0.goSlot[0].obj_tr, boneName, false);

    public void SetSolverTarget(Transform target)
    {
        _ = target ? target : throw new ArgumentNullException(nameof(target));
        Solver.target = target;
    }

    public void SetChain(params Transform[] chain)
    {
        _ = chain ?? throw new ArgumentNullException(nameof(chain));
        Solver.SetChain(EmptyChain, Solver.GetRoot());
        Solver.SetChain(chain, Solver.GetRoot());
    }

    public void LockSolver() =>
        Solver.SetIKPositionWeight(0f);

    public void UnlockSolver() =>
        Solver.SetIKPositionWeight(1f);

    public void FixLocalPositions()
    {
        if (Solver.IKPositionWeight <= 0f)
            return;

        foreach (var bone in Solver.bones)
            bone.transform.localPosition = bone.defaultLocalPosition;
    }

    public void ApplyHandOrFootPreset(HandPresetModel presetModel, HandOrFootType type)
    {
        var preset = DeserializeHandOrFootPreset(presetModel);

        if (preset is null)
            return;

        if (type is HandOrFootType.HandLeft or HandOrFootType.HandRight && preset.Type is not (HandOrFootType.HandLeft or HandOrFootType.HandRight))
            throw new ArgumentException($"{nameof(type)} is not compatible with {nameof(preset.Type)}");

        var handOrFoot = GetControllerByType(this, type);

        StopAnimation();

        handOrFoot.ApplyPreset(preset);

        ApplyLimits();

        Dirty = true;

        static HandOrFootPreset DeserializeHandOrFootPreset(HandPresetModel presetModel)
        {
            try
            {
                using var fileStream = File.OpenRead(presetModel.Filename);

                return new HandPresetSerializer().Deserialize(fileStream);
            }
            catch
            {
                Utility.LogError($"Could not load hand preset: {presetModel.Filename}");

                return null;
            }
        }
    }

    public void Flip()
    {
        StopAnimation();

        var spineBonesAndRotations = BackupSpine();
        var bonePairsAndRotations = BackupLimbs();

        MirrorRoot();
        MirrorSpine(spineBonesAndRotations);
        MirrorLimbs(bonePairsAndRotations);
        MirrorHandsAndFeet();

        ApplyLimits();

        Dirty = true;

        static Quaternion MirrorRotation(Vector3 rotation) =>
            Quaternion.Euler(360f - rotation.x, 360f - (rotation.y + 90f) - 90f, rotation.z);

        (Transform Bone, Vector3 EulerAngles)[] BackupSpine() =>
            new[]
            {
                "Bip01 Pelvis", "Bip01 Spine", "Bip01 Spine0a", "Bip01 Spine1", "Bip01 Spine1a", "Bip01 Neck",
            }
            .Select(GetBone)
            .Select(bone => (bone, bone.eulerAngles))
            .ToArray();

        ((Transform Bone, Vector3 Rotation) Left, (Transform Bone, Vector3 Rotation) Right)[] BackupLimbs() =>
            new[]
            {
                "Bip01 ? Clavicle", "Bip01 ? UpperArm", "Bip01 ? Forearm", "Bip01 ? Thigh", "Bip01 ? Calf",
                "Bip01 ? Hand", "Bip01 ? Foot",
            }
            .Select(bone => (Left: GetBone(bone.Replace('?', 'L')), Right: GetBone(bone.Replace('?', 'R'))))
            .Select(bone => (Left: (Bone: bone.Left, Rotation: bone.Left.eulerAngles), Right: (Bone: bone.Right, Rotation: bone.Right.eulerAngles)))
            .ToArray();

        void MirrorRoot()
        {
            var root = GetBone("Bip01");
            var rootRotation = root.eulerAngles;

            root.rotation = Quaternion.Euler(
                360f - (rootRotation.x + 270f) - 270f,
                360f - (rootRotation.y + 90f) - 90f,
                360f - rootRotation.z);
        }

        static void MirrorSpine(IEnumerable<(Transform Bone, Vector3 Rotation)> spineBonesAndRotations)
        {
            foreach (var (bone, rotation) in spineBonesAndRotations)
                bone.rotation = MirrorRotation(rotation);
        }

        static void MirrorLimbs(
            IEnumerable<((Transform Bone, Vector3 Rotation) Left, (Transform Bone, Vector3 Rotation) Right)> bonePairsAndRotations)
        {
            foreach (var (left, right) in bonePairsAndRotations)
                (left.Bone.rotation, right.Bone.rotation) = (MirrorRotation(right.Rotation), MirrorRotation(left.Rotation));
        }

        void MirrorHandsAndFeet()
        {
            var handLeft = LeftHand.GetPresetData();
            var handRight = RightHand.GetPresetData();
            var footLeft = LeftFoot.GetPresetData();
            var footRight = RightFoot.GetPresetData();

            LeftHand.ApplyPreset(handRight);
            RightHand.ApplyPreset(handLeft);
            LeftFoot.ApplyPreset(footRight);
            RightFoot.ApplyPreset(footLeft);
        }
    }

    public void CopyPoseFrom(CharacterController other)
    {
        _ = other ?? throw new ArgumentNullException(nameof(other));

        if (other == character)
            return;

        StopAnimation();

        ApplyAnimationFrameBinary(other.IK.GetAnimationFrameData());

        ApplyLimits();

        Dirty = true;
    }

    public void CopyHandOrFootFrom(CharacterController copyTarget, HandOrFootType copyWhat, HandOrFootType copyTo)
    {
        _ = copyTarget ?? throw new ArgumentNullException(nameof(copyTarget));

        if (copyWhat is HandOrFootType.HandLeft or HandOrFootType.HandRight && copyTo is not (HandOrFootType.HandLeft or HandOrFootType.HandRight))
            throw new ArgumentException($"{nameof(copyWhat)} is not compatible with {nameof(copyTo)}");

        var copyHandOrFoot = GetControllerByType(copyTarget.IK, copyWhat);
        var targetHandOrFoot = GetControllerByType(this, copyTo);

        StopAnimation();

        targetHandOrFoot.ApplyPreset(copyHandOrFoot.GetPresetData());

        ApplyLimits();

        Dirty = true;
    }

    public void SwapHands()
    {
        StopAnimation();

        var leftPreset = LeftHand.GetPresetData();
        var rightPreset = RightHand.GetPresetData();

        LeftHand.ApplyPreset(rightPreset);
        RightHand.ApplyPreset(leftPreset);

        ApplyLimits();

        Dirty = true;
    }

    public (bool MuneL, bool MuneR) ApplyAnimationFrameBinary(byte[] data)
    {
        _ = data ?? throw new ArgumentNullException(nameof(data));

        StopAnimation();

        var (muneL, muneR) = CacheBoneData.SetFrameBinary(data);

        ApplyLimits();

        Dirty = true;

        return (muneL, muneR);
    }

    public byte[] GetAnimationData() =>
        CacheBoneData.GetAnmBinary(true, true);

    public byte[] GetAnimationFrameData() =>
        CacheBoneData.GetFrameBinary(!MuneLEnabled, !MuneREnabled);

    public HandOrFootPreset GetHandOrFootPreset(HandOrFootType type) =>
        GetControllerByType(this, type).GetPresetData();

    private static HandController GetControllerByType(IKController ikController, HandOrFootType type) =>
        type switch
        {
            HandOrFootType.HandLeft => ikController.LeftHand,
            HandOrFootType.HandRight => ikController.RightHand,
            HandOrFootType.FootLeft => ikController.LeftFoot,
            HandOrFootType.FootRight => ikController.RightFoot,
            _ => throw new InvalidEnumArgumentException(nameof(type), (int)type, typeof(HandOrFootType)),
        };

    private void OnCharacterProcessing(object sender, CharacterProcessingEventArgs e)
    {
        var bodyMpn = (MPN)Enum.Parse(typeof(MPN), nameof(MPN.body));

        if (!e.ChangingSlots.Contains(bodyMpn))
            return;

        foreach (var rotationLimit in rotationLimitCache.Values)
            Object.Destroy(rotationLimit);

        boneCache = [];

        rotationLimitCache = [];
        digitRotationLimits = [];
        limbRotationLimits = [];
    }

    private void InitializeRotationLimits()
    {
        if (rotationLimitCache?.Count > 0)
            return;

        rotationLimitCache = InitializeRotationLimits();

        Dictionary<string, RotationLimit> InitializeRotationLimits()
        {
            var cache = new Dictionary<string, RotationLimit>();

            InitializeLimbRotationLimits(cache);
            InitializeDigitRotationLimits(cache);

            return cache;

            void InitializeLimbRotationLimits(Dictionary<string, RotationLimit> cache)
            {
                var limbBoneNames = new[]
                {
                    "Bip01 L Forearm", "Bip01 R Forearm", "Bip01 L Calf", "Bip01 R Calf",
                };

                limbRotationLimits = new ToggleableRotationLimitHinge[limbBoneNames.Length];

                foreach (var (i, bone) in limbBoneNames.WithIndex())
                    cache[bone] = limbRotationLimits[i] = InitializeHinge(GetBone(bone));
            }

            void InitializeDigitRotationLimits(Dictionary<string, RotationLimit> cache)
            {
                var digitBoneNames = new List<string>();

                var digits = new[]
                {
                    "Bip01 ? Finger02", "Bip01 ? Finger12", "Bip01 ? Finger22", "Bip01 ? Finger32", "Bip01 ? Finger42",
                    "Bip01 ? Toe01", "Bip01 ? Toe11", "Bip01 ? Toe21",
                };

                foreach (var digit in digits)
                {
                    var leftJoint = GetBone(digit.Replace('?', 'L'));
                    var rightJoint = GetBone(digit.Replace('?', 'R'));
                    var jointCount = digit.Contains("Finger") ? 2 : 1;

                    for (var i = jointCount; i > 0; --i)
                    {
                        digitBoneNames.Add(leftJoint.name);
                        digitBoneNames.Add(rightJoint.name);

                        leftJoint = leftJoint.parent;
                        rightJoint = rightJoint.parent;
                    }
                }

                digitRotationLimits = new ToggleableRotationLimitHinge[digitBoneNames.Count];

                foreach (var (i, bone) in digitBoneNames.WithIndex())
                    cache[bone] = digitRotationLimits[i] = InitializeHinge(GetBone(bone));
            }

            ToggleableRotationLimitHinge InitializeHinge(Transform bone)
            {
                var backupRotation = bone.localRotation;

                bone.localRotation = Quaternion.identity;

                var jointIsDigit = bone.name.Contains("Finger") || bone.name.Contains("Toe");
                var rotationLimit = bone.GetOrAddComponent<ToggleableRotationLimitHinge>();

                rotationLimit.Limited = jointIsDigit ? LimitDigitRotations : LimitLimbRotations;

                rotationLimit.axis = jointIsDigit ? Vector3.back : Vector3.forward;
                rotationLimit.min = jointIsDigit ? -180f : 0f;
                rotationLimit.max = 180f;

                bone.localRotation = backupRotation;

                return rotationLimit;
            }
        }
    }

    private bool ApplyLimits()
    {
        if (!LimitDigitRotations || !LimitDigitRotations)
            return false;

        InitializeRotationLimits();

        var changed = false;

        if (LimitLimbRotations)
            foreach (var hinge in limbRotationLimits)
                changed |= hinge.Apply();

        if (LimitDigitRotations)
            foreach (var hinge in digitRotationLimits)
                changed |= hinge.Apply();

        return changed;
    }

    private bool SetLimits(bool value, bool digits)
    {
        if (digits && limitDigitRotations == value || limitLimbRotations == value)
            return false;

        if (digits)
            limitDigitRotations = value;
        else
            limitLimbRotations = value;

        InitializeRotationLimits();

        Solver.useRotationLimits = limitLimbRotations || limitDigitRotations;

        var change = false;

        foreach (var hinge in digits ? digitRotationLimits : limbRotationLimits)
        {
            hinge.Limited = value;

            if (value)
                change |= hinge.Apply();
        }

        Dirty = false;

        RaisePropertyChanged(digits ? nameof(LimitDigitRotations) : nameof(LimitLimbRotations));

        return change && !character.Animation.Playing;
    }

    private void StopAnimation() =>
        character.Animation.Playing = false;

    private void RaisePropertyChanged(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

        PropertyChanged?.Invoke(this, new(name));
    }
}
