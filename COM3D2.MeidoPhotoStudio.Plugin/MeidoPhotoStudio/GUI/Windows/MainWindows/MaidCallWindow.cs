using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class MaidCallWindow : BaseMainWindow
    {
        private MeidoManager meidoManager;
        private MaidSelectorPane maidSelectorPane;
        private Button placementButton;
        private Button placementOKButton;
        public MaidCallWindow(MeidoManager meidoManager) : base()
        {
            this.meidoManager = meidoManager;
            placementButton = new Button(Translation.Get("placementDropdown", "normal"));
            placementButton.ControlEvent += (o, a) => Debug.Log("Change placement");
            Controls.Add(placementButton);

            placementOKButton = new Button(Translation.Get("maidCallWindow", "okButton"));
            placementOKButton.ControlEvent += (o, a) => Debug.Log("Placement changed");
            Controls.Add(placementOKButton);

            maidSelectorPane = new MaidSelectorPane(meidoManager);
        }

        protected override void ReloadTranslation()
        {
            placementButton.Label = Translation.Get("placementDropdown", "normal");
            placementOKButton.Label = Translation.Get("maidCallWindow", "okButton");
        }

        public override void Draw(params GUILayoutOption[] layoutOptions)
        {
            GUILayout.BeginHorizontal();
            placementButton.Draw(GUILayout.Width(150));
            placementOKButton.Draw();
            GUILayout.EndHorizontal();

            maidSelectorPane.Draw();
        }
    }
}
