using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class MeidoDragPointManager
{
    private static readonly Dictionary<AttachPoint, Bone> PointToBone = new()
    {
        [AttachPoint.Head] = Bone.Head,
        [AttachPoint.Neck] = Bone.HeadNub,
        [AttachPoint.UpperArmL] = Bone.UpperArmL,
        [AttachPoint.UpperArmR] = Bone.UpperArmR,
        [AttachPoint.ForearmL] = Bone.ForearmL,
        [AttachPoint.ForearmR] = Bone.ForearmR,
        [AttachPoint.MuneL] = Bone.MuneL,
        [AttachPoint.MuneR] = Bone.MuneR,
        [AttachPoint.HandL] = Bone.HandL,
        [AttachPoint.HandR] = Bone.HandR,
        [AttachPoint.Pelvis] = Bone.Pelvis,
        [AttachPoint.ThighL] = Bone.ThighL,
        [AttachPoint.ThighR] = Bone.ThighR,
        [AttachPoint.CalfL] = Bone.CalfL,
        [AttachPoint.CalfR] = Bone.CalfR,
        [AttachPoint.FootL] = Bone.FootL,
        [AttachPoint.FootR] = Bone.FootR,
        [AttachPoint.Spine1a] = Bone.Spine1a,
        [AttachPoint.Spine1] = Bone.Spine1,
        [AttachPoint.Spine0a] = Bone.Spine0a,
        [AttachPoint.Spine0] = Bone.Spine,
    };

    private static readonly Bone[] SpineBones =
    {
        Bone.Neck, Bone.Spine, Bone.Spine0a, Bone.Spine1, Bone.Spine1a, Bone.Hip, Bone.ThighL, Bone.ThighR,
    };

    private static bool cubeActive;
    private static bool cubeSmall;

    private static EventHandler cubeActiveChange;

    private static EventHandler cubeSmallChange;

    private readonly Meido meido;
    private readonly Dictionary<Bone, DragPointMeido> dragPoints = new();

    private Dictionary<Bone, Transform> boneTransform = new();
    private DragPointBody dragBody;
    private DragPointBody dragCube;
    private bool initialized;
    private bool isBone;
    private bool active = true;

    public MeidoDragPointManager(Meido meido) =>
        this.meido = meido;

    public event EventHandler<MeidoUpdateEventArgs> SelectMaid;

    private enum Bone
    {
        // Head
        Head,
        HeadNub,
        ClavicleL,
        ClavicleR,

        // Arms
        UpperArmL,
        UpperArmR,
        ForearmL,
        ForearmR,

        // IKHandL and IKHandR
        HandL,
        HandR,

        // Mune
        MuneL,
        MuneSubL,
        MuneR,
        MuneSubR,

        // Spine
        Neck,
        Spine,
        Spine0a,
        Spine1,
        Spine1a,
        ThighL,
        ThighR,

        // Hip
        Pelvis,
        Hip,

        // Legs
        CalfL,
        CalfR,
        FootL,
        FootR,

        // Dragpoint specific
        Cube,
        Body,
        Torso,

        // Fingers
        Finger0L,
        Finger01L,
        Finger02L,
        Finger0NubL,
        Finger1L,
        Finger11L,
        Finger12L,
        Finger1NubL,
        Finger2L,
        Finger21L,
        Finger22L,
        Finger2NubL,
        Finger3L,
        Finger31L,
        Finger32L,
        Finger3NubL,
        Finger4L,
        Finger41L,
        Finger42L,
        Finger4NubL,
        Finger0R,
        Finger01R,
        Finger02R,
        Finger0NubR,
        Finger1R,
        Finger11R,
        Finger12R,
        Finger1NubR,
        Finger2R,
        Finger21R,
        Finger22R,
        Finger2NubR,
        Finger3R,
        Finger31R,
        Finger32R,
        Finger3NubR,
        Finger4R,
        Finger41R,
        Finger42R,
        Finger4NubR,

        // Toes
        Toe0L,
        Toe01L,
        Toe0NubL,
        Toe1L,
        Toe11L,
        Toe1NubL,
        Toe2L,
        Toe21L,
        Toe2NubL,
        Toe0R,
        Toe01R,
        Toe0NubR,
        Toe1R,
        Toe11R,
        Toe1NubR,
        Toe2R,
        Toe21R,
        Toe2NubR,
    }

    public static bool CubeActive
    {
        get => cubeActive;
        set
        {
            if (value == cubeActive)
                return;

            cubeActive = value;
            cubeActiveChange?.Invoke(null, EventArgs.Empty);
        }
    }

    public static bool CubeSmall
    {
        get => cubeSmall;
        set
        {
            if (value == cubeSmall)
                return;

            cubeSmall = value;
            cubeSmallChange?.Invoke(null, EventArgs.Empty);
        }
    }

    public bool IsBone
    {
        get => isBone;
        set
        {
            if (!initialized)
                return;

            if (isBone == value)
                return;

            isBone = value;

            foreach (var dragPoint in dragPoints.Values)
                dragPoint.IsBone = isBone;

            foreach (var bone in SpineBones)
                dragPoints[bone].gameObject.SetActive(isBone);
        }
    }

    public bool Active
    {
        get => active;
        set
        {
            if (!initialized)
                return;

            if (active == value)
                return;

            active = value;

            foreach (var dragPoint in dragPoints.Values)
                dragPoint.gameObject.SetActive(active);

            foreach (var bone in SpineBones)
                dragPoints[bone].gameObject.SetActive(active && IsBone);

            var head = (DragPointHead)dragPoints[Bone.Head];

            head.gameObject.SetActive(true);
            head.IsIK = !active;
            dragBody.IsIK = !active;
        }
    }

    public void Deserialize(BinaryReader reader)
    {
        var sixtyFourFlag = reader.ReadBoolean();
        var upperBone = sixtyFourFlag ? Bone.Finger4NubR : Bone.Toe2NubR;

        // finger rotations. Toe rotations as well if sixtyFourFlag is false
        for (var bone = Bone.Finger0L; bone <= upperBone; ++bone)
            boneTransform[bone].localRotation = reader.ReadQuaternion();

        var bones = sixtyFourFlag
            ? new[]
            {
                Bone.Pelvis, Bone.Spine, Bone.Spine0a, Bone.Spine1, Bone.Spine1a, Bone.Neck, Bone.UpperArmL,
                Bone.UpperArmR, Bone.ForearmL, Bone.ForearmR, Bone.ThighL, Bone.ThighR, Bone.CalfL, Bone.CalfR,
                Bone.HandL, Bone.HandR, Bone.FootL, Bone.FootR,
            }
            : new[]
            {
                Bone.Hip, Bone.Pelvis, Bone.Spine, Bone.Spine0a, Bone.Spine1, Bone.Spine1a, Bone.Neck,
                Bone.ClavicleL, Bone.ClavicleR, Bone.UpperArmL, Bone.UpperArmR, Bone.ForearmL, Bone.ForearmR,
                Bone.ThighL, Bone.ThighR, Bone.CalfL, Bone.CalfR, Bone.MuneL, Bone.MuneR, Bone.MuneSubL,
                Bone.MuneSubR, Bone.HandL, Bone.HandR, Bone.FootL, Bone.FootR,
            };

        var localRotationIndex = Array.IndexOf(bones, Bone.CalfR);

        for (var i = 0; i < bones.Length; i++)
        {
            var bone = bones[i];

            if (bone is Bone.ClavicleL)
            {
                /*
                 * Versions of MM possibly serialized ClavicleL improperly.
                 * At least I think that's what happened otherwise why would they make this check at all.
                 * https://git.coder.horse/meidomustard/modifiedMM/src/master/MultipleMaids/CM3D2/MultipleMaids/Plugin/MultipleMaids.Update.cs#L4355
                 *
                 * Just look at the way MM serializes rotations.
                 * https://git.coder.horse/meidomustard/modifiedMM/src/master/MultipleMaids/CM3D2/MultipleMaids/Plugin/MultipleMaids.Update.cs#L2364
                 * It is most definitely possible MM dev missed a component.
                 *
                 * Also why is strArray9.Length == 2 acceptable? If the length were only 2,
                 * float.Parse(strArray9[2]) would throw an index out of range exception???
                 */
                if (!reader.ReadBoolean())
                {
                    reader.ReadQuaternion();

                    continue;
                }
            }

            var rotation = reader.ReadQuaternion();

            if (sixtyFourFlag || i > localRotationIndex)
                boneTransform[bone].localRotation = rotation;
            else
                boneTransform[bone].rotation = rotation;
        }

        // WHY????
        GameMain.Instance.StartCoroutine(ApplyHipPosition(reader.ReadVector3()));
    }

    public void Flip()
    {
        meido.Stop = true;

        var single = new[] { Bone.Pelvis, Bone.Spine, Bone.Spine0a, Bone.Spine1, Bone.Spine1a, Bone.Neck };
        var pair = new[]
            {
                Bone.ClavicleL, Bone.ClavicleR, Bone.UpperArmL, Bone.UpperArmR, Bone.ForearmL, Bone.ForearmR,
                Bone.ThighL, Bone.ThighR, Bone.CalfL, Bone.CalfR, Bone.HandL, Bone.HandR, Bone.FootL, Bone.FootR,
            };

        var singleRotations = single.Select(bone => boneTransform[bone].eulerAngles).ToList();
        var pairRotations = pair.Select(bone => boneTransform[bone].eulerAngles).ToList();

        var hip = boneTransform[Bone.Hip];
        var vecHip = hip.eulerAngles;

        var hipL = meido.Maid.body0.GetBone("Hip_L");
        var vecHipL = hipL.eulerAngles;

        var hipR = meido.Maid.body0.GetBone("Hip_R");
        var vecHipR = hipR.eulerAngles;

        hip.rotation =
            Quaternion.Euler(360f - (vecHip.x + 270f) - 270f, 360f - (vecHip.y + 90f) - 90f, 360f - vecHip.z);

        hipL.rotation = FlipRotation(vecHipR);
        hipR.rotation = FlipRotation(vecHipL);

        for (var i = 0; i < single.Length; i++)
        {
            var bone = single[i];

            boneTransform[bone].rotation = FlipRotation(singleRotations[i]);
        }

        for (var i = 0; i < pair.Length; i += 2)
        {
            var boneA = pair[i];
            var boneB = pair[i + 1];

            boneTransform[boneA].rotation = FlipRotation(pairRotations[i + 1]);
            boneTransform[boneB].rotation = FlipRotation(pairRotations[i]);
        }

        var leftHand = SerializeHand(right: false);
        var rightHand = SerializeHand(right: true);

        DeserializeHand(leftHand, right: true, true);
        DeserializeHand(rightHand, right: false, true);

        leftHand = SerializeFoot(right: false);
        rightHand = SerializeFoot(right: true);

        DeserializeFoot(leftHand, right: true, true);
        DeserializeFoot(rightHand, right: false, true);
    }

    public Transform GetAttachPointTransform(AttachPoint point) =>
        point is AttachPoint.None ? null : boneTransform[PointToBone[point]];

    public byte[] SerializeHand(bool right)
    {
        var start = right ? Bone.Finger0R : Bone.Finger0L;
        var end = right ? Bone.Finger4R : Bone.Finger4L;

        return SerializeFinger(start, end);
    }

    public void DeserializeHand(byte[] handBinary, bool right, bool mirroring = false)
    {
        var start = right ? Bone.Finger0R : Bone.Finger0L;
        var end = right ? Bone.Finger4R : Bone.Finger4L;

        DeserializeFinger(start, end, handBinary, mirroring);
    }

    public byte[] SerializeFoot(bool right)
    {
        var start = right ? Bone.Toe0R : Bone.Toe0L;
        var end = right ? Bone.Toe2R : Bone.Toe2L;

        return SerializeFinger(start, end);
    }

    public void DeserializeFoot(byte[] footBinary, bool right, bool mirroring = false)
    {
        var start = right ? Bone.Toe0R : Bone.Toe0L;
        var end = right ? Bone.Toe2R : Bone.Toe2L;

        DeserializeFinger(start, end, footBinary, mirroring);
    }

    public void Destroy()
    {
        foreach (var dragPoint in dragPoints.Values)
            if (dragPoint)
                UnityEngine.Object.Destroy(dragPoint.gameObject);

        if (dragCube)
            UnityEngine.Object.Destroy(dragCube.gameObject);

        if (dragBody)
            UnityEngine.Object.Destroy(dragBody.gameObject);

        boneTransform.Clear();
        dragPoints.Clear();
        cubeActiveChange -= OnCubeActive;
        cubeSmallChange -= OnCubeSmall;
        initialized = false;
    }

    public void Initialize()
    {
        if (initialized)
            return;

        initialized = true;
        cubeActiveChange += OnCubeActive;
        cubeSmallChange += OnCubeSmall;

        InitializeBones();
        InitializeDragPoints();
        SetDragPointScale(meido.Maid.transform.localScale.x);
    }

    public void SetDragPointScale(float scale)
    {
        foreach (var dragPoint in dragPoints.Values)
            dragPoint.DragPointScale = scale;

        dragBody.DragPointScale = scale;
    }

    private static DragPointLimb[] MakeArmChain(Transform lower, Meido meido)
    {
        var limbDragPointSize = Vector3.one * 0.12f;

        var realLower = CMT.SearchObjName(meido.Body.goSlot[0].obj_tr, lower.name, false);

        var dragPoints = new DragPointLimb[3];

        for (var i = dragPoints.Length - 1; i >= 0; i--)
        {
            var joint = lower;
            var positionJoint = realLower;

            dragPoints[i] = DragPoint.Make<DragPointLimb>(PrimitiveType.Sphere, limbDragPointSize);
            dragPoints[i].Initialize(meido, () => positionJoint.position, () => Vector3.zero);
            dragPoints[i].Set(joint);
            dragPoints[i].AddGizmo();
            dragPoints[i].Gizmo.SetAlternateTarget(positionJoint);

            lower = lower.parent;
            realLower = realLower.parent;
        }

        return dragPoints;
    }

    private static DragPointFinger[] MakeFingerChain(Transform lower, Meido meido)
    {
        var fingerDragPointSize = Vector3.one * 0.01f;

        var dragPoints = new DragPointFinger[3];

        var realLower = CMT.SearchObjName(meido.Body.goSlot[0].obj_tr, lower.parent.name, false);

        for (var i = dragPoints.Length - 1; i >= 0; i--)
        {
            var joint = lower;
            var positionJoint = realLower;

            dragPoints[i] = DragPoint.Make<DragPointFinger>(PrimitiveType.Sphere, fingerDragPointSize);
            dragPoints[i].Initialize(meido, () => positionJoint.position, () => Vector3.zero);
            dragPoints[i].Set(joint);

            lower = lower.parent;
            realLower = realLower.parent;
        }

        return dragPoints;
    }

    /*
       Somebody smarter than me please help me find a way to do this better T_T
       inb4 for loop.
       */
    private System.Collections.IEnumerator ApplyHipPosition(Vector3 hipPosition)
    {
        boneTransform[Bone.Hip].position = hipPosition;

        yield return new WaitForEndOfFrame();

        boneTransform[Bone.Hip].position = hipPosition;

        yield return new WaitForEndOfFrame();

        boneTransform[Bone.Hip].position = hipPosition;
    }

    private Quaternion FlipRotation(Vector3 rotation) =>
        Quaternion.Euler(360f - rotation.x, 360f - (rotation.y + 90f) - 90f, rotation.z);

    private byte[] SerializeFinger(Bone start, Bone end)
    {
        var joints = boneTransform[start].name.Split(' ')[2].StartsWith("Finger") ? 4 : 3;

        byte[] buf;

        using var memoryStream = new MemoryStream();
        using var binaryWriter = new BinaryWriter(memoryStream);

        for (var bone = start; bone <= end; bone += joints)
            for (var i = 0; i < joints - 1; i++)
                binaryWriter.WriteQuaternion(boneTransform[bone + i].localRotation);

        buf = memoryStream.ToArray();

        return buf;
    }

    private void DeserializeFinger(Bone start, Bone end, byte[] fingerBinary, bool mirroring = false)
    {
        var joints = boneTransform[start].name.Split(' ')[2].StartsWith("Finger") ? 4 : 3;
        var mirror = mirroring ? -1 : 1;

        using var memoryStream = new MemoryStream(fingerBinary);
        using var binaryReader = new BinaryReader(memoryStream);

        for (var bone = start; bone <= end; bone += joints)
            for (var i = 0; i < joints - 1; i++)
                boneTransform[bone + i].localRotation = new(
                    binaryReader.ReadSingle() * mirror,
                    binaryReader.ReadSingle() * mirror,
                    binaryReader.ReadSingle(),
                    binaryReader.ReadSingle());
    }

    private void InitializeDragPoints()
    {
        dragCube = DragPoint.Make<DragPointBody>(PrimitiveType.Cube, Vector3.one * 0.12f);
        dragCube.Initialize(() => meido.Maid.transform.position, () => Vector3.zero);
        dragCube.Set(meido.Maid.transform);

        dragCube.IsCube = true;
        dragCube.ConstantScale = true;
        dragCube.Select += OnSelectBody;
        dragCube.EndScale += OnSetDragPointScale;
        dragCube.gameObject.SetActive(CubeActive);

        dragBody = DragPoint.Make<DragPointBody>(PrimitiveType.Capsule, new Vector3(0.2f, 0.3f, 0.24f));

        dragBody.Initialize(
            () => new(
                (boneTransform[Bone.Hip].position.x + boneTransform[Bone.Spine0a].position.x) / 2f,
                (boneTransform[Bone.Spine1].position.y + boneTransform[Bone.Spine0a].position.y) / 2f,
                (boneTransform[Bone.Spine0a].position.z + boneTransform[Bone.Hip].position.z) / 2f),
            () => new(
                boneTransform[Bone.Spine0a].eulerAngles.x,
                boneTransform[Bone.Spine0a].eulerAngles.y,
                boneTransform[Bone.Spine0a].eulerAngles.z + 90f));

        dragBody.Set(meido.Maid.transform);
        dragBody.Select += OnSelectBody;
        dragBody.EndScale += OnSetDragPointScale;

        // Neck Dragpoint
        var dragNeck = DragPoint.Make<DragPointHead>(PrimitiveType.Sphere, new(0.2f, 0.24f, 0.2f));

        dragNeck.Initialize(
            meido,
            () => new(
                boneTransform[Bone.Head].position.x,
                (boneTransform[Bone.Head].position.y * 1.2f + boneTransform[Bone.HeadNub].position.y * 0.8f) / 2f,
                boneTransform[Bone.Head].position.z),
            () => new(
                boneTransform[Bone.Head].eulerAngles.x,
                boneTransform[Bone.Head].eulerAngles.y,
                boneTransform[Bone.Head].eulerAngles.z + 90f));

        dragNeck.Set(boneTransform[Bone.Neck]);
        dragNeck.Select += OnSelectFace;

        dragPoints[Bone.Head] = dragNeck;

        // Head Dragpoint
        var dragHead = DragPoint.Make<DragPointSpine>(PrimitiveType.Sphere, Vector3.one * 0.045f);

        dragHead.Initialize(meido, () => boneTransform[Bone.Head].position, () => Vector3.zero);
        dragHead.Set(boneTransform[Bone.Head]);
        dragHead.AddGizmo();

        dragPoints[Bone.HeadNub] = dragHead;

        // Torso Dragpoint
        var spineTrans1 = boneTransform[Bone.Spine1];
        var spineTrans2 = boneTransform[Bone.Spine1a];

        var dragTorso = DragPoint.Make<DragPointTorso>(PrimitiveType.Capsule, new Vector3(0.2f, 0.19f, 0.24f));

        dragTorso.Initialize(
            meido,
            () => new(spineTrans1.position.x, spineTrans2.position.y, spineTrans1.position.z - 0.05f),
            () => new(spineTrans1.eulerAngles.x, spineTrans1.eulerAngles.y, spineTrans1.eulerAngles.z + 90f));

        dragTorso.Set(boneTransform[Bone.Spine1a]);

        dragPoints[Bone.Torso] = dragTorso;

        // Pelvis Dragpoint
        var pelvisTrans = boneTransform[Bone.Pelvis];
        var spineTrans = boneTransform[Bone.Spine];

        var dragPelvis = DragPoint.Make<DragPointPelvis>(PrimitiveType.Capsule, new(0.2f, 0.15f, 0.24f));

        dragPelvis.Initialize(
            meido,
            () => new(
                pelvisTrans.position.x, (pelvisTrans.position.y + spineTrans.position.y) / 2f, pelvisTrans.position.z),
            () => new(pelvisTrans.eulerAngles.x + 90f, pelvisTrans.eulerAngles.y + 90f, pelvisTrans.eulerAngles.z));

        dragPelvis.Set(boneTransform[Bone.Pelvis]);

        dragPoints[Bone.Pelvis] = dragPelvis;

        InitializeMuneDragPoint(left: true);
        InitializeMuneDragPoint(left: false);

        var armDragPointL = MakeArmChain(boneTransform[Bone.HandL], meido);

        dragPoints[Bone.UpperArmL] = armDragPointL[0];
        dragPoints[Bone.ForearmL] = armDragPointL[1];
        dragPoints[Bone.HandL] = armDragPointL[2];

        var armDragPointR = MakeArmChain(boneTransform[Bone.HandR], meido);

        dragPoints[Bone.UpperArmR] = armDragPointR[0];
        dragPoints[Bone.ForearmR] = armDragPointR[1];
        dragPoints[Bone.HandR] = armDragPointR[2];

        var legDragPointL = MakeLegChain(boneTransform[Bone.FootL]);

        dragPoints[Bone.CalfL] = legDragPointL[0];
        dragPoints[Bone.FootL] = legDragPointL[1];

        var legDragPointR = MakeLegChain(boneTransform[Bone.FootR]);

        dragPoints[Bone.CalfR] = legDragPointR[0];
        dragPoints[Bone.FootR] = legDragPointR[1];

        InitializeSpineDragPoint(SpineBones);

        for (var bone = Bone.Finger4NubR; bone >= Bone.Finger0L; bone -= 4)
        {
            var chain = MakeFingerChain(boneTransform[bone], meido);
            var i = 2;

            for (var joint = bone - 1; joint > bone - 4; joint--)
            {
                dragPoints[joint] = chain[i];
                i--;
            }
        }

        MakeToeChain(Bone.Toe0L, Bone.Toe2R);
    }

    private void InitializeMuneDragPoint(bool left)
    {
        var mune = left ? Bone.MuneL : Bone.MuneR;
        var sub = left ? Bone.MuneSubL : Bone.MuneSubR;
        var muneDragPoint = DragPoint.Make<DragPointMune>(PrimitiveType.Sphere, Vector3.one * 0.12f);

        muneDragPoint.Initialize(
            meido,
            () => (boneTransform[mune].position + boneTransform[sub].position) / 2f,
            () => Vector3.zero);

        muneDragPoint.Set(boneTransform[sub]);
        dragPoints[mune] = muneDragPoint;
    }

    private DragPointLimb[] MakeLegChain(Transform lower)
    {
        var limbDragPointSize = Vector3.one * 0.12f;
        var dragPoints = new DragPointLimb[2];

        for (var i = dragPoints.Length - 1; i >= 0; i--)
        {
            var joint = lower;

            dragPoints[i] = DragPoint.Make<DragPointLimb>(PrimitiveType.Sphere, limbDragPointSize);
            dragPoints[i].Initialize(meido, () => joint.position, () => Vector3.zero);
            dragPoints[i].Set(joint);
            dragPoints[i].AddGizmo();

            lower = lower.parent;
        }

        return dragPoints;
    }

    private void MakeToeChain(Bone start, Bone end)
    {
        const int joints = 3;

        var fingerDragPointSize = Vector3.one * 0.01f;

        for (var bone = start; bone <= end; bone += joints)
        {
            for (var i = 1; i < joints; i++)
            {
                var trans = boneTransform[bone + i];
                var chain = DragPoint.Make<DragPointFinger>(PrimitiveType.Sphere, fingerDragPointSize);

                chain.Initialize(meido, () => trans.position, () => Vector3.zero);
                chain.Set(trans);
                dragPoints[bone + i] = chain;
            }
        }
    }

    private void InitializeSpineDragPoint(params Bone[] bones)
    {
        var spineDragPointSize = DragPointMeido.BoneScale;

        foreach (var bone in bones)
        {
            var spine = boneTransform[bone];
            var primitive = bone is Bone.Hip ? PrimitiveType.Cube : PrimitiveType.Sphere;
            var dragPoint = DragPoint.Make<DragPointSpine>(primitive, spineDragPointSize);

            dragPoint.Initialize(meido, () => spine.position, () => Vector3.zero);

            dragPoint.Set(spine);
            dragPoint.AddGizmo();
            dragPoints[bone] = dragPoint;
            dragPoints[bone].gameObject.SetActive(false);
        }
    }

    private void OnCubeActive(object sender, EventArgs args) =>
        dragCube.gameObject.SetActive(CubeActive);

    private void OnCubeSmall(object sender, EventArgs args) =>
        dragCube.DragPointScale = CubeSmall ? DragPointGeneral.SmallCube : 1f;

    private void OnSetDragPointScale(object sender, EventArgs args) =>
        SetDragPointScale(meido.Maid.transform.localScale.x);

    private void OnSelectBody(object sender, EventArgs args) =>
        SelectMaid?.Invoke(this, new MeidoUpdateEventArgs(meido.Slot, fromMaid: true, isBody: true));

    private void OnSelectFace(object sender, EventArgs args) =>
        SelectMaid?.Invoke(this, new MeidoUpdateEventArgs(meido.Slot, fromMaid: true, isBody: false));

    private void InitializeBones()
    {
        // TODO: Move to external file somehow
        var transform = meido.Body.m_Bones.transform;

        boneTransform = new()
        {
            [Bone.Head] = CMT.SearchObjName(transform, "Bip01 Head"),
            [Bone.Neck] = CMT.SearchObjName(transform, "Bip01 Neck"),
            [Bone.HeadNub] = CMT.SearchObjName(transform, "Bip01 HeadNub"),
            [Bone.MuneL] = CMT.SearchObjName(transform, "Mune_L"),
            [Bone.MuneSubL] = CMT.SearchObjName(transform, "Mune_L_sub"),
            [Bone.MuneR] = CMT.SearchObjName(transform, "Mune_R"),
            [Bone.MuneSubR] = CMT.SearchObjName(transform, "Mune_R_sub"),
            [Bone.Pelvis] = CMT.SearchObjName(transform, "Bip01 Pelvis"),
            [Bone.Hip] = CMT.SearchObjName(transform, "Bip01"),
            [Bone.Spine] = CMT.SearchObjName(transform, "Bip01 Spine"),
            [Bone.Spine0a] = CMT.SearchObjName(transform, "Bip01 Spine0a"),
            [Bone.Spine1] = CMT.SearchObjName(transform, "Bip01 Spine1"),
            [Bone.Spine1a] = CMT.SearchObjName(transform, "Bip01 Spine1a"),
            [Bone.ClavicleL] = CMT.SearchObjName(transform, "Bip01 L Clavicle"),
            [Bone.ClavicleR] = CMT.SearchObjName(transform, "Bip01 R Clavicle"),
            [Bone.UpperArmL] = CMT.SearchObjName(transform, "Bip01 L UpperArm"),
            [Bone.ForearmL] = CMT.SearchObjName(transform, "Bip01 L Forearm"),
            [Bone.HandL] = CMT.SearchObjName(transform, "Bip01 L Hand"),
            [Bone.UpperArmR] = CMT.SearchObjName(transform, "Bip01 R UpperArm"),
            [Bone.ForearmR] = CMT.SearchObjName(transform, "Bip01 R Forearm"),
            [Bone.HandR] = CMT.SearchObjName(transform, "Bip01 R Hand"),
            [Bone.ThighL] = CMT.SearchObjName(transform, "Bip01 L Thigh"),
            [Bone.CalfL] = CMT.SearchObjName(transform, "Bip01 L Calf"),
            [Bone.FootL] = CMT.SearchObjName(transform, "Bip01 L Foot"),
            [Bone.ThighR] = CMT.SearchObjName(transform, "Bip01 R Thigh"),
            [Bone.CalfR] = CMT.SearchObjName(transform, "Bip01 R Calf"),
            [Bone.FootR] = CMT.SearchObjName(transform, "Bip01 R Foot"),

            // fingers
            [Bone.Finger0L] = CMT.SearchObjName(transform, "Bip01 L Finger0"),
            [Bone.Finger01L] = CMT.SearchObjName(transform, "Bip01 L Finger01"),
            [Bone.Finger02L] = CMT.SearchObjName(transform, "Bip01 L Finger02"),
            [Bone.Finger0NubL] = CMT.SearchObjName(transform, "Bip01 L Finger0Nub"),
            [Bone.Finger1L] = CMT.SearchObjName(transform, "Bip01 L Finger1"),
            [Bone.Finger11L] = CMT.SearchObjName(transform, "Bip01 L Finger11"),
            [Bone.Finger12L] = CMT.SearchObjName(transform, "Bip01 L Finger12"),
            [Bone.Finger1NubL] = CMT.SearchObjName(transform, "Bip01 L Finger1Nub"),
            [Bone.Finger2L] = CMT.SearchObjName(transform, "Bip01 L Finger2"),
            [Bone.Finger21L] = CMT.SearchObjName(transform, "Bip01 L Finger21"),
            [Bone.Finger22L] = CMT.SearchObjName(transform, "Bip01 L Finger22"),
            [Bone.Finger2NubL] = CMT.SearchObjName(transform, "Bip01 L Finger2Nub"),
            [Bone.Finger3L] = CMT.SearchObjName(transform, "Bip01 L Finger3"),
            [Bone.Finger31L] = CMT.SearchObjName(transform, "Bip01 L Finger31"),
            [Bone.Finger32L] = CMT.SearchObjName(transform, "Bip01 L Finger32"),
            [Bone.Finger3NubL] = CMT.SearchObjName(transform, "Bip01 L Finger3Nub"),
            [Bone.Finger4L] = CMT.SearchObjName(transform, "Bip01 L Finger4"),
            [Bone.Finger41L] = CMT.SearchObjName(transform, "Bip01 L Finger41"),
            [Bone.Finger42L] = CMT.SearchObjName(transform, "Bip01 L Finger42"),
            [Bone.Finger4NubL] = CMT.SearchObjName(transform, "Bip01 L Finger4Nub"),
            [Bone.Finger0R] = CMT.SearchObjName(transform, "Bip01 R Finger0"),
            [Bone.Finger01R] = CMT.SearchObjName(transform, "Bip01 R Finger01"),
            [Bone.Finger02R] = CMT.SearchObjName(transform, "Bip01 R Finger02"),
            [Bone.Finger0NubR] = CMT.SearchObjName(transform, "Bip01 R Finger0Nub"),
            [Bone.Finger1R] = CMT.SearchObjName(transform, "Bip01 R Finger1"),
            [Bone.Finger11R] = CMT.SearchObjName(transform, "Bip01 R Finger11"),
            [Bone.Finger12R] = CMT.SearchObjName(transform, "Bip01 R Finger12"),
            [Bone.Finger1NubR] = CMT.SearchObjName(transform, "Bip01 R Finger1Nub"),
            [Bone.Finger2R] = CMT.SearchObjName(transform, "Bip01 R Finger2"),
            [Bone.Finger21R] = CMT.SearchObjName(transform, "Bip01 R Finger21"),
            [Bone.Finger22R] = CMT.SearchObjName(transform, "Bip01 R Finger22"),
            [Bone.Finger2NubR] = CMT.SearchObjName(transform, "Bip01 R Finger2Nub"),
            [Bone.Finger3R] = CMT.SearchObjName(transform, "Bip01 R Finger3"),
            [Bone.Finger31R] = CMT.SearchObjName(transform, "Bip01 R Finger31"),
            [Bone.Finger32R] = CMT.SearchObjName(transform, "Bip01 R Finger32"),
            [Bone.Finger3NubR] = CMT.SearchObjName(transform, "Bip01 R Finger3Nub"),
            [Bone.Finger4R] = CMT.SearchObjName(transform, "Bip01 R Finger4"),
            [Bone.Finger41R] = CMT.SearchObjName(transform, "Bip01 R Finger41"),
            [Bone.Finger42R] = CMT.SearchObjName(transform, "Bip01 R Finger42"),
            [Bone.Finger4NubR] = CMT.SearchObjName(transform, "Bip01 R Finger4Nub"),

            // Toes
            [Bone.Toe0L] = CMT.SearchObjName(transform, "Bip01 L Toe0"),
            [Bone.Toe01L] = CMT.SearchObjName(transform, "Bip01 L Toe01"),
            [Bone.Toe0NubL] = CMT.SearchObjName(transform, "Bip01 L Toe0Nub"),
            [Bone.Toe1L] = CMT.SearchObjName(transform, "Bip01 L Toe1"),
            [Bone.Toe11L] = CMT.SearchObjName(transform, "Bip01 L Toe11"),
            [Bone.Toe1NubL] = CMT.SearchObjName(transform, "Bip01 L Toe1Nub"),
            [Bone.Toe2L] = CMT.SearchObjName(transform, "Bip01 L Toe2"),
            [Bone.Toe21L] = CMT.SearchObjName(transform, "Bip01 L Toe21"),
            [Bone.Toe2NubL] = CMT.SearchObjName(transform, "Bip01 L Toe2Nub"),
            [Bone.Toe0R] = CMT.SearchObjName(transform, "Bip01 R Toe0"),
            [Bone.Toe01R] = CMT.SearchObjName(transform, "Bip01 R Toe01"),
            [Bone.Toe0NubR] = CMT.SearchObjName(transform, "Bip01 R Toe0Nub"),
            [Bone.Toe1R] = CMT.SearchObjName(transform, "Bip01 R Toe1"),
            [Bone.Toe11R] = CMT.SearchObjName(transform, "Bip01 R Toe11"),
            [Bone.Toe1NubR] = CMT.SearchObjName(transform, "Bip01 R Toe1Nub"),
            [Bone.Toe2R] = CMT.SearchObjName(transform, "Bip01 R Toe2"),
            [Bone.Toe21R] = CMT.SearchObjName(transform, "Bip01 R Toe21"),
            [Bone.Toe2NubR] = CMT.SearchObjName(transform, "Bip01 R Toe2Nub"),
        };
    }
}
