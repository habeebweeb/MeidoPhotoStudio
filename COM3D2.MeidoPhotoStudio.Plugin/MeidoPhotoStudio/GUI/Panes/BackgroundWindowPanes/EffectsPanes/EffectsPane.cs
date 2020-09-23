using System.Collections.Generic;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class EffectsPane : BasePane
    {
        private readonly Dictionary<string, BasePane> effectPanes = new Dictionary<string, BasePane>();
        private readonly List<string> effectList = new List<string>();
        private readonly SelectionGrid effectToggles;
        private BasePane currentEffectPane;

        public BasePane this[string effectUI]
        {
            private get => effectPanes[effectUI];
            set
            {
                effectPanes[effectUI] = value;
                effectList.Add(effectUI);
                effectToggles.SetItems(Translation.GetArray("effectsPane", effectList), 0);
            }
        }

        public EffectsPane()
        {
            effectToggles = new SelectionGrid(new[] { "dummy" /* thicc */ });
            effectToggles.ControlEvent += (s, a) => SetEffectPane(effectList[effectToggles.SelectedItemIndex]);
        }

        protected override void ReloadTranslation()
        {
            effectToggles.SetItems(Translation.GetArray("effectsPane", effectList));
        }

        private void SetEffectPane(string effectUI)
        {
            currentEffectPane = effectPanes[effectUI];
            currentEffectPane.UpdatePane();
        }

        public override void UpdatePane() => currentEffectPane.UpdatePane();

        public override void Draw()
        {
            MpsGui.Header("Effects");
            MpsGui.WhiteLine();
            effectToggles.Draw();
            MpsGui.BlackLine();
            currentEffectPane.Draw();
        }
    }
}
