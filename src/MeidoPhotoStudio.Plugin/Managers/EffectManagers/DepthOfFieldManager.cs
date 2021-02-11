using UnityEngine;

namespace MeidoPhotoStudio.Plugin
{
    public class DepthOfFieldEffectManager : IEffectManager
    {
        public const string header = "EFFECT_DOF";
        private DepthOfFieldScatter DepthOfField { get; set; }
        public bool Ready { get; private set; }
        public bool Active { get; private set; }
        private readonly float initialValue = 0f;
        private float focalLength;
        public float FocalLength
        {
            get => focalLength;
            set => focalLength = DepthOfField.focalLength = value;
        }

        private float focalSize;
        public float FocalSize
        {
            get => focalSize;
            set => focalSize = DepthOfField.focalSize = value;
        }
        private float aperture;
        public float Aperture
        {
            get => aperture;
            set => aperture = DepthOfField.aperture = value;
        }
        private float maxBlurSize;
        public float MaxBlurSize
        {
            get => maxBlurSize;
            set => maxBlurSize = DepthOfField.maxBlurSize = value;
        }
        private bool visualizeFocus;
        public bool VisualizeFocus
        {
            get => visualizeFocus;
            set => visualizeFocus = DepthOfField.visualizeFocus = value;
        }

        public void Activate()
        {
            if (DepthOfField == null)
            {
                Ready = true;
                DepthOfField = GameMain.Instance.MainCamera.GetOrAddComponent<DepthOfFieldScatter>();
                if (DepthOfField.dofHdrShader == null)
                {
                    DepthOfField.dofHdrShader = Shader.Find("Hidden/Dof/DepthOfFieldHdr");
                }
                if (DepthOfField.dx11BokehShader == null)
                {
                    DepthOfField.dx11BokehShader = Shader.Find("Hidden/Dof/DX11Dof");
                }
                if (DepthOfField.dx11BokehTexture == null)
                {
                    DepthOfField.dx11BokehTexture = Resources.Load("Textures/hexShape") as Texture2D;
                }
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
            else Reset();
        }

        public void Update() { }
    }
}
