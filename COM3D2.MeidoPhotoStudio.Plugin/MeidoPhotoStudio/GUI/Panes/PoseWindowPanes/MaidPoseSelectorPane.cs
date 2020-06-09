using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class MaidPoseSelectorPane : BasePane
    {
        private MeidoManager meidoManager;
        private Button poseLeftButton;
        private Button poseRightButton;
        private Button poseGroupLeftButton;
        private Button poseGroupRightButton;
        private Dropdown poseGroupDropdown;
        private Dropdown poseDropdown;
        private string selectedPoseGroup;
        private int selectedPose;

        public MaidPoseSelectorPane(MeidoManager meidoManager)
        {
            this.meidoManager = meidoManager;

            List<string> poseGroups = new List<string>(Constants.PoseGroupList.Count);

            for (int i = 0; i < Constants.PoseGroupList.Count; i++)
            {
                string poseGroup = Constants.PoseGroupList[i];
                poseGroups.Add(i < Constants.CustomPoseGroupsIndex
                    ? Translation.Get("poseGroupDropdown", poseGroup)
                    : poseGroup
                );
            }

            this.poseGroupDropdown = new Dropdown(poseGroups.ToArray());
            this.poseGroupDropdown.SelectionChange += ChangePoseGroup;

            this.poseDropdown = new Dropdown(MakePoseList(Constants.PoseDict[Constants.PoseGroupList[0]]));
            this.poseDropdown.SelectionChange += ChangePose;

            this.poseGroupLeftButton = new Button("<");
            this.poseGroupLeftButton.ControlEvent += (s, a) => poseGroupDropdown.Step(-1);

            this.poseGroupRightButton = new Button(">");
            this.poseGroupRightButton.ControlEvent += (s, a) => poseGroupDropdown.Step(1);

            this.poseLeftButton = new Button("<");
            this.poseLeftButton.ControlEvent += (s, a) => poseDropdown.Step(-1);

            this.poseRightButton = new Button(">");
            this.poseRightButton.ControlEvent += (s, a) => poseDropdown.Step(1);
        }

        protected override void ReloadTranslation()
        {
            List<string> poseGroups = new List<string>(Constants.PoseGroupList.Count);

            for (int i = 0; i < Constants.PoseGroupList.Count; i++)
            {
                string poseGroup = Constants.PoseGroupList[i];
                poseGroups.Add(i < Constants.CustomPoseGroupsIndex
                    ? Translation.Get("poseGroupDropdown", poseGroup)
                    : poseGroup
                );
            }

            updating = true;
            this.poseGroupDropdown.SetDropdownItems(poseGroups.ToArray(), this.poseGroupDropdown.SelectedItemIndex);
            updating = false;
        }

        private void ChangePoseGroup(object sender, EventArgs args)
        {
            if (updating) return;
            string newPoseGroup = Constants.PoseGroupList[this.poseGroupDropdown.SelectedItemIndex];
            if (selectedPoseGroup == newPoseGroup)
            {
                this.poseDropdown.SelectedItemIndex = 0;
            }
            else
            {
                selectedPoseGroup = newPoseGroup;
                if (this.poseGroupDropdown.SelectedItemIndex >= Constants.CustomPoseGroupsIndex)
                {
                    this.poseDropdown.SetDropdownItems(MakePoseList(Constants.CustomPoseDict[selectedPoseGroup]));
                }
                else
                {
                    this.poseDropdown.SetDropdownItems(MakePoseList(Constants.PoseDict[selectedPoseGroup]));
                }
            }
        }

        private void ChangePose(object sender, EventArgs args)
        {
            selectedPose = poseDropdown.SelectedItemIndex;

            if (updating) return;
            PoseInfo poseInfo = MakePoseInfo();
            meidoManager.ActiveMeido.SetPose(poseInfo);
        }

        private PoseInfo MakePoseInfo()
        {
            int poseGroup = this.poseGroupDropdown.SelectedItemIndex;
            int pose = this.poseDropdown.SelectedItemIndex;

            string poseName;
            if (this.poseGroupDropdown.SelectedItemIndex >= Constants.CustomPoseGroupsIndex)
                poseName = Constants.CustomPoseDict[selectedPoseGroup][selectedPose].Value;
            else
                poseName = Constants.PoseDict[selectedPoseGroup][selectedPose];

            return new PoseInfo(poseGroup, pose, poseName);
        }

        private string[] MakePoseList(IEnumerable<string> poseList)
        {
            return poseList.Select((pose, i) => $"{i + 1}:{pose}").ToArray();
        }

        private string[] MakePoseList(List<KeyValuePair<string, string>> poseList)
        {
            return poseList.Select((kvp, i) => $"{i + 1}:{kvp.Key}").ToArray();
        }

        public override void Update()
        {
            this.updating = true;
            PoseInfo poseInfo = this.meidoManager.ActiveMeido.poseInfo;
            this.poseGroupDropdown.SelectedItemIndex = poseInfo.PoseGroupIndex;
            this.poseDropdown.SelectedItemIndex = poseInfo.PoseIndex;
            this.updating = false;
        }

        public override void Draw(params GUILayoutOption[] layoutOptions)
        {
            float arrowButtonSize = 30;
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

            GUILayout.BeginHorizontal();
            this.poseGroupLeftButton.Draw(arrowLayoutOptions);
            this.poseGroupDropdown.Draw(dropdownLayoutOptions);
            this.poseGroupRightButton.Draw(arrowLayoutOptions);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            this.poseLeftButton.Draw(arrowLayoutOptions);
            this.poseDropdown.Draw(dropdownLayoutOptions);
            this.poseRightButton.Draw(arrowLayoutOptions);
            GUILayout.EndHorizontal();
        }
    }
}
