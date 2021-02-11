using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin
{
    public class MpnAttachPropPane : BasePane
    {
        private readonly MeidoManager meidoManager;
        private readonly Dropdown mpnAttachDropdown;
        private readonly Button previousPropButton;
        private readonly Button nextPropButton;
        private readonly Button attachPropButton;
        private readonly Button detachPropButton;
        private readonly Button detachAllButton;
        private string header;

        public MpnAttachPropPane(MeidoManager meidoManager)
        {
            this.meidoManager = meidoManager;

            mpnAttachDropdown = new Dropdown(new[] { string.Empty });

            if (!Constants.MpnAttachInitialized) Constants.MenuFilesChange += InitializeMpnAttach;

            SetDropdownList();

            previousPropButton = new Button("<");
            previousPropButton.ControlEvent += (s, a) => mpnAttachDropdown.Step(-1);

            nextPropButton = new Button(">");
            nextPropButton.ControlEvent += (s, a) => mpnAttachDropdown.Step(1);

            attachPropButton = new Button(Translation.Get("attachMpnPropPane", "attachButton"));
            attachPropButton.ControlEvent += (s, a) => AttachProp();

            detachPropButton = new Button(Translation.Get("attachMpnPropPane", "detachButton"));
            detachPropButton.ControlEvent += (s, a) => AttachProp(detach: true);

            detachAllButton = new Button(Translation.Get("attachMpnPropPane", "detachAllButton"));
            detachAllButton.ControlEvent += (s, a) => DetachAll();

            header = Translation.Get("attachMpnPropPane", "header");
        }

        protected override void ReloadTranslation()
        {
            attachPropButton.Label = Translation.Get("attachMpnPropPane", "attachButton");
            detachPropButton.Label = Translation.Get("attachMpnPropPane", "detachButton");
            detachAllButton.Label = Translation.Get("attachMpnPropPane", "detachAllButton");
            header = Translation.Get("attachMpnPropPane", "header");
            SetDropdownList();
        }

        public override void Draw()
        {
            GUI.enabled = meidoManager.HasActiveMeido && Constants.MpnAttachInitialized;

            GUILayoutOption noExpand = GUILayout.ExpandWidth(false);

            MpsGui.Header(header);
            MpsGui.WhiteLine();

            GUILayout.BeginHorizontal();
            previousPropButton.Draw(noExpand);
            mpnAttachDropdown.Draw(GUILayout.Width(153f));
            nextPropButton.Draw(noExpand);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            attachPropButton.Draw();
            detachPropButton.Draw();
            GUILayout.EndHorizontal();
            detachAllButton.Draw();

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

            mpnAttachDropdown.SetDropdownItems(dropdownList.ToArray());
        }

        private void AttachProp(bool detach = false)
        {
            if (!meidoManager.HasActiveMeido) return;

            MpnAttachProp prop = Constants.MpnAttachPropList[mpnAttachDropdown.SelectedItemIndex];

            meidoManager.ActiveMeido.SetMpnProp(prop, detach);
        }

        private void DetachAll()
        {
            if (!meidoManager.HasActiveMeido) return;

            meidoManager.ActiveMeido.DetachAllMpnAttach();
        }
    }
}
