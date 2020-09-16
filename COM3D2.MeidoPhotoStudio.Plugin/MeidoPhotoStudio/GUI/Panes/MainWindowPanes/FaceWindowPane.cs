using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class FaceWindowPane : BaseMainWindowPane
    {
        private MeidoManager meidoManager;
        private MaidFaceSliderPane maidFaceSliderPane;
        private MaidFaceBlendPane maidFaceBlendPane;
        private MaidSwitcherPane maidSwitcherPane;
        private readonly SaveFacePane saveFacePane;
        private readonly Toggle saveFaceToggle;
        private bool saveFaceMode = false;

        public FaceWindowPane(MeidoManager meidoManager, MaidSwitcherPane maidSwitcherPane)
        {
            this.meidoManager = meidoManager;

            this.maidSwitcherPane = maidSwitcherPane;

            this.maidFaceSliderPane = AddPane(new MaidFaceSliderPane(this.meidoManager));
            this.maidFaceBlendPane = AddPane(new MaidFaceBlendPane(this.meidoManager));
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
            if (!this.meidoManager.HasActiveMeido) return;
            if (ActiveWindow)
            {
                this.meidoManager.ActiveMeido.StopBlink();
                base.UpdatePanes();
            }
        }
    }
}
