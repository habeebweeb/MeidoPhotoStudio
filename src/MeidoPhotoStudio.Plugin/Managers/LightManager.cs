using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class LightManager : IManager
{
    public const string Header = "LIGHT";

    private static bool cubeActive = true;

    private readonly List<DragPointLight> lightList = new();

    private int selectedLightIndex;

    public LightManager() =>
        Activate();

    public event EventHandler Rotate;

    public event EventHandler Scale;

    public event EventHandler ListModified;

    public event EventHandler Select;

    private static event EventHandler CubeActiveChange;

    public static bool CubeActive
    {
        get => cubeActive;
        set
        {
            if (value == cubeActive)
                return;

            cubeActive = value;
            CubeActiveChange?.Invoke(null, EventArgs.Empty);
        }
    }

    public int SelectedLightIndex
    {
        get => selectedLightIndex;
        set
        {
            selectedLightIndex = Mathf.Clamp(value, 0, lightList.Count - 1);
            lightList[SelectedLightIndex].IsActiveLight = true;
        }
    }

    public string[] LightNameList =>
        lightList.Select(light => LightName(light.Name)).ToArray();

    public string ActiveLightName =>
        LightName(lightList[SelectedLightIndex].Name);

    public DragPointLight CurrentLight =>
        lightList[SelectedLightIndex];

    public void Activate()
    {
        GameMain.Instance.MainCamera.GetComponent<Camera>().backgroundColor = Color.black;
        AddLight(GameMain.Instance.MainLight.gameObject, true);
        CubeActiveChange += OnCubeActive;
    }

    public void Deactivate()
    {
        for (var i = 0; i < lightList.Count; i++)
            DestroyLight(lightList[i]);

        selectedLightIndex = 0;
        lightList.Clear();

        GameMain.Instance.MainLight.Reset();

        var mainLight = GameMain.Instance.MainLight.GetComponent<Light>();

        mainLight.type = LightType.Directional;
        DragPointLight.SetLightProperties(mainLight, new());
        CubeActiveChange -= OnCubeActive;
    }

    public void Update()
    {
    }

    public void AddLight(GameObject lightGo = null, bool isMain = false)
    {
        // TODO: null propagation does not work with UntiyEngine.Object
        var go = lightGo ?? new GameObject("MPS Light");
        var light = DragPoint.Make<DragPointLight>(PrimitiveType.Cube, Vector3.one * 0.12f);

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
        if (selectedLightIndex is 0)
            return;

        DeleteLight(SelectedLightIndex);
    }

    public void DeleteLight(int lightIndex, bool noUpdate = false)
    {
        if (lightIndex is 0)
            return;

        DestroyLight(lightList[lightIndex]);
        lightList.RemoveAt(lightIndex);

        if (lightIndex <= SelectedLightIndex)
            SelectedLightIndex--;

        if (noUpdate)
            return;

        OnListModified();
    }

    public void SetColourModeActive(bool isColourMode) =>
        lightList[0].IsColourMode = isColourMode;

    public void ClearLights()
    {
        for (var i = lightList.Count - 1; i > 0; i--)
            DeleteLight(i);

        selectedLightIndex = 0;
    }

    private void DestroyLight(DragPointLight light)
    {
        if (!light)
            return;

        light.Rotate -= OnRotate;
        light.Scale -= OnScale;
        light.Delete -= OnDelete;
        light.Select -= OnSelect;

        UnityEngine.Object.Destroy(light.gameObject);
    }

    private string LightName(string name) =>
        Translation.Get("lightType", name);

    private void OnDelete(object sender, EventArgs args)
    {
        var theLight = (DragPointLight)sender;

        for (var i = 1; i < lightList.Count; i++)
        {
            var light = lightList[i];

            if (light == theLight)
            {
                DeleteLight(i);

                return;
            }
        }
    }

    private void OnRotate(object sender, EventArgs args) =>
        OnTransformEvent((DragPointLight)sender, Rotate);

    private void OnScale(object sender, EventArgs args) =>
        OnTransformEvent((DragPointLight)sender, Scale);

    private void OnTransformEvent(DragPointLight light, EventHandler handler)
    {
        if (light.IsActiveLight)
            handler?.Invoke(this, EventArgs.Empty);
    }

    private void OnSelect(object sender, EventArgs args)
    {
        var theLight = (DragPointLight)sender;
        var select = lightList.FindIndex(light => light == theLight);

        if (select < 0)
            return;

        SelectedLightIndex = select;
        Select?.Invoke(this, EventArgs.Empty);
    }

    private void OnListModified() =>
        ListModified?.Invoke(this, EventArgs.Empty);

    private void OnCubeActive(object sender, EventArgs args)
    {
        foreach (var dragPoint in lightList)
            dragPoint.gameObject.SetActive(CubeActive);
    }
}
