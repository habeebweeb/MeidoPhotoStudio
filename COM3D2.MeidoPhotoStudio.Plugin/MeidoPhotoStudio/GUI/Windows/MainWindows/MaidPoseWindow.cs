using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class MaidPoseWindow : BaseMainWindow
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
        public MaidPoseWindow(MeidoManager meidoManager)
        {
            this.meidoManager = meidoManager;
            this.meidoManager.SelectMeido += SelectMeido;

            this.poseGroupDropdown = new Dropdown(Translation.GetList("poseGroupDropdown", Constants.PoseGroupList));
            this.poseGroupDropdown.SelectionChange += ChangePoseGroup;

            this.poseDropdown = new Dropdown(Constants.PoseDict[Constants.PoseGroupList[0]].ToArray());
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

        private void ChangePoseGroup(object sender, EventArgs args)
        {
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
                    List<KeyValuePair<string, string>> pairList = Constants.CustomPoseDict[selectedPoseGroup];
                    string[] poseList = pairList.Select(pair => pair.Key).ToArray();
                    this.poseDropdown.SetDropdownItems(poseList);
                }
                else
                {
                    this.poseDropdown.SetDropdownItems(Constants.PoseDict[selectedPoseGroup].ToArray());
                }
            }
        }

        private void ChangePose(object sender, EventArgs args)
        {
            selectedPose = poseDropdown.SelectedItemIndex;
            string poseName;
            if (this.poseGroupDropdown.SelectedItemIndex >= Constants.CustomPoseGroupsIndex)
                poseName = Constants.CustomPoseDict[selectedPoseGroup][selectedPose].Value;
            else
                poseName = Constants.PoseDict[selectedPoseGroup][selectedPose];

            meidoManager.ActiveMeido.SetPose(poseName);
        }

        public override void Draw(params GUILayoutOption[] layoutOptions)
        {
            float arrowButtonSize = 30;
            GUILayoutOption[] arrowLayoutOptions = {
                GUILayout.Width(arrowButtonSize),
                GUILayout.Height(arrowButtonSize)
            };

            float dropdownButtonHeight = arrowButtonSize;
            float dropdownButtonWidth = 143f;
            GUILayoutOption[] dropdownLayoutOptions = new GUILayoutOption[] {
                GUILayout.Height(dropdownButtonHeight),
                GUILayout.Width(dropdownButtonWidth)
            };

            MaidSwitcherPane.Draw();

            bool previousState = GUI.enabled;
            GUI.enabled = meidoManager.HasActiveMeido;

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

            GUI.enabled = previousState;
        }

        private void SelectMeido(object sender, EventArgs args)
        {

        }
    }
}
