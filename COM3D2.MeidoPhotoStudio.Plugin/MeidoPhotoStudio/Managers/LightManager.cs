using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using static MPSLight;
    internal class LightManager
    {
        private List<MPSLight> LightList { get; set; } = new List<MPSLight>();
        private int selectedLightIndex = 0;
        public int SelectedLightIndex
        {
            get => selectedLightIndex;
            set
            {
                selectedLightIndex = Mathf.Clamp(value, 0, LightList.Count - 1);
                LightList[SelectedLightIndex].isActiveLight = true;
            }
        }
        public string[] LightNameList => LightList.Select(light => LightName(light.Name)).ToArray();
        public string ActiveLightName => LightName(LightList[SelectedLightIndex].Name);
        public MPSLight CurrentLight
        {
            get
            {
                return LightList[SelectedLightIndex];
            }
        }
        public event EventHandler Rotate;
        public event EventHandler Scale;
        public event EventHandler ListModified;
        public event EventHandler Select;
        private DragType dragTypeOld = DragType.None;
        private DragType currentDragType = DragType.None;
        private bool gizmoActive = false;
        enum DragType
        {
            None, Move, Rotate, Scale, Delete, Select
        }

        public void Activate()
        {
            GameMain.Instance.MainCamera.GetComponent<Camera>().backgroundColor = Color.black;
            AddLight(GameMain.Instance.MainLight.gameObject, true);
        }

        public void Deactivate()
        {
            for (int i = 0; i < LightList.Count; i++)
            {
                DestroyLight(LightList[i]);
            }
            selectedLightIndex = 0;
            LightList.Clear();

            GameMain.Instance.MainLight.Reset();

            Light mainLight = GameMain.Instance.MainLight.GetComponent<Light>();
            mainLight.type = LightType.Directional;
            MPSLight.SetLightProperties(mainLight, new LightProperty());
        }

        public void Update()
        {
            if (Input.GetKey(KeyCode.Z))
            {
                currentDragType = DragType.Move;
            }
            else if (Input.GetKey(KeyCode.X))
            {
                currentDragType = DragType.Rotate;
            }
            else if (Input.GetKey(KeyCode.C))
            {
                currentDragType = DragType.Scale;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                currentDragType = DragType.Delete;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                currentDragType = DragType.Select;
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
            foreach (MPSLight light in LightList)
            {
                bool active;
                if (currentDragType >= DragType.Delete || currentDragType == DragType.None)
                {
                    if (currentDragType == DragType.Delete)
                    {
                        active = !light.IsMain;
                    }
                    else
                    {
                        active = currentDragType == DragType.Select;
                    }
                }
                else
                {
                    if (light.SelectedLightType == MPSLightType.Normal)
                    {
                        active = false;
                    }
                    else if (light.SelectedLightType == MPSLightType.Point)
                    {
                        active = currentDragType != DragType.Rotate;
                    }
                    else
                    {
                        active = true;
                    }
                }
                light.DragLight.SetDragProp(gizmoActive && active, active, active);
            }
        }

        public void AddLight(GameObject lightGo = null, bool isMain = false)
        {
            MPSLight light = new MPSLight(lightGo, isMain);
            light.Rotate += OnRotate;
            light.Scale += OnScale;
            light.Delete += OnDelete;
            light.Select += OnSelect;
            LightList.Add(light);

            LightList[SelectedLightIndex].isActiveLight = false;
            SelectedLightIndex = LightList.Count;
            OnListModified();
        }

        public void DeleteActiveLight()
        {
            if (selectedLightIndex == 0) return;

            DeleteLight(SelectedLightIndex);
        }

        public void DeleteLight(int lightIndex, bool noUpdate = false)
        {
            if (lightIndex == 0) return;

            DestroyLight(LightList[lightIndex]);
            LightList.RemoveAt(lightIndex);

            if (lightIndex <= SelectedLightIndex) SelectedLightIndex -= 1;

            if (noUpdate) return;
            OnListModified();
        }

        public void SetColourModeActive(bool isColourMode)
        {
            LightList[0].IsColourMode = isColourMode;
        }

        public void ClearLights()
        {
            for (int i = LightList.Count - 1; i > 0; i--)
            {
                DeleteLight(i);
            }
            selectedLightIndex = 0;
        }

        private void DestroyLight(MPSLight light)
        {
            light.Rotate -= OnRotate;
            light.Scale -= OnScale;
            light.Delete -= OnDelete;
            light.Select -= OnSelect;
            light.Destroy();
        }

        private string LightName(string name)
        {
            return Translation.Get("lightType", name);
        }

        private void OnDelete(object sender, EventArgs args)
        {
            MPSLight theLight = (MPSLight)sender;
            for (int i = 1; i < LightList.Count; i++)
            {
                MPSLight light = LightList[i];
                if (light == theLight)
                {
                    DeleteLight(i);
                    return;
                }
            }
        }

        private void OnRotate(object sender, EventArgs args)
        {
            OnTransformEvent((MPSLight)sender, Rotate);
        }

        private void OnScale(object sender, EventArgs args)
        {
            OnTransformEvent((MPSLight)sender, Scale);
        }

        private void OnTransformEvent(MPSLight light, EventHandler handler)
        {
            if (light.isActiveLight)
            {
                handler?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnSelect(object sender, EventArgs args)
        {
            MPSLight theLight = (MPSLight)sender;
            int select = LightList.FindIndex(light => light == theLight);
            if (select >= 0)
            {
                this.SelectedLightIndex = select;
                this.Select?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnListModified()
        {
            ListModified?.Invoke(this, EventArgs.Empty);
        }
    }
}
