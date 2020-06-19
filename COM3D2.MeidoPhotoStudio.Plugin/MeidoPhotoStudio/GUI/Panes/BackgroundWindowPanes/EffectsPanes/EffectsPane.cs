using System;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class EffectsPane : BasePane
    {
        private Dictionary<string, BasePane> effectPanes = new Dictionary<string, BasePane>();
        private SelectionGrid effectToggles;
        private BasePane currentEffectPane;
        private List<string> effectList = new List<string>();

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
            effectToggles = new SelectionGrid(new[] { "dummy" });
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

        public override void UpdatePane()
        {
            currentEffectPane.UpdatePane();
        }

        public override void Draw()
        {
            MiscGUI.Header("Effects");
            MiscGUI.WhiteLine();
            effectToggles.Draw();
            currentEffectPane.Draw();
        }
    }
}
