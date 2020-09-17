using System.Collections.Generic;
using UnityEngine;
using static TBody;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class MaidDressingPane : BasePane
    {
        public static readonly SlotID[] clothingSlots = {
            // main slots
            SlotID.wear, SlotID.skirt, SlotID.bra, SlotID.panz, SlotID.headset, SlotID.megane,
            SlotID.accUde, SlotID.glove, SlotID.accSenaka, SlotID.stkg, SlotID.shoes, SlotID.body,
            // detailed slots
            SlotID.accAshi, SlotID.accHana, SlotID.accHat, SlotID.accHeso, SlotID.accKamiSubL,
            SlotID.accKamiSubR, SlotID.accKami_1_, SlotID.accKami_2_, SlotID.accKami_3_, SlotID.accKubi,
            SlotID.accKubiwa, SlotID.accMiMiL, SlotID.accMiMiR, SlotID.accNipL, SlotID.accNipR,
            SlotID.accShippo, SlotID.accXXX
            // unused slots
            // SlotID.mizugi, SlotID.onepiece, SlotID.accHead,
        };
        public static readonly SlotID[] bodySlots = {
            SlotID.body, SlotID.head, SlotID.eye, SlotID.hairF, SlotID.hairR,
            SlotID.hairS, SlotID.hairT, SlotID.hairAho, SlotID.chikubi, SlotID.underhair,
            SlotID.moza, SlotID.accHa
        };
        public static readonly SlotID[] wearSlots = {
            SlotID.wear, SlotID.mizugi, SlotID.onepiece
        };
        public static readonly SlotID[] headwearSlots = {
            SlotID.headset, SlotID.accHat, SlotID.accKamiSubL,
            SlotID.accKamiSubR, SlotID.accKami_1_, SlotID.accKami_2_, SlotID.accKami_3_
        };
        private readonly MeidoManager meidoManager;
        private readonly Dictionary<SlotID, Toggle> ClothingToggles;
        private readonly Dictionary<SlotID, bool> LoadedSlots;
        private readonly Toggle detailedClothingToggle;
        private readonly Toggle curlingFrontToggle;
        private readonly Toggle curlingBackToggle;
        private readonly Toggle pantsuShiftToggle;
        private bool detailedClothing;

        public MaidDressingPane(MeidoManager meidoManager)
        {
            this.meidoManager = meidoManager;

            ClothingToggles = new Dictionary<SlotID, Toggle>(clothingSlots.Length);
            LoadedSlots = new Dictionary<SlotID, bool>(clothingSlots.Length);
            foreach (SlotID slot in clothingSlots)
            {
                Toggle slotToggle = new Toggle(Translation.Get("clothing", slot.ToString()));
                slotToggle.ControlEvent += (s, a) => ToggleClothing(slot, slotToggle.Value);
                ClothingToggles.Add(slot, slotToggle);
                LoadedSlots[slot] = true;
            }

            detailedClothingToggle = new Toggle(Translation.Get("clothing", "detail"));
            detailedClothingToggle.ControlEvent += (s, a) => UpdateDetailedClothing();

            curlingFrontToggle = new Toggle(Translation.Get("clothing", "curlingFront"));
            curlingFrontToggle.ControlEvent += (s, a) => ToggleCurling(Meido.Curl.front, curlingFrontToggle.Value);
            curlingBackToggle = new Toggle(Translation.Get("clothing", "curlingBack"));
            curlingBackToggle.ControlEvent += (s, a) => ToggleCurling(Meido.Curl.back, curlingBackToggle.Value);
            pantsuShiftToggle = new Toggle(Translation.Get("clothing", "shiftPanties"));
            pantsuShiftToggle.ControlEvent += (s, a) => ToggleCurling(Meido.Curl.shift, pantsuShiftToggle.Value);

            UpdateDetailedClothing();
        }

        protected override void ReloadTranslation()
        {
            foreach (SlotID slot in clothingSlots)
            {
                Toggle clothingToggle = ClothingToggles[slot];
                if (slot == SlotID.headset)
                {
                    clothingToggle.Label = detailedClothing
                        ? Translation.Get("clothing", "headset")
                        : Translation.Get("clothing", "headwear");
                }
                else clothingToggle.Label = Translation.Get("clothing", slot.ToString());
            }

            detailedClothingToggle.Label = Translation.Get("clothing", "detail");
            curlingFrontToggle.Label = Translation.Get("clothing", "curlingFront");
            curlingBackToggle.Label = Translation.Get("clothing", "curlingBack");
            pantsuShiftToggle.Label = Translation.Get("clothing", "shiftPanties");
        }

        public void ToggleClothing(SlotID slot, bool enabled)
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
                foreach (SlotID wearSlot in headwearSlots)
                {
                    body.SetMask(wearSlot, enabled);
                    ClothingToggles[wearSlot].Value = enabled;
                }
                updating = false;
            }
            else
            {
                if (slot == SlotID.wear)
                {
                    foreach (SlotID wearSlot in wearSlots)
                    {
                        body.SetMask(wearSlot, enabled);
                    }
                }
                else if (slot == SlotID.megane)
                {
                    body.SetMask(SlotID.megane, enabled);
                    body.SetMask(SlotID.accHead, enabled);
                }
                else body.SetMask(slot, enabled);
            }
        }

        public void ToggleCurling(Meido.Curl curl, bool enabled)
        {
            if (updating) return;

            meidoManager.ActiveMeido.SetCurling(curl, enabled);

            if (enabled)
            {
                updating = true;
                if (curl == Meido.Curl.front && curlingBackToggle.Value)
                {
                    curlingBackToggle.Value = false;
                }
                else if (curl == Meido.Curl.back && curlingFrontToggle.Value)
                {
                    curlingFrontToggle.Value = false;
                }
                updating = false;
            }
        }

        public override void UpdatePane()
        {
            if (!meidoManager.HasActiveMeido) return;

            updating = true;

            Meido meido = meidoManager.ActiveMeido;
            TBody body = meido.Maid.body0;
            foreach (SlotID clothingSlot in clothingSlots)
            {
                bool toggleValue = false;
                bool hasSlot = false;
                if (clothingSlot == SlotID.wear)
                {
                    foreach (SlotID wearSlot in wearSlots)
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
                    foreach (SlotID headwearSlot in headwearSlots)
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

                ClothingToggles[clothingSlot].Value = hasSlot && toggleValue;
                LoadedSlots[clothingSlot] = hasSlot;
            }

            curlingFrontToggle.Value = meido.CurlingFront;
            curlingBackToggle.Value = meido.CurlingBack;
            pantsuShiftToggle.Value = meido.PantsuShift;

            updating = false;
        }

        private void DrawSlotGroup(params SlotID[] slots)
        {
            GUILayout.BeginHorizontal();
            for (int i = 0; i < slots.Length; i++)
            {
                SlotID slot = slots[i];
                GUI.enabled = Enabled && LoadedSlots[slot];
                ClothingToggles[slot].Draw();
                if (i < slots.Length - 1) GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }

        private void UpdateDetailedClothing()
        {
            detailedClothing = detailedClothingToggle.Value;
            ClothingToggles[SlotID.headset].Label = detailedClothing
                ? Translation.Get("clothing", "headset")
                : Translation.Get("clothing", "headwear");
            UpdatePane();
        }

        public override void Draw()
        {
            Enabled = meidoManager.HasActiveMeido;

            GUI.enabled = Enabled;
            detailedClothingToggle.Draw();
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

            GUI.enabled = Enabled;

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
