using System;
using System.Collections.Generic;
using UnityEngine;
using static TBody;

namespace MeidoPhotoStudio.Plugin
{
    using static Meido;
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
            SlotID.accMiMiR, SlotID.accNipL, SlotID.accNipR, SlotID.accShippo, SlotID.accXXX
            // unused slots
            // SlotID.mizugi, SlotID.onepiece, SlotID.accHead,
        };

        public static readonly SlotID[] BodySlots =
        {
            SlotID.body, SlotID.head, SlotID.eye, SlotID.hairF, SlotID.hairR, SlotID.hairS, SlotID.hairT,
            SlotID.hairAho, SlotID.chikubi, SlotID.underhair, SlotID.moza, SlotID.accHa
        };

        public static readonly SlotID[] WearSlots = { SlotID.wear, SlotID.mizugi, SlotID.onepiece };

        public static readonly SlotID[] HeadwearSlots =
        {
            SlotID.headset, SlotID.accHat, SlotID.accKamiSubL, SlotID.accKamiSubR, SlotID.accKami_1_,
            SlotID.accKami_2_, SlotID.accKami_3_
        };

        private readonly MeidoManager meidoManager;
        private readonly Dictionary<SlotID, Toggle> clothingToggles;
        private readonly Dictionary<SlotID, bool> loadedSlots;
        private readonly Toggle detailedClothingToggle;
        private readonly SelectionGrid maskModeGrid;
        private readonly Toggle curlingFrontToggle;
        private readonly Toggle curlingBackToggle;
        private readonly Toggle pantsuShiftToggle;
        private bool detailedClothing;
        private static readonly string[] maskLabels = { "all", "underwear", "nude" };

        public MaidDressingPane(MeidoManager meidoManager)
        {
            this.meidoManager = meidoManager;

            clothingToggles = new Dictionary<SlotID, Toggle>(ClothingSlots.Length);
            loadedSlots = new Dictionary<SlotID, bool>(ClothingSlots.Length);
            foreach (SlotID slot in ClothingSlots)
            {
                var slotToggle = new Toggle(Translation.Get("clothing", slot.ToString()));
                slotToggle.ControlEvent += (s, a) => ToggleClothing(slot, slotToggle.Value);
                clothingToggles.Add(slot, slotToggle);
                loadedSlots[slot] = true;
            }

            detailedClothingToggle = new Toggle(Translation.Get("clothing", "detail"));
            detailedClothingToggle.ControlEvent += (s, a) => UpdateDetailedClothing();

            curlingFrontToggle = new Toggle(Translation.Get("clothing", "curlingFront"));
            curlingFrontToggle.ControlEvent += (s, a) => ToggleCurling(Curl.Front, curlingFrontToggle.Value);
            curlingBackToggle = new Toggle(Translation.Get("clothing", "curlingBack"));
            curlingBackToggle.ControlEvent += (s, a) => ToggleCurling(Curl.Back, curlingBackToggle.Value);
            pantsuShiftToggle = new Toggle(Translation.Get("clothing", "shiftPanties"));
            pantsuShiftToggle.ControlEvent += (s, a) => ToggleCurling(Curl.Shift, pantsuShiftToggle.Value);

            maskModeGrid = new SelectionGrid(Translation.GetArray("clothing", maskLabels));
            maskModeGrid.ControlEvent += (s, a) => SetMaskMode((Mask)maskModeGrid.SelectedItemIndex);

            UpdateDetailedClothing();
        }

        protected override void ReloadTranslation()
        {
            foreach (SlotID slot in ClothingSlots)
            {
                Toggle clothingToggle = clothingToggles[slot];
                if (slot == SlotID.headset)
                {
                    clothingToggle.Label = detailedClothing
                        ? Translation.Get("clothing", "headset")
                        : Translation.Get("clothing", "headwear");
                }
                else clothingToggle.Label = Translation.Get("clothing", slot.ToString());
            }

            updating = true;
            maskModeGrid.SetItems(Translation.GetArray("clothing", maskLabels));
            updating = false;

            detailedClothingToggle.Label = Translation.Get("clothing", "detail");
            curlingFrontToggle.Label = Translation.Get("clothing", "curlingFront");
            curlingBackToggle.Label = Translation.Get("clothing", "curlingBack");
            pantsuShiftToggle.Label = Translation.Get("clothing", "shiftPanties");
        }

        private void ToggleClothing(SlotID slot, bool enabled)
        {
            if (updating) return;

            if (slot == SlotID.body)
            {
                meidoManager.ActiveMeido.SetBodyMask(enabled);
                return;
            }

            TBody body = meidoManager.ActiveMeido.Maid.body0;

            if (!detailedClothing && slot == SlotID.headset)
            {
                updating = true;
                foreach (SlotID wearSlot in HeadwearSlots)
                {
                    body.SetMask(wearSlot, enabled);
                    clothingToggles[wearSlot].Value = enabled;
                }
                updating = false;
            }
            else
            {
                if (slot == SlotID.wear)
                {
                    foreach (SlotID wearSlot in WearSlots) body.SetMask(wearSlot, enabled);
                }
                else if (slot == SlotID.megane)
                {
                    body.SetMask(SlotID.megane, enabled);
                    body.SetMask(SlotID.accHead, enabled);
                }
                else body.SetMask(slot, enabled);
            }
        }

        private void ToggleCurling(Curl curl, bool enabled)
        {
            if (updating) return;

            meidoManager.ActiveMeido.SetCurling(curl, enabled);

            if (!enabled) return;

            updating = true;
            if (curl == Curl.Front && curlingBackToggle.Value) curlingBackToggle.Value = false;
            else if (curl == Curl.Back && curlingFrontToggle.Value) curlingFrontToggle.Value = false;

            updating = false;
        }

        private void SetMaskMode(Mask mask)
        {
            if (updating) return;

            meidoManager.ActiveMeido.SetMaskMode(mask);

            UpdatePane();
        }

        public override void UpdatePane()
        {
            if (!meidoManager.HasActiveMeido) return;

            updating = true;

            Meido meido = meidoManager.ActiveMeido;
            TBody body = meido.Maid.body0;

            foreach (SlotID clothingSlot in ClothingSlots)
            {
                var toggleValue = false;
                var hasSlot = false;
                if (clothingSlot == SlotID.wear)
                {
                    foreach (SlotID wearSlot in WearSlots)
                    {
                        if (body.GetMask(wearSlot)) toggleValue = true;
                        if (body.GetSlotLoaded(wearSlot)) hasSlot = true;
                        if (hasSlot && toggleValue) break;
                    }
                }
                else if (clothingSlot == SlotID.megane)
                {
                    toggleValue = body.GetMask(SlotID.megane) || body.GetMask(SlotID.accHead);
                    hasSlot = body.GetSlotLoaded(SlotID.megane) || body.GetSlotLoaded(SlotID.accHead);
                }
                else if (!detailedClothing && clothingSlot == SlotID.headset)
                {
                    foreach (SlotID headwearSlot in HeadwearSlots)
                    {
                        if (body.GetMask(headwearSlot)) toggleValue = true;
                        if (body.GetSlotLoaded(headwearSlot)) hasSlot = true;
                        if (hasSlot && toggleValue) break;
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

            MaskMode maskMode = meido.CurrentMaskMode;

            maskModeGrid.SelectedItemIndex = maskMode == MaskMode.Nude ? (int)Mask.Nude : (int)maskMode;

            updating = false;
        }

        private void DrawSlotGroup(params SlotID[] slots)
        {
            GUILayout.BeginHorizontal();
            for (var i = 0; i < slots.Length; i++)
            {
                SlotID slot = slots[i];
                GUI.enabled = Enabled && loadedSlots[slot];
                clothingToggles[slot].Draw();
                if (i < slots.Length - 1) GUILayout.FlexibleSpace();
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
    }
}
