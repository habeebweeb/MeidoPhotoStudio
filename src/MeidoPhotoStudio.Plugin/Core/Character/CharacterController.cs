using System;
using System.Collections.Generic;
using System.Linq;

using MeidoPhotoStudio.Database.Character;
using MeidoPhotoStudio.Plugin.Core.Character.Pose;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class CharacterController(CharacterModel characterModel)
{
    private IEnumerable<MPN> processingProps = [];
    private bool initialized;
    private bool subscribedToSequenceEvents;

    public event EventHandler<TransformChangeEventArgs> ChangedTransform;

    public event EventHandler<CharacterProcessingEventArgs> ProcessingCharacterProps;

    public event EventHandler<CharacterProcessingEventArgs> ProcessedCharacterProps;

    public CharacterModel CharacterModel { get; } = characterModel
        ?? throw new ArgumentNullException(nameof(characterModel));

    public AnimationController Animation { get; private set; }

    public IKController IK { get; private set; }

    public HeadController Head { get; private set; }

    public FaceController Face { get; private set; }

    public ClothingController Clothing { get; private set; }

    public int Slot { get; private set; }

    public Maid Maid =>
        CharacterModel.Maid;

    public GameObject GameObject =>
        Maid.gameObject;

    public Transform Transform =>
        Maid.transform;

    public bool Busy =>
        Maid.IsBusy && !initialized;

    public string ID =>
        CharacterModel.ID;

    public Transform GetBone(string boneName)
    {
        if (Busy)
            throw new InvalidOperationException("Maid is busy");

        if (!Maid.body0.isLoadedBody)
            throw new InvalidOperationException("Body is not loaded");

        return Maid.body0.GetBone(boneName);
    }

    public void FocusOnBody()
    {
        if (!Maid.Visible || !Maid.body0.isLoadedBody)
            return;

        var bodyTransform = GetBone("Bip01");

        var bodyPosition = bodyTransform.position;
        var bodyDistance = Mathf.Max(GameMain.Instance.MainCamera.GetDistance(), 3f);
        var cameraRotation = GameMain.Instance.MainCamera.transform.eulerAngles;
        var bodyAngle = new Vector2(cameraRotation.y, cameraRotation.x);

        WfCameraMoveSupportUtility.StartMove(bodyPosition, bodyDistance, bodyAngle);
    }

    public void FocusOnFace()
    {
        if (!Maid.Visible || !Maid.body0.isLoadedBody)
            return;

        var head = GetBone("Bip01 Head");

        var facePosition = head.position;
        var faceRotation = (head.rotation * Quaternion.Euler(Vector3.right * 90f)).eulerAngles;
        var faceAngle = new Vector2(faceRotation.y, faceRotation.x);

        WfCameraMoveSupportUtility.StartMove(facePosition, 1f, faceAngle);
    }

    public override string ToString() =>
        characterModel.ToString();

    internal void Load(int slot)
    {
        Slot = slot;
        Maid.ActiveSlotNo = slot;

        if (!subscribedToSequenceEvents)
        {
            AllProcPropSeqPatcher.SequenceStarting += OnSequenceStarting;
            AllProcPropSeqPatcher.SequenceEnded += OnSequenceEnded;

            subscribedToSequenceEvents = true;
        }

        if (!Maid.body0.isLoadedBody)
        {
            Maid.Visible = true;

            ProcessedCharacterProps += OnBodyInitialized;

            Maid.DutPropAll();
            Maid.AllProcPropSeqStart();

            void OnBodyInitialized(object sender, EventArgs e)
            {
                PostLoad();

                ProcessedCharacterProps -= OnBodyInitialized;
            }
        }
        else
        {
            if (initialized && Maid.Visible)
                return;

            Maid.Visible = true;

            PostLoad();
        }

        void PostLoad()
        {
            if (Animation is null)
            {
                Animation = new(this);
                Animation.Apply(new GameAnimationModel("normal", "maid_stand01"));
            }

            if (Face is null)
            {
                Face = new(this);
                Face.ApplyBlendSet(new GameBlendSetModel(PhotoFaceData.data[0]));
            }

            IK ??= new(this);
            Head ??= new(this);
            Clothing ??= new(this);

            Head.FreeLook = false;
            Head.HeadToCamera = true;
            Head.EyeToCamera = true;

            Face.Blink = true;

            Clothing.CustomFloorHeight = false;

            initialized = true;
        }
    }

    internal void Unload()
    {
        Maid.Visible = false;
        Slot = -1;
        Maid.ActiveSlotNo = -1;
    }

    internal void Deactivate(bool keepLoaded = false)
    {
        AllProcPropSeqPatcher.SequenceStarting -= OnSequenceStarting;
        AllProcPropSeqPatcher.SequenceEnded -= OnSequenceEnded;

        subscribedToSequenceEvents = false;

        Head?.ResetBothEyeRotations();

        if (Maid.body0)
            Maid.body0.BoneHitHeightY = 0f;

        Maid.SetPos(Vector3.zero);
        Maid.SetRot(Vector3.zero);
        Maid.SetPosOffset(Vector3.zero);
        Maid.transform.localScale = Vector3.one;

        if (keepLoaded)
            return;

        Maid.ResetAll();
        Maid.Uninit();
    }

    internal void UpdateTransform(TransformChangeEventArgs.TransformType type) =>
        ChangedTransform?.Invoke(this, new(type));

    private void OnSequenceStarting(object sender, ProcStartEventArgs e)
    {
        if (!Maid.Visible)
            return;

        if (Maid != e.Maid)
            return;

        var mpnStart = (int)Enum.Parse(typeof(MPN_TYPE_RANGE), nameof(MPN_TYPE_RANGE.BODY_RELOAD_START)) - 1;
        var mpnEnd = (int)Enum.Parse(typeof(MPN_TYPE_RANGE), nameof(MPN_TYPE_RANGE.WEAR_END));

        processingProps = Enumerable.Range(mpnStart, mpnEnd - mpnStart + 1)
            .Select(mpn => (MPN)mpn)
            .Where(mpn => Maid.GetProp(mpn).boDut)
            .ToArray();

        ProcessingCharacterProps?.Invoke(this, new(processingProps));
    }

    private void OnSequenceEnded(object sender, ProcStartEventArgs e)
    {
        if (!Maid.Visible)
            return;

        if (Maid != e.Maid)
            return;

        ProcessedCharacterProps?.Invoke(this, new(processingProps));
    }
}
