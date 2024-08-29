using MeidoPhotoStudio.Database.Character;
using MeidoPhotoStudio.Plugin.Core.Character.Pose;
using MeidoPhotoStudio.Plugin.Core.UndoRedo;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class CharacterUndoRedoController(CharacterController characterController, UndoRedoService undoRedoService) : UndoRedoControllerBase(undoRedoService)
{
    private readonly CharacterController characterController = characterController ?? throw new ArgumentNullException(nameof(characterController));

    private ITransactionalUndoRedo<PoseBackup> poseController;

    private ITransactionalUndoRedo<PoseBackup> PoseController =>
        poseController ??= MakeCustomTransactionalUndoRedo(
            () => new PoseBackup(characterController.IK, characterController.Head, characterController.Animation),
            backup => backup.Apply(characterController.IK, characterController.Head, characterController.Animation));

    public void StartPoseChange() =>
        PoseController.StartChange();

    public void EndPoseChange() =>
        PoseController.EndChange();

    public void CancelPoseChange() =>
        PoseController.Cancel();

    private readonly struct PoseBackup : IValueBackup<IKController>
    {
        private readonly byte[] backupData;

        public PoseBackup(IKController ik, HeadController head, AnimationController animation)
        {
            _ = ik ?? throw new ArgumentNullException(nameof(ik));
            _ = head ?? throw new ArgumentNullException(nameof(head));
            _ = animation ?? throw new ArgumentNullException(nameof(animation));

            backupData = CreateBackupData(ik, head, animation);

            static byte[] CreateBackupData(IKController ik, HeadController head, AnimationController animation)
            {
                using var memoryStream = new MemoryStream();
                using var writer = new BinaryWriter(memoryStream);

                WriteAnimationModel(writer, animation.Animation);
                WriteAnimationSetting(writer, animation);
                WriteMuneSetting(writer, ik);
                WriteIKLimitSetting(writer, ik);
                writer.Write(ik.Dirty);

                if (!animation.Playing)
                    foreach (var bone in GetBackupBones())
                    {
                        if (bone is "Mune_L" or "Mune_L_sub" && ik.MuneLEnabled
                            || bone is "Mune_R" or "Mune_R_sub" && ik.MuneREnabled
                            || bone is "Bip01 Head" && head.HeadToCamera)
                            continue;

                        var transform = ik.GetBone(bone);

                        if (!transform)
                            continue;

                        writer.Write(true);
                        writer.Write(bone);
                        writer.WriteQuaternion(transform.localRotation);

                        if (bone == "Bip01")
                            writer.WriteVector3(transform.localPosition);
                    }

                writer.Write(false);

                return memoryStream.ToArray();

                static void WriteAnimationModel(BinaryWriter writer, IAnimationModel animation)
                {
                    writer.Write(animation.Custom);
                    writer.Write(animation.Category);
                    writer.Write(animation.Filename);

                    if (animation is CustomAnimationModel customAnimation)
                        writer.Write(customAnimation.ID);
                }

                static void WriteAnimationSetting(BinaryWriter writer, AnimationController animation)
                {
                    writer.Write(animation.Playing);
                    writer.Write(animation.Time);
                }

                static void WriteMuneSetting(BinaryWriter writer, IKController ik)
                {
                    writer.Write(ik.MuneLEnabled);
                    writer.Write(ik.MuneREnabled);
                }

                static void WriteIKLimitSetting(BinaryWriter writer, IKController ik)
                {
                    writer.Write(ik.LimitLimbRotations);
                    writer.Write(ik.LimitDigitRotations);
                }
            }
        }

        public void Apply(IKController ik, HeadController head, AnimationController animation)
        {
            _ = ik ?? throw new ArgumentNullException(nameof(ik));
            _ = head ?? throw new ArgumentNullException(nameof(head));

            var (animationModel, animationSetting, muneSetting, limitSetting, dirty, boneSetting) = DeserializeData(backupData);

            animation.Apply(animationModel);

            animation.Playing = animationSetting.Playing;
            animation.Time = animationSetting.Time;

            ik.MuneLEnabled = muneSetting.MuneL;
            ik.MuneREnabled = muneSetting.MuneR;

            ik.LimitLimbRotations = limitSetting.LimitLimbs;
            ik.LimitDigitRotations = limitSetting.LimitDigits;

            if (!animation.Playing)
                foreach (var (bone, rotation, position) in boneSetting)
                {
                    var transform = ik.GetBone(bone);

                    if (!transform)
                        continue;

                    transform.localRotation = rotation;

                    if (bone is "Bip01")
                        transform.localPosition = position;
                }

            ik.Dirty = dirty;
        }

        public void Apply(IKController @object) => throw new NotImplementedException();

        public bool Equals(IValueBackup<IKController> other) =>
            other is PoseBackup backup && Equals(backup);

        public override bool Equals(object other) =>
            other is PoseBackup backup && Equals(backup);

        public bool Equals(PoseBackup other)
        {
            return backupData is null
                ? other.backupData is null
                : other.backupData is not null && backupData.SequenceEqual(other.backupData);

#pragma warning disable CS8321
            static void CompareData(byte[] aData, byte[] bData)
#pragma warning restore CS8321
            {
                if (aData is null || bData is null)
                {
                    Utility.LogDebug("Data are null");

                    return;
                }

                if (aData.Length != bData.Length)
                {
                    Utility.LogDebug("data lengths different");

                    return;
                }

                var aBackup = DeserializeData(aData);
                var bBackup = DeserializeData(bData);

                if (!aBackup.AnimationModel.Equals(bBackup.AnimationModel))
                {
                    Utility.LogDebug($"""
                        Animation models differ
                        A: {aBackup.AnimationModel}
                        B: {bBackup.AnimationModel}
                        """);

                    return;
                }

                if (aBackup.AnimationSetting != bBackup.AnimationSetting)
                {
                    Utility.LogDebug($"""
                        AnimationSettings differ
                        A: {aBackup.AnimationSetting}
                        B: {bBackup.AnimationSetting}
                        """);

                    return;
                }

                if (aBackup.MuneSetting != bBackup.MuneSetting)
                {
                    Utility.LogDebug($"""
                        mune setting differs
                        A: {aBackup.MuneSetting}
                        B: {bBackup.MuneSetting}
                        """);

                    return;
                }

                if (aBackup.LimitSetting != bBackup.LimitSetting)
                {
                    Utility.LogDebug($"""
                        limit setting differs
                        A: {aBackup.LimitSetting}
                        B: {bBackup.LimitSetting}
                        """);

                    return;
                }

                if (aBackup.Dirty != bBackup.Dirty)
                {
                    Utility.LogDebug($"""
                        dirty differs
                        A: {aBackup.Dirty}
                        B: {bBackup.Dirty}
                        """);

                    return;
                }

                if (aBackup.IKData.Count() != bBackup.IKData.Count())
                {
                    Utility.LogDebug($"""
                        bone counts differs");
                        A: {aBackup.IKData.Count()}
                        B: {bBackup.IKData.Count()}
                       """);

                    return;
                }

                foreach (var (aIKData, bIKData) in aBackup.IKData.Zip(bBackup.IKData))
                {
                    if (!string.Equals(aIKData.Item1, bIKData.Item1, StringComparison.Ordinal))
                    {
                        Utility.LogDebug($"""
                            bone names differ
                            A: {aIKData.Item1}
                            B: {bIKData.Item1}
                            """);

                        return;
                    }

                    if (aIKData.Item2 != bIKData.Item2)
                    {
                        Utility.LogDebug($"""
                            {aIKData.Item1} rotation differ
                            A: {aIKData.Item2}
                            B: {bIKData.Item2}
                            """);
                    }
                }
            }
        }

        public override int GetHashCode() =>
            810838655 + EqualityComparer<byte[]>.Default.GetHashCode(backupData);

        private static string[] GetBackupBones() =>
            ["Bip01", "Bip01 Footsteps", "Bip01 Pelvis", "Bip01 L Thigh", "Bip01 L Calf", "Bip01 L Foot", "Bip01 L Toe0",
            "Bip01 L Toe01", "Bip01 L Toe1", "Bip01 L Toe11", "Bip01 L Toe2", "Bip01 L Toe21", "Bip01 R Thigh",
            "Bip01 R Calf", "Bip01 R Foot", "Bip01 R Toe0", "Bip01 R Toe01", "Bip01 R Toe1", "Bip01 R Toe11",
            "Bip01 R Toe2", "Bip01 R Toe21", "Bip01 Spine", "Bip01 Spine0a", "Bip01 Spine1", "Bip01 Spine1a",
            "Bip01 L Clavicle", "Bip01 L UpperArm", "Bip01 L Forearm", "Bip01 L Hand", "Bip01 L Finger0",
            "Bip01 L Finger01", "Bip01 L Finger02", "Bip01 L Finger1", "Bip01 L Finger11", "Bip01 L Finger12",
            "Bip01 L Finger2", "Bip01 L Finger21", "Bip01 L Finger22", "Bip01 L Finger3", "Bip01 L Finger31",
            "Bip01 L Finger32", "Bip01 L Finger4", "Bip01 L Finger41", "Bip01 L Finger42", "Bip01 Neck",
            "Bip01 Head",
            "Bip01 R Clavicle", "Bip01 R UpperArm", "Bip01 R Forearm", "Bip01 R Hand", "Bip01 R Finger0",
            "Bip01 R Finger01", "Bip01 R Finger02", "Bip01 R Finger1", "Bip01 R Finger11", "Bip01 R Finger12",
            "Bip01 R Finger2", "Bip01 R Finger21", "Bip01 R Finger22", "Bip01 R Finger3", "Bip01 R Finger31",
            "Bip01 R Finger32", "Bip01 R Finger4", "Bip01 R Finger41", "Bip01 R Finger42",
            "Mune_L", "Mune_L_sub", "Mune_R", "Mune_R_sub"];

        private static BackupData DeserializeData(byte[] backupData)
        {
            using var memoryStream = new MemoryStream(backupData);
            using var reader = new BinaryReader(memoryStream);

            var animationModel = ReadAnimationModel(reader);
            var animationSetting = ReadAnimationSetting(reader);
            var muneSetting = ReadMuneSetting(reader);
            var limitSetting = ReadIKLimitSetting(reader);
            var dirty = reader.ReadBoolean();

            var ikData = new List<(string, Quaternion, Vector3)>();

            while (reader.ReadBoolean())
            {
                var bone = reader.ReadString();
                var rotation = reader.ReadQuaternion();
                var position = Vector3.zero;

                if (bone is "Bip01")
                    position = reader.ReadVector3();

                ikData.Add((bone, rotation, position));
            }

            return new(animationModel, animationSetting, muneSetting, limitSetting, dirty, ikData);

            static IAnimationModel ReadAnimationModel(BinaryReader reader)
            {
                var custom = reader.ReadBoolean();
                var category = reader.ReadString();
                var filename = reader.ReadString();

                return custom
                    ? new CustomAnimationModel(reader.ReadInt64(), category, filename)
                    : new GameAnimationModel(category, filename);
            }

            static (bool Playing, float Time) ReadAnimationSetting(BinaryReader reader) =>
                (reader.ReadBoolean(), reader.ReadSingle());

            static (bool MuneL, bool MuneR) ReadMuneSetting(BinaryReader reader) =>
                (reader.ReadBoolean(), reader.ReadBoolean());

            static (bool LimitLimbs, bool LimitDigits) ReadIKLimitSetting(BinaryReader reader) =>
                (reader.ReadBoolean(), reader.ReadBoolean());
        }

        private readonly record struct BackupData(
            IAnimationModel AnimationModel,
            (bool Playing, float Time) AnimationSetting,
            (bool MuneL, bool MuneR) MuneSetting,
            (bool LimitLimbs, bool LimitDigits) LimitSetting,
            bool Dirty,
            IEnumerable<(string, Quaternion, Vector3)> IKData);
    }

    private readonly record struct EyeRotationBackup(Quaternion LeftEyeDelta, Quaternion RightEyeDelta)
        : IValueBackup<HeadController>
    {
        public EyeRotationBackup(HeadController controller)
            : this(controller.LeftEyeRotationDelta, controller.RightEyeRotationDelta)
        {
        }

        public void Apply(HeadController controller) =>
            (controller.LeftEyeRotationDelta, controller.RightEyeRotationDelta) = (LeftEyeDelta, RightEyeDelta);

        public bool Equals(IValueBackup<HeadController> other) =>
            other is EyeRotationBackup backup && Equals(backup);
    }

    private readonly struct ClothingBackup : IEquatable<ClothingBackup>
    {
        private readonly Dictionary<SlotID, bool> clothingBackup;
        private readonly TBody.MaskMode dressingMode;

        public ClothingBackup(ClothingController clothing)
        {
            _ = clothing ?? throw new ArgumentNullException(nameof(clothing));

            clothingBackup = ClothingSlots.ToDictionary(slot => slot, slot => clothing[slot]);
            dressingMode = clothing.DressingMode;
        }

        private static IEnumerable<SlotID> ClothingSlots =>
            new[]
            {
                SlotID.wear, SlotID.mizugi, SlotID.onepiece, SlotID.skirt, SlotID.bra, SlotID.panz, SlotID.headset,
                SlotID.megane, SlotID.accUde, SlotID.glove, SlotID.accSenaka, SlotID.stkg,
                SlotID.shoes, SlotID.accAshi, SlotID.accHana, SlotID.accHat, SlotID.accHeso, SlotID.accKamiSubL,
                SlotID.accKamiSubR, SlotID.accKami_1_, SlotID.accKami_2_, SlotID.accKami_3_, SlotID.accKubi,
                SlotID.accKubiwa, SlotID.accMiMiL, SlotID.accMiMiR, SlotID.accNipL, SlotID.accNipR,
                SlotID.accShippo, SlotID.accXXX,
            };

        public void Apply(ClothingController clothing)
        {
            _ = clothing ?? throw new ArgumentNullException(nameof(clothing));

            clothing.DressingMode = dressingMode;

            foreach (var (slot, enabled) in clothingBackup)
                clothing[slot] = enabled;
        }

        public override bool Equals(object other) =>
            other is ClothingBackup backup && Equals(backup);

        public bool Equals(ClothingBackup other)
        {
            var backup = clothingBackup;

            return dressingMode == other.dressingMode
                && (clothingBackup is null
                    ? other.clothingBackup is null
                    : other.clothingBackup is not null
                        && clothingBackup.Count == other.clothingBackup.Count
                        && backup.Keys.All(key => backup[key] == other.clothingBackup[key]));
        }

        public override int GetHashCode() =>
            (clothingBackup, dressingMode).GetHashCode();
    }
}
