using UnityEngine;

namespace MeidoPhotoStudio.Plugin
{
    public class FaceWindowPane : BaseMainWindowPane
    {
        private readonly MeidoManager meidoManager;
        private readonly MaidFaceSliderPane maidFaceSliderPane;
        private readonly MaidFaceBlendPane maidFaceBlendPane;
        private readonly MaidSwitcherPane maidSwitcherPane;
        private readonly SaveFacePane saveFacePane;
        private readonly Toggle saveFaceToggle;
        private bool saveFaceMode;

        public FaceWindowPane(MeidoManager meidoManager, MaidSwitcherPane maidSwitcherPane)
        {
            this.meidoManager = meidoManager;

            this.maidSwitcherPane = maidSwitcherPane;

            maidFaceSliderPane = AddPane(new MaidFaceSliderPane(this.meidoManager));
            maidFaceBlendPane = AddPane(new MaidFaceBlendPane(this.meidoManager));
            saveFacePane = AddPane(new SaveFacePane(this.meidoManager));

            saveFaceToggle = new Toggle(Translation.Get("maidFaceWindow", "savePaneToggle"));
            saveFaceToggle.ControlEvent += (s, a) => saveFaceMode = !saveFaceMode;
        }

        protected override void ReloadTranslation()
        {
            saveFaceToggle.Label = Translation.Get("maidFaceWindow", "savePaneToggle");
        }

        public override void Draw()
        {
            tabsPane.Draw();
            maidSwitcherPane.Draw();

            maidFaceBlendPane.Draw();

            scrollPos = GUILayout.BeginScrollView(scrollPos);

            maidFaceSliderPane.Draw();

            GUI.enabled = meidoManager.HasActiveMeido;
            saveFaceToggle.Draw();
            GUI.enabled = true;

            if (saveFaceMode) saveFacePane.Draw();

            GUILayout.EndScrollView();
        }

        public override void UpdatePanes()
        {
            if (!meidoManager.HasActiveMeido) return;
            if (ActiveWindow)
            {
                meidoManager.ActiveMeido.StopBlink();
                base.UpdatePanes();
            }
        }
    }
}
