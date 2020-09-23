using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class FogEffectManager : IEffectManager
    {
        public const string header = "EFFECT_FOG";
        private GlobalFog Fog { get; set; }
        public bool Ready { get; private set; }
        public bool Active { get; private set; }
        private readonly float initialDistance = 4f;
        private readonly float initialDensity = 1f;
        private readonly float initialHeightScale = 1f;
        private readonly float initialHeight = 0f;
        private readonly Color initialColour = Color.white;
        private float distance;
        public float Distance
        {
            get => distance;
            set => distance = Fog.startDistance = value;
        }
        private float density;
        public float Density
        {
            get => density;
            set => density = Fog.globalDensity = value;
        }
        private float heightScale;
        public float HeightScale
        {
            get => heightScale;
            set => heightScale = Fog.heightScale = value;
        }
        private float height;
        public float Height
        {
            get => height;
            set => height = Fog.height = value;
        }
        public float FogColourRed
        {
            get => FogColour.r;
            set
            {
                Color fogColour = FogColour;
                FogColour = new Color(value, fogColour.g, fogColour.b);
            }
        }
        public float FogColourGreen
        {
            get => FogColour.g;
            set
            {
                Color fogColour = FogColour;
                FogColour = new Color(fogColour.r, value, fogColour.b);
            }
        }
        public float FogColourBlue
        {
            get => FogColour.b;
            set
            {
                Color fogColour = FogColour;
                FogColour = new Color(fogColour.r, fogColour.g, value);
            }
        }
        private Color fogColour;
        public Color FogColour
        {
            get => fogColour;
            set => fogColour = Fog.globalFogColor = value;
        }

        public void Serialize(System.IO.BinaryWriter binaryWriter)
        {
            binaryWriter.Write(header);
            binaryWriter.Write(Distance);
            binaryWriter.Write(Density);
            binaryWriter.Write(HeightScale);
            binaryWriter.Write(Height);
            binaryWriter.WriteColour(FogColour);
            binaryWriter.Write(Active);
        }

        public void Deserialize(System.IO.BinaryReader binaryReader)
        {
            Distance = binaryReader.ReadSingle();
            Density = binaryReader.ReadSingle();
            HeightScale = binaryReader.ReadSingle();
            Height = binaryReader.ReadSingle();
            FogColour = binaryReader.ReadColour();
            SetEffectActive(binaryReader.ReadBoolean());
        }

        public void Activate()
        {
            if (Fog == null)
            {
                Ready = true;
                Fog = GameMain.Instance.MainCamera.GetOrAddComponent<GlobalFog>();
                if (Fog.fogShader == null) Fog.fogShader = Shader.Find("Hidden/GlobalFog");
                Distance = initialDistance;
                Density = initialDensity;
                HeightScale = initialHeightScale;
                Height = initialHeight;
                FogColour = initialColour;
            }
            SetEffectActive(false);
        }

        public void Deactivate()
        {
            Distance = initialDistance;
            Density = initialDensity;
            HeightScale = initialHeightScale;
            Height = initialHeight;
            FogColour = initialColour;
            Fog.enabled = false;
            Active = false;
        }

        public void Reset()
        {
            Fog.startDistance = initialDistance;
            Fog.globalDensity = initialDensity;
            Fog.heightScale = initialHeightScale;
            Fog.height = initialHeight;
            Fog.globalFogColor = initialColour;
        }

        public void SetEffectActive(bool active)
        {
            Fog.enabled = active;
            if (Active = active)
            {
                Fog.startDistance = Distance;
                Fog.globalDensity = Density;
                Fog.heightScale = HeightScale;
                Fog.height = Height;
                Fog.globalFogColor = FogColour;
            }
            else Reset();
        }

        public void Update() { }
    }
}
