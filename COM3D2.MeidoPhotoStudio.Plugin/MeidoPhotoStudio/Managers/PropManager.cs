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
        private List<DragDogu> doguList = new List<DragDogu>();
        private DragType dragTypeOld = DragType.None;
        private DragType currentDragType = DragType.None;
        private bool showGizmos = false;
        private enum DragType
        {
            None, Move, Rotate, Scale, Delete, Other
        }
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
            foreach (DragDogu dogu in doguList)
            {
                dogu.Delete -= DeleteDogu;
                GameObject.Destroy(dogu.gameObject);
            }
            doguList.Clear();
            CubeSmallChange -= OnCubeSmall;
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                //     showGizmos = !showGizmos;
                //     currentDragType = dragTypeOld = DragType.None;
                //     UpdateDragType();
            }

            if (CubeActive && (Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.X) || Input.GetKey(KeyCode.C)
                || Input.GetKey(KeyCode.D))
            )
            {
                currentDragType = DragType.Other;
            }
            else
            {
                currentDragType = DragType.None;
            }

            if (currentDragType != dragTypeOld) UpdateDragType();

            dragTypeOld = currentDragType;
        }

        private void UpdateDragType()
        {
            bool dragPointActive = (currentDragType == DragType.Other);
            foreach (DragDogu dogu in doguList)
            {
                dogu.SetDragProp(showGizmos, dragPointActive, dragPointActive);
            }
        }

        private GameObject GetDeploymentObject()
        {
            GameObject go = GameObject.Find("Deployment Object Parent");
            if (go == null) go = new GameObject("Deployment Object Parent");
            return go;
        }

        public void SpawnObject(string assetName)
        {
            // TODO: Add a couple more things to ignore list
            GameObject dogu = null;
            string doguName = Translation.Get("propNames", assetName, false);
            Vector3 doguPosition = new Vector3(0f, 0f, 0.5f);
            Vector3 doguScale = Vector3.one;

            if (assetName.EndsWith(".menu"))
            {
                dogu = MenuFileUtility.LoadModel(assetName);
                string handItem = Utility.HandItemToOdogu(assetName);
                if (Translation.Has("propNames", handItem))
                {
                    doguName = Translation.Get("propNames", handItem);
                }
            }
            else if (assetName.StartsWith("BG_"))
            {
                assetName = assetName.Remove(0, 3);
                GameObject obj = GameMain.Instance.BgMgr.CreateAssetBundle(assetName);
                if (obj == null)
                {
                    obj = (Resources.Load("BG/" + assetName) ?? Resources.Load("BG/2_0/" + assetName)) as GameObject;
                }

                if (obj != null)
                {
                    dogu = GameObject.Instantiate(obj);
                    doguPosition = Vector3.zero;
                    doguScale = Vector3.one * 0.1f;
                    doguName = Translation.Get("bgNames", "assetName");
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
                GameObject obj = GameMain.Instance.BgMgr.CreateAssetBundle(assetParts[0]);
                if (obj == null)
                {
                    obj = Resources.Load("BG/" + assetParts[0]) as GameObject;
                }

                GameObject bg = GameObject.Instantiate(obj);
                int num = int.Parse(assetParts[1]);
                dogu = bg.transform.GetChild(num).gameObject;
                dogu.transform.SetParent(null);
                GameObject.Destroy(bg);
                bg.SetActive(false);
            }
            else
            {
                GameObject obj = GameMain.Instance.BgMgr.CreateAssetBundle(assetName);

                if (obj == null) obj = Resources.Load("Prefab/" + assetName) as GameObject;

                dogu = GameObject.Instantiate(obj) as GameObject;
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
                // TODO: Figure out why some props aren't centred properly
                // Doesn't happen in MM but even after copy pasting the code, it doesn't work :/
                GameObject deploymentObject = GetDeploymentObject();
                GameObject finalDogu = new GameObject(doguName);

                dogu.transform.localScale = doguScale;

                dogu.transform.SetParent(finalDogu.transform, true);
                finalDogu.transform.SetParent(deploymentObject.transform, false);

                finalDogu.transform.position = doguPosition;

                GameObject dragPoint = BaseDrag.MakeDragPoint(
                    PrimitiveType.Cube, Vector3.one * 0.12f, BaseDrag.LightBlue
                );

                DragDogu dragDogu = dragPoint.AddComponent<DragDogu>();
                dragDogu.Initialize(finalDogu);
                dragDogu.Delete += DeleteDogu;
                dragDogu.SetDragProp(showGizmos, false, false);
                doguList.Add(dragDogu);
                dragDogu.DragPointScale = dragDogu.BaseScale * (CubeSmall ? 0.4f : 1f);
                OnDoguListChange();
            }
            else
            {
                Debug.LogError($"Could not spawn object '{assetName}'");
            }
        }

        public DragDogu GetDogu(int doguIndex)
        {
            if (doguList.Count == 0 || doguIndex >= doguList.Count || doguIndex < 0) return null;
            return doguList[doguIndex];
        }

        public void AttachProp(
            int doguIndex, DragPointManager.AttachPoint attachPoint, Meido meido, bool worldPositionStays = true
        )
        {
            if (doguList.Count == 0 || doguIndex >= doguList.Count || doguIndex < 0) return;
            AttachProp(doguList[doguIndex], attachPoint, meido, worldPositionStays);
        }

        private void AttachProp(
            DragDogu dragDogu, DragPointManager.AttachPoint attachPoint, Meido meido, bool worldPositionStays = true
        )
        {
            GameObject dogu = dragDogu.Dogu;

            Transform attachPointTransform = meido?.GetBoneTransform(attachPoint) ?? GetDeploymentObject().transform;

            dragDogu.attachPointInfo = new DragPointManager.AttachPointInfo(
                attachPoint: meido == null ? DragPointManager.AttachPoint.None : attachPoint,
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
            foreach (DragDogu dogu in doguList)
            {
                if (dogu.attachPointInfo.AttachPoint != DragPointManager.AttachPoint.None)
                {
                    dogu.Dogu.transform.SetParent(GetDeploymentObject().transform, true);
                }
            }
        }

        private void ReattachProps(object sender, EventArgs args)
        {
            foreach (DragDogu dragDogu in doguList)
            {
                Meido meido = this.meidoManager.GetMeido(dragDogu.attachPointInfo.MaidGuid);
                bool worldPositionStays = meido == null;
                AttachProp(dragDogu, dragDogu.attachPointInfo.AttachPoint, meido, worldPositionStays);
            }
        }

        private void DeleteDogu(object sender, EventArgs args)
        {
            doguList.RemoveAll(dragDogu =>
                {
                    if (dragDogu.DeleteMe)
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
            foreach (DragDogu dogu in doguList)
            {
                dogu.DragPointScale = dogu.BaseScale * (CubeSmall ? 0.4f : 1f);
            }
        }

        private void OnDoguListChange()
        {
            this.DoguListChange?.Invoke(this, EventArgs.Empty);
        }
    }
}
