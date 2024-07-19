namespace MeidoPhotoStudio.Plugin.Core.Effects;

public class DepthOfFieldController(UnityEngine.Camera camera) : EffectControllerBase
{
    private readonly UnityEngine.Camera camera = camera ? camera : throw new ArgumentNullException(nameof(camera));

    private DepthOfFieldBackup initialDepthOfFieldSettings;
    private DepthOfFieldScatter depthOfField;

    public override bool Active
    {
        get => DepthOfField.enabled;
        set
        {
            if (value == Active)
                return;

            DepthOfField.enabled = value;

            base.Active = value;
        }
    }

    public float FocalLength
    {
        get => DepthOfField.focalLength;
        set
        {
            DepthOfField.focalLength = value;

            RaisePropertyChanged(nameof(FocalLength));
        }
    }

    public float FocalSize
    {
        get => DepthOfField.focalSize;
        set
        {
            DepthOfField.focalSize = value;

            RaisePropertyChanged(nameof(FocalSize));
        }
    }

    public float Aperture
    {
        get => DepthOfField.aperture;
        set
        {
            DepthOfField.aperture = value;

            RaisePropertyChanged(nameof(Aperture));
        }
    }

    public float MaxBlurSize
    {
        get => DepthOfField.maxBlurSize;
        set
        {
            DepthOfField.maxBlurSize = value;

            RaisePropertyChanged(nameof(MaxBlurSize));
        }
    }

    public bool VisualizeFocus
    {
        get => DepthOfField.visualizeFocus;
        set
        {
            DepthOfField.visualizeFocus = value;

            RaisePropertyChanged(nameof(VisualizeFocus));
        }
    }

    private DepthOfFieldScatter DepthOfField
    {
        get
        {
            if (depthOfField)
                return depthOfField;

            depthOfField = camera.GetOrAddComponent<DepthOfFieldScatter>();

            if (!depthOfField.dofHdrShader)
                depthOfField.dofHdrShader = Shader.Find("Hidden/Dof/DepthOfFieldHdr");

            if (!depthOfField.dx11BokehShader)
                depthOfField.dx11BokehShader = Shader.Find("Hidden/Dof/DX11Dof");

            if (!depthOfField.dx11BokehTexture)
                depthOfField.dx11BokehTexture = Resources.Load("Textures/hexShape") as Texture2D;

            initialDepthOfFieldSettings = DepthOfFieldBackup.Create(depthOfField);

            return depthOfField;
        }
    }

    public override void Reset() =>
        ApplyBackup(initialDepthOfFieldSettings);

    private void ApplyBackup(DepthOfFieldBackup backup) =>
        (FocalLength, FocalSize, Aperture, MaxBlurSize, VisualizeFocus) = backup;

    private readonly record struct DepthOfFieldBackup(
        float FocalLength, float FocalSize, float Aperture, float MaxBlurSize, bool VisualizeFocus)
    {
        public static DepthOfFieldBackup Create(DepthOfFieldScatter depthOfField) =>
            new(
                depthOfField.focalLength,
                depthOfField.focalSize,
                depthOfField.aperture,
                depthOfField.maxBlurSize,
                depthOfField.visualizeFocus);
    }
}
