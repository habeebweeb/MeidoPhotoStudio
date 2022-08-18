using System.Linq;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class CopyPosePane : BasePane
{
    private readonly MeidoManager meidoManager;
    private readonly Button copyButton;
    private readonly Dropdown meidoDropdown;

    private int[] copyMeidoSlot;
    private string copyIKHeader;

    public CopyPosePane(MeidoManager meidoManager)
    {
        this.meidoManager = meidoManager;

        meidoDropdown = new(new[] { Translation.Get("systemMessage", "noMaids") });

        copyButton = new(Translation.Get("copyPosePane", "copyButton"));
        copyButton.ControlEvent += (_, _) =>
            CopyPose();

        copyIKHeader = Translation.Get("copyPosePane", "header");
    }

    private bool PlentyOfMaids =>
        meidoManager.ActiveMeidoList.Count >= 2;

    private Meido FromMeido =>
        meidoManager.HasActiveMeido
            ? meidoManager.ActiveMeidoList[copyMeidoSlot[meidoDropdown.SelectedItemIndex]]
            : null;

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

    public override void UpdatePane() =>
        SetMeidoDropdown();

    protected override void ReloadTranslation()
    {
        if (!PlentyOfMaids)
            meidoDropdown.SetDropdownItem(0, Translation.Get("systemMessage", "noMaids"));

        copyButton.Label = Translation.Get("copyPosePane", "copyButton");
        copyIKHeader = Translation.Get("copyPosePane", "header");
    }

    private void CopyPose()
    {
        if (meidoManager.ActiveMeidoList.Count >= 2)
            meidoManager.ActiveMeido.CopyPose(FromMeido);
    }

    private void SetMeidoDropdown()
    {
        if (meidoManager.ActiveMeidoList.Count >= 2)
        {
            var copyMeidoList = meidoManager.ActiveMeidoList
                .Where(meido => meido.Slot != meidoManager.ActiveMeido.Slot);

            copyMeidoSlot = copyMeidoList.Select(meido => meido.Slot).ToArray();

            var dropdownList = copyMeidoList
                .Select((meido, i) => $"{copyMeidoSlot[i] + 1}: {meido.LastName} {meido.FirstName}").ToArray();

            meidoDropdown.SetDropdownItems(dropdownList, 0);
        }
        else
        {
            meidoDropdown.SetDropdownItems(new[] { Translation.Get("systemMessage", "noMaids") });
        }
    }
}
