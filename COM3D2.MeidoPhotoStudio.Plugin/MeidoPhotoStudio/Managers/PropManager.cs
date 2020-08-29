using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using BepInEx.Configuration;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using static MenuFileUtility;
    internal class PropManager : IManager, ISerializable
    {
        public const string header = "PROP";
        private static readonly ConfigEntry<bool> modItemsOnly;
        public static bool ModItemsOnly => modItemsOnly.Value;
        private MeidoManager meidoManager;
        private static bool cubeActive = true;
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
        private static event EventHandler CubeActiveChange;
        private static event EventHandler CubeSmallChange;
        private List<DragPointDogu> doguList = new List<DragPointDogu>();
        public int DoguCount => doguList.Count;
        public event EventHandler DoguListChange;
        public event EventHandler DoguSelectChange;
        public string[] PropNameList
        {
            get
            {
                return doguList.Count == 0
                    ? new[] { Translation.Get("systemMessage", "noProps") }
                    : doguList.Select(dogu => dogu.Name).ToArray();
            }
        }
        public int CurrentDoguIndex { get; private set; } = 0;
        public DragPointDogu CurrentDogu => DoguCount == 0 ? null : doguList[CurrentDoguIndex];

        static PropManager()
        {
            modItemsOnly = Configuration.Config.Bind<bool>(
                "Prop", "ModItemsOnly",
                false,
                "Disable waiting for and loading base game clothing"
            );
        }

        public PropManager(MeidoManager meidoManager)
        {
            this.meidoManager = meidoManager;
            this.meidoManager.BeginCallMeidos += DetachProps;
            this.meidoManager.EndCallMeidos += OnEndCall;
        }

        public void Serialize(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(header);
            binaryWriter.Write(doguList.Count);
            foreach (DragPointDogu dogu in doguList)
            {
                binaryWriter.Write(dogu.assetName);
                AttachPointInfo info = dogu.attachPointInfo;
                info.Serialize(binaryWriter);
                binaryWriter.WriteVector3(dogu.MyObject.position);
                binaryWriter.WriteQuaternion(dogu.MyObject.rotation);
                binaryWriter.WriteVector3(dogu.MyObject.localScale);
            }
        }

        public void Deserialize(BinaryReader binaryReader)
        {
            Dictionary<string, string> modToModPath = null;
            ClearDogu();
            int numberOfProps = binaryReader.ReadInt32();
            for (int i = 0; i < numberOfProps; i++)
            {
                string assetName = binaryReader.ReadString();
                bool result = false;

                if (assetName.EndsWith(".menu") && assetName.Contains('#') && modToModPath == null)
                {
                    modToModPath = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
                    foreach (string mod in Menu.GetModFiles()) modToModPath.Add(Path.GetFileName(mod), mod);
                }

                result = SpawnFromAssetString(assetName, modToModPath);

                AttachPointInfo info = AttachPointInfo.Deserialize(binaryReader);

                Vector3 position = binaryReader.ReadVector3();
                Quaternion rotation = binaryReader.ReadQuaternion();
                Vector3 scale = binaryReader.ReadVector3();
                if (result)
                {
                    DragPointDogu dogu = doguList[i];
                    Transform obj = dogu.MyObject;
                    obj.position = position;
                    obj.rotation = rotation;
                    obj.localScale = scale;
                    dogu.attachPointInfo = info;
                }
            }
            CurrentDoguIndex = 0;
            GameMain.Instance.StartCoroutine(DeserializeAttach());
        }

        private System.Collections.IEnumerator DeserializeAttach()
        {
            yield return new WaitForEndOfFrame();

            foreach (DragPointDogu dogu in doguList)
            {
                AttachPointInfo info = dogu.attachPointInfo;
                if (info.AttachPoint != AttachPoint.None)
                {
                    Meido parent = meidoManager.GetMeido(info.MaidIndex);
                    if (parent != null)
                    {
                        Transform obj = dogu.MyObject;
                        Vector3 position = obj.position;
                        Vector3 scale = obj.localScale;
                        Quaternion rotation = obj.rotation;

                        Transform point = parent.IKManager.GetAttachPointTransform(info.AttachPoint);
                        dogu.MyObject.SetParent(point, true);
                        info = new AttachPointInfo(
                            info.AttachPoint,
                            parent.Maid.status.guid,
                            parent.Slot
                        );
                        dogu.attachPointInfo = info;

                        obj.position = position;
                        obj.localScale = scale;
                        obj.rotation = rotation;
                    }
                }
            }
        }

        public void Activate()
        {
            CubeSmallChange += OnCubeSmall;
            CubeActiveChange += OnCubeActive;
        }

        public void Deactivate()
        {
            ClearDogu();
            CubeSmallChange -= OnCubeSmall;
            CubeActiveChange -= OnCubeActive;
        }

        public void Update() { }

        private GameObject GetDeploymentObject()
        {
            return GameObject.Find("Deployment Object Parent")
                ?? new GameObject("Deployment Object Parent");
        }

        public bool SpawnModItemProp(ModItem modItem)
        {
            GameObject dogu = MenuFileUtility.LoadModel(modItem);
            string name = modItem.MenuFile;
            if (modItem.IsOfficialMod) name = Path.GetFileName(name);
            if (dogu != null) AttachDragPoint(dogu, modItem.ToString(), name, new Vector3(0f, 0f, 0.5f));
            return dogu != null;
        }

        public bool SpawnMyRoomProp(MyRoomItem item)
        {
            MyRoomCustom.PlacementData.Data data = MyRoomCustom.PlacementData.GetData(item.ID);
            GameObject dogu = GameObject.Instantiate(data.GetPrefab());
            string name = Translation.Get("myRoomPropNames", item.PrefabName);
            if (dogu != null)
            {
                GameObject finalDogu = new GameObject();
                dogu.transform.SetParent(finalDogu.transform, true);
                finalDogu.transform.SetParent(GetDeploymentObject().transform, false);
                AttachDragPoint(finalDogu, item.ToString(), name, new Vector3(0f, 0f, 0.5f));
            }
            else Utility.LogInfo($"Could not load MyRoomCreative prop '{item.PrefabName}'");
            return dogu != null;
        }

        public bool SpawnBG(string assetName)
        {
            if (assetName.StartsWith("BG_")) assetName = assetName.Substring(3);
            GameObject obj = GameMain.Instance.BgMgr.CreateAssetBundle(assetName)
                ?? Resources.Load<GameObject>("BG/" + assetName)
                ?? Resources.Load<GameObject>("BG/2_0/" + assetName);

            if (obj != null)
            {
                GameObject dogu = GameObject.Instantiate(obj);
                string name = Translation.Get("bgNames", assetName);
                dogu.transform.localScale = Vector3.one * 0.1f;
                AttachDragPoint(dogu, $"BG_{assetName}", name, new Vector3(0f, 0f, 0.5f));
            }
            return obj != null;
        }

        public bool SpawnObject(string assetName)
        {
            // TODO: Add a couple more things to ignore list
            GameObject dogu = null;
            string doguName = Translation.Get("propNames", assetName, false);
            Vector3 doguPosition = new Vector3(0f, 0f, 0.5f);

            if (assetName.EndsWith(".menu"))
            {
                dogu = MenuFileUtility.LoadModel(assetName);
                string handItem = Utility.HandItemToOdogu(assetName);
                if (Translation.Has("propNames", handItem)) doguName = Translation.Get("propNames", handItem);
            }
            else if (assetName.StartsWith("mirror"))
            {
                Material mirrorMaterial = new Material(Shader.Find("Mirror"));
                dogu = GameObject.CreatePrimitive(PrimitiveType.Plane);
                Renderer mirrorRenderer = dogu.GetComponent<Renderer>();
                mirrorRenderer.material = mirrorMaterial;
                mirrorRenderer.enabled = true;
                MirrorReflection2 mirrorReflection = dogu.AddComponent<MirrorReflection2>();
                mirrorReflection.m_TextureSize = 2048;

                Vector3 localPosition = new Vector3(0f, 0.96f, 0f);
                dogu.transform.Rotate(dogu.transform.right, 90f);

                switch (assetName)
                {
                    case "mirror1":
                        dogu.transform.localScale = new Vector3(0.2f, 0.4f, 0.2f);
                        break;
                    case "mirror2":
                        dogu.transform.localScale = new Vector3(0.1f, 0.4f, 0.2f);
                        break;
                    case "mirror3":
                        localPosition.y = 0.85f;
                        dogu.transform.localScale = new Vector3(0.03f, 0.18f, 0.124f);
                        break;
                }
                dogu.transform.localPosition = localPosition;
            }
            else if (assetName.IndexOf(':') >= 0)
            {
                string[] assetParts = assetName.Split(':');
                GameObject obj = GameMain.Instance.BgMgr.CreateAssetBundle(assetParts[0])
                    ?? Resources.Load<GameObject>("BG/" + assetParts[0]);
                try
                {
                    GameObject bg = GameObject.Instantiate(obj);
                    int num = int.Parse(assetParts[1]);
                    dogu = bg.transform.GetChild(num).gameObject;
                    dogu.transform.SetParent(null);
                    GameObject.Destroy(bg);
                }
                catch { }
            }
            else
            {
                GameObject obj = GameMain.Instance.BgMgr.CreateAssetBundle(assetName)
                    ?? Resources.Load<GameObject>("Prefab/" + assetName);
                try
                {
                    dogu = GameObject.Instantiate<GameObject>(obj);
                    dogu.transform.localPosition = Vector3.zero;

                    MeshRenderer[] meshRenderers = dogu.GetComponentsInChildren<MeshRenderer>();
                    for (int i = 0; i < meshRenderers.Length; i++)
                    {
                        if (meshRenderers[i] != null
                            && meshRenderers[i].gameObject.name.ToLower().IndexOf("castshadow") < 0
                        ) meshRenderers[i].shadowCastingMode = ShadowCastingMode.Off;
                    }

                    Collider collider = dogu.transform.GetComponent<Collider>();
                    if (collider != null) collider.enabled = false;
                    foreach (Transform transform in dogu.transform)
                    {
                        collider = transform.GetComponent<Collider>();
                        if (collider != null)
                        {
                            collider.enabled = false;
                        }
                    }
                }
                catch { }
                #region particle system experiment
                // if (asset.StartsWith("Particle/"))
                // {
                //     ParticleSystem particleSystem = go.GetComponent<ParticleSystem>();
                //     if (particleSystem != null)
                //     {
                //         ParticleSystem.MainModule main;
                //         main = particleSystem.main;
                //         main.loop = true;
                //         main.duration = Mathf.Infinity;

                //         ParticleSystem[] particleSystems = particleSystem.GetComponents<ParticleSystem>();
                //         foreach (ParticleSystem part in particleSystems)
                //         {
                //             ParticleSystem.EmissionModule emissionModule = part.emission;
                //             ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[emissionModule.burstCount];
                //             emissionModule.GetBursts(bursts);
                //             for (int i = 0; i < bursts.Length; i++)
                //             {
                //                 bursts[i].cycleCount = Int32.MaxValue;
                //             }
                //             emissionModule.SetBursts(bursts);
                //             main = part.main;
                //             main.loop = true;
                //             main.duration = Mathf.Infinity;
                //         }
                //     }
                // }
                #endregion
            }

            if (dogu != null)
            {
                AttachDragPoint(dogu, assetName, doguName, doguPosition);
                return true;
            }
            else
            {
                Utility.LogInfo($"Could not spawn object '{assetName}'");
            }
            return false;
        }

        private bool SpawnFromAssetString(string assetName, Dictionary<string, string> modDict = null)
        {
            bool result = false;
            if (assetName.EndsWith(".menu"))
            {
                if (assetName.Contains('#'))
                {
                    string[] assetParts = assetName.Split('#');
                    string menuFile = modDict == null ? Menu.GetModPathFileName(assetParts[0]) : modDict[assetParts[0]];

                    ModItem item = ModItem.OfficialMod(menuFile);
                    item.BaseMenuFile = assetParts[1];
                    result = SpawnModItemProp(item);
                }
                else if (assetName.StartsWith("handitem")) result = SpawnObject(assetName);
                else result = SpawnModItemProp(ModItem.Mod(assetName));
            }
            else if (assetName.StartsWith("MYR_"))
            {
                string[] assetParts = assetName.Split('#');
                int id = int.Parse(assetParts[0].Substring(4));
                string prefabName;
                if (assetParts.Length == 2 && !string.IsNullOrEmpty(assetParts[1])) prefabName = assetParts[1];
                else
                {
                    // deserialize modifiedMM and maybe MM 23.0+.
                    MyRoomCustom.PlacementData.Data data = MyRoomCustom.PlacementData.GetData(id);
                    prefabName = !string.IsNullOrEmpty(data.resourceName) ? data.resourceName : data.assetName;
                }
                result = SpawnMyRoomProp(new MyRoomItem() { ID = id, PrefabName = prefabName });
            }
            else if (assetName.StartsWith("BG_")) result = SpawnBG(assetName);
            else result = SpawnObject(assetName);

            return result;
        }

        private void AttachDragPoint(GameObject dogu, string assetName, string name, Vector3 position)
        {
            // TODO: Figure out why some props aren't centred properly
            // Doesn't happen in MM but even after copy pasting the code, it doesn't work :/
            dogu.name = name;
            dogu.transform.position = position;

            DragPointDogu dragDogu = DragPoint.Make<DragPointDogu>(
                PrimitiveType.Cube, Vector3.one * 0.12f, DragPoint.LightBlue
            );
            dragDogu.Initialize(() => dogu.transform.position, () => Vector3.zero);
            dragDogu.Set(dogu.transform);
            dragDogu.AddGizmo(scale: 0.45f, mode: CustomGizmo.GizmoMode.World);
            dragDogu.ConstantScale = true;
            dragDogu.Delete += DeleteDogu;
            dragDogu.Select += SelectDogu;
            dragDogu.DragPointScale = CubeSmall ? DragPointGeneral.smallCube : 1f;
            dragDogu.assetName = assetName;

            doguList.Add(dragDogu);
            OnDoguListChange();
        }

        public void SetCurrentDogu(int doguIndex)
        {
            if (doguIndex >= 0 && doguIndex < DoguCount)
            {
                this.CurrentDoguIndex = doguIndex;
                this.DoguSelectChange?.Invoke(this, EventArgs.Empty);
            }
        }

        public void RemoveDogu(int doguIndex)
        {
            if (doguIndex >= 0 && doguIndex < DoguCount)
            {
                DestroyDogu(doguList[doguIndex]);
                doguList.RemoveAt(doguIndex);
                CurrentDoguIndex = Utility.Bound(CurrentDoguIndex, 0, DoguCount - 1);
                OnDoguListChange();
            }
        }

        public void CopyDogu(int doguIndex)
        {
            if (doguIndex >= 0 && doguIndex < DoguCount)
            {
                SpawnFromAssetString(doguList[doguIndex].assetName);
            }
        }

        public void AttachProp(
            int doguIndex, AttachPoint attachPoint, Meido meido, bool worldPositionStays = true
        )
        {
            if (doguList.Count == 0 || doguIndex >= doguList.Count || doguIndex < 0) return;
            AttachProp(doguList[doguIndex], attachPoint, meido, worldPositionStays);
        }

        private void AttachProp(
            DragPointDogu dragDogu, AttachPoint attachPoint, Meido meido, bool worldPositionStays = true
        )
        {
            GameObject dogu = dragDogu.MyGameObject;

            Transform attachPointTransform = meido?.IKManager.GetAttachPointTransform(attachPoint);
            // ?? GetDeploymentObject().transform;

            dragDogu.attachPointInfo = new AttachPointInfo(
                attachPoint: meido == null ? AttachPoint.None : attachPoint,
                maidGuid: meido == null ? String.Empty : meido.Maid.status.guid,
                maidIndex: meido == null ? -1 : meido.Slot
            );

            Vector3 position = dogu.transform.position;
            Quaternion rotation = dogu.transform.rotation;
            Vector3 scale = dogu.transform.localScale;

            dogu.transform.SetParent(attachPointTransform, worldPositionStays);

            if (worldPositionStays)
            {
                dogu.transform.position = position;
                dogu.transform.rotation = rotation;
            }
            else
            {
                dogu.transform.localPosition = Vector3.zero;
                dogu.transform.rotation = Quaternion.identity;
            }

            dogu.transform.localScale = scale;

            if (meido == null) Utility.FixGameObjectScale(dogu);
        }

        private void DetachProps(object sender, EventArgs args)
        {
            foreach (DragPointDogu dogu in doguList)
            {
                if (dogu.attachPointInfo.AttachPoint != AttachPoint.None)
                {
                    dogu.MyObject.SetParent(null, /*GetDeploymentObject().transform*/ true);
                }
            }
        }

        private void ClearDogu()
        {
            for (int i = DoguCount - 1; i >= 0; i--)
            {
                DestroyDogu(doguList[i]);
            }
            doguList.Clear();
            CurrentDoguIndex = 0;
        }

        private void OnEndCall(object sender, EventArgs args) => ReattachProps(useGuid: true);

        private void ReattachProps(bool useGuid, bool forceStay = false)
        {
            foreach (DragPointDogu dragDogu in doguList)
            {
                AttachPointInfo info = dragDogu.attachPointInfo;
                Meido meido = useGuid
                    ? this.meidoManager.GetMeido(info.MaidGuid)
                    : this.meidoManager.GetMeido(info.MaidIndex);
                bool worldPositionStays = forceStay || meido == null;
                AttachProp(dragDogu, dragDogu.attachPointInfo.AttachPoint, meido, worldPositionStays);
            }
        }

        private void DeleteDogu(object sender, EventArgs args)
        {
            DragPointDogu dogu = (DragPointDogu)sender;
            RemoveDogu(doguList.FindIndex(dragDogu => dragDogu == dogu));
        }

        private void DestroyDogu(DragPointDogu dogu)
        {
            if (dogu == null) return;
            dogu.Delete -= DeleteDogu;
            dogu.Select -= SelectDogu;
            GameObject.Destroy(dogu.gameObject);
        }

        private void SelectDogu(object sender, EventArgs args)
        {
            DragPointDogu dogu = (DragPointDogu)sender;
            SetCurrentDogu(doguList.IndexOf(dogu));
        }

        private void OnCubeSmall(object sender, EventArgs args)
        {
            foreach (DragPointDogu dogu in doguList)
            {
                dogu.DragPointScale = CubeSmall ? DragPointGeneral.smallCube : 1f;
            }
        }

        private void OnCubeActive(object sender, EventArgs args)
        {
            foreach (DragPointDogu dragPoint in doguList)
            {
                dragPoint.gameObject.SetActive(CubeActive);
            }
        }

        private void OnDoguListChange()
        {
            this.DoguListChange?.Invoke(this, EventArgs.Empty);
        }
    }
}
