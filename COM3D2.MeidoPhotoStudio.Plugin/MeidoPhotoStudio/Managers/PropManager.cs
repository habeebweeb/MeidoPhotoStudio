using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;


namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class PropManager
    {
        private List<DragDogu> doguList = new List<DragDogu>();
        private DragType dragTypeOld = DragType.None;
        private DragType currentDragType = DragType.None;
        private bool showGizmos = false;
        enum DragType
        {
            None, Move, Rotate, Scale, Delete, Other
        }

        public void Activate() { }

        public void Deactivate()
        {
            foreach (DragDogu dogu in doguList)
            {
                GameObject.Destroy(dogu.gameObject);
            }
            doguList.Clear();
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                showGizmos = !showGizmos;
                currentDragType = dragTypeOld = DragType.None;
                UpdateDragType();
            }

            if (Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.X) || Input.GetKey(KeyCode.C)
                || Input.GetKey(KeyCode.D)
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
            bool dragPointActive = currentDragType == DragType.Other;
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
            if (assetName.StartsWith("mirror"))
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
                    if (meshRenderers[i] != null)
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
                GameObject finalDogu = new GameObject();

                dogu.transform.SetParent(finalDogu.transform, true);
                finalDogu.transform.SetParent(deploymentObject.transform, false);

                finalDogu.transform.position = new Vector3(0f, 0f, 0.5f);

                GameObject dragPoint = BaseDrag.MakeDragPoint(
                    PrimitiveType.Cube, Vector3.one * 0.12f, BaseDrag.LightBlue
                );

                DragDogu dragDogu = dragPoint.AddComponent<DragDogu>();
                dragDogu.Initialize(finalDogu);
                dragDogu.Delete += (s, a) => DeleteDogu();
                dragDogu.SetDragProp(showGizmos, false, false);
                doguList.Add(dragDogu);
            }
        }

        private void DeleteDogu()
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
        }
    }
}
