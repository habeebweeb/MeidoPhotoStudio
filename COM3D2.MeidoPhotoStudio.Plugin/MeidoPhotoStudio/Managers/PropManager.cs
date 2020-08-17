using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;


namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class PropManager
    {
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
        public string[] PropNameList
        {
            get
            {
                return doguList.Count == 0
                    ? new[] { Translation.Get("systemMessage", "noProps") }
                    : doguList.Select(dogu => dogu.Name).ToArray();
            }
        }

        public PropManager(MeidoManager meidoManager)
        {
            this.meidoManager = meidoManager;
            this.meidoManager.BeginCallMeidos += DetachProps;
            this.meidoManager.EndCallMeidos += ReattachProps;
        }

        public void Activate()
        {
            CubeSmallChange += OnCubeSmall;
        }

        public void Deactivate()
        {
            foreach (DragPointDogu dogu in doguList)
            {
                dogu.Delete -= DeleteDogu;
                GameObject.Destroy(dogu.gameObject);
            }
            doguList.Clear();
            CubeSmallChange -= OnCubeSmall;
        }

        private GameObject GetDeploymentObject()
        {
            return GameObject.Find("Deployment Object Parent")
                ?? new GameObject("Deployment Object Parent");
        }

        public void SpawnModItemProp(MenuFileUtility.ModItem modItem)
        {
            GameObject dogu = MenuFileUtility.LoadModel(modItem);
            string name = modItem.Name;
            if (dogu != null) AttachDragPoint(dogu, name, new Vector3(0f, 0f, 0.5f));
        }

        public void SpawnMyRoomProp(MenuFileUtility.MyRoomItem item)
        {
            MyRoomCustom.PlacementData.Data data = MyRoomCustom.PlacementData.GetData(item.ID);
            GameObject dogu = GameObject.Instantiate(data.GetPrefab());
            string name = Translation.Get("myRoomPropNames", item.PrefabName);
            else Utility.LogInfo($"Could not load MyRoomCreative prop '{item.PrefabName}'");
        }

        public void SpawnBG(string assetName)
        {
            GameObject obj = GameMain.Instance.BgMgr.CreateAssetBundle(assetName)
                ?? Resources.Load<GameObject>("BG/" + assetName)
                ?? Resources.Load<GameObject>("BG/2_0/" + assetName);

            if (obj != null)
            {
                GameObject dogu = GameObject.Instantiate(obj);
                string name = Translation.Get("bgNames", assetName);
                dogu.transform.localScale = Vector3.one * 0.1f;
                AttachDragPoint(dogu, name, Vector3.zero);
            }
        }

        public void SpawnObject(string assetName)
        {
            // TODO: Add a couple more things to ignore list
            GameObject dogu = null;
            string doguName = Translation.Get("propNames", assetName, false);
            Vector3 doguPosition = new Vector3(0f, 0f, 0.5f);

            if (assetName.EndsWith(".menu"))
            {
                dogu = MenuFileUtility.LoadModel(assetName);
                string handItem = Utility.HandItemToOdogu(assetName);
                if (Translation.Has("propNames", handItem))
                {
                    doguName = Translation.Get("propNames", handItem);
                }
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

                GameObject bg = GameObject.Instantiate(obj);
                int num = int.Parse(assetParts[1]);
                dogu = bg.transform.GetChild(num).gameObject;
                dogu.transform.SetParent(null);
                GameObject.Destroy(bg);
                bg.SetActive(false);
            }
            else
            {
                GameObject obj = GameMain.Instance.BgMgr.CreateAssetBundle(assetName)
                    ?? Resources.Load<GameObject>("Prefab/" + assetName);

                dogu = GameObject.Instantiate<GameObject>(obj);
                dogu.transform.localPosition = Vector3.zero;

                MeshRenderer[] meshRenderers = dogu.GetComponentsInChildren<MeshRenderer>();
                for (int i = 0; i < meshRenderers.Length; i++)
                {
                    if (meshRenderers[i] != null
                        && meshRenderers[i].gameObject.name.ToLower().IndexOf("castshadow") < 0
                    )
                    {
                        meshRenderers[i].shadowCastingMode = ShadowCastingMode.Off;
                    }
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
                AttachDragPoint(dogu, doguName, doguPosition);
            }
            else
            {
                Utility.LogInfo($"Could not spawn object '{assetName}'");
            }
        }

        private void AttachDragPoint(GameObject dogu, string name, Vector3 position)
        {
            // TODO: Figure out why some props aren't centred properly
            // Doesn't happen in MM but even after copy pasting the code, it doesn't work :/
            GameObject deploymentObject = GetDeploymentObject();
            GameObject finalDogu = new GameObject(name);

            dogu.transform.SetParent(finalDogu.transform, true);
            finalDogu.transform.SetParent(deploymentObject.transform, false);

            finalDogu.transform.position = position;

            DragPointDogu dragDogu = DragPoint.Make<DragPointDogu>(
                PrimitiveType.Cube, Vector3.one * 0.12f, DragPoint.LightBlue
            );
            dragDogu.Initialize(() => finalDogu.transform.position, () => Vector3.zero);
            dragDogu.Set(finalDogu.transform);
            dragDogu.AddGizmo(scale: 0.45f, mode: CustomGizmo.GizmoMode.World);
            dragDogu.ConstantScale = true;
            dragDogu.Delete += DeleteDogu;
            dragDogu.DragPointScale = CubeSmall ? DragPointGeneral.smallCube : 1f;

            doguList.Add(dragDogu);
            OnDoguListChange();
        }

        public DragPointDogu GetDogu(int doguIndex)
        {
            if (doguList.Count == 0 || doguIndex >= doguList.Count || doguIndex < 0) return null;
            return doguList[doguIndex];
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

            Transform attachPointTransform = meido?.GetBoneTransform(attachPoint) ?? GetDeploymentObject().transform;

            dragDogu.attachPointInfo = new AttachPointInfo(
                attachPoint: meido == null ? AttachPoint.None : attachPoint,
                maidGuid: meido == null ? String.Empty : meido.Maid.status.guid,
                maidIndex: meido == null ? -1 : meido.ActiveSlot
            );

            worldPositionStays = meido == null ? true : worldPositionStays;

            Vector3 position = dogu.transform.position;
            Quaternion rotation = dogu.transform.rotation;

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

            if (meido == null) Utility.FixGameObjectScale(dogu);
        }

        private void DetachProps(object sender, EventArgs args)
        {
            foreach (DragPointDogu dogu in doguList)
            {
                if (dogu.attachPointInfo.AttachPoint != AttachPoint.None)
                {
                    dogu.MyObject.SetParent(GetDeploymentObject().transform, true);
                }
            }
        }

        private void ReattachProps(object sender, EventArgs args)
        {
            foreach (DragPointDogu dragDogu in doguList)
            {
                Meido meido = this.meidoManager.GetMeido(dragDogu.attachPointInfo.MaidGuid);
                bool worldPositionStays = meido == null;
                AttachProp(dragDogu, dragDogu.attachPointInfo.AttachPoint, meido, worldPositionStays);
            }
        }

        private void DeleteDogu(object sender, EventArgs args)
        {
            DragPointDogu dogu = (DragPointDogu)sender;
            doguList.RemoveAll(dragDogu =>
                {
                    if (dragDogu == dogu)
                    {
                        GameObject.Destroy(dragDogu.gameObject);
                        return true;
                    }
                    return false;
                }
            );
            OnDoguListChange();
        }

        private void OnCubeSmall(object sender, EventArgs args)
        {
            foreach (DragPointDogu dogu in doguList)
            {
                dogu.DragPointScale = CubeSmall ? DragPointGeneral.smallCube : 1f;
            }
        }

        private void OnDoguListChange()
        {
            this.DoguListChange?.Invoke(this, EventArgs.Empty);
        }
    }
}
