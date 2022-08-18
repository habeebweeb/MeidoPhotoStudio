using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class DepthOfFieldEffectManager : IEffectManager
{
    public const string Header = "EFFECT_DOF";

    private readonly float initialValue = 0f;

    private float focalLength;
    private float focalSize;
    private float aperture;
    private float maxBlurSize;
    private bool visualizeFocus;

    public bool Ready { get; private set; }

    public bool Active { get; private set; }

    public float FocalLength
    {
        get => focalLength;
        set => focalLength = DepthOfField.focalLength = value;
    }

    public float FocalSize
    {
        get => focalSize;
        set => focalSize = DepthOfField.focalSize = value;
    }

    public float Aperture
    {
        get => aperture;
        set => aperture = DepthOfField.aperture = value;
    }

    public float MaxBlurSize
    {
        get => maxBlurSize;
        set => maxBlurSize = DepthOfField.maxBlurSize = value;
    }

    public bool VisualizeFocus
    {
        get => visualizeFocus;
        set => visualizeFocus = DepthOfField.visualizeFocus = value;
    }

    private DepthOfFieldScatter DepthOfField { get; set; }

    public void Activate()
    {
        if (!DepthOfField)
        {
            Ready = true;
            DepthOfField = GameMain.Instance.MainCamera.GetOrAddComponent<DepthOfFieldScatter>();

            if (!DepthOfField.dofHdrShader)
                DepthOfField.dofHdrShader = Shader.Find("Hidden/Dof/DepthOfFieldHdr");

            if (!DepthOfField.dx11BokehShader)
                DepthOfField.dx11BokehShader = Shader.Find("Hidden/Dof/DX11Dof");

            if (!DepthOfField.dx11BokehTexture)
                DepthOfField.dx11BokehTexture = Resources.Load("Textures/hexShape") as Texture2D;
        }

        SetEffectActive(false);
    }

    public void Deactivate()
    {
        FocalLength = initialValue;
        FocalSize = initialValue;
        Aperture = initialValue;
        MaxBlurSize = initialValue;
        VisualizeFocus = false;
        DepthOfField.enabled = false;
        Active = false;
    }

    public void Reset()
    {
        DepthOfField.focalLength = initialValue;
        DepthOfField.focalSize = initialValue;
        DepthOfField.aperture = initialValue;
        DepthOfField.maxBlurSize = initialValue;
    }

    public void SetEffectActive(bool active)
    {
        DepthOfField.enabled = active;

        if (Active = active)
        {
            DepthOfField.focalLength = FocalLength;
            DepthOfField.focalSize = FocalSize;
            DepthOfField.aperture = Aperture;
            DepthOfField.maxBlurSize = MaxBlurSize;
        }
        else
        {
            Reset();
        }
    }

    public void Update()
    {
    }
}
