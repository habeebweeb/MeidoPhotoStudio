using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

using Curling = MeidoPhotoStudio.Plugin.Core.Character.ClothingController.Curling;
using MaskMode = TBody.MaskMode;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class DressingPane : BasePane
{
    private static readonly string[] DressingModeTranslationKeys = ["all", "underwear", "nude"];

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
    private readonly PaneHeader paneHeader;

    public DressingPane(SelectionController<CharacterController> characterSelectionController)
    {
        this.characterSelectionController = characterSelectionController
            ?? throw new ArgumentNullException(nameof(characterSelectionController));

        this.characterSelectionController.Selecting += OnCharacterSelectionChanging;
        this.characterSelectionController.Selected += OnCharacterSelectionChanged;

        detailedClothingToggle = new(Translation.Get("dressingPane", "detailedClothing"));
        detailedClothingToggle.ControlEvent += OnDetailedClothingChanged;

        dressingGrid = new(Translation.GetArray("dressingPane", DressingModeTranslationKeys));
        dressingGrid.ControlEvent += OnDressingChanged;

        clothingToggles = ClothingSlots
            .ToDictionary(slot => slot, CreateSlotToggle, EnumEqualityComparer<SlotID>.Instance);

        loadedSlots = ClothingSlots
            .ToDictionary(slot => slot, _ => false);

        curlingFrontToggle = new(Translation.Get("dressingPane", "curlingFront"));
        curlingFrontToggle.ControlEvent += OnCurlingFrontChanged;

        curlingBackToggle = new(Translation.Get("dressingPane", "curlingBack"));
        curlingBackToggle.ControlEvent += OnCurlingBackChanged;

        underwearShiftToggle = new(Translation.Get("dressingPane", "shiftPanties"));
        underwearShiftToggle.ControlEvent += OnUnderwearShiftChanged;

        paneHeader = new(Translation.Get("dressingPane", "header"), true);

        Toggle CreateSlotToggle(SlotID slot)
        {
            var toggle = new Toggle(Translation.Get("clothing", slot.ToLower()));

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

        if (!paneHeader.Enabled)
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

    protected override void ReloadTranslation()
    {
        paneHeader.Label = Translation.Get("dressingPane", "header");
        detailedClothingToggle.Label = Translation.Get("dressingPane", "detailedClothing");
        dressingGrid.SetItemsWithoutNotify(Translation.GetArray("dressingPane", DressingModeTranslationKeys));

        foreach (var (slot, clothingToggle) in clothingToggles)
            clothingToggle.Label = Translation.Get("clothing", slot.ToLower());

        clothingToggles[SlotID.headset].Label = detailedClothingToggle.Value
            ? Translation.Get("clothing", "headset")
            : Translation.Get("clothing", "headwear");

        curlingFrontToggle.Label = Translation.Get("dressingPane", "curlingFront");
        curlingBackToggle.Label = Translation.Get("dressingPane", "curlingBack");
        underwearShiftToggle.Label = Translation.Get("dressingPane", "shiftPanties");
    }

    private void UpdateControls()
    {
        UpdateDressingGrid();
        UpdateClothingToggles();
        UpdateCurlingToggles();
    }

    private void UpdateClothingToggles()
    {
        if (CurrentClothing is null)
            return;

        foreach (var slot in ClothingSlots)
            clothingToggles[slot].SetEnabledWithoutNotify(slot switch
            {
                SlotID.wear => WearSlots.Any(slot => CurrentClothing[slot]),
                SlotID.megane => CurrentClothing[SlotID.megane] || CurrentClothing[SlotID.accHead],
                SlotID.body => CurrentClothing.BodyVisible,
                SlotID.headset when !detailedClothingToggle.Value => HeadwearSlots.Any(slot => CurrentClothing[slot]),
                _ => CurrentClothing[slot],
            });

        clothingToggles[SlotID.headset].Label = detailedClothingToggle.Value
            ? Translation.Get("clothing", "headset")
            : Translation.Get("clothing", "headwear");
    }

    private void UpdateCurlingToggles()
    {
        if (CurrentClothing is null)
            return;

        curlingFrontToggle.SetEnabledWithoutNotify(CurrentClothing[Curling.Front]);
        curlingBackToggle.SetEnabledWithoutNotify(CurrentClothing[Curling.Back]);
        underwearShiftToggle.SetEnabledWithoutNotify(CurrentClothing[Curling.Shift]);
    }

    private void UpdateDressingGrid()
    {
        if (CurrentClothing is null)
            return;

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

    private void OnCharacterSelectionChanging(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (e.Selected is null)
            return;

        e.Selected.Clothing.PropertyChanged -= OnClothingPropertyChanged;
        e.Selected.Clothing.ClothingChanged -= OnClothingKeyChanged;
        e.Selected.Clothing.CurlingChanged -= OnCurlingKeyChanged;
    }

    private void OnCharacterSelectionChanged(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (CurrentClothing is null)
            return;

        e.Selected.Clothing.PropertyChanged += OnClothingPropertyChanged;
        e.Selected.Clothing.ClothingChanged += OnClothingKeyChanged;
        e.Selected.Clothing.CurlingChanged += OnCurlingKeyChanged;

        UpdateLoadedSlots();
        UpdateControls();
    }

    private void OnClothingPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var clothing = (ClothingController)sender;

        if (e.PropertyName is nameof(ClothingController.BodyVisible))
        {
            clothingToggles[SlotID.body].SetEnabledWithoutNotify(clothing.BodyVisible);
        }
        else if (e.PropertyName is nameof(ClothingController.DressingMode))
        {
            UpdateDressingGrid();
            UpdateClothingToggles();
        }
    }

    private void OnClothingKeyChanged(object sender, KeyedPropertyChangeEventArgs<SlotID> e)
    {
        var clothing = (ClothingController)sender;

        if (WearSlots.Contains(e.Key))
        {
            clothingToggles[SlotID.wear].SetEnabledWithoutNotify(WearSlots.Any(slot => clothing[slot]));
        }
        else if (e.Key is SlotID.megane or SlotID.accHead)
        {
            clothingToggles[SlotID.megane].SetEnabledWithoutNotify(clothing[SlotID.megane] || clothing[SlotID.accHead]);
        }
        else if (!detailedClothingToggle.Value && HeadwearSlots.Contains(e.Key))
        {
            clothingToggles[SlotID.headset].SetEnabledWithoutNotify(HeadwearSlots.Any(slot => clothing[slot]));
        }
        else
        {
            clothingToggles[e.Key].SetEnabledWithoutNotify(clothing[e.Key]);
        }
    }

    private void OnCurlingKeyChanged(object sender, KeyedPropertyChangeEventArgs<Curling> e)
    {
        var clothing = (ClothingController)sender;

        if (e.Key is Curling.Front)
            curlingFrontToggle.SetEnabledWithoutNotify(clothing[e.Key]);
        else if (e.Key is Curling.Back)
            curlingBackToggle.SetEnabledWithoutNotify(clothing[e.Key]);
        else if (e.Key is Curling.Shift)
            underwearShiftToggle.SetEnabledWithoutNotify(clothing[e.Key]);
    }

    private void OnDressingChanged(object sender, EventArgs e)
    {
        if (CurrentClothing is null)
            return;

        CurrentClothing.DressingMode = DressingModes[dressingGrid.SelectedItemIndex];

        UpdateClothingToggles();
    }

    private void OnDetailedClothingChanged(object sender, EventArgs e)
    {
        UpdateLoadedSlots();
        UpdateClothingToggles();
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
