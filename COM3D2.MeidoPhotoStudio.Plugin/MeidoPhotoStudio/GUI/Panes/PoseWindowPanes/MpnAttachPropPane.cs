using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class MpnAttachPropPane : BasePane
    {
        private MeidoManager meidoManager;
        private Dropdown mpnAttachDropdown;
        private Button previousPropButton;
        private Button nextPropButton;
        private Button attachPropButton;
        private Button detachPropButton;
        private Button detachAllButton;

        public MpnAttachPropPane(MeidoManager meidoManager)
        {
            this.meidoManager = meidoManager;

            this.mpnAttachDropdown = new Dropdown(new[] { string.Empty });

            if (!Constants.MpnAttachInitialized) Constants.MenuFilesChange += InitializeMpnAttach;

            SetDropdownList();

            this.previousPropButton = new Button("<");
            this.previousPropButton.ControlEvent += (s, a) => this.mpnAttachDropdown.Step(-1);

            this.nextPropButton = new Button(">");
            this.nextPropButton.ControlEvent += (s, a) => this.mpnAttachDropdown.Step(1);

            this.attachPropButton = new Button(Translation.Get("attachMpnPropPane", "attachButton"));
            this.attachPropButton.ControlEvent += (s, a) => AttachProp();

            this.detachPropButton = new Button(Translation.Get("attachMpnPropPane", "detachButton"));
            this.detachPropButton.ControlEvent += (s, a) => AttachProp(detach: true);

            this.detachAllButton = new Button(Translation.Get("attachMpnPropPane", "detachAllButton"));
            this.detachAllButton.ControlEvent += (s, a) => DetachAll();
        }

        protected override void ReloadTranslation()
        {
            this.attachPropButton.Label = Translation.Get("attachMpnPropPane", "attachButton");
            this.detachPropButton.Label = Translation.Get("attachMpnPropPane", "detachButton");
            this.detachAllButton.Label = Translation.Get("attachMpnPropPane", "detachAllButton");
            SetDropdownList();
        }

        public override void Draw()
        {
            GUI.enabled = this.meidoManager.HasActiveMeido && Constants.MpnAttachInitialized;

            GUILayoutOption noExpand = GUILayout.ExpandWidth(false);

            GUILayout.BeginHorizontal();
            this.previousPropButton.Draw(noExpand);
            this.mpnAttachDropdown.Draw(GUILayout.Width(153f));
            this.nextPropButton.Draw(noExpand);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            this.attachPropButton.Draw();
            this.detachPropButton.Draw();
            GUILayout.EndHorizontal();
            this.detachAllButton.Draw();

            GUI.enabled = true;
        }

        private void InitializeMpnAttach(object sender, MenuFilesEventArgs args)
        {
            if (args.Type == MenuFilesEventArgs.EventType.MpnAttach) SetDropdownList();
        }

        private void SetDropdownList()
        {
            IEnumerable<string> dropdownList = !Constants.MpnAttachInitialized
                ? new[] { Translation.Get("systemMessage", "initializing") }
                : Translation.GetArray(
                    "mpnAttachPropNames", Constants.MpnAttachPropList.Select(mpnProp => mpnProp.MenuFile)
                );

            this.mpnAttachDropdown.SetDropdownItems(dropdownList.ToArray());
        }

        private void AttachProp(bool detach = false)
        {
            if (!this.meidoManager.HasActiveMeido) return;

            MpnAttachProp prop = Constants.MpnAttachPropList[this.mpnAttachDropdown.SelectedItemIndex];

            this.meidoManager.ActiveMeido.SetMpnProp(prop, detach);
        }

        private void DetachAll()
        {
            if (!this.meidoManager.HasActiveMeido) return;

            this.meidoManager.ActiveMeido.DetachAllMpnAttach();
        }
    }
}
