using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class CopyPosePane : BasePane
    {
        private MeidoManager meidoManager;
        private Button copyButton;
        private Dropdown meidoDropdown;
        private int[] copyMeidoSlot;
        private bool PlentyOfMaids => this.meidoManager.ActiveMeidoList.Count >= 2;
        private Meido FromMeido
        {
            get => this.meidoManager.HasActiveMeido
                ? this.meidoManager.ActiveMeidoList[this.copyMeidoSlot[this.meidoDropdown.SelectedItemIndex]]
                : null;
        }

        public CopyPosePane(MeidoManager meidoManager)
        {
            this.meidoManager = meidoManager;

            this.meidoDropdown = new Dropdown(new[] { Translation.Get("systemMessage", "noMaids") });

            this.copyButton = new Button(Translation.Get("copyPosePane", "copyButton"));
            this.copyButton.ControlEvent += (s, a) => CopyPose();
        }

        protected override void ReloadTranslation()
        {
            if (!PlentyOfMaids)
            {
                this.meidoDropdown.SetDropdownItem(0, Translation.Get("systemMessage", "noMaids"));
            }
            this.copyButton.Label = Translation.Get("copyPosePane", "copyButton");
        }

        public override void Draw()
        {
            GUI.enabled = PlentyOfMaids;
            GUILayout.BeginHorizontal();
            this.meidoDropdown.Draw(GUILayout.Width(160f));
            this.copyButton.Draw(GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
            GUI.enabled = false;
        }

        public override void UpdatePane()
        {
            SetMeidoDropdown();
        }

        private void CopyPose()
        {
            if (this.meidoManager.ActiveMeidoList.Count >= 2)
            {
                this.meidoManager.ActiveMeido.CopyPose(FromMeido);
            }
        }

        private void SetMeidoDropdown()
        {
            if (this.meidoManager.ActiveMeidoList.Count >= 2)
            {
                IEnumerable<Meido> copyMeidoList = this.meidoManager.ActiveMeidoList
                    .Where(meido => meido.ActiveSlot != this.meidoManager.ActiveMeido.ActiveSlot);

                copyMeidoSlot = copyMeidoList.Select(meido => meido.ActiveSlot).ToArray();

                string[] dropdownList = copyMeidoList
                    .Select((meido, i) => $"{copyMeidoSlot[i] + 1}: {meido.LastName} {meido.FirstName}").ToArray();

                this.meidoDropdown.SetDropdownItems(dropdownList, 0);
            }
            else this.meidoDropdown.SetDropdownItems(new[] { Translation.Get("systemMessage", "noMaids") });
        }
    }
}
