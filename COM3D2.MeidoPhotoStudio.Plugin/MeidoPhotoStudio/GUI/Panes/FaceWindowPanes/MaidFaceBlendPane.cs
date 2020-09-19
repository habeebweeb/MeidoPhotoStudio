using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class MaidFaceBlendPane : BasePane
    {
        private readonly MeidoManager meidoManager;
        private readonly SelectionGrid faceBlendSourceGrid;
        private readonly Dropdown faceBlendCategoryDropdown;
        private readonly Button prevCategoryButton;
        private readonly Button nextCategoryButton;
        private readonly Dropdown faceBlendDropdown;
        private readonly Button facePrevButton;
        private readonly Button faceNextButton;
        private static readonly string[] tabTranslations = { "baseTab", "customTab" };
        private bool facePresetMode;
        private bool faceListEnabled;
        private Dictionary<string, List<string>> CurrentFaceDict => facePresetMode
            ? Constants.CustomFaceDict : Constants.FaceDict;
        private List<string> CurrentFaceGroupList => facePresetMode
            ? Constants.CustomFaceGroupList : Constants.FaceGroupList;
        private string SelectedFaceGroup => CurrentFaceGroupList[faceBlendCategoryDropdown.SelectedItemIndex];
        private List<string> CurrentFaceList => CurrentFaceDict[SelectedFaceGroup];
        private int SelectedFaceIndex => faceBlendDropdown.SelectedItemIndex;
        private string SelectedFace => CurrentFaceList[SelectedFaceIndex];

        public MaidFaceBlendPane(MeidoManager meidoManager)
        {
            Constants.CustomFaceChange += OnPresetChange;
            this.meidoManager = meidoManager;

            faceBlendSourceGrid = new SelectionGrid(Translation.GetArray("maidFaceWindow", tabTranslations));
            faceBlendSourceGrid.ControlEvent += (s, a) =>
            {
                facePresetMode = faceBlendSourceGrid.SelectedItemIndex == 1;
                if (updating) return;
                string[] list = facePresetMode
                    ? CurrentFaceGroupList.ToArray()
                    : Translation.GetArray("faceBlendCategory", Constants.FaceGroupList);
                faceBlendCategoryDropdown.SetDropdownItems(list, 0);
            };

            faceBlendCategoryDropdown = new Dropdown(
                Translation.GetArray("faceBlendCategory", Constants.FaceGroupList)
            );
            faceBlendCategoryDropdown.SelectionChange += (s, a) =>
            {
                faceListEnabled = CurrentFaceList.Count > 0;
                faceBlendDropdown.SetDropdownItems(UIFaceList(), 0);
            };

            prevCategoryButton = new Button("<");
            prevCategoryButton.ControlEvent += (s, a) => faceBlendCategoryDropdown.Step(-1);

            nextCategoryButton = new Button(">");
            nextCategoryButton.ControlEvent += (s, a) => faceBlendCategoryDropdown.Step(1);

            faceBlendDropdown = new Dropdown(UIFaceList());
            faceBlendDropdown.SelectionChange += (s, a) =>
            {
                if (!faceListEnabled || updating) return;
                this.meidoManager.ActiveMeido.SetFaceBlendSet(SelectedFace, facePresetMode);
            };

            facePrevButton = new Button("<");
            facePrevButton.ControlEvent += (s, a) => faceBlendDropdown.Step(-1);

            faceNextButton = new Button(">");
            faceNextButton.ControlEvent += (s, a) => faceBlendDropdown.Step(1);

            faceListEnabled = CurrentFaceList.Count > 0;
        }

        protected override void ReloadTranslation()
        {
            updating = true;
            faceBlendSourceGrid.SetItems(Translation.GetArray("maidFaceWindow", tabTranslations));
            if (!facePresetMode)
            {
                faceBlendCategoryDropdown.SetDropdownItems(
                    Translation.GetArray("faceBlendCategory", Constants.FaceGroupList)
                );
            }
            updating = false;
        }

        public override void Draw()
        {
            const float buttonHeight = 30;
            GUILayoutOption[] arrowLayoutOptions = {
                GUILayout.Width(buttonHeight),
                GUILayout.Height(buttonHeight)
            };

            const float dropdownButtonWidth = 153f;
            GUILayoutOption[] dropdownLayoutOptions = new GUILayoutOption[] {
                GUILayout.Height(buttonHeight),
                GUILayout.Width(dropdownButtonWidth)
            };

            GUI.enabled = meidoManager.HasActiveMeido;

            faceBlendSourceGrid.Draw();

            MpsGui.WhiteLine();

            GUILayout.BeginHorizontal();
            prevCategoryButton.Draw(arrowLayoutOptions);
            faceBlendCategoryDropdown.Draw(dropdownLayoutOptions);
            nextCategoryButton.Draw(arrowLayoutOptions);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUI.enabled = GUI.enabled && faceListEnabled;
            facePrevButton.Draw(arrowLayoutOptions);
            faceBlendDropdown.Draw(dropdownLayoutOptions);
            faceNextButton.Draw(arrowLayoutOptions);
            GUILayout.EndHorizontal();
            GUI.enabled = true;
        }

        private string[] UIFaceList()
        {
            return CurrentFaceList.Count == 0
                ? (new[] { "No Face Presets" })
                : CurrentFaceList.Select(face => facePresetMode
                    ? Path.GetFileNameWithoutExtension(face)
                    : Translation.Get("faceBlendPresetsDropdown", face)
                ).ToArray();
        }

        private void OnPresetChange(object sender, PresetChangeEventArgs args)
        {
            if (args == PresetChangeEventArgs.Empty)
            {
                if (facePresetMode)
                {
                    updating = true;
                    faceBlendCategoryDropdown.SetDropdownItems(CurrentFaceGroupList.ToArray(), 0);
                    faceBlendDropdown.SetDropdownItems(UIFaceList(), 0);
                    updating = false;
                }
            }
            else
            {
                updating = true;
                faceBlendSourceGrid.SelectedItemIndex = 1;
                faceBlendCategoryDropdown.SetDropdownItems(
                    CurrentFaceGroupList.ToArray(), CurrentFaceGroupList.IndexOf(args.Category)
                );
                updating = false;
                faceBlendDropdown.SetDropdownItems(UIFaceList(), CurrentFaceList.IndexOf(args.Path));
            }
        }
    }
}
