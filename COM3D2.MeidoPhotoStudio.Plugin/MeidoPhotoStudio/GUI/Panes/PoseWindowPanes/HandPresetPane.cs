using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class HandPresetPane : BasePane
    {
        private MeidoManager meidoManager;
        private Dropdown presetCategoryDropdown;
        private Button nextCategoryButton;
        private Button previousCategoryButton;
        private Dropdown presetDropdown;
        private Button nextPresetButton;
        private Button previousPresetButton;
        private Button leftHandButton;
        private Button rightHandButton;
        private string SelectedCategory => Constants.CustomHandGroupList[presetCategoryDropdown.SelectedItemIndex];
        private List<string> CurrentPresetList => Constants.CustomHandDict[SelectedCategory];
        private string CurrentPreset => CurrentPresetList[presetDropdown.SelectedItemIndex];
        private string previousCategory;
        private bool presetListEnabled = true;

        public HandPresetPane(MeidoManager meidoManager)
        {
            Constants.customHandChange += SaveHandEnd;
            this.meidoManager = meidoManager;

            this.presetCategoryDropdown = new Dropdown(Constants.CustomHandGroupList.ToArray());
            this.presetCategoryDropdown.SelectionChange += (s, a) => ChangePresetCategory();

            this.nextCategoryButton = new Button(">");
            this.nextCategoryButton.ControlEvent += (s, a) => this.presetCategoryDropdown.Step(1);

            this.previousCategoryButton = new Button("<");
            this.previousCategoryButton.ControlEvent += (s, a) =>
            {
                this.presetCategoryDropdown.Step(-1);
            };

            this.presetDropdown = new Dropdown(UIPresetList());

            this.nextPresetButton = new Button(">");
            this.nextPresetButton.ControlEvent += (s, a) => this.presetDropdown.Step(1);

            this.previousPresetButton = new Button("<");
            this.previousPresetButton.ControlEvent += (s, a) => this.presetDropdown.Step(-1);

            this.leftHandButton = new Button(Translation.Get("handPane", "leftHand"));
            this.leftHandButton.ControlEvent += (s, a) => SetHandPreset(right: false);

            this.rightHandButton = new Button(Translation.Get("handPane", "rightHand"));
            this.rightHandButton.ControlEvent += (s, a) => SetHandPreset(right: true);

            this.previousCategory = SelectedCategory;
            this.presetListEnabled = CurrentPresetList.Count > 0;
        }

        protected override void ReloadTranslation()
        {
            this.leftHandButton.Label = Translation.Get("handPane", "leftHand");
            this.rightHandButton.Label = Translation.Get("handPane", "rightHand");
            if (CurrentPresetList.Count == 0) this.presetDropdown.SetDropdownItems(UIPresetList());
        }

        public override void Draw()
        {
            GUILayoutOption dropdownWidth = GUILayout.Width(156f);
            GUILayoutOption noExpandWidth = GUILayout.ExpandWidth(false);

            GUI.enabled = this.meidoManager.HasActiveMeido;

            GUILayout.BeginHorizontal();
            this.presetCategoryDropdown.Draw(dropdownWidth);
            this.previousCategoryButton.Draw(noExpandWidth);
            this.nextCategoryButton.Draw(noExpandWidth);
            GUILayout.EndHorizontal();

            GUI.enabled = GUI.enabled && presetListEnabled;

            GUILayout.BeginHorizontal();
            this.presetDropdown.Draw(dropdownWidth);
            this.previousPresetButton.Draw(noExpandWidth);
            this.nextPresetButton.Draw(noExpandWidth);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            this.rightHandButton.Draw();
            this.leftHandButton.Draw();
            GUILayout.EndHorizontal();

            GUI.enabled = true;
        }

        private void ChangePresetCategory()
        {
            presetListEnabled = CurrentPresetList.Count > 0;
            if (previousCategory == SelectedCategory)
            {
                this.presetDropdown.SelectedItemIndex = 0;
            }
            else
            {
                previousCategory = SelectedCategory;
                this.presetDropdown.SetDropdownItems(UIPresetList(), 0);
            }
        }

        private void SetHandPreset(bool right = false)
        {
            if (!meidoManager.HasActiveMeido) return;

            this.meidoManager.ActiveMeido.SetHandPreset(CurrentPreset, right);
        }

        private void SaveHandEnd(object sender, CustomPoseEventArgs args)
        {
            this.presetCategoryDropdown.SetDropdownItems(
                Constants.CustomHandGroupList.ToArray(), Constants.CustomHandGroupList.IndexOf(args.Category)
            );
            this.presetDropdown.SetDropdownItems(UIPresetList(), CurrentPresetList.IndexOf(args.Path));
        }

        private string[] UIPresetList()
        {
            if (CurrentPresetList.Count == 0) return new[] { Translation.Get("handPane", "noPresetsMessage") };
            else return CurrentPresetList.Select(file => Path.GetFileNameWithoutExtension(file)).ToArray();
        }
    }
}
