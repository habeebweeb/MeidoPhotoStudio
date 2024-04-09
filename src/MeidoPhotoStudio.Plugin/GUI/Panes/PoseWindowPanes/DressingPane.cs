using MeidoPhotoStudio.Plugin.Core;
using MeidoPhotoStudio.Plugin.Core.Character;

using Curling = MeidoPhotoStudio.Plugin.Core.Character.ClothingController.Curling;
using MaskMode = TBody.MaskMode;

namespace MeidoPhotoStudio.Plugin;

public class DressingPane : BasePane
{
    private static readonly SlotID[] ClothingSlots =
    [
        SlotID.wear, SlotID.skirt, SlotID.bra, SlotID.panz, SlotID.headset, SlotID.megane, SlotID.accUde,
        SlotID.glove, SlotID.accSenaka, SlotID.stkg, SlotID.shoes, SlotID.body,

        SlotID.accAshi, SlotID.accHana, SlotID.accHat, SlotID.accHeso, SlotID.accKamiSubL, SlotID.accKamiSubR,
        SlotID.accKami_1_, SlotID.accKami_2_, SlotID.accKami_3_, SlotID.accKubi, SlotID.accKubiwa, SlotID.accMiMiL,
        SlotID.accMiMiR, SlotID.accNipL, SlotID.accNipR, SlotID.accShippo, SlotID.accXXX,
    ];

    private static readonly SlotID[] WearSlots = [SlotID.wear, SlotID.mizugi, SlotID.onepiece];

    private static readonly SlotID[] HeadwearSlots =
    [
        SlotID.headset, SlotID.accHat, SlotID.accKamiSubL, SlotID.accKamiSubR, SlotID.accKami_1_, SlotID.accKami_2_,
        SlotID.accKami_3_,
    ];

    private static readonly SlotID[][] SlotGroups =
    [
        [SlotID.wear, SlotID.skirt],
        [SlotID.bra, SlotID.panz],
        [SlotID.headset, SlotID.megane],
        [SlotID.accUde, SlotID.glove, SlotID.accSenaka],
        [SlotID.stkg, SlotID.shoes, SlotID.body],
    ];

    private static readonly SlotID[][] DetailedSlotGroups =
    [
        [SlotID.accShippo, SlotID.accHat],
        [SlotID.accKami_1_, SlotID.accKami_2_, SlotID.accKami_3_],
        [SlotID.accKamiSubL, SlotID.accKamiSubR],
        [SlotID.accMiMiL, SlotID.accMiMiR],
        [SlotID.accNipL, SlotID.accNipR],
        [SlotID.accHana, SlotID.accKubi, SlotID.accKubiwa],
        [SlotID.accHeso, SlotID.accAshi, SlotID.accXXX],
    ];

    private static readonly MaskMode[] DressingModes = [MaskMode.None, MaskMode.Underwear, MaskMode.Nude];

    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly Dictionary<SlotID, Toggle> clothingToggles;
    private readonly Dictionary<SlotID, bool> loadedSlots;
    private readonly Toggle detailedClothingToggle;
    private readonly SelectionGrid dressingGrid;
    private readonly Toggle curlingFrontToggle;
    private readonly Toggle curlingBackToggle;
    private readonly Toggle underwearShiftToggle;
    private readonly Toggle paneHeader;

    public DressingPane(SelectionController<CharacterController> characterSelectionController)
    {
        this.characterSelectionController = characterSelectionController
            ?? throw new ArgumentNullException(nameof(characterSelectionController));

        characterSelectionController.Selected += OnCharacterSelectionChanged;

        detailedClothingToggle = new("Detailed Clothing");
        detailedClothingToggle.ControlEvent += OnDetailedClothingChanged;

        dressingGrid = new(["All", "Underwear", "Nude"]);
        dressingGrid.ControlEvent += OnDressingChanged;

        clothingToggles = ClothingSlots
            .ToDictionary(slot => slot, CreateSlotToggle, EnumEqualityComparer<SlotID>.Instance);

        loadedSlots = ClothingSlots
            .ToDictionary(slot => slot, _ => false);

        curlingFrontToggle = new("Curl Front");
        curlingFrontToggle.ControlEvent += OnCurlingFrontChanged;

        curlingBackToggle = new("Curl Rear");
        curlingBackToggle.ControlEvent += OnCurlingBackChanged;

        underwearShiftToggle = new("Shift");
        underwearShiftToggle.ControlEvent += OnUnderwearShiftChanged;

        paneHeader = new("Clothing", true);

        Toggle CreateSlotToggle(SlotID slot)
        {
            var toggle = new Toggle(slot.ToString());

            toggle.ControlEvent += (_, _) =>
                OnSlotToggleChanged(slot, toggle.Value);

            return toggle;
        }
    }

    private ClothingController CurrentClothing =>
        characterSelectionController.Current?.Clothing;

    public override void Draw()
    {
        var enabled = characterSelectionController.Current is not null;

        GUI.enabled = enabled;

        paneHeader.Draw();
        MpsGui.WhiteLine();

        if (!paneHeader.Value)
            return;

        detailedClothingToggle.Draw();

        MpsGui.BlackLine();

        dressingGrid.Draw();

        MpsGui.BlackLine();

        foreach (var slotGroup in SlotGroups)
            DrawSlotGroup(slotGroup);

        if (detailedClothingToggle.Value)
        {
            MpsGui.BlackLine();

            foreach (var slotGroup in DetailedSlotGroups)
                DrawSlotGroup(slotGroup);
        }

        MpsGui.BlackLine();

        DrawCurlingToggles();

        void DrawSlotGroup(SlotID[] slots)
        {
            GUILayout.BeginHorizontal();

            for (var i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];

                GUI.enabled = enabled && loadedSlots[slot];
                clothingToggles[slot].Draw();

                if (i < slots.Length - 1)
                    GUILayout.FlexibleSpace();
            }

            GUILayout.EndHorizontal();
        }

        void DrawCurlingToggles()
        {
            GUILayout.BeginHorizontal();

            GUI.enabled = enabled && (CurrentClothing?.SupportsCurlingType(Curling.Front) ?? false);
            curlingFrontToggle.Draw();

            GUILayout.FlexibleSpace();

            GUI.enabled = enabled && (CurrentClothing?.SupportsCurlingType(Curling.Back) ?? false);
            curlingBackToggle.Draw();

            GUILayout.FlexibleSpace();

            GUI.enabled = enabled && (CurrentClothing?.SupportsCurlingType(Curling.Shift) ?? false);
            underwearShiftToggle.Draw();

            GUILayout.EndHorizontal();
        }
    }

    private void UpdateControls()
    {
        if (CurrentClothing is null)
            return;

        foreach (var slot in ClothingSlots)
        {
            var enabled = false;

            if (slot is SlotID.wear)
                enabled = WearSlots.Any(slot => CurrentClothing[slot]);
            else if (slot is SlotID.megane)
                enabled = CurrentClothing[SlotID.megane] || CurrentClothing[SlotID.accHead];
            else if (!detailedClothingToggle.Value && slot is SlotID.headset)
                enabled = HeadwearSlots.Any(slot => CurrentClothing[slot]);
            else
                enabled = CurrentClothing[slot];

            clothingToggles[slot].SetEnabledWithoutNotify(enabled);
        }

        curlingFrontToggle.SetEnabledWithoutNotify(CurrentClothing[Curling.Front]);
        curlingBackToggle.SetEnabledWithoutNotify(CurrentClothing[Curling.Back]);
        underwearShiftToggle.SetEnabledWithoutNotify(CurrentClothing[Curling.Shift]);

        var dressingIndex = Array.IndexOf(DressingModes, CurrentClothing.DressingMode);

        dressingGrid.SetValueWithoutNotify(dressingIndex);
    }

    private void UpdateLoadedSlots()
    {
        foreach (var slot in ClothingSlots)
            loadedSlots[slot] = slot switch
            {
                SlotID.wear => WearSlots.Any(CurrentClothing.SlotLoaded),
                SlotID.megane => CurrentClothing.SlotLoaded(SlotID.megane) || CurrentClothing.SlotLoaded(SlotID.accHead),
                SlotID.headset when !detailedClothingToggle.Value => HeadwearSlots.Any(CurrentClothing.SlotLoaded),
                _ => CurrentClothing.SlotLoaded(slot),
            };
    }

    private void OnCharacterSelectionChanged(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (CurrentClothing is null)
            return;

        UpdateLoadedSlots();
        UpdateControls();
    }

    private void OnDressingChanged(object sender, EventArgs e)
    {
        if (CurrentClothing is null)
            return;

        CurrentClothing.DressingMode = DressingModes[dressingGrid.SelectedItemIndex];

        UpdateControls();
    }

    private void OnDetailedClothingChanged(object sender, EventArgs e)
    {
        UpdateLoadedSlots();
        UpdateControls();
    }

    private void OnSlotToggleChanged(SlotID slot, bool value)
    {
        if (CurrentClothing is null)
            return;

        if (slot is SlotID.body)
        {
            CurrentClothing.BodyVisible = value;

            return;
        }

        if (!detailedClothingToggle.Value && slot is SlotID.headset)
        {
            foreach (var headwearSlot in HeadwearSlots)
            {
                CurrentClothing[headwearSlot] = value;
                clothingToggles[headwearSlot].SetEnabledWithoutNotify(value);
            }
        }
        else
        {
            if (slot is SlotID.wear)
            {
                foreach (var wearSlot in WearSlots)
                    CurrentClothing[wearSlot] = value;
            }
            else if (slot is SlotID.megane)
            {
                CurrentClothing[SlotID.megane] = value;
                CurrentClothing[SlotID.accHead] = value;
            }
            else
            {
                CurrentClothing[slot] = value;
            }
        }

        CurrentClothing[slot] = value;
    }

    private void OnCurlingFrontChanged(object sender, EventArgs e)
    {
        if (CurrentClothing is null)
            return;

        CurrentClothing[Curling.Front] = curlingFrontToggle.Value;

        curlingBackToggle.SetEnabledWithoutNotify(false);
    }

    private void OnCurlingBackChanged(object sender, EventArgs e)
    {
        if (CurrentClothing is null)
            return;

        CurrentClothing[Curling.Back] = curlingBackToggle.Value;

        curlingFrontToggle.SetEnabledWithoutNotify(false);
    }

    private void OnUnderwearShiftChanged(object sender, EventArgs e)
    {
        if (CurrentClothing is null)
            return;

        CurrentClothing[Curling.Shift] = underwearShiftToggle.Value;
    }
}
