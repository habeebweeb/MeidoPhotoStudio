using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class MaidPoseSelectorPane : BasePane
    {
        private static readonly string[] tabTranslations = new[] { "baseTab", "customTab" };
        private readonly MeidoManager meidoManager;
        private readonly Button poseLeftButton;
        private readonly Button poseRightButton;
        private readonly Button poseGroupLeftButton;
        private readonly Button poseGroupRightButton;
        private readonly Dropdown poseGroupDropdown;
        private readonly Dropdown poseDropdown;
        private readonly SelectionGrid poseModeGrid;
        private Dictionary<string, List<string>> CurrentPoseDict
            => customPoseMode ? Constants.CustomPoseDict : Constants.PoseDict;
        private List<string> CurrentPoseGroupList
            => customPoseMode ? Constants.CustomPoseGroupList : Constants.PoseGroupList;
        private string SelectedPoseGroup => CurrentPoseGroupList[poseGroupDropdown.SelectedItemIndex];
        private List<string> CurrentPoseList => CurrentPoseDict[SelectedPoseGroup];
        private int SelectedPoseIndex => poseDropdown.SelectedItemIndex;
        private string SelectedPose => CurrentPoseList[SelectedPoseIndex];
        private PoseInfo CurrentPoseInfo => new PoseInfo(SelectedPoseGroup, SelectedPose, customPoseMode);
        private bool customPoseMode;
        private bool poseListEnabled;
        private string previousPoseGroup;

        public MaidPoseSelectorPane(MeidoManager meidoManager)
        {
            Constants.CustomPoseChange += SavePoseEnd;
            this.meidoManager = meidoManager;

            poseModeGrid = new SelectionGrid(Translation.GetArray("posePane", tabTranslations));
            poseModeGrid.ControlEvent += (s, a) => SetPoseMode();

            poseGroupDropdown = new Dropdown(Translation.GetArray("poseGroupDropdown", Constants.PoseGroupList));
            poseGroupDropdown.SelectionChange += (s, a) => ChangePoseGroup();

            poseDropdown = new Dropdown(UIPoseList());
            poseDropdown.SelectionChange += (s, a) => ChangePose();

            poseGroupLeftButton = new Button("<");
            poseGroupLeftButton.ControlEvent += (s, a) => poseGroupDropdown.Step(-1);

            poseGroupRightButton = new Button(">");
            poseGroupRightButton.ControlEvent += (s, a) => poseGroupDropdown.Step(1);

            poseLeftButton = new Button("<");
            poseLeftButton.ControlEvent += (s, a) => poseDropdown.Step(-1);

            poseRightButton = new Button(">");
            poseRightButton.ControlEvent += (s, a) => poseDropdown.Step(1);

            customPoseMode = poseModeGrid.SelectedItemIndex == 1;
            previousPoseGroup = SelectedPoseGroup;
            poseListEnabled = CurrentPoseList.Count > 0;
        }

        protected override void ReloadTranslation()
        {
            updating = true;
            poseModeGrid.SetItems(Translation.GetArray("posePane", tabTranslations));
            if (!customPoseMode)
            {
                poseGroupDropdown.SetDropdownItems(
                    Translation.GetArray("poseGroupDropdown", Constants.PoseGroupList)
                );
            }
            updating = false;
        }

        public override void Draw()
        {
            const float buttonHeight = 30f;
            GUILayoutOption[] arrowLayoutOptions = {
                GUILayout.Width(buttonHeight),
                GUILayout.Height(buttonHeight)
            };

            const float dropdownButtonWidth = 153f;
            GUILayoutOption[] dropdownLayoutOptions = new GUILayoutOption[] {
                GUILayout.Height(buttonHeight),
                GUILayout.Width(dropdownButtonWidth)
            };

            GUI.enabled = meidoManager.HasActiveMeido && !meidoManager.ActiveMeido.Stop;

            poseModeGrid.Draw();
            MpsGui.WhiteLine();

            GUILayout.BeginHorizontal();
            poseGroupLeftButton.Draw(arrowLayoutOptions);
            poseGroupDropdown.Draw(dropdownLayoutOptions);
            poseGroupRightButton.Draw(arrowLayoutOptions);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUI.enabled = GUI.enabled && poseListEnabled;
            poseLeftButton.Draw(arrowLayoutOptions);
            poseDropdown.Draw(dropdownLayoutOptions);
            poseRightButton.Draw(arrowLayoutOptions);
            GUILayout.EndHorizontal();

            GUI.enabled = true;
        }

        public override void UpdatePane()
        {
            updating = true;

            PoseInfo poseInfo = meidoManager.ActiveMeido.CachedPose;

            bool oldPoseMode = customPoseMode;

            poseModeGrid.SelectedItemIndex = poseInfo.CustomPose ? 1 : 0;

            int poseGroupIndex = CurrentPoseGroupList.IndexOf(poseInfo.PoseGroup);

            if (poseGroupIndex < 0) poseGroupIndex = 0;

            int poseIndex = CurrentPoseDict[poseInfo.PoseGroup].IndexOf(poseInfo.Pose);

            if (poseIndex < 0) poseIndex = 0;

            if (oldPoseMode != customPoseMode)
            {
                string[] list = customPoseMode
                    ? CurrentPoseGroupList.ToArray()
                    : Translation.GetArray("poseGroupDropdown", CurrentPoseGroupList);

                poseGroupDropdown.SetDropdownItems(list);
            }

            poseGroupDropdown.SelectedItemIndex = poseGroupIndex;
            poseDropdown.SelectedItemIndex = poseIndex;

            updating = false;
        }

        private void SavePoseEnd(object sender, CustomPoseEventArgs args)
        {
            updating = true;
            poseModeGrid.SelectedItemIndex = 1;
            poseGroupDropdown.SetDropdownItems(
                CurrentPoseGroupList.ToArray(), CurrentPoseGroupList.IndexOf(args.Category)
            );
            updating = false;

            poseDropdown.SetDropdownItems(UIPoseList(), CurrentPoseDict[args.Category].IndexOf(args.Path));
            poseListEnabled = true;
        }

        private void SetPoseMode()
        {
            customPoseMode = poseModeGrid.SelectedItemIndex == 1;

            if (updating) return;

            string[] list = customPoseMode
                ? CurrentPoseGroupList.ToArray()
                : Translation.GetArray("poseGroupDropdown", CurrentPoseGroupList);

            poseGroupDropdown.SetDropdownItems(list, 0);
        }

        private void ChangePoseGroup()
        {
            if (previousPoseGroup == SelectedPoseGroup)
            {
                poseDropdown.SelectedItemIndex = 0;
            }
            else
            {
                previousPoseGroup = SelectedPoseGroup;
                poseDropdown.SetDropdownItems(UIPoseList(), 0);
            }
        }

        private void ChangePose()
        {
            if (!poseListEnabled || updating) return;
            meidoManager.ActiveMeido.SetPose(CurrentPoseInfo);
        }

        private string[] UIPoseList()
        {
            return CurrentPoseList.Count == 0
                ? new[] { "No Poses" }
                : CurrentPoseList
                    .Select((pose, i) => $"{i + 1}:{(customPoseMode ? Path.GetFileNameWithoutExtension(pose) : pose)}")
                    .ToArray();
        }
    }
}
