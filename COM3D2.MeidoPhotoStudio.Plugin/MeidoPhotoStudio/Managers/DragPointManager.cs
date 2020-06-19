using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using ModKey = Utility.ModKey;
    internal class DragPointManager
    {
        enum IKMode
        {
            None, UpperLock, Mune, RotLocal, BodyTransform, FingerRotLocalY, FingerRotLocalXZ, BodySelect,
            UpperRot
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

        private static readonly Dictionary<IKMode, DragInfo[]> IKGroupBone = new Dictionary<IKMode, DragInfo[]>()
        {
            [IKMode.None] = new[] {
                DragInfo.DragBone(Bone.UpperArmL), DragInfo.DragBone(Bone.ForearmL), DragInfo.DragBone(Bone.HandL),
                DragInfo.DragBone(Bone.UpperArmR), DragInfo.DragBone(Bone.ForearmR), DragInfo.DragBone(Bone.HandR),
                DragInfo.DragBone(Bone.CalfL), DragInfo.DragBone(Bone.FootL), DragInfo.DragBone(Bone.CalfR),
                DragInfo.DragBone(Bone.FootR), DragInfo.DragBone(Bone.Neck), DragInfo.DragBone(Bone.Spine1a),
                DragInfo.DragBone(Bone.Spine1), DragInfo.DragBone(Bone.Spine0a), DragInfo.DragBone(Bone.Spine),
                DragInfo.DragBone(Bone.Hip)
            },
            [IKMode.UpperLock] = new[] {
                DragInfo.Gizmo(Bone.Neck), DragInfo.Gizmo(Bone.Spine1a), DragInfo.Gizmo(Bone.Spine1),
                DragInfo.Gizmo(Bone.Spine0a), DragInfo.Gizmo(Bone.Spine), DragInfo.DragBone(Bone.Hip),
                DragInfo.DragBone(Bone.HandR), DragInfo.DragBone(Bone.HandL), DragInfo.DragBone(Bone.FootL),
                DragInfo.DragBone(Bone.FootR)
            },
            [IKMode.RotLocal] = new[] {
                DragInfo.Gizmo(Bone.HandL), DragInfo.Gizmo(Bone.HandR), DragInfo.Gizmo(Bone.FootL),
                DragInfo.Gizmo(Bone.FootR), DragInfo.Gizmo(Bone.Hip)
            },
            [IKMode.Mune] = new[] {
                DragInfo.Gizmo(Bone.ForearmL), DragInfo.Gizmo(Bone.ForearmR), DragInfo.Gizmo(Bone.CalfL),
                DragInfo.Gizmo(Bone.CalfR), DragInfo.Drag(Bone.MuneL), DragInfo.Drag(Bone.MuneR),
                DragInfo.Drag(Bone.Head)
            },
            [IKMode.UpperRot] = new[] {
                DragInfo.Gizmo(Bone.UpperArmL), DragInfo.Gizmo(Bone.UpperArmR), DragInfo.Gizmo(Bone.ThighL),
                DragInfo.Gizmo(Bone.ThighR)
            },
            [IKMode.BodyTransform] = new[] { DragInfo.Drag(Bone.Body), DragInfo.Drag(Bone.Cube) },
            [IKMode.BodySelect] = new[] { DragInfo.Drag(Bone.Head), DragInfo.Drag(Bone.Body) },
            [IKMode.FingerRotLocalXZ] = IKGroup[IKMode.FingerRotLocalXZ]
                .Select(bone => DragInfo.DragBone(bone)).ToArray(),
            [IKMode.FingerRotLocalY] = IKGroup[IKMode.FingerRotLocalY]
                .Select(bone => DragInfo.DragBone(bone)).ToArray()
        };
        private Meido meido;
        private Maid maid;
        private Dictionary<Bone, BaseDrag> DragPoint;
        private Dictionary<Bone, Transform> BoneTransform;
        private IKMode ikMode;
        private IKMode ikModeOld = IKMode.None;
        public event EventHandler<MeidoUpdateEventArgs> SelectMaid;
        private bool active = false;
        public bool Active
        {
            get => active;
            set
            {
                if (this.active == value) return;
                this.active = value;
                this.SetActive(this.active);
            }
        }
        private bool isBone = false;
        public bool IsBone
        {
            get => isBone;
            set
            {
                if (this.isBone == value) return;
                this.isBone = value;
                this.SetBoneMode(this.isBone);
            }
        }
        private static bool cubeActive = false;

        public DragPointManager(Meido meido)
        {
            this.meido = meido;
            this.maid = meido.Maid;
            this.meido.BodyLoad += Initialize;
        }

        public void Destroy()
        {
            foreach (KeyValuePair<Bone, BaseDrag> dragPoint in DragPoint)
            {
                GameObject.Destroy(dragPoint.Value.gameObject);
            }
            BoneTransform.Clear();
            DragPoint.Clear();
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
            else if (Utility.GetModKey(ModKey.Alt))
            {
                bool shift = IsBone && Utility.GetModKey(ModKey.Shift);
                ikMode = shift ? IKMode.UpperRot : IKMode.RotLocal;
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

        private void Initialize(object sender, EventArgs args)
        {
            meido.BodyLoad -= Initialize;
            InitializeBones();
            InitializeDragPoints();
            this.Active = true;
            this.SetBoneMode(false);
        }

        private void SetBoneMode(bool active)
        {
            foreach (KeyValuePair<Bone, BaseDrag> dragPoint in DragPoint)
            {
                dragPoint.Value.IsBone = this.IsBone;
                if (!this.IsBone)
                {
                    dragPoint.Value.SetDragProp(false, true, dragPoint.Key >= Bone.Finger0L);
                }
            }
            UpdateIK();
        }

        private void SetActive(bool active)
        {
            if (active)
            {
                ikMode = ikModeOld = IKMode.None;
                ((DragHead)DragPoint[Bone.Head]).IsIK = true;
                UpdateIK();
            }
            else
            {
                foreach (KeyValuePair<Bone, BaseDrag> dragPoint in DragPoint)
                {
                    dragPoint.Value.gameObject.SetActive(false);
                }
                ((DragHead)DragPoint[Bone.Head]).IsIK = false;
                DragPoint[Bone.Head].SetDragProp(false, true, false);
                DragPoint[Bone.Body].SetDragProp(false, true, false);
                DragPoint[Bone.Cube].SetDragProp(false, true, true);
            }
        }

        private void UpdateIK()
        {
            if (this.Active)
            {
                if (this.IsBone) UpdateBoneIK();
                else
                {
                    foreach (KeyValuePair<Bone, BaseDrag> dragPoint in DragPoint)
                    {
                        dragPoint.Value.gameObject.SetActive(false);
                    }

                    foreach (Bone bone in IKGroup[ikMode])
                    {
                        DragPoint[bone].gameObject.SetActive(true);
                    }

                    if (ikMode == IKMode.BodyTransform)
                    {
                        DragPoint[Bone.Cube].gameObject.SetActive(cubeActive);
                    }
                }
            }
            else
            {
                if (ikMode == IKMode.BodySelect)
                {
                    DragPoint[Bone.Body].gameObject.SetActive(true);
                    DragPoint[Bone.Head].gameObject.SetActive(true);
                }
                else if (ikMode == IKMode.BodyTransform)
                {
                    DragPoint[Bone.Body].gameObject.SetActive(true);
                    DragPoint[Bone.Cube].gameObject.SetActive(cubeActive);
                }
                else if (ikMode == IKMode.UpperRot || ikMode == IKMode.RotLocal)
                {
                    DragPoint[Bone.Head].gameObject.SetActive(true);
                }
                else
                {
                    DragPoint[Bone.Body].gameObject.SetActive(false);
                    DragPoint[Bone.Head].gameObject.SetActive(false);
                    DragPoint[Bone.Cube].gameObject.SetActive(false);
                }
            }
        }

        private void UpdateBoneIK()
        {
            foreach (KeyValuePair<Bone, BaseDrag> dragPoint in DragPoint)
            {
                dragPoint.Value.gameObject.SetActive(false);
                dragPoint.Value.SetDragProp(false, false, dragPoint.Key >= Bone.Finger0L);
            }

            foreach (DragInfo info in IKGroupBone[ikMode])
            {
                BaseDrag drag = DragPoint[info.Bone];
                drag.gameObject.SetActive(true);
                drag.SetDragProp(info.GizmoActive, info.DragPointActive, info.DragPointVisible);
            }
        }

        private void OnSelectFace(object sender, EventArgs args)
        {
            OnMeidoSelect(new MeidoUpdateEventArgs(meido.ActiveSlot, true, false));
        }

        private void OnSelectBody(object sender, EventArgs args)
        {
            OnMeidoSelect(new MeidoUpdateEventArgs(meido.ActiveSlot, true, true));
        }

        private void OnSetDragPointScale(object sender, EventArgs args)
        {
            this.SetDragPointScale(maid.transform.localScale.x);
        }

        private void OnMeidoSelect(MeidoUpdateEventArgs args)
        {
            SelectMaid?.Invoke(this, args);
        }

        private void SetDragPointScale(float scale)
        {
            foreach (KeyValuePair<Bone, BaseDrag> kvp in DragPoint)
            {
                BaseDrag dragPoint = kvp.Value;
                dragPoint.DragPointScale = dragPoint.BaseScale * scale;
            }
        }

        // TODO: Rework this a little to reduce number of needed BaseDrag derived components
        private void InitializeDragPoints()
        {
            DragPoint = new Dictionary<Bone, BaseDrag>();

            Vector3 limbDragPointSize = Vector3.one * 0.12f;
            Vector3 limbDragPointSizeBone = Vector3.one * 0.07f;
            Vector3 fingerDragPointSize = Vector3.one * 0.015f;

            Func<Transform[], Transform[], Transform[], bool, BaseDrag[]> MakeIKChainDragPoint =
                (upper, middle, lower, leg) =>
            {
                GameObject[] dragPoints = new GameObject[3];
                for (int i = 0; i < dragPoints.Length; i++)
                {
                    dragPoints[i] =
                         BaseDrag.MakeDragPoint(PrimitiveType.Sphere, limbDragPointSize, BaseDrag.LightBlue);
                }

                return new BaseDrag[3] {
                    dragPoints[0].AddComponent<DragJointForearm>()
                        .Initialize(upper, false, meido, () => upper[2].position, () => Vector3.zero),
                    dragPoints[1].AddComponent<DragJointForearm>()
                        .Initialize(middle, leg, meido, () => middle[2].position, () => Vector3.zero),
                    dragPoints[2].AddComponent<DragJointHand>()
                        .Initialize(lower, leg, meido, () => lower[2].position, () => Vector3.zero)
                };
            };

            // TODO: Modify dragpoint sizes for each joint
            Action<Bone, Bone, int> MakeFingerDragPoint = (start, end, joints) =>
            {
                for (Bone it = start; it <= end; it += joints)
                {
                    for (int i = 0; i < joints - 1; i++)
                    {
                        Bone bone = it + 1 + i;
                        DragPoint[bone] = BaseDrag.MakeDragPoint(PrimitiveType.Sphere, fingerDragPointSize, BaseDrag.Blue)
                            .AddComponent<DragJointFinger>()
                            .Initialize(new Transform[3] {
                                BoneTransform[bone - 1],
                                BoneTransform[bone - 1],
                                BoneTransform[bone]
                                }, i == 0, meido, () => BoneTransform[bone].position, () => Vector3.zero
                            );
                        DragPoint[bone].gameObject.layer = 0;
                        DragPoint[bone].DragPointVisible = true;
                    }
                }
            };

            // Cube Dragpoint
            DragPoint[Bone.Cube] =
                BaseDrag.MakeDragPoint(PrimitiveType.Cube, new Vector3(0.12f, 0.12f, 0.12f), BaseDrag.Blue)
                .AddComponent<DragBody>()
                .Initialize(meido,
                    () => maid.transform.position,
                    () => maid.transform.eulerAngles
                );
            DragBody dragCube = (DragBody)DragPoint[Bone.Cube];
            dragCube.Scale += OnSetDragPointScale;
            dragCube.DragPointVisible = true;

            // Body Dragpoint
            DragPoint[Bone.Body] =
                BaseDrag.MakeDragPoint(PrimitiveType.Capsule, new Vector3(0.2f, 0.3f, 0.24f), BaseDrag.LightBlue)
                .AddComponent<DragBody>()
                .Initialize(meido,
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
            DragBody dragBody = (DragBody)DragPoint[Bone.Body];
            dragBody.Select += OnSelectBody;
            dragBody.Scale += OnSetDragPointScale;

            // Head Dragpoint
            DragPoint[Bone.Head] =
                BaseDrag.MakeDragPoint(PrimitiveType.Sphere, new Vector3(0.2f, 0.24f, 0.2f), BaseDrag.LightBlue)
                .AddComponent<DragHead>()
                .Initialize(BoneTransform[Bone.Neck], meido,
                () => new Vector3(
                    BoneTransform[Bone.Head].position.x,
                    (BoneTransform[Bone.Head].position.y * 1.2f + BoneTransform[Bone.HeadNub].position.y * 0.8f) / 2f,
                    BoneTransform[Bone.Head].position.z
                ),
                () => new Vector3(
                    BoneTransform[Bone.Head].eulerAngles.x,
                    BoneTransform[Bone.Head].eulerAngles.y,
                    BoneTransform[Bone.Head].eulerAngles.z + 90f

                )
            );
            DragHead dragHead = (DragHead)DragPoint[Bone.Head];
            dragHead.Select += OnSelectFace;

            // Torso Dragpoint
            DragPoint[Bone.Torso] =
                BaseDrag.MakeDragPoint(PrimitiveType.Capsule, new Vector3(0.2f, 0.19f, 0.24f), BaseDrag.LightBlue)
                .AddComponent<DragTorso>();
            Transform spineTrans1 = BoneTransform[Bone.Spine1];
            Transform spineTrans2 = BoneTransform[Bone.Spine1a];
            Transform[] spineParts = new Transform[4] {
                BoneTransform[Bone.Spine1a],
                BoneTransform[Bone.Spine1],
                BoneTransform[Bone.Spine0a],
                BoneTransform[Bone.Spine]
            };
            DragTorso dragTorso = (DragTorso)DragPoint[Bone.Torso];
            dragTorso.Initialize(spineParts, meido,
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

            // Pelvis Dragpoint
            DragPoint[Bone.Pelvis] =
                BaseDrag.MakeDragPoint(PrimitiveType.Capsule, new Vector3(0.2f, 0.15f, 0.24f), BaseDrag.LightBlue)
                .AddComponent<DragPelvis>();
            Transform pelvisTrans = BoneTransform[Bone.Pelvis];
            Transform spineTrans = BoneTransform[Bone.Spine];
            DragPelvis dragPelvis = (DragPelvis)DragPoint[Bone.Pelvis];
            dragPelvis.Initialize(BoneTransform[Bone.Pelvis], meido,
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

            // Left Mune Dragpoint
            DragPoint[Bone.MuneL] =
                BaseDrag.MakeDragPoint(PrimitiveType.Sphere, new Vector3(0.12f, 0.12f, 0.12f), BaseDrag.LightBlue)
                .AddComponent<DragMune>();
            Transform[] muneIKChainL = new Transform[3] {
                BoneTransform[Bone.MuneL],
                BoneTransform[Bone.MuneL],
                BoneTransform[Bone.MuneSubL]
            };
            DragMune dragMuneL = (DragMune)DragPoint[Bone.MuneL];
            dragMuneL.Initialize(muneIKChainL, meido,
                () => (BoneTransform[Bone.MuneL].position + BoneTransform[Bone.MuneSubL].position) / 2f,
                () => Vector3.zero
            );

            // Right Mune Dragpoint
            DragPoint[Bone.MuneR] =
                BaseDrag.MakeDragPoint(PrimitiveType.Sphere, new Vector3(0.12f, 0.12f, 0.12f), BaseDrag.LightBlue)
                .AddComponent<DragMune>();
            Transform[] muneIKChainR = new Transform[3] {
                BoneTransform[Bone.MuneR],
                BoneTransform[Bone.MuneR],
                BoneTransform[Bone.MuneSubR]
            };
            DragMune dragMuneR = (DragMune)DragPoint[Bone.MuneR];
            dragMuneR.Initialize(muneIKChainR, meido,
                () => (BoneTransform[Bone.MuneR].position + BoneTransform[Bone.MuneSubR].position) / 2f,
                () => Vector3.zero
            );

            // Left Arm Dragpoint
            BaseDrag[] ikChainArmL = MakeIKChainDragPoint(
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
            BaseDrag[] ikChainArmR = MakeIKChainDragPoint(
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
            BaseDrag[] ikChainLegL = MakeIKChainDragPoint(
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
            BaseDrag[] ikChainLegR = MakeIKChainDragPoint(
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
            GameObject.Destroy(ikChainLegL[0].gameObject);
            GameObject.Destroy(ikChainLegR[0].gameObject);

            // Spine Dragpoints
            for (Bone bone = Bone.Neck; bone <= Bone.ThighR; ++bone)
            {
                Transform pos = BoneTransform[bone];
                DragPoint[bone] = BaseDrag.MakeDragPoint(PrimitiveType.Sphere, Vector3.one * 0.04f, BaseDrag.LightBlue)
                    .AddComponent<DragSpine>()
                    .Initialize(BoneTransform[bone], false, meido,
                        () => pos.position,
                        () => Vector3.zero
                    );
            }

            // Hip DragPoint
            DragPoint[Bone.Hip] = BaseDrag.MakeDragPoint(PrimitiveType.Cube, Vector3.one * 0.045f, BaseDrag.LightBlue)
                .AddComponent<DragSpine>()
                .Initialize(BoneTransform[Bone.Hip], true, meido,
                    () => BoneTransform[Bone.Hip].position,
                    () => Vector3.zero
                );

            MakeFingerDragPoint(Bone.Finger0L, Bone.Finger4R, 4);
            MakeFingerDragPoint(Bone.Toe0L, Bone.Toe2R, 3);
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

        private struct DragInfo
        {
            public Bone Bone { get; private set; }
            public bool GizmoActive { get; private set; }
            public bool DragPointActive { get; private set; }
            public bool DragPointVisible { get; private set; }
            public DragInfo(Bone bone, bool gizmoActive, bool dragPointActive, bool dragPointVisible)
            {
                this.Bone = bone;
                this.GizmoActive = gizmoActive;
                this.DragPointActive = dragPointActive;
                this.DragPointVisible = dragPointVisible;
            }
            public static DragInfo Gizmo(Bone bone)
            {
                return new DragInfo(bone, true, false, false);
            }
            public static DragInfo Drag(Bone bone)
            {
                return new DragInfo(bone, false, true, false);
            }
            public static DragInfo DragBone(Bone bone)
            {
                return new DragInfo(bone, false, true, true);
            }
        }
    }
}
