using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class FaceWindowPane : BaseWindowPane
    {
        private MeidoManager meidoManager;
        private MaidFaceSliderPane maidFaceSliderPane;
        private MaidFaceBlendPane maidFaceBlendPane;
        private MaidSwitcherPane maidSwitcherPane;

        public FaceWindowPane(MeidoManager meidoManager, MaidSwitcherPane maidSwitcherPane)
        {
            this.meidoManager = meidoManager;

            this.maidSwitcherPane = maidSwitcherPane;

            this.maidFaceSliderPane = new MaidFaceSliderPane(this.meidoManager);
            this.maidFaceBlendPane = new MaidFaceBlendPane(this.meidoManager);
        }

        public override void Draw()
        {
            this.maidSwitcherPane.Draw();

            this.scrollPos = GUILayout.BeginScrollView(this.scrollPos);

            this.maidFaceBlendPane.Draw();

            this.maidFaceSliderPane.Draw();

            GUILayout.EndScrollView();
        }

        public override void UpdatePanes()
        {
            if (!this.meidoManager.HasActiveMeido) return;
            if (ActiveWindow)
            {
                this.meidoManager.ActiveMeido.Maid.boMabataki = false;
                this.meidoManager.ActiveMeido.Maid.body0.Face.morph.EyeMabataki = 0f;
                this.maidFaceSliderPane.UpdatePane();
                this.maidFaceBlendPane.UpdatePane();
            }
        }
    }
}
