using System.Collections.Generic;

using UnityEngine;

using static MeidoPhotoStudio.Plugin.Meido;
using static TBody;

namespace MeidoPhotoStudio.Plugin;

public class MaidDressingPane : BasePane
{
    public static readonly SlotID[] ClothingSlots =
    {
        // main slots
        SlotID.wear, SlotID.skirt, SlotID.bra, SlotID.panz, SlotID.headset, SlotID.megane, SlotID.accUde,
        SlotID.glove, SlotID.accSenaka, SlotID.stkg, SlotID.shoes, SlotID.body,

        // detailed slots
        SlotID.accAshi, SlotID.accHana, SlotID.accHat, SlotID.accHeso, SlotID.accKamiSubL, SlotID.accKamiSubR,
        SlotID.accKami_1_, SlotID.accKami_2_, SlotID.accKami_3_, SlotID.accKubi, SlotID.accKubiwa, SlotID.accMiMiL,
        SlotID.accMiMiR, SlotID.accNipL, SlotID.accNipR, SlotID.accShippo, SlotID.accXXX,

        // unused slots
        // SlotID.mizugi, SlotID.onepiece, SlotID.accHead,
    };

    public static readonly SlotID[] BodySlots =
    {
        SlotID.body, SlotID.head, SlotID.eye, SlotID.hairF, SlotID.hairR, SlotID.hairS, SlotID.hairT, SlotID.hairAho,
        SlotID.chikubi, SlotID.underhair, SlotID.moza, SlotID.accHa,
    };

    public static readonly SlotID[] WearSlots = { SlotID.wear, SlotID.mizugi, SlotID.onepiece };

    public static readonly SlotID[] HeadwearSlots =
    {
        SlotID.headset, SlotID.accHat, SlotID.accKamiSubL, SlotID.accKamiSubR, SlotID.accKami_1_, SlotID.accKami_2_,
        SlotID.accKami_3_,
    };

    private static readonly string[] MaskLabels = { "all", "underwear", "nude" };

    private readonly MeidoManager meidoManager;
    private readonly Dictionary<SlotID, Toggle> clothingToggles;
    private readonly Dictionary<SlotID, bool> loadedSlots;
    private readonly Toggle detailedClothingToggle;
    private readonly SelectionGrid maskModeGrid;
    private readonly Toggle curlingFrontToggle;
    private readonly Toggle curlingBackToggle;
    private readonly Toggle pantsuShiftToggle;

    private bool detailedClothing;

    public MaidDressingPane(MeidoManager meidoManager)
    {
        this.meidoManager = meidoManager;

        clothingToggles = new(ClothingSlots.Length);
        loadedSlots = new(ClothingSlots.Length);

        foreach (var slot in ClothingSlots)
        {
            var slotToggle = new Toggle(Translation.Get("clothing", slot.ToString()));

            slotToggle.ControlEvent += (_, _) =>
                ToggleClothing(slot, slotToggle.Value);

            clothingToggles.Add(slot, slotToggle);
            loadedSlots[slot] = true;
        }

        detailedClothingToggle = new(Translation.Get("clothing", "detail"));
        detailedClothingToggle.ControlEvent += (_, _) =>
            UpdateDetailedClothing();

        curlingFrontToggle = new(Translation.Get("clothing", "curlingFront"));
        curlingFrontToggle.ControlEvent += (_, _) =>
            ToggleCurling(Curl.Front, curlingFrontToggle.Value);

        curlingBackToggle = new(Translation.Get("clothing", "curlingBack"));
        curlingBackToggle.ControlEvent += (_, _) =>
            ToggleCurling(Curl.Back, curlingBackToggle.Value);

        pantsuShiftToggle = new(Translation.Get("clothing", "shiftPanties"));
        pantsuShiftToggle.ControlEvent += (_, _) =>
            ToggleCurling(Curl.Shift, pantsuShiftToggle.Value);

        maskModeGrid = new(Translation.GetArray("clothing", MaskLabels));
        maskModeGrid.ControlEvent += (_, _) =>
            SetMaskMode((Mask)maskModeGrid.SelectedItemIndex);

        UpdateDetailedClothing();
    }

    public override void UpdatePane()
    {
        if (!meidoManager.HasActiveMeido)
            return;

        updating = true;

        var meido = meidoManager.ActiveMeido;
        var body = meido.Maid.body0;

        foreach (var clothingSlot in ClothingSlots)
        {
            var toggleValue = false;
            var hasSlot = false;

            if (clothingSlot is SlotID.wear)
            {
                foreach (var wearSlot in WearSlots)
                {
                    if (body.GetMask(wearSlot))
                        toggleValue = true;

                    if (body.GetSlotLoaded(wearSlot))
                        hasSlot = true;

                    if (hasSlot && toggleValue)
                        break;
                }
            }
            else if (clothingSlot is SlotID.megane)
            {
                toggleValue = body.GetMask(SlotID.megane) || body.GetMask(SlotID.accHead);
                hasSlot = body.GetSlotLoaded(SlotID.megane) || body.GetSlotLoaded(SlotID.accHead);
            }
            else if (!detailedClothing && clothingSlot is SlotID.headset)
            {
                foreach (var headwearSlot in HeadwearSlots)
                {
                    if (body.GetMask(headwearSlot))
                        toggleValue = true;

                    if (body.GetSlotLoaded(headwearSlot))
                        hasSlot = true;

                    if (hasSlot && toggleValue)
                        break;
                }
            }
            else
            {
                toggleValue = body.GetMask(clothingSlot);
                hasSlot = body.GetSlotLoaded(clothingSlot);
            }

            clothingToggles[clothingSlot].Value = hasSlot && toggleValue;
            loadedSlots[clothingSlot] = hasSlot;
        }

        curlingFrontToggle.Value = meido.CurlingFront;
        curlingBackToggle.Value = meido.CurlingBack;
        pantsuShiftToggle.Value = meido.PantsuShift;

        var maskMode = meido.CurrentMaskMode;

        maskModeGrid.SelectedItemIndex = maskMode is MaskMode.Nude ? (int)Mask.Nude : (int)maskMode;

        updating = false;
    }

    public override void Draw()
    {
        GUI.enabled = Enabled = meidoManager.HasActiveMeido;

        detailedClothingToggle.Draw();

        MpsGui.BlackLine();

        maskModeGrid.Draw();

        MpsGui.BlackLine();

        DrawSlotGroup(SlotID.wear, SlotID.skirt);
        DrawSlotGroup(SlotID.bra, SlotID.panz);
        DrawSlotGroup(SlotID.headset, SlotID.megane);
        DrawSlotGroup(SlotID.accUde, SlotID.glove, SlotID.accSenaka);
        DrawSlotGroup(SlotID.stkg, SlotID.shoes, SlotID.body);

        if (detailedClothing)
        {
            MpsGui.BlackLine();
            DrawSlotGroup(SlotID.accShippo, SlotID.accHat);
            DrawSlotGroup(SlotID.accKami_1_, SlotID.accKami_2_, SlotID.accKami_3_);
            DrawSlotGroup(SlotID.accKamiSubL, SlotID.accKamiSubR);
            DrawSlotGroup(SlotID.accMiMiL, SlotID.accMiMiR);
            DrawSlotGroup(SlotID.accNipL, SlotID.accNipR);
            DrawSlotGroup(SlotID.accHana, SlotID.accKubi, SlotID.accKubiwa);
            DrawSlotGroup(SlotID.accHeso, SlotID.accAshi, SlotID.accXXX);
        }

        MpsGui.BlackLine();

        GUILayout.BeginHorizontal();
        curlingFrontToggle.Draw();
        GUILayout.FlexibleSpace();
        curlingBackToggle.Draw();
        GUILayout.FlexibleSpace();
        pantsuShiftToggle.Draw();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUI.enabled = true;
    }

    protected override void ReloadTranslation()
    {
        foreach (var slot in ClothingSlots)
        {
            var clothingToggle = clothingToggles[slot];

            if (slot is SlotID.headset)
                clothingToggle.Label = detailedClothing
                    ? Translation.Get("clothing", "headset")
                    : Translation.Get("clothing", "headwear");
            else
                clothingToggle.Label = Translation.Get("clothing", slot.ToString());
        }

        updating = true;
        maskModeGrid.SetItems(Translation.GetArray("clothing", MaskLabels));
        updating = false;

        detailedClothingToggle.Label = Translation.Get("clothing", "detail");
        curlingFrontToggle.Label = Translation.Get("clothing", "curlingFront");
        curlingBackToggle.Label = Translation.Get("clothing", "curlingBack");
        pantsuShiftToggle.Label = Translation.Get("clothing", "shiftPanties");
    }

    private void ToggleClothing(SlotID slot, bool enabled)
    {
        if (updating)
            return;

        if (slot is SlotID.body)
        {
            meidoManager.ActiveMeido.SetBodyMask(enabled);

            return;
        }

        var body = meidoManager.ActiveMeido.Maid.body0;

        if (!detailedClothing && slot is SlotID.headset)
        {
            updating = true;

            foreach (var wearSlot in HeadwearSlots)
            {
                body.SetMask(wearSlot, enabled);
                clothingToggles[wearSlot].Value = enabled;
            }

            updating = false;
        }
        else
        {
            if (slot is SlotID.wear)
            {
                foreach (var wearSlot in WearSlots)
                    body.SetMask(wearSlot, enabled);
            }
            else if (slot is SlotID.megane)
            {
                body.SetMask(SlotID.megane, enabled);
                body.SetMask(SlotID.accHead, enabled);
            }
            else
            {
                body.SetMask(slot, enabled);
            }
        }
    }

    private void ToggleCurling(Curl curl, bool enabled)
    {
        if (updating)
            return;

        meidoManager.ActiveMeido.SetCurling(curl, enabled);

        if (!enabled)
            return;

        updating = true;

        if (curl is Curl.Front && curlingBackToggle.Value)
            curlingBackToggle.Value = false;
        else if (curl is Curl.Back && curlingFrontToggle.Value)
            curlingFrontToggle.Value = false;

        updating = false;
    }

    private void SetMaskMode(Mask mask)
    {
        if (updating)
            return;

        meidoManager.ActiveMeido.SetMaskMode(mask);

        UpdatePane();
    }

    private void DrawSlotGroup(params SlotID[] slots)
    {
        GUILayout.BeginHorizontal();

        for (var i = 0; i < slots.Length; i++)
        {
            var slot = slots[i];

            GUI.enabled = Enabled && loadedSlots[slot];
            clothingToggles[slot].Draw();

            if (i < slots.Length - 1)
                GUILayout.FlexibleSpace();
        }

        GUILayout.EndHorizontal();

        GUI.enabled = Enabled;
    }

    private void UpdateDetailedClothing()
    {
        detailedClothing = detailedClothingToggle.Value;
        clothingToggles[SlotID.headset].Label = detailedClothing
            ? Translation.Get("clothing", "headset")
            : Translation.Get("clothing", "headwear");

        UpdatePane();
    }
}
