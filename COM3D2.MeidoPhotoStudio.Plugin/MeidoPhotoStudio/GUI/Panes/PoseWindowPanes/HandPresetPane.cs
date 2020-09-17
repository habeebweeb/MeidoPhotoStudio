using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class HandPresetPane : BasePane
    {
        private readonly MeidoManager meidoManager;
        private readonly Dropdown presetCategoryDropdown;
        private readonly Button nextCategoryButton;
        private readonly Button previousCategoryButton;
        private readonly Dropdown presetDropdown;
        private readonly Button nextPresetButton;
        private readonly Button previousPresetButton;
        private readonly Button leftHandButton;
        private readonly Button rightHandButton;
        private string SelectedCategory => Constants.CustomHandGroupList[presetCategoryDropdown.SelectedItemIndex];
        private List<string> CurrentPresetList => Constants.CustomHandDict[SelectedCategory];
        private string CurrentPreset => CurrentPresetList[presetDropdown.SelectedItemIndex];
        private string previousCategory;
        private bool presetListEnabled = true;

        public HandPresetPane(MeidoManager meidoManager)
        {
            Constants.CustomHandChange += SaveHandEnd;
            this.meidoManager = meidoManager;

            presetCategoryDropdown = new Dropdown(Constants.CustomHandGroupList.ToArray());
            presetCategoryDropdown.SelectionChange += (s, a) => ChangePresetCategory();

            nextCategoryButton = new Button(">");
            nextCategoryButton.ControlEvent += (s, a) => presetCategoryDropdown.Step(1);

            previousCategoryButton = new Button("<");
            previousCategoryButton.ControlEvent += (s, a) => presetCategoryDropdown.Step(-1);

            presetDropdown = new Dropdown(UIPresetList());

            nextPresetButton = new Button(">");
            nextPresetButton.ControlEvent += (s, a) => presetDropdown.Step(1);

            previousPresetButton = new Button("<");
            previousPresetButton.ControlEvent += (s, a) => presetDropdown.Step(-1);

            leftHandButton = new Button(Translation.Get("handPane", "leftHand"));
            leftHandButton.ControlEvent += (s, a) => SetHandPreset(right: false);

            rightHandButton = new Button(Translation.Get("handPane", "rightHand"));
            rightHandButton.ControlEvent += (s, a) => SetHandPreset(right: true);

            previousCategory = SelectedCategory;
            presetListEnabled = CurrentPresetList.Count > 0;
        }

        protected override void ReloadTranslation()
        {
            leftHandButton.Label = Translation.Get("handPane", "leftHand");
            rightHandButton.Label = Translation.Get("handPane", "rightHand");
            if (CurrentPresetList.Count == 0) presetDropdown.SetDropdownItems(UIPresetList());
        }

        public override void Draw()
        {
            GUILayoutOption dropdownWidth = GUILayout.Width(156f);
            GUILayoutOption noExpandWidth = GUILayout.ExpandWidth(false);

            GUI.enabled = meidoManager.HasActiveMeido;

            GUILayout.BeginHorizontal();
            presetCategoryDropdown.Draw(dropdownWidth);
            previousCategoryButton.Draw(noExpandWidth);
            nextCategoryButton.Draw(noExpandWidth);
            GUILayout.EndHorizontal();

            GUI.enabled = GUI.enabled && presetListEnabled;

            GUILayout.BeginHorizontal();
            presetDropdown.Draw(dropdownWidth);
            previousPresetButton.Draw(noExpandWidth);
            nextPresetButton.Draw(noExpandWidth);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            rightHandButton.Draw();
            leftHandButton.Draw();
            GUILayout.EndHorizontal();

            GUI.enabled = true;
        }

        private void ChangePresetCategory()
        {
            presetListEnabled = CurrentPresetList.Count > 0;
            if (previousCategory == SelectedCategory) presetDropdown.SelectedItemIndex = 0;
            else
            {
                previousCategory = SelectedCategory;
                presetDropdown.SetDropdownItems(UIPresetList(), 0);
            }
        }

        private void SetHandPreset(bool right = false)
        {
            if (!meidoManager.HasActiveMeido) return;

            meidoManager.ActiveMeido.SetHandPreset(CurrentPreset, right);
        }

        private void SaveHandEnd(object sender, CustomPoseEventArgs args)
        {
            presetCategoryDropdown.SetDropdownItems(
                Constants.CustomHandGroupList.ToArray(), Constants.CustomHandGroupList.IndexOf(args.Category)
            );
            presetDropdown.SetDropdownItems(UIPresetList(), CurrentPresetList.IndexOf(args.Path));
        }

        private string[] UIPresetList()
        {
            return CurrentPresetList.Count == 0
                ? new[] { Translation.Get("handPane", "noPresetsMessage") }
                : CurrentPresetList.Select(file => Path.GetFileNameWithoutExtension(file)).ToArray();
        }
    }
}
