using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class LightManager : IManager
    {
        public const string header = "LIGHT";
        private List<DragPointLight> lightList = new List<DragPointLight>();
        private int selectedLightIndex = 0;
        public int SelectedLightIndex
        {
            get => selectedLightIndex;
            set
            {
                selectedLightIndex = Mathf.Clamp(value, 0, lightList.Count - 1);
                lightList[SelectedLightIndex].IsActiveLight = true;
            }
        }
        public string[] LightNameList => lightList.Select(light => LightName(light.Name)).ToArray();
        public string ActiveLightName => LightName(lightList[SelectedLightIndex].Name);
        public DragPointLight CurrentLight
        {
            get
            {
                return lightList[SelectedLightIndex];
            }
        }
        public event EventHandler Rotate;
        public event EventHandler Scale;
        public event EventHandler ListModified;
        public event EventHandler Select;
        // TODO: enabling and disabling gizmos for a variety of dragpoints

        public void Serialize(System.IO.BinaryWriter binaryWriter)
        {
            binaryWriter.Write(header);
            binaryWriter.Write(lightList.Count);
            foreach (DragPointLight light in lightList)
            {
                light.Serialize(binaryWriter);
            }
        }

        public void Deserialize(System.IO.BinaryReader binaryReader)
        {
            ClearLights();
            int numberOfLights = binaryReader.ReadInt32();
            lightList[0].Deserialize(binaryReader);
            for (int i = 1; i < numberOfLights; i++)
            {
                AddLight();
                lightList[i].Deserialize(binaryReader);
            }
        }

        public void Activate()
        {
            GameMain.Instance.MainCamera.GetComponent<Camera>().backgroundColor = Color.black;
            AddLight(GameMain.Instance.MainLight.gameObject, true);
        }

        public void Deactivate()
        {
            for (int i = 0; i < lightList.Count; i++)
            {
                DestroyLight(lightList[i]);
            }
            selectedLightIndex = 0;
            lightList.Clear();

            GameMain.Instance.MainLight.Reset();

            Light mainLight = GameMain.Instance.MainLight.GetComponent<Light>();
            mainLight.type = LightType.Directional;
            DragPointLight.SetLightProperties(mainLight, new LightProperty());
        }

        public void Update() { }

        public void AddLight(GameObject lightGo = null, bool isMain = false)
        {
            GameObject go = lightGo ?? new GameObject();
            DragPointLight light = DragPoint.Make<DragPointLight>(
                PrimitiveType.Cube, Vector3.one * 0.12f, DragPoint.LightBlue
            );
            light.Initialize(() => go.transform.position, () => go.transform.eulerAngles);
            light.Set(go.transform);
            light.IsMain = isMain;

            light.Rotate += OnRotate;
            light.Scale += OnScale;
            light.Delete += OnDelete;
            light.Select += OnSelect;

            lightList.Add(light);

            CurrentLight.IsActiveLight = false;
            SelectedLightIndex = lightList.Count;
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

            DestroyLight(lightList[lightIndex]);
            lightList.RemoveAt(lightIndex);

            if (lightIndex <= SelectedLightIndex) SelectedLightIndex -= 1;

            if (noUpdate) return;
            OnListModified();
        }

        public void SetColourModeActive(bool isColourMode)
        {
            lightList[0].IsColourMode = isColourMode;
        }

        public void ClearLights()
        {
            for (int i = lightList.Count - 1; i > 0; i--)
            {
                DeleteLight(i);
            }
            selectedLightIndex = 0;
        }

        private void DestroyLight(DragPointLight light)
        {
            if (light == null) return;
            light.Rotate -= OnRotate;
            light.Scale -= OnScale;
            light.Delete -= OnDelete;
            light.Select -= OnSelect;
            GameObject.Destroy(light.gameObject);
        }

        private string LightName(string name)
        {
            return Translation.Get("lightType", name);
        }

        private void OnDelete(object sender, EventArgs args)
        {
            DragPointLight theLight = (DragPointLight)sender;
            for (int i = 1; i < lightList.Count; i++)
            {
                DragPointLight light = lightList[i];
                if (light == theLight)
                {
                    DeleteLight(i);
                    return;
                }
            }
        }

        private void OnRotate(object sender, EventArgs args)
        {
            OnTransformEvent((DragPointLight)sender, Rotate);
        }

        private void OnScale(object sender, EventArgs args)
        {
            OnTransformEvent((DragPointLight)sender, Scale);
        }

        private void OnTransformEvent(DragPointLight light, EventHandler handler)
        {
            if (light.IsActiveLight)
            {
                handler?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnSelect(object sender, EventArgs args)
        {
            DragPointLight theLight = (DragPointLight)sender;
            int select = lightList.FindIndex(light => light == theLight);
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
