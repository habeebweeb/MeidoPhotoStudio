using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

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

        mpnAttachDropdown = new(new[] { string.Empty });

        if (!Constants.MpnAttachInitialized)
            Constants.MenuFilesChange += InitializeMpnAttach;

        SetDropdownList();

        previousPropButton = new("<");
        previousPropButton.ControlEvent += (_, _) =>
            mpnAttachDropdown.Step(-1);

        nextPropButton = new(">");
        nextPropButton.ControlEvent += (_, _) =>
            mpnAttachDropdown.Step(1);

        attachPropButton = new(Translation.Get("attachMpnPropPane", "attachButton"));
        attachPropButton.ControlEvent += (_, _) =>
            AttachProp();

        detachPropButton = new(Translation.Get("attachMpnPropPane", "detachButton"));
        detachPropButton.ControlEvent += (_, _) =>
            AttachProp(detach: true);

        detachAllButton = new(Translation.Get("attachMpnPropPane", "detachAllButton"));
        detachAllButton.ControlEvent += (_, _) =>
            DetachAll();

        header = Translation.Get("attachMpnPropPane", "header");
    }

    public override void Draw()
    {
        GUI.enabled = meidoManager.HasActiveMeido && Constants.MpnAttachInitialized;

        var noExpand = GUILayout.ExpandWidth(false);

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

    protected override void ReloadTranslation()
    {
        attachPropButton.Label = Translation.Get("attachMpnPropPane", "attachButton");
        detachPropButton.Label = Translation.Get("attachMpnPropPane", "detachButton");
        detachAllButton.Label = Translation.Get("attachMpnPropPane", "detachAllButton");
        header = Translation.Get("attachMpnPropPane", "header");

        SetDropdownList();
    }

    private void InitializeMpnAttach(object sender, MenuFilesEventArgs args)
    {
        if (args.Type is MenuFilesEventArgs.EventType.MpnAttach)
            SetDropdownList();
    }

    private void SetDropdownList()
    {
        IEnumerable<string> dropdownList = !Constants.MpnAttachInitialized
            ? new[] { Translation.Get("systemMessage", "initializing") }
            : Translation.GetArray(
                "mpnAttachPropNames", Constants.MpnAttachPropList.Select(mpnProp => mpnProp.MenuFile));

        mpnAttachDropdown.SetDropdownItems(dropdownList.ToArray());
    }

    private void AttachProp(bool detach = false)
    {
        if (!meidoManager.HasActiveMeido)
            return;

        var prop = Constants.MpnAttachPropList[mpnAttachDropdown.SelectedItemIndex];

        meidoManager.ActiveMeido.SetMpnProp(prop, detach);
    }

    private void DetachAll()
    {
        if (!meidoManager.HasActiveMeido)
            return;

        meidoManager.ActiveMeido.DetachAllMpnAttach();
    }
}
