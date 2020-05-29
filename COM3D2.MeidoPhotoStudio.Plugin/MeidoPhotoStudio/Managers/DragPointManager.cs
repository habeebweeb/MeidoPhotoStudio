using System;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using ModKey = Utility.ModKey;
    public class DragPointManager
    {
        enum IKMode
        {
            None, UpperLock, Mune, RotLocal, BodyTransform, FingerRotLocalY, FingerRotLocalXZ, BodySelect
        }
        enum Bone
        {
            Head, HeadNub, ClavicleL, ClavicleR,
            UpperArmL, UpperArmR, ForearmL, ForearmR,
            HandL, HandR, IKHandL, IKHandR,
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
        private static readonly Dictionary<IKMode, Bone[]> IKGroup = new Dictionary<IKMode, Bone[]>()
        {
            [IKMode.None] = new[]
            {
                Bone.UpperArmL, Bone.ForearmL, Bone.HandL, Bone.UpperArmR,
                Bone.ForearmR, Bone.HandR, Bone.CalfL, Bone.FootL, Bone.CalfR, Bone.FootR
            },
            [IKMode.UpperLock] = new[] { Bone.HandL, Bone.HandR, Bone.FootL, Bone.FootR },
            [IKMode.Mune] = new[] { Bone.Head, Bone.MuneL, Bone.MuneR },
            [IKMode.RotLocal] = new[]
            {
                Bone.Head, Bone.CalfL, Bone.CalfR, Bone.Torso,
                Bone.Pelvis, Bone.HandL, Bone.HandR, Bone.FootL, Bone.FootR
            },
            [IKMode.BodyTransform] = new[] { Bone.Body, Bone.Cube },
            [IKMode.BodySelect] = new[] { Bone.Head, Bone.Body },
            [IKMode.FingerRotLocalXZ] = new[]
            {
                Bone.Finger01L, Bone.Finger02L, Bone.Finger0NubL,
                Bone.Finger11L, Bone.Finger12L, Bone.Finger1NubL,
                Bone.Finger21L, Bone.Finger22L, Bone.Finger2NubL,
                Bone.Finger31L, Bone.Finger32L, Bone.Finger3NubL,
                Bone.Finger41L, Bone.Finger42L, Bone.Finger4NubL,
                Bone.Finger01R, Bone.Finger02R, Bone.Finger0NubR,
                Bone.Finger11R, Bone.Finger12R, Bone.Finger1NubR,
                Bone.Finger21R, Bone.Finger22R, Bone.Finger2NubR,
                Bone.Finger31R, Bone.Finger32R, Bone.Finger3NubR,
                Bone.Finger41R, Bone.Finger42R, Bone.Finger4NubR,
                Bone.Toe01L, Bone.Toe0NubL, Bone.Toe11L, Bone.Toe1NubL,
                Bone.Toe21L, Bone.Toe2NubL, Bone.Toe01R, Bone.Toe0NubR,
                Bone.Toe11R, Bone.Toe1NubR, Bone.Toe21R, Bone.Toe2NubR
            },
            [IKMode.FingerRotLocalY] = new[] {
                Bone.Finger01L, Bone.Finger11L, Bone.Finger21L, Bone.Finger31L, Bone.Finger41L,
                Bone.Finger01R, Bone.Finger11R, Bone.Finger21R, Bone.Finger31R, Bone.Finger41R,
                Bone.Toe01L, Bone.Toe11L, Bone.Toe21L, Bone.Toe01R, Bone.Toe11R, Bone.Toe21R
            }
        };
        private Meido meido;
        private Maid maid;
        private Dictionary<Bone, GameObject> DragPoint;
        private Dictionary<Bone, Transform> BoneTransform;
        private IKMode ikMode;
        private IKMode ikModeOld = IKMode.None;
        public event EventHandler<MeidoChangeEventArgs> SelectMaid;
        public bool Initialized { get; private set; }
        public bool Active { get; set; }
        public DragPointManager(Meido meido)
        {
            meido.BodyLoad += Initialize;
            this.meido = meido;
            this.maid = meido.Maid;
        }

        public void Initialize(object sender, EventArgs args)
        {
            if (Initialized) return;

            this.Active = true;
            InitializeBones();
            InitializeDragPoints();
            Initialized = true;
            meido.BodyLoad -= Initialize;
        }

        public void Destroy()
        {
            foreach (KeyValuePair<Bone, GameObject> dragPoint in DragPoint)
            {
                GameObject.Destroy(dragPoint.Value);
            }
            DragPoint = null;
            BoneTransform = null;
            Initialized = false;
            this.Active = false;
        }

        public void Deactivate()
        {
            this.Active = false;
            foreach (KeyValuePair<Bone, GameObject> dragPoint in DragPoint)
            {
                dragPoint.Value.SetActive(false);
            }
        }

        public void Activate()
        {
            this.Active = true;
            ikMode = ikModeOld = IKMode.None;
            UpdateIK();
        }

        public void Update()
        {
            if (Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.X) || Input.GetKey(KeyCode.C))
            {
                ikMode = IKMode.BodyTransform;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                ikMode = IKMode.BodySelect;
            }
            else if (Utility.GetModKey(ModKey.Control) && Utility.GetModKey(ModKey.Alt))
            {
                ikMode = IKMode.Mune;
            }
            else if (Utility.GetModKey(ModKey.Shift) && Input.GetKey(KeyCode.Space))
            {
                ikMode = IKMode.FingerRotLocalY;
            }
            else if (Utility.GetModKey(ModKey.Alt) /* && Utility.GetModKey(ModKey.Shift) */)
            {
                ikMode = IKMode.RotLocal;
            }
            else if (Input.GetKey(KeyCode.Space))
            {
                ikMode = IKMode.FingerRotLocalXZ;
            }
            else if (Utility.GetModKey(ModKey.Control))
            {
                ikMode = IKMode.UpperLock;
            }
            else
            {
                ikMode = IKMode.None;
            }

            if (ikMode != ikModeOld) UpdateIK();

            ikModeOld = ikMode;
        }

        private void UpdateIK()
        {
            if (Active)
            {
                foreach (KeyValuePair<Bone, GameObject> dragPoint in DragPoint)
                {
                    dragPoint.Value.SetActive(false);
                }

                foreach (Bone bone in IKGroup[ikMode])
                {
                    DragPoint[bone].SetActive(true);
                }
            }
            else
            {
                if (ikMode == IKMode.BodySelect)
                {
                    DragPoint[Bone.Body].SetActive(true);
                    DragPoint[Bone.Head].SetActive(true);
                }
                else if (ikMode == IKMode.BodyTransform)
                {
                    DragPoint[Bone.Body].SetActive(true);
                    DragPoint[Bone.Cube].SetActive(true);
                }
                else
                {
                    DragPoint[Bone.Body].SetActive(false);
                    DragPoint[Bone.Head].SetActive(false);
                    DragPoint[Bone.Cube].SetActive(false);
                }
            }
        }

        // TODO: Rework this a little to reduce number of needed BaseDrag derived components
        private void InitializeDragPoints()
        {
            DragPoint = new Dictionary<Bone, GameObject>();

            Vector3 limbDragPointSize = Vector3.one * 0.12f;
            Vector3 fingerDragPointSize = Vector3.one * 0.015f;

            Material transparentBlue = new Material(Shader.Find("Transparent/Diffuse"))
            {
                color = new Color(0.4f, 0.4f, 1f, 0.3f)
            };

            Material transparentBlue2 = new Material(Shader.Find("Transparent/Diffuse"))
            {
                color = new Color(0.5f, 0.5f, 1f, 0.8f)
            };

            Func<PrimitiveType, Vector3, Material, GameObject> MakeDragPoint = (primitive, scale, material) =>
            {
                GameObject dragPoint = GameObject.CreatePrimitive(primitive);
                dragPoint.transform.localScale = scale;
                if (material != null) dragPoint.GetComponent<Renderer>().material = material;
                dragPoint.layer = 8;
                return dragPoint;
            };

            Func<Transform[], Transform[], Transform[], bool, GameObject[]> MakeIKChainDragPoint = (upper, middle, lower, leg) =>
            {
                GameObject[] dragPoints = new GameObject[3];
                for (int i = 0; i < dragPoints.Length; i++)
                {
                    dragPoints[i] = MakeDragPoint(PrimitiveType.Sphere, limbDragPointSize, transparentBlue);
                }

                DragJointForearm dragUpper = dragPoints[0].AddComponent<DragJointForearm>();
                dragUpper.Initialize(upper, false, maid, () => upper[2].position, () => Vector3.zero);
                dragUpper.DragEvent += OnDragEvent;
                DragJointForearm dragMiddle = dragPoints[1].AddComponent<DragJointForearm>();
                dragMiddle.Initialize(middle, leg, maid, () => middle[2].position, () => Vector3.zero);
                dragMiddle.DragEvent += OnDragEvent;
                DragJointHand dragLower = dragPoints[2].AddComponent<DragJointHand>();
                dragLower.Initialize(lower, leg, maid, () => lower[2].position, () => Vector3.zero);
                dragLower.DragEvent += OnDragEvent;
                return dragPoints;
            };

            // Cube Dragpoint
            DragPoint[Bone.Cube] = MakeDragPoint(PrimitiveType.Cube, new Vector3(0.12f, 0.12f, 0.12f), transparentBlue2);

            DragPoint[Bone.Cube].AddComponent<DragBody>()
                .Initialize(maid,
                    () => maid.transform.position,
                    () => maid.transform.eulerAngles
                );

            // Body Dragpoint
            DragPoint[Bone.Body] = MakeDragPoint(PrimitiveType.Capsule, new Vector3(0.2f, 0.3f, 0.24f), transparentBlue);

            DragBody dragBody = DragPoint[Bone.Body].AddComponent<DragBody>();
            dragBody.Initialize(maid,
                () => new Vector3(
                    (BoneTransform[Bone.Hip].position.x + BoneTransform[Bone.Spine0a].position.x) / 2f,
                    (BoneTransform[Bone.Spine1].position.y + BoneTransform[Bone.Spine0a].position.y) / 2f,
                    (BoneTransform[Bone.Spine0a].position.z + BoneTransform[Bone.Hip].position.z) / 2f
                ),
                () => new Vector3(
                    BoneTransform[Bone.Spine0a].transform.eulerAngles.x,
                    BoneTransform[Bone.Spine0a].transform.eulerAngles.y,
                    BoneTransform[Bone.Spine0a].transform.eulerAngles.z + 90f
                )
            );
            dragBody.Select += (s, e) => OnMeidoSelect(new MeidoChangeEventArgs(meido.ActiveSlot, true));

            // Head Dragpoint
            DragPoint[Bone.Head] = MakeDragPoint(PrimitiveType.Sphere, new Vector3(0.2f, 0.24f, 0.2f), transparentBlue);
            DragHead dragHead = DragPoint[Bone.Head].AddComponent<DragHead>();
            dragHead.Initialize(BoneTransform[Bone.Neck], maid,
                () => new Vector3(
                    BoneTransform[Bone.Head].position.x,
                    (BoneTransform[Bone.Head].position.y * 1.2f + BoneTransform[Bone.HeadNub].position.y * 0.8f) / 2f,
                    BoneTransform[Bone.Head].position.z
                ),
                () => new Vector3(BoneTransform[Bone.Head].eulerAngles.x, BoneTransform[Bone.Head].eulerAngles.y, BoneTransform[Bone.Head].eulerAngles.z + 90f)
            );
            dragHead.Select += (s, a) => OnMeidoSelect(new MeidoChangeEventArgs(meido.ActiveSlot, true, false));
            dragHead.DragEvent += OnDragEvent;

            // Torso Dragpoint
            DragPoint[Bone.Torso] = MakeDragPoint(PrimitiveType.Capsule, new Vector3(0.2f, 0.19f, 0.24f), transparentBlue);
            Transform spineTrans1 = BoneTransform[Bone.Spine1];
            Transform spineTrans2 = BoneTransform[Bone.Spine1a];
            Transform[] spineParts = new Transform[4] {
                BoneTransform[Bone.Spine1a],
                BoneTransform[Bone.Spine1],
                BoneTransform[Bone.Spine0a],
                BoneTransform[Bone.Spine]
            };
            DragTorso dragTorso = DragPoint[Bone.Torso].AddComponent<DragTorso>();
            dragTorso.Initialize(maid, spineParts,
                () => new Vector3(
                    spineTrans1.position.x,
                    (spineTrans2.position.y * 2f) / 2f,
                    spineTrans1.position.z
                ),
                () => new Vector3(
                    spineTrans1.eulerAngles.x,
                    spineTrans1.eulerAngles.y,
                    spineTrans1.eulerAngles.z + 90f
                )
            );
            dragTorso.DragEvent += OnDragEvent;

            // Pelvis Dragpoint
            DragPoint[Bone.Pelvis] = MakeDragPoint(PrimitiveType.Capsule, new Vector3(0.2f, 0.15f, 0.24f), transparentBlue);
            Transform pelvisTrans = BoneTransform[Bone.Pelvis];
            Transform spineTrans = BoneTransform[Bone.Spine];
            DragPelvis dragPelvis = DragPoint[Bone.Pelvis].AddComponent<DragPelvis>();
            dragPelvis.Initialize(maid, BoneTransform[Bone.Pelvis],
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
            dragPelvis.DragEvent += OnDragEvent;

            // Left Mune Dragpoint
            DragPoint[Bone.MuneL] = MakeDragPoint(PrimitiveType.Sphere, new Vector3(0.12f, 0.12f, 0.12f), transparentBlue);
            DragMune dragMuneL = DragPoint[Bone.MuneL].AddComponent<DragMune>();
            Transform[] muneIKChainL = new Transform[3] {
                BoneTransform[Bone.MuneL],
                BoneTransform[Bone.MuneL],
                BoneTransform[Bone.MuneSubL]
            };
            dragMuneL.Initialize(muneIKChainL, maid,
                () => (BoneTransform[Bone.MuneL].position + BoneTransform[Bone.MuneSubL].position) / 2f,
                () => Vector3.zero
            );
            dragMuneL.DragEvent += OnDragEvent;

            // Right Mune Dragpoint
            DragPoint[Bone.MuneR] = MakeDragPoint(PrimitiveType.Sphere, new Vector3(0.12f, 0.12f, 0.12f), transparentBlue);
            DragMune dragMuneR = DragPoint[Bone.MuneR].AddComponent<DragMune>();
            Transform[] muneIKChainR = new Transform[3] {
                BoneTransform[Bone.MuneR],
                BoneTransform[Bone.MuneR],
                BoneTransform[Bone.MuneSubR]
            };
            dragMuneR.Initialize(muneIKChainR, maid,
                () => (BoneTransform[Bone.MuneR].position + BoneTransform[Bone.MuneSubR].position) / 2f,
                () => Vector3.zero
            );
            dragMuneR.DragEvent += OnDragEvent;

            // Left Arm Dragpoint
            GameObject[] ikChainArmL = MakeIKChainDragPoint(
                new Transform[3] {
                    BoneTransform[Bone.ClavicleL],
                    BoneTransform[Bone.ClavicleL],
                    BoneTransform[Bone.UpperArmL]
                },
                new Transform[3] {
                    BoneTransform[Bone.UpperArmL],
                    BoneTransform[Bone.UpperArmL],
                    BoneTransform[Bone.ForearmL]
                },
                new Transform[3] {
                    BoneTransform[Bone.UpperArmL],
                    BoneTransform[Bone.ForearmL],
                    BoneTransform[Bone.HandL]
                },
                false
            );
            DragPoint[Bone.UpperArmL] = ikChainArmL[0];
            DragPoint[Bone.ForearmL] = ikChainArmL[1];
            DragPoint[Bone.HandL] = ikChainArmL[2];

            // Right Arm Dragpoint
            GameObject[] ikChainArmR = MakeIKChainDragPoint(
                new Transform[3] {
                    BoneTransform[Bone.ClavicleR],
                    BoneTransform[Bone.ClavicleR],
                    BoneTransform[Bone.UpperArmR]
                },
                new Transform[3] {
                    BoneTransform[Bone.UpperArmR],
                    BoneTransform[Bone.UpperArmR],
                    BoneTransform[Bone.ForearmR]
                },
                new Transform[3] {
                    BoneTransform[Bone.UpperArmR],
                    BoneTransform[Bone.ForearmR],
                    BoneTransform[Bone.HandR]
                },
                false
            );
            DragPoint[Bone.UpperArmR] = ikChainArmR[0];
            DragPoint[Bone.ForearmR] = ikChainArmR[1];
            DragPoint[Bone.HandR] = ikChainArmR[2];

            // Left Leg Dragpoint
            GameObject[] ikChainLegL = MakeIKChainDragPoint(
                new Transform[3] {
                    BoneTransform[Bone.ThighL],
                    BoneTransform[Bone.CalfL],
                    BoneTransform[Bone.FootL]
                },
                new Transform[3] {
                    BoneTransform[Bone.ThighL],
                    BoneTransform[Bone.ThighL],
                    BoneTransform[Bone.CalfL]
                },
                new Transform[3] {
                    BoneTransform[Bone.ThighL],
                    BoneTransform[Bone.CalfL],
                    BoneTransform[Bone.FootL]
                },
                true
            );
            DragPoint[Bone.CalfL] = ikChainLegL[1];
            DragPoint[Bone.FootL] = ikChainLegL[2];

            // Right Arm Dragpoint
            GameObject[] ikChainLegR = MakeIKChainDragPoint(
                new Transform[3] {
                    BoneTransform[Bone.ThighR],
                    BoneTransform[Bone.CalfR],
                    BoneTransform[Bone.FootR]
                },
                new Transform[3] {
                    BoneTransform[Bone.ThighR],
                    BoneTransform[Bone.ThighR],
                    BoneTransform[Bone.CalfR]
                },
                new Transform[3] {
                    BoneTransform[Bone.ThighR],
                    BoneTransform[Bone.CalfR],
                    BoneTransform[Bone.FootR]
                },
                true
            );
            DragPoint[Bone.CalfR] = ikChainLegR[1];
            DragPoint[Bone.FootR] = ikChainLegR[2];

            // destroy unused thigh dragpoints 
            GameObject.Destroy(ikChainLegL[0]);
            GameObject.Destroy(ikChainLegR[0]);

            // Spine Dragpoints
            for (Bone bone = Bone.Neck; bone <= Bone.ThighR; ++bone)
            {
                Transform pos = BoneTransform[bone];
                DragPoint[bone] = MakeDragPoint(PrimitiveType.Sphere, limbDragPointSize, transparentBlue);
                DragSpine dragSpine = DragPoint[bone].AddComponent<DragSpine>();
                dragSpine.Initialize(BoneTransform[bone], maid,
                    () => pos.position,
                    () => Vector3.zero
                );
                dragSpine.DragEvent += OnDragEvent;
            }

            // Finger Dragpoints
            for (Bone finger = Bone.Finger0L; finger <= Bone.Finger4R; finger += 4)
            {
                for (int i = 0; i < 3; i++)
                {
                    Bone bone = finger + 1 + i; // Bone.Finger01
                    DragPoint[bone] = MakeDragPoint(PrimitiveType.Sphere, fingerDragPointSize, transparentBlue);
                    Transform[] trans = new Transform[3] {
                        BoneTransform[bone - 1],
                        BoneTransform[bone - 1],
                        BoneTransform[bone]
                    };
                    Func<Vector3> pos = () => BoneTransform[bone].position;
                    bool baseFinger = i == 0;
                    DragJointFinger dragJointFinger = DragPoint[bone].AddComponent<DragJointFinger>();
                    dragJointFinger.Initialize(trans, baseFinger, maid, pos, () => Vector3.zero);
                    dragJointFinger.DragEvent += OnDragEvent;
                }
            }

            // Toe Dragpoints
            for (Bone toe = Bone.Toe0L; toe <= Bone.Toe2R; toe += 3)
            {
                for (int i = 0; i < 2; i++)
                {
                    Bone bone = toe + 1 + i; // Bone.Toe01
                    DragPoint[bone] = MakeDragPoint(PrimitiveType.Sphere, fingerDragPointSize, transparentBlue);
                    Transform[] trans = new Transform[3] {
                        BoneTransform[bone - 1],
                        BoneTransform[bone - 1],
                        BoneTransform[bone]
                    };
                    Func<Vector3> pos = () => BoneTransform[bone].position;
                    bool baseFinger = i == 0;
                    DragJointFinger dragJointFinger = DragPoint[bone].AddComponent<DragJointFinger>();
                    dragJointFinger.Initialize(trans, baseFinger, maid, pos, () => Vector3.zero);
                    dragJointFinger.DragEvent += OnDragEvent;
                }
            }

            ikModeOld = IKMode.None;
            ikMode = IKMode.None;

            UpdateIK();
        }

        private void InitializeBones()
        {
            BoneTransform = new Dictionary<Bone, Transform>()
            {
                [Bone.Head] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 Head", true),
                [Bone.Neck] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 Neck", true),
                [Bone.HeadNub] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 HeadNub", true),
                [Bone.IKHandL] = CMT.SearchObjName(maid.body0.m_Bones.transform, "_IK_handL", true),
                [Bone.IKHandR] = CMT.SearchObjName(maid.body0.m_Bones.transform, "_IK_handR", true),
                [Bone.MuneL] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Mune_L", true),
                [Bone.MuneSubL] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Mune_L_sub", true),
                [Bone.MuneR] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Mune_R", true),
                [Bone.MuneSubR] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Mune_R_sub", true),
                [Bone.Pelvis] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 Pelvis", true),
                [Bone.Hip] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01", true),
                [Bone.Spine] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 Spine", true),
                [Bone.Spine0a] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 Spine0a", true),
                [Bone.Spine1] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 Spine1", true),
                [Bone.Spine1a] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 Spine1a", true),
                [Bone.ClavicleL] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Clavicle", true),
                [Bone.ClavicleR] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Clavicle", true),
                [Bone.UpperArmL] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L UpperArm", true),
                [Bone.ForearmL] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Forearm", true),
                [Bone.HandL] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Hand", true),
                [Bone.UpperArmR] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R UpperArm", true),
                [Bone.ForearmR] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Forearm", true),
                [Bone.HandR] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Hand", true),
                [Bone.ThighL] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Thigh", true),
                [Bone.CalfL] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Calf", true),
                [Bone.FootL] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Foot", true),
                [Bone.ThighR] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Thigh", true),
                [Bone.CalfR] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Calf", true),
                [Bone.FootR] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Foot", true),
                // fingers
                [Bone.Finger0L] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Finger0", true),
                [Bone.Finger01L] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Finger01", true),
                [Bone.Finger02L] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Finger02", true),
                [Bone.Finger0NubL] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Finger0Nub", true),
                [Bone.Finger1L] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Finger1", true),
                [Bone.Finger11L] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Finger11", true),
                [Bone.Finger12L] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Finger12", true),
                [Bone.Finger1NubL] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Finger1Nub", true),
                [Bone.Finger2L] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Finger2", true),
                [Bone.Finger21L] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Finger21", true),
                [Bone.Finger22L] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Finger22", true),
                [Bone.Finger2NubL] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Finger2Nub", true),
                [Bone.Finger3L] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Finger3", true),
                [Bone.Finger31L] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Finger31", true),
                [Bone.Finger32L] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Finger32", true),
                [Bone.Finger3NubL] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Finger3Nub", true),
                [Bone.Finger4L] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Finger4", true),
                [Bone.Finger41L] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Finger41", true),
                [Bone.Finger42L] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Finger42", true),
                [Bone.Finger4NubL] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Finger4Nub", true),
                [Bone.Finger0R] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Finger0", true),
                [Bone.Finger01R] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Finger01", true),
                [Bone.Finger02R] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Finger02", true),
                [Bone.Finger0NubR] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Finger0Nub", true),
                [Bone.Finger1R] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Finger1", true),
                [Bone.Finger11R] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Finger11", true),
                [Bone.Finger12R] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Finger12", true),
                [Bone.Finger1NubR] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Finger1Nub", true),
                [Bone.Finger2R] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Finger2", true),
                [Bone.Finger21R] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Finger21", true),
                [Bone.Finger22R] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Finger22", true),
                [Bone.Finger2NubR] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Finger2Nub", true),
                [Bone.Finger3R] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Finger3", true),
                [Bone.Finger31R] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Finger31", true),
                [Bone.Finger32R] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Finger32", true),
                [Bone.Finger3NubR] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Finger3Nub", true),
                [Bone.Finger4R] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Finger4", true),
                [Bone.Finger41R] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Finger41", true),
                [Bone.Finger42R] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Finger42", true),
                [Bone.Finger4NubR] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Finger4Nub", true),
                // Toes
                [Bone.Toe0L] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Toe0", true),
                [Bone.Toe01L] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Toe01", true),
                [Bone.Toe0NubL] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Toe0Nub", true),
                [Bone.Toe1L] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Toe1", true),
                [Bone.Toe11L] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Toe11", true),
                [Bone.Toe1NubL] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Toe1Nub", true),
                [Bone.Toe2L] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Toe2", true),
                [Bone.Toe21L] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Toe21", true),
                [Bone.Toe2NubL] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 L Toe2Nub", true),
                [Bone.Toe0R] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Toe0", true),
                [Bone.Toe01R] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Toe01", true),
                [Bone.Toe0NubR] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Toe0Nub", true),
                [Bone.Toe1R] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Toe1", true),
                [Bone.Toe11R] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Toe11", true),
                [Bone.Toe1NubR] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Toe1Nub", true),
                [Bone.Toe2R] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Toe2", true),
                [Bone.Toe21R] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Toe21", true),
                [Bone.Toe2NubR] = CMT.SearchObjName(maid.body0.m_Bones.transform, "Bip01 R Toe2Nub", true)
            };
        }

        private void OnMeidoSelect(MeidoChangeEventArgs args)
        {
            SelectMaid?.Invoke(this, args);
        }

        private void OnDragEvent(object sender, EventArgs args)
        {
            this.meido.IsStop = true;
        }
    }
}
