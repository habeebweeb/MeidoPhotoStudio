using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin
{
    public enum AttachPoint
    {
        None, Head, Neck, UpperArmL, UpperArmR, ForearmL, ForearmR, MuneL, MuneR, HandL, HandR,
        Pelvis, ThighL, ThighR, CalfL, CalfR, FootL, FootR, Spine1a, Spine1, Spine0a, Spine0
    }

    public class MeidoDragPointManager
    {
        private enum Bone
        {
            Head, HeadNub, ClavicleL, ClavicleR,
            UpperArmL, UpperArmR, ForearmL, ForearmR,
            HandL, HandR, /*IKHandL, IKHandR,*/
            MuneL, MuneSubL, MuneR, MuneSubR,
            Neck, Spine, Spine0a, Spine1, Spine1a, ThighL, ThighR,
            Pelvis, Hip,
            CalfL, CalfR, FootL, FootR,
            // Dragpoint specific
            Cube, Body, Torso,
            // Fingers
            Finger0L, Finger01L, Finger02L, Finger0NubL,
            Finger1L, Finger11L, Finger12L, Finger1NubL,
            Finger2L, Finger21L, Finger22L, Finger2NubL,
            Finger3L, Finger31L, Finger32L, Finger3NubL,
            Finger4L, Finger41L, Finger42L, Finger4NubL,
            Finger0R, Finger01R, Finger02R, Finger0NubR,
            Finger1R, Finger11R, Finger12R, Finger1NubR,
            Finger2R, Finger21R, Finger22R, Finger2NubR,
            Finger3R, Finger31R, Finger32R, Finger3NubR,
            Finger4R, Finger41R, Finger42R, Finger4NubR,
            // Toes
            Toe0L, Toe01L, Toe0NubL,
            Toe1L, Toe11L, Toe1NubL,
            Toe2L, Toe21L, Toe2NubL,
            Toe0R, Toe01R, Toe0NubR,
            Toe1R, Toe11R, Toe1NubR,
            Toe2R, Toe21R, Toe2NubR
        }
        private static readonly Dictionary<AttachPoint, Bone> PointToBone = new Dictionary<AttachPoint, Bone>()
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
            [AttachPoint.Spine0] = Bone.Spine
        };
        private static readonly Bone[] SpineBones =
        {
            Bone.Neck, Bone.Spine, Bone.Spine0a, Bone.Spine1, Bone.Spine1a, Bone.Hip, Bone.ThighL, Bone.ThighR
        };
        private static bool cubeActive;
        public static bool CubeActive
        {
            get => cubeActive;
            set
            {
                if (value != cubeActive)
                {
                    cubeActive = value;
                    CubeActiveChange?.Invoke(null, EventArgs.Empty);
                }
            }
        }
        private static bool cubeSmall;
        public static bool CubeSmall
        {
            get => cubeSmall;
            set
            {
                if (value != cubeSmall)
                {
                    cubeSmall = value;
                    CubeSmallChange?.Invoke(null, EventArgs.Empty);
                }
            }
        }
        private static EventHandler CubeActiveChange;
        private static EventHandler CubeSmallChange;
        private readonly Meido meido;
        private readonly Dictionary<Bone, DragPointMeido> DragPoints = new Dictionary<Bone, DragPointMeido>();
        private Dictionary<Bone, Transform> BoneTransform = new Dictionary<Bone, Transform>();
        private DragPointBody dragBody;
        private DragPointBody dragCube;
        private bool initialized;
        public event EventHandler<MeidoUpdateEventArgs> SelectMaid;
        private bool isBone;
        public bool IsBone
        {
            get => isBone;
            set
            {
                if (!initialized) return;
                if (isBone != value)
                {
                    isBone = value;
                    foreach (DragPointMeido dragPoint in DragPoints.Values) dragPoint.IsBone = isBone;
                    foreach (Bone bone in SpineBones) DragPoints[bone].gameObject.SetActive(isBone);
                }
            }
        }
        private bool active = true;
        public bool Active
        {
            get => active;
            set
            {
                if (!initialized) return;
                if (active != value)
                {
                    active = value;
                    foreach (DragPointMeido dragPoint in DragPoints.Values) dragPoint.gameObject.SetActive(active);
                    foreach (Bone bone in SpineBones) DragPoints[bone].gameObject.SetActive(active && IsBone);
                    DragPointHead head = (DragPointHead)DragPoints[Bone.Head];
                    head.gameObject.SetActive(true);
                    head.IsIK = !active;
                    dragBody.IsIK = !active;
                }
            }
        }

        public MeidoDragPointManager(Meido meido) => this.meido = meido;

        public void Deserialize(BinaryReader binaryReader)
        {
            Bone[] bones = {
                Bone.Hip, Bone.Pelvis, Bone.Spine, Bone.Spine0a, Bone.Spine1, Bone.Spine1a, Bone.Neck,
                Bone.ClavicleL, Bone.ClavicleR, Bone.UpperArmL, Bone.UpperArmR, Bone.ForearmL, Bone.ForearmR,
                Bone.ThighL, Bone.ThighR, Bone.CalfL, Bone.CalfR, Bone.MuneL, Bone.MuneR, Bone.MuneSubL, Bone.MuneSubR,
                Bone.HandL, Bone.HandR, Bone.FootL, Bone.FootR
            };
            int localRotationIndex = Array.IndexOf(bones, Bone.CalfR);
            for (Bone bone = Bone.Finger0L; bone <= Bone.Toe2NubR; ++bone)
            {
                BoneTransform[bone].localRotation = binaryReader.ReadQuaternion();
            }
            for (int i = 0; i < bones.Length; i++)
            {
                Bone bone = bones[i];
                Quaternion rotation = binaryReader.ReadQuaternion();
                if (i > localRotationIndex) BoneTransform[bone].localRotation = rotation;
                else BoneTransform[bone].rotation = rotation;
            }
            // WHY????
            GameMain.Instance.StartCoroutine(ApplyHipPosition(binaryReader.ReadVector3()));
        }

        /*
            Somebody smarter than me please help me find a way to do this better T_T
            inb4 for loop.
         */
        private System.Collections.IEnumerator ApplyHipPosition(Vector3 hipPosition)
        {
            BoneTransform[Bone.Hip].position = hipPosition;
            yield return new WaitForEndOfFrame();
            BoneTransform[Bone.Hip].position = hipPosition;
            yield return new WaitForEndOfFrame();
            BoneTransform[Bone.Hip].position = hipPosition;
        }

        public Transform GetAttachPointTransform(AttachPoint point)
            => point == AttachPoint.None ? null : BoneTransform[PointToBone[point]];

        public void Flip()
        {
            meido.Stop = true;
            Bone[] single = new[] { Bone.Pelvis, Bone.Spine, Bone.Spine0a, Bone.Spine1, Bone.Spine1a, Bone.Neck };
            Bone[] pair = new[] {
                Bone.ClavicleL, Bone.ClavicleR, Bone.UpperArmL, Bone.UpperArmR, Bone.ForearmL, Bone.ForearmR,
                Bone.ThighL, Bone.ThighR, Bone.CalfL, Bone.CalfR, Bone.HandL, Bone.HandR, Bone.FootL, Bone.FootR
            };

            List<Vector3> singleRotations = single.Select(bone => BoneTransform[bone].eulerAngles).ToList();
            List<Vector3> pairRotations = pair.Select(bone => BoneTransform[bone].eulerAngles).ToList();

            Transform hip = BoneTransform[Bone.Hip];
            Vector3 vecHip = hip.eulerAngles;

            Transform hipL = meido.Maid.body0.GetBone("Hip_L");
            Vector3 vecHipL = hipL.eulerAngles;

            Transform hipR = meido.Maid.body0.GetBone("Hip_R");
            Vector3 vecHipR = hipR.eulerAngles;

            hip.rotation = Quaternion.Euler(
                360f - (vecHip.x + 270f) - 270f, 360f - (vecHip.y + 90f) - 90f, 360f - vecHip.z
            );

            hipL.rotation = FlipRotation(vecHipR);
            hipR.rotation = FlipRotation(vecHipL);

            for (int i = 0; i < single.Length; i++)
            {
                Bone bone = single[i];
                BoneTransform[bone].rotation = FlipRotation(singleRotations[i]);
            }

            for (int i = 0; i < pair.Length; i += 2)
            {
                Bone boneA = pair[i];
                Bone boneB = pair[i + 1];
                BoneTransform[boneA].rotation = FlipRotation(pairRotations[i + 1]);
                BoneTransform[boneB].rotation = FlipRotation(pairRotations[i]);
            }

            byte[] leftHand = SerializeHand(right: false);
            byte[] rightHand = SerializeHand(right: true);
            DeserializeHand(leftHand, right: true, true);
            DeserializeHand(rightHand, right: false, true);
            leftHand = SerializeFoot(right: false);
            rightHand = SerializeFoot(right: true);
            DeserializeFoot(leftHand, right: true, true);
            DeserializeFoot(rightHand, right: false, true);
        }

        private Quaternion FlipRotation(Vector3 rotation)
        {
            return Quaternion.Euler(360f - rotation.x, 360f - (rotation.y + 90f) - 90f, rotation.z);
        }

        public byte[] SerializeHand(bool right)
        {
            Bone start = right ? Bone.Finger0R : Bone.Finger0L;
            Bone end = right ? Bone.Finger4R : Bone.Finger4L;
            return SerializeFinger(start, end);
        }

        public void DeserializeHand(byte[] handBinary, bool right, bool mirroring = false)
        {
            Bone start = right ? Bone.Finger0R : Bone.Finger0L;
            Bone end = right ? Bone.Finger4R : Bone.Finger4L;
            DeserializeFinger(start, end, handBinary, mirroring);
        }

        public byte[] SerializeFoot(bool right)
        {
            Bone start = right ? Bone.Toe0R : Bone.Toe0L;
            Bone end = right ? Bone.Toe2R : Bone.Toe2L;
            return SerializeFinger(start, end);
        }

        public void DeserializeFoot(byte[] footBinary, bool right, bool mirroring = false)
        {
            Bone start = right ? Bone.Toe0R : Bone.Toe0L;
            Bone end = right ? Bone.Toe2R : Bone.Toe2L;
            DeserializeFinger(start, end, footBinary, mirroring);
        }

        private byte[] SerializeFinger(Bone start, Bone end)
        {
            int joints = BoneTransform[start].name.Split(' ')[2].StartsWith("Finger") ? 4 : 3;

            byte[] buf;

            using (MemoryStream memoryStream = new MemoryStream())
            using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
            {
                for (Bone bone = start; bone <= end; bone += joints)
                {
                    for (int i = 0; i < joints - 1; i++)
                    {
                        binaryWriter.WriteQuaternion(BoneTransform[bone + i].localRotation);
                    }
                }
                buf = memoryStream.ToArray();
            }

            return buf;
        }

        private void DeserializeFinger(Bone start, Bone end, byte[] fingerBinary, bool mirroring = false)
        {
            int joints = BoneTransform[start].name.Split(' ')[2].StartsWith("Finger") ? 4 : 3;

            int mirror = mirroring ? -1 : 1;

            using MemoryStream memoryStream = new MemoryStream(fingerBinary);
            using BinaryReader binaryReader = new BinaryReader(memoryStream);

            for (Bone bone = start; bone <= end; bone += joints)
            {
                for (int i = 0; i < joints - 1; i++)
                {
                    BoneTransform[bone + i].localRotation = new Quaternion
                    (
                        binaryReader.ReadSingle() * mirror,
                        binaryReader.ReadSingle() * mirror,
                        binaryReader.ReadSingle(),
                        binaryReader.ReadSingle()
                    );
                }
            }
        }

        public void Destroy()
        {
            foreach (DragPointMeido dragPoint in DragPoints.Values)
            {
                if (dragPoint != null)
                {
                    GameObject.Destroy(dragPoint.gameObject);
                }
            }
            if (dragCube != null) GameObject.Destroy(dragCube.gameObject);
            if (dragBody != null) GameObject.Destroy(dragBody.gameObject);
            BoneTransform.Clear();
            DragPoints.Clear();
            CubeActiveChange -= OnCubeActive;
            CubeSmallChange -= OnCubeSmall;
            initialized = false;
        }

        public void Initialize()
        {
            if (initialized) return;
            initialized = true;
            CubeActiveChange += OnCubeActive;
            CubeSmallChange += OnCubeSmall;
            InitializeBones();
            InitializeDragPoints();
            SetDragPointScale(meido.Maid.transform.localScale.x);
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
                () => new Vector3(
                    (BoneTransform[Bone.Hip].position.x + BoneTransform[Bone.Spine0a].position.x) / 2f,
                    (BoneTransform[Bone.Spine1].position.y + BoneTransform[Bone.Spine0a].position.y) / 2f,
                    (BoneTransform[Bone.Spine0a].position.z + BoneTransform[Bone.Hip].position.z) / 2f
                ),
                () => new Vector3(
                    BoneTransform[Bone.Spine0a].eulerAngles.x,
                    BoneTransform[Bone.Spine0a].eulerAngles.y,
                    BoneTransform[Bone.Spine0a].eulerAngles.z + 90f
                )
            );
            dragBody.Set(meido.Maid.transform);
            dragBody.Select += OnSelectBody;
            dragBody.EndScale += OnSetDragPointScale;

            // Neck Dragpoint
            DragPointHead dragNeck = DragPoint.Make<DragPointHead>(
                PrimitiveType.Sphere, new Vector3(0.2f, 0.24f, 0.2f)
            );
            dragNeck.Initialize(meido,
                () => new Vector3(
                    BoneTransform[Bone.Head].position.x,
                    ((BoneTransform[Bone.Head].position.y * 1.2f) + (BoneTransform[Bone.HeadNub].position.y * 0.8f)) / 2f,
                    BoneTransform[Bone.Head].position.z
                ),
                () => new Vector3(
                    BoneTransform[Bone.Head].eulerAngles.x,
                    BoneTransform[Bone.Head].eulerAngles.y,
                    BoneTransform[Bone.Head].eulerAngles.z + 90f
                )
            );
            dragNeck.Set(BoneTransform[Bone.Neck]);
            dragNeck.Select += OnSelectFace;

            DragPoints[Bone.Head] = dragNeck;

            // Head Dragpoint
            DragPointSpine dragHead = DragPoint.Make<DragPointSpine>(PrimitiveType.Sphere, Vector3.one * 0.045f);
            dragHead.Initialize(meido, () => BoneTransform[Bone.Head].position, () => Vector3.zero);
            dragHead.Set(BoneTransform[Bone.Head]);
            dragHead.AddGizmo();

            DragPoints[Bone.HeadNub] = dragHead;

            // Torso Dragpoint
            Transform spineTrans1 = BoneTransform[Bone.Spine1];
            Transform spineTrans2 = BoneTransform[Bone.Spine1a];

            DragPointTorso dragTorso = DragPoint.Make<DragPointTorso>(
                PrimitiveType.Capsule, new Vector3(0.2f, 0.19f, 0.24f)
            );
            dragTorso.Initialize(meido,
                () => new Vector3(
                    spineTrans1.position.x,
                    spineTrans2.position.y,
                    spineTrans1.position.z - 0.05f
                ),
                () => new Vector3(
                    spineTrans1.eulerAngles.x,
                    spineTrans1.eulerAngles.y,
                    spineTrans1.eulerAngles.z + 90f
                )
            );
            dragTorso.Set(BoneTransform[Bone.Spine1a]);

            DragPoints[Bone.Torso] = dragTorso;

            // Pelvis Dragpoint
            Transform pelvisTrans = BoneTransform[Bone.Pelvis];
            Transform spineTrans = BoneTransform[Bone.Spine];

            DragPointPelvis dragPelvis = DragPoint.Make<DragPointPelvis>(
                PrimitiveType.Capsule, new Vector3(0.2f, 0.15f, 0.24f)
            );
            dragPelvis.Initialize(meido,

                () => new Vector3(
                    pelvisTrans.position.x,
                    (pelvisTrans.position.y + spineTrans.position.y) / 2f,
                    pelvisTrans.position.z
                ),
                () => new Vector3(
                    pelvisTrans.eulerAngles.x + 90f,
                    pelvisTrans.eulerAngles.y + 90f,
                    pelvisTrans.eulerAngles.z
                )
            );
            dragPelvis.Set(BoneTransform[Bone.Pelvis]);

            DragPoints[Bone.Pelvis] = dragPelvis;

            InitializeMuneDragPoint(left: true);
            InitializeMuneDragPoint(left: false);

            DragPointLimb[] armDragPointL = MakeIKChain(BoneTransform[Bone.HandL]);
            DragPoints[Bone.UpperArmL] = armDragPointL[0];
            DragPoints[Bone.ForearmL] = armDragPointL[1];
            DragPoints[Bone.HandL] = armDragPointL[2];

            DragPointLimb[] armDragPointR = MakeIKChain(BoneTransform[Bone.HandR]);
            DragPoints[Bone.UpperArmR] = armDragPointR[0];
            DragPoints[Bone.ForearmR] = armDragPointR[1];
            DragPoints[Bone.HandR] = armDragPointR[2];

            DragPointLimb[] legDragPointL = MakeIKChain(BoneTransform[Bone.FootL]);
            DragPoints[Bone.CalfL] = legDragPointL[0];
            DragPoints[Bone.FootL] = legDragPointL[1];

            DragPointLimb[] legDragPointR = MakeIKChain(BoneTransform[Bone.FootR]);
            DragPoints[Bone.CalfR] = legDragPointR[0];
            DragPoints[Bone.FootR] = legDragPointR[1];

            InitializeSpineDragPoint(SpineBones);

            InitializeFingerDragPoint(Bone.Finger0L, Bone.Finger4R);
            InitializeFingerDragPoint(Bone.Toe0L, Bone.Toe2R);
        }

        private void InitializeMuneDragPoint(bool left)
        {
            Bone mune = left ? Bone.MuneL : Bone.MuneR;
            Bone sub = left ? Bone.MuneSubL : Bone.MuneSubR;
            DragPointMune muneDragPoint = DragPoint.Make<DragPointMune>(PrimitiveType.Sphere, Vector3.one * 0.12f);
            muneDragPoint.Initialize(meido,
                () => (BoneTransform[mune].position + BoneTransform[sub].position) / 2f,
                () => Vector3.zero
            );
            muneDragPoint.Set(BoneTransform[sub]);
            DragPoints[mune] = muneDragPoint;
        }

        private DragPointLimb[] MakeIKChain(Transform lower)
        {
            Vector3 limbDragPointSize = Vector3.one * 0.12f;
            // Ignore Thigh transform when making a leg IK chain
            bool isLeg = lower.name.EndsWith("Foot");
            DragPointLimb[] dragPoints = new DragPointLimb[isLeg ? 2 : 3];
            for (int i = dragPoints.Length - 1; i >= 0; i--)
            {
                Transform joint = lower;
                dragPoints[i] = DragPoint.Make<DragPointLimb>(PrimitiveType.Sphere, limbDragPointSize);
                dragPoints[i].Initialize(meido, () => joint.position, () => Vector3.zero);
                dragPoints[i].Set(joint);
                dragPoints[i].AddGizmo();
                lower = lower.parent;
            }
            return dragPoints;
        }

        private void InitializeFingerDragPoint(Bone start, Bone end)
        {
            Vector3 fingerDragPointSize = Vector3.one * 0.01f;
            int joints = BoneTransform[start].name.Split(' ')[2].StartsWith("Finger") ? 4 : 3;
            for (Bone bone = start; bone <= end; bone += joints)
            {
                for (int i = 1; i < joints; i++)
                {
                    Transform trans = BoneTransform[bone + i];
                    DragPointFinger chain = DragPoint.Make<DragPointFinger>(PrimitiveType.Sphere, fingerDragPointSize);
                    chain.Initialize(meido, () => trans.position, () => Vector3.zero);
                    chain.Set(trans);
                    DragPoints[bone + i] = chain;
                }
            }
        }

        private void InitializeSpineDragPoint(params Bone[] bones)
        {
            Vector3 spineDragPointSize = DragPointMeido.boneScale;
            foreach (Bone bone in bones)
            {
                Transform spine = BoneTransform[bone];
                PrimitiveType primitive = bone == Bone.Hip ? PrimitiveType.Cube : PrimitiveType.Sphere;
                DragPointSpine dragPoint = DragPoint.Make<DragPointSpine>(primitive, spineDragPointSize);
                dragPoint.Initialize(meido,
                    () => spine.position,
                    () => Vector3.zero
                );
                dragPoint.Set(spine);
                dragPoint.AddGizmo();
                DragPoints[bone] = dragPoint;
                DragPoints[bone].gameObject.SetActive(false);
            }
        }

        private void OnCubeActive(object sender, EventArgs args)
        {
            dragCube.gameObject.SetActive(CubeActive);
        }

        private void OnCubeSmall(object sender, EventArgs args)
        {
            dragCube.DragPointScale = CubeSmall ? DragPointGeneral.smallCube : 1f;
        }

        private void OnSetDragPointScale(object sender, EventArgs args)
        {
            SetDragPointScale(meido.Maid.transform.localScale.x);
        }

        private void OnSelectBody(object sender, EventArgs args)
        {
            SelectMaid?.Invoke(this, new MeidoUpdateEventArgs(meido.Slot, fromMaid: true, isBody: true));
        }

        private void OnSelectFace(object sender, EventArgs args)
        {
            SelectMaid?.Invoke(this, new MeidoUpdateEventArgs(meido.Slot, fromMaid: true, isBody: false));
        }

        public void SetDragPointScale(float scale)
        {
            foreach (DragPointMeido dragPoint in DragPoints.Values) dragPoint.DragPointScale = scale;
            dragBody.DragPointScale = scale;
        }

        private void InitializeBones()
        {
            // TODO: Move to external file somehow
            Transform transform = meido.Body.m_Bones.transform;
            BoneTransform = new Dictionary<Bone, Transform>()
            {
                [Bone.Head] = CMT.SearchObjName(transform, "Bip01 Head"),
                [Bone.Neck] = CMT.SearchObjName(transform, "Bip01 Neck"),
                [Bone.HeadNub] = CMT.SearchObjName(transform, "Bip01 HeadNub"),
                /*[Bone.IKHandL] = CMT.SearchObjName(transform, "_IK_handL"),
                [Bone.IKHandR] = CMT.SearchObjName(transform, "_IK_handR"),*/
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
                [Bone.Toe2NubR] = CMT.SearchObjName(transform, "Bip01 R Toe2Nub")
            };
        }
    }

    public readonly struct AttachPointInfo
    {
        private readonly AttachPoint attachPoint;
        public AttachPoint AttachPoint => attachPoint;
        public string MaidGuid { get; }
        
        private readonly int maidIndex;
        public int MaidIndex => maidIndex;
        private static readonly AttachPointInfo empty = new(AttachPoint.None, string.Empty, -1);
        public static ref readonly AttachPointInfo Empty => ref empty;

        public AttachPointInfo(AttachPoint attachPoint, Meido meido)
        {
            this.attachPoint = attachPoint;
            MaidGuid = meido.Maid.status.guid;
            maidIndex = meido.Slot;
        }

        public AttachPointInfo(AttachPoint attachPoint, string maidGuid, int maidIndex)
        {
            this.attachPoint = attachPoint;
            MaidGuid = maidGuid;
            this.maidIndex = maidIndex;
        }
    }
}
