using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using RootMotion.FinalIK;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class IKLockController
{
    private readonly CharacterController character;
    private readonly string endBoneName;

    private Transform[] chain;
    private FABRIK fabrik;
    private Transform ikTarget;
    private Transform endBone;
    private bool lockRotation;
    private Quaternion lockedRotation = Quaternion.identity;

    public IKLockController(CharacterController character, string endBoneName)
    {
        this.character = character ?? throw new ArgumentNullException(nameof(character));
        this.endBoneName = string.IsNullOrEmpty(endBoneName)
            ? throw new ArgumentException($"'{nameof(endBoneName)}' cannot be null or empty.", nameof(endBoneName))
            : endBoneName;

        if (!character.IK.GetBone(endBoneName))
            throw new ArgumentException($"Bone '{endBoneName}' does not exist");

        this.character.ProcessingCharacterProps += OnCharacterProcessing;

        IK.ChangedLimbLimiting += OnLimbLimitingChanged;
        Animation.ChangedPlayState += OnPlayStateChanged;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public IEnumerable<Transform> Chain
    {
        get
        {
            if (chain is not null)
                return chain;

            var bone = IK.GetBone(endBoneName);

            return chain = [bone.parent.parent, bone.parent, bone];
        }
    }

    public bool LockPosition
    {
        get => Solver.IKPositionWeight is 1f;
        set
        {
            var newValue = Convert.ToSingle(value);

            if (Solver.IKPositionWeight == newValue)
                return;

            Solver.OnPostUpdate -= OnSolverPostUpdate;
            Solver.OnPreUpdate -= OnSolverPreUpdate;

            if (value)
            {
                Animation.Playing = false;
                ikTarget.position = EndBone.position;
                Solver.OnPostUpdate += OnSolverPostUpdate;
                Solver.OnPreUpdate += OnSolverPreUpdate;
            }

            Solver.IKPositionWeight = newValue;

            PropertyChanged?.Invoke(this, new(nameof(LockPosition)));
        }
    }

    public bool LockRotation
    {
        get => lockRotation;
        set
        {
            if (lockRotation == value)
                return;

            lockRotation = value;
            lockedRotation = EndBone.rotation;

            IK.OnLateUpdate -= OnBodyLateUpdate;

            if (value)
            {
                Animation.Playing = false;
                IK.OnLateUpdate += OnBodyLateUpdate;
            }

            PropertyChanged?.Invoke(this, new(nameof(LockRotation)));
        }
    }

    private IKSolverFABRIK Solver
    {
        get
        {
            if (fabrik)
                return fabrik.solver;

            Transform[] bones = [.. Chain];

            fabrik = bones[0].gameObject.GetOrAddComponent<FABRIK>();
            fabrik.fixTransforms = false;

            fabrik.solver.useRotationLimits = IK.LimitLimbRotations;
            fabrik.solver.maxIterations = 1;
            fabrik.solver.bones = [];
            fabrik.solver.SetChain(bones, character.Maid.body0.trBip);

            fabrik.solver.target = IKTarget;
            fabrik.solver.IKPositionWeight = 0f;

            return fabrik.solver;
        }
    }

    private IKController IK =>
        character.IK;

    private AnimationController Animation =>
        character.Animation;

    private Transform IKTarget =>
        ikTarget ? ikTarget : ikTarget = IK.GetIKSolverTarget(endBoneName);

    private Transform EndBone =>
        endBone ? endBone : endBone = IK.GetBone(endBoneName);

    public void UpdateLockedRotation() =>
        lockedRotation = EndBone.rotation;

    public void SetAllLocksEnabled(bool enabled) =>
        LockPosition = LockRotation = enabled;

    internal void Dispose()
    {
        IK.OnLateUpdate -= OnBodyLateUpdate;
        IK.ChangedLimbLimiting -= OnLimbLimitingChanged;
        Animation.ChangedPlayState -= OnPlayStateChanged;
        character.ProcessingCharacterProps -= OnCharacterProcessing;
        PropertyChanged = null;

        DestroyFABRIK();
    }

    private void OnPlayStateChanged(object sender, EventArgs e)
    {
        if (sender is not AnimationController { Playing: true })
            return;

        LockPosition = LockRotation = false;
    }

    private void OnLimbLimitingChanged(object sender, EventArgs e) =>
        Solver.useRotationLimits = IK.LimitLimbRotations;

    private void OnCharacterProcessing(object sender, CharacterProcessingEventArgs e)
    {
        if (!e.ChangingSlots.Contains(SafeMpn.body))
            return;

        chain = null;
        DestroyFABRIK();

        character.ProcessedCharacterProps += OnCharacterProcessed;

        void OnCharacterProcessed(object sender, CharacterProcessingEventArgs e)
        {
            character.ProcessedCharacterProps -= OnCharacterProcessed;

            PropertyChanged?.Invoke(this, new(nameof(LockPosition)));
            LockRotation = false;
        }
    }

    private void DestroyFABRIK()
    {
        if (!fabrik)
            return;

        fabrik.solver.target = null;
        fabrik.solver.OnPostUpdate -= OnSolverPostUpdate;
        fabrik.solver.OnPreUpdate -= OnSolverPreUpdate;

        Object.Destroy(fabrik);
    }

    private void OnSolverPreUpdate()
    {
        if (Animation.Playing)
            LockPosition = false;
    }

    private void OnSolverPostUpdate() =>
        IK.FixLocalPositions();

    private void OnBodyLateUpdate()
    {
        if (!lockRotation)
            return;

        EndBone.rotation = lockedRotation;
    }
}
