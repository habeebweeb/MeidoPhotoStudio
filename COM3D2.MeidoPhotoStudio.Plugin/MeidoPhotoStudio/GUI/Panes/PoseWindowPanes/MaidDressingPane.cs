using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static TBody;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class MaidDressingPane : BasePane
    {
        private MeidoManager meidoManager;
        private Dictionary<SlotID, Toggle> ClothingToggles;
        private Dictionary<SlotID, bool> LoadedSlots;
        public enum Curl
        {
            front, back, shift
        }
        private static readonly SlotID[] clothingSlots = {
            // main slots
            SlotID.wear, SlotID.skirt, SlotID.bra, SlotID.panz, SlotID.headset, SlotID.megane,
            SlotID.accUde, SlotID.glove, SlotID.accSenaka, SlotID.stkg, SlotID.shoes, SlotID.body,
            // detailed slots
            SlotID.accAshi, SlotID.accHana, SlotID.accHat, SlotID.accHeso, SlotID.accKamiSubL,
            SlotID.accKamiSubR, SlotID.accKami_1_, SlotID.accKami_2_, SlotID.accKami_3_, SlotID.accKubi,
            SlotID.accKubiwa, SlotID.accMiMiL, SlotID.accMiMiR, SlotID.accNipL, SlotID.accNipR,
            SlotID.accShippo, SlotID.accXXX, 
            // unused slots
            // SlotID.mizugi, SlotID.onepiece, SlotID.accHead,
        };

        public static readonly SlotID[] bodySlots = {
            SlotID.body, SlotID.head, SlotID.eye, SlotID.hairF, SlotID.hairR,
            SlotID.hairS, SlotID.hairT, SlotID.hairAho, SlotID.chikubi, SlotID.underhair,
            SlotID.moza, SlotID.accHa
        };

        private static readonly SlotID[] wearSlots = {
            SlotID.wear, SlotID.mizugi, SlotID.onepiece
        };

        private static readonly SlotID[] headwearSlots = {
            SlotID.headset, SlotID.accHat, SlotID.accKamiSubL,
            SlotID.accKamiSubR, SlotID.accKami_1_, SlotID.accKami_2_, SlotID.accKami_3_
        };

        private Toggle detailedClothingToggle;
        private Toggle curlingFrontToggle;
        private Toggle curlingBackToggle;
        private Toggle pantsuShiftToggle;
        private bool detailedClothing = false;

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
            curlingFrontToggle.ControlEvent += (s, a) => ToggleCurling(Curl.front, curlingFrontToggle.Value);
            curlingBackToggle = new Toggle(Translation.Get("clothing", "curlingBack"));
            curlingBackToggle.ControlEvent += (s, a) => ToggleCurling(Curl.back, curlingBackToggle.Value);
            pantsuShiftToggle = new Toggle(Translation.Get("clothing", "shiftPanties"));
            pantsuShiftToggle.ControlEvent += (s, a) => ToggleCurling(Curl.shift, pantsuShiftToggle.Value);

            UpdateDetailedClothing();
        }

        public void ToggleClothing(SlotID slot, bool enabled)
        {
            if (this.updating) return;

            if (slot == SlotID.body)
            {
                this.meidoManager.ActiveMeido.SetBodyMask(enabled);
                return;
            }

            TBody body = this.meidoManager.ActiveMeido.Maid.body0;

            if (!detailedClothing && slot == SlotID.headset)
            {
                this.updating = true;
                foreach (SlotID wearSlot in headwearSlots)
                {
                    body.SetMask(wearSlot, enabled);
                    ClothingToggles[wearSlot].Value = enabled;
                }
                this.updating = false;
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

        public void ToggleCurling(Curl curl, bool enabled)
        {
            if (updating) return;
            Maid maid = this.meidoManager.ActiveMeido.Maid;
            string[] name = curl == Curl.shift
                ? new[] { "panz", "mizugi" }
                : new[] { "skirt", "onepiece" };
            if (enabled)
            {
                string action = curl == Curl.shift
                    ? "パンツずらし"
                    : curl == Curl.front
                        ? "めくれスカート" : "めくれスカート後ろ";
                maid.ItemChangeTemp(name[0], action);
                maid.ItemChangeTemp(name[1], action);
                this.updating = true;
                if (curl == Curl.front && curlingBackToggle.Value)
                {
                    curlingBackToggle.Value = false;
                }
                else if (curl == Curl.back && curlingFrontToggle.Value)
                {
                    curlingFrontToggle.Value = false;
                }
                this.updating = false;
            }
            else
            {
                maid.ResetProp(name[0]);
                maid.ResetProp(name[1]);
            }
            maid.AllProcProp();
        }

        public override void Update()
        {
            this.updating = true;
            Maid maid = this.meidoManager.ActiveMeido.Maid;
            TBody body = maid.body0;
            foreach (SlotID clothingSlot in clothingSlots)
            {
                bool toggleValue;
                bool hasSlot;
                if (clothingSlot == SlotID.wear)
                {
                    toggleValue = body.GetMask(SlotID.wear) || body.GetMask(SlotID.mizugi)
                        || body.GetMask(SlotID.onepiece);
                    hasSlot = body.GetSlotLoaded(SlotID.wear) || body.GetSlotLoaded(SlotID.mizugi)
                        || body.GetMask(SlotID.onepiece);
                }
                else if (clothingSlot == SlotID.megane)
                {
                    toggleValue = body.GetMask(SlotID.megane) || body.GetMask(SlotID.accHead);
                    hasSlot = body.GetSlotLoaded(SlotID.megane) || body.GetSlotLoaded(SlotID.accHead);
                }
                else
                {
                    toggleValue = body.GetMask(clothingSlot);
                    hasSlot = body.GetSlotLoaded(clothingSlot);
                }

                ClothingToggles[clothingSlot].Value = hasSlot ? toggleValue : false;
                LoadedSlots[clothingSlot] = hasSlot;
            }

            curlingFrontToggle.Value = maid.IsItemChange("skirt", "めくれスカート")
                || maid.IsItemChange("onepiece", "めくれスカート");

            curlingBackToggle.Value = maid.IsItemChange("skirt", "めくれスカート後ろ")
                || maid.IsItemChange("onepiece", "めくれスカート後ろ");

            pantsuShiftToggle.Value = maid.IsItemChange("panz", "パンツずらし")
                || maid.IsItemChange("mizugi", "パンツずらし");

            this.updating = false;
        }

        private void DrawSlotGroup(params SlotID[] slots)
        {
            GUILayout.BeginHorizontal();
            for (int i = 0; i < slots.Length; i++)
            {
                SlotID slot = slots[i];
                if (!this.Enabled) GUI.enabled = false;
                else GUI.enabled = LoadedSlots[slot];
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
        }

        public override void Draw(params GUILayoutOption[] layoutOptions)
        {
            this.Enabled = this.meidoManager.HasActiveMeido;

            GUI.enabled = this.Enabled;
            detailedClothingToggle.Draw();
            MiscGUI.BlackLine();

            DrawSlotGroup(SlotID.wear, SlotID.skirt);
            DrawSlotGroup(SlotID.bra, SlotID.panz);
            DrawSlotGroup(SlotID.headset, SlotID.megane);
            DrawSlotGroup(SlotID.accUde, SlotID.glove, SlotID.accSenaka);
            DrawSlotGroup(SlotID.stkg, SlotID.shoes, SlotID.body);

            if (detailedClothing)
            {
                MiscGUI.BlackLine();
                DrawSlotGroup(SlotID.accShippo, SlotID.accHat);
                DrawSlotGroup(SlotID.accKami_1_, SlotID.accKami_2_, SlotID.accKami_3_);
                DrawSlotGroup(SlotID.accKamiSubL, SlotID.accKamiSubR);
                DrawSlotGroup(SlotID.accMiMiL, SlotID.accMiMiR);
                DrawSlotGroup(SlotID.accNipL, SlotID.accNipR);
                DrawSlotGroup(SlotID.accHana, SlotID.accKubi, SlotID.accKubiwa);
                DrawSlotGroup(SlotID.accHeso, SlotID.accAshi, SlotID.accXXX);
            }

            GUI.enabled = this.Enabled;

            GUILayout.BeginHorizontal();
            curlingFrontToggle.Draw();
            GUILayout.FlexibleSpace();
            curlingBackToggle.Draw();
            GUILayout.FlexibleSpace();
            pantsuShiftToggle.Draw();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
}
