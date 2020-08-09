using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class MaidPoseSelectorPane : BasePane
    {
        private MeidoManager meidoManager;
        private Button poseLeftButton;
        private Button poseRightButton;
        private Button poseGroupLeftButton;
        private Button poseGroupRightButton;
        private Dropdown poseGroupDropdown;
        private Dropdown poseDropdown;
        private SelectionGrid poseModeGrid;
        private bool customPoseMode = false;
        private bool poseListEnabled = true;
        private Dictionary<string, List<string>> CurrentPoseDict
        {
            get => customPoseMode ? Constants.CustomPoseDict : Constants.PoseDict;
        }
        private List<string> CurrentPoseGroupList
        {
            get => customPoseMode ? Constants.CustomPoseGroupList : Constants.PoseGroupList;
        }
        private string SelectedPoseGroup => CurrentPoseGroupList[poseGroupDropdown.SelectedItemIndex];
        private List<string> CurrentPoseList => CurrentPoseDict[SelectedPoseGroup];
        private int SelectedPoseIndex => poseDropdown.SelectedItemIndex;
        private string SelectedPose => CurrentPoseList[SelectedPoseIndex];
        private PoseInfo CurrentPoseInfo => new PoseInfo(SelectedPoseGroup, SelectedPose, customPoseMode);
        private string previousPoseGroup;
        private static readonly string[] tabTranslations = new[] { "baseTab", "customTab" };

        public MaidPoseSelectorPane(MeidoManager meidoManager)
        {
            Constants.customPoseChange += SavePoseEnd;
            this.meidoManager = meidoManager;

            this.poseModeGrid = new SelectionGrid(Translation.GetArray("posePane", tabTranslations));
            this.poseModeGrid.ControlEvent += (s, a) => SetPoseMode();

            this.poseGroupDropdown = new Dropdown(Translation.GetArray("poseGroupDropdown", Constants.PoseGroupList));
            this.poseGroupDropdown.SelectionChange += (s, a) => ChangePoseGroup();

            this.poseDropdown = new Dropdown(UIPoseList(Constants.PoseDict[Constants.PoseGroupList[0]]));
            this.poseDropdown.SelectionChange += (s, a) => ChangePose();

            this.poseGroupLeftButton = new Button("<");
            this.poseGroupLeftButton.ControlEvent += (s, a) => poseGroupDropdown.Step(-1);

            this.poseGroupRightButton = new Button(">");
            this.poseGroupRightButton.ControlEvent += (s, a) => poseGroupDropdown.Step(1);

            this.poseLeftButton = new Button("<");
            this.poseLeftButton.ControlEvent += (s, a) => poseDropdown.Step(-1);

            this.poseRightButton = new Button(">");
            this.poseRightButton.ControlEvent += (s, a) => poseDropdown.Step(1);

            previousPoseGroup = SelectedPoseGroup;
        }

        protected override void ReloadTranslation()
        {
            this.poseModeGrid.SetItems(Translation.GetArray("posePane", tabTranslations));
            if (!customPoseMode)
            {
                this.poseGroupDropdown.SetDropdownItems(
                    Translation.GetArray("poseGroupDropdown", Constants.PoseGroupList)
                );
            }
        }

        public override void Draw()
        {
            float arrowButtonSize = 30f;
            GUILayoutOption[] arrowLayoutOptions = {
                GUILayout.Width(arrowButtonSize),
                GUILayout.Height(arrowButtonSize)
            };

            float dropdownButtonHeight = arrowButtonSize;
            float dropdownButtonWidth = 153f;
            GUILayoutOption[] dropdownLayoutOptions = new GUILayoutOption[] {
                GUILayout.Height(dropdownButtonHeight),
                GUILayout.Width(dropdownButtonWidth)
            };

            GUI.enabled = meidoManager.HasActiveMeido && !meidoManager.ActiveMeido.IsStop;

            this.poseModeGrid.Draw();
            MiscGUI.WhiteLine();

            GUILayout.BeginHorizontal();
            this.poseGroupLeftButton.Draw(arrowLayoutOptions);
            this.poseGroupDropdown.Draw(dropdownLayoutOptions);
            this.poseGroupRightButton.Draw(arrowLayoutOptions);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUI.enabled = GUI.enabled && poseListEnabled;
            this.poseLeftButton.Draw(arrowLayoutOptions);
            this.poseDropdown.Draw(dropdownLayoutOptions);
            this.poseRightButton.Draw(arrowLayoutOptions);
            GUILayout.EndHorizontal();

            GUI.enabled = true;
        }

        public override void UpdatePane()
        {
            this.updating = true;

            PoseInfo poseInfo = this.meidoManager.ActiveMeido.CachedPose;

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

                this.poseGroupDropdown.SetDropdownItems(list);
            }

            this.poseGroupDropdown.SelectedItemIndex = poseGroupIndex;
            this.poseDropdown.SelectedItemIndex = poseIndex;

            this.updating = false;
        }

        private void SavePoseEnd(object sender, CustomPoseEventArgs args)
        {
            this.updating = true;
            this.poseModeGrid.SelectedItemIndex = 1;
            this.poseGroupDropdown.SetDropdownItems(
                CurrentPoseGroupList.ToArray(), CurrentPoseGroupList.IndexOf(args.Category)
            );
            this.updating = false;

            this.poseDropdown.SetDropdownItems(
                UIPoseList(CurrentPoseList), CurrentPoseDict[args.Category].IndexOf(args.Path)
            );
        }

        private void SetPoseMode()
        {
            customPoseMode = poseModeGrid.SelectedItemIndex == 1;

            if (this.updating) return;

            string[] list = customPoseMode
                ? CurrentPoseGroupList.ToArray()
                : Translation.GetArray("poseGroupDropdown", CurrentPoseGroupList);

            this.poseGroupDropdown.SetDropdownItems(list, 0);
        }

        private void ChangePoseGroup()
        {
            if (previousPoseGroup == SelectedPoseGroup)
            {
                this.poseDropdown.SelectedItemIndex = 0;
            }
            else
            {
                previousPoseGroup = SelectedPoseGroup;
                List<string> poseList = CurrentPoseList;

                poseListEnabled = true;
                if (poseList.Count == 0)
                {
                    poseListEnabled = false;
                    this.poseDropdown.SetDropdownItems(new[] { "No Poses" }, 0);
                }
                else
                {
                    this.poseDropdown.SetDropdownItems(UIPoseList(CurrentPoseList), 0);
                }
            }
        }

        private void ChangePose()
        {
            if (!poseListEnabled || updating) return;
            meidoManager.ActiveMeido.SetPose(CurrentPoseInfo);
        }

        private string[] UIPoseList(IEnumerable<string> poseList)
        {
            return poseList.Select((pose, i) =>
            {
                return $"{i + 1}:{(customPoseMode ? Path.GetFileNameWithoutExtension(pose) : pose)}";
            }).ToArray();
        }
    }
}
