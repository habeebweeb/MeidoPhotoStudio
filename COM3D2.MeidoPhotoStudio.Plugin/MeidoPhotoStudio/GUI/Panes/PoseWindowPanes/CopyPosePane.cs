using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class CopyPosePane : BasePane
    {
        private readonly MeidoManager meidoManager;
        private readonly Button copyButton;
        private readonly Dropdown meidoDropdown;
        private int[] copyMeidoSlot;
        private bool PlentyOfMaids => meidoManager.ActiveMeidoList.Count >= 2;
        private Meido FromMeido => meidoManager.HasActiveMeido
            ? meidoManager.ActiveMeidoList[copyMeidoSlot[meidoDropdown.SelectedItemIndex]]
            : null;
        private string copyIKHeader;

        public CopyPosePane(MeidoManager meidoManager)
        {
            this.meidoManager = meidoManager;

            meidoDropdown = new Dropdown(new[] { Translation.Get("systemMessage", "noMaids") });

            copyButton = new Button(Translation.Get("copyPosePane", "copyButton"));
            copyButton.ControlEvent += (s, a) => CopyPose();

            copyIKHeader = Translation.Get("copyPosePane", "header");
        }

        protected override void ReloadTranslation()
        {
            if (!PlentyOfMaids)
            {
                meidoDropdown.SetDropdownItem(0, Translation.Get("systemMessage", "noMaids"));
            }
            copyButton.Label = Translation.Get("copyPosePane", "copyButton");
            copyIKHeader = Translation.Get("copyPosePane", "header");
        }

        public override void Draw()
        {
            GUI.enabled = PlentyOfMaids;

            MpsGui.Header(copyIKHeader);
            MpsGui.WhiteLine();

            GUILayout.BeginHorizontal();
            meidoDropdown.Draw(GUILayout.Width(160f));
            copyButton.Draw(GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUI.enabled = true;
        }

        public override void UpdatePane() => SetMeidoDropdown();

        private void CopyPose()
        {
            if (meidoManager.ActiveMeidoList.Count >= 2) meidoManager.ActiveMeido.CopyPose(FromMeido);
        }

        private void SetMeidoDropdown()
        {
            if (meidoManager.ActiveMeidoList.Count >= 2)
            {
                IEnumerable<Meido> copyMeidoList = meidoManager.ActiveMeidoList
                    .Where(meido => meido.Slot != meidoManager.ActiveMeido.Slot);

                copyMeidoSlot = copyMeidoList.Select(meido => meido.Slot).ToArray();

                string[] dropdownList = copyMeidoList
                    .Select((meido, i) => $"{copyMeidoSlot[i] + 1}: {meido.LastName} {meido.FirstName}").ToArray();

                meidoDropdown.SetDropdownItems(dropdownList, 0);
            }
            else meidoDropdown.SetDropdownItems(new[] { Translation.Get("systemMessage", "noMaids") });
        }
    }
}
