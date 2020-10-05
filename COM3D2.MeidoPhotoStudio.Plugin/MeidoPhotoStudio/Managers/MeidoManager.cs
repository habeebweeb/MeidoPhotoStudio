using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class MeidoManager : IManager, ISerializable
    {
        public const string header = "MEIDO";
        private static readonly CharacterMgr characterMgr = GameMain.Instance.CharacterMgr;
        private int undress;
        private int numberOfMeidos;
        private int tempEditMaidIndex = -1;
        public Meido[] Meidos { get; private set; }
        public HashSet<int> SelectedMeidoSet { get; } = new HashSet<int>();
        public List<int> SelectMeidoList { get; } = new List<int>();
        public List<Meido> ActiveMeidoList { get; } = new List<Meido>();
        public Meido ActiveMeido => ActiveMeidoList.Count > 0 ? ActiveMeidoList[SelectedMeido] : null;
        public Meido EditMeido => tempEditMaidIndex >= 0 ? Meidos[tempEditMaidIndex] : Meidos[EditMaidIndex];
        public bool HasActiveMeido => ActiveMeido != null;
        public event EventHandler<MeidoUpdateEventArgs> UpdateMeido;
        public event EventHandler EndCallMeidos;
        public event EventHandler BeginCallMeidos;
        private int selectedMeido;
        public int SelectedMeido
        {
            get => selectedMeido;
            private set => selectedMeido = Utility.Bound(value, 0, ActiveMeidoList.Count - 1);
        }
        public int EditMaidIndex { get; private set; }
        public bool Busy => ActiveMeidoList.Any(meido => meido.Busy);
        private bool globalGravity;
        public bool GlobalGravity
        {
            get => globalGravity;
            set
            {
                globalGravity = value;

                if (!HasActiveMeido) return;

                Meido activeMeido = ActiveMeido;
                int activeMeidoSlot = activeMeido.Slot;

                foreach (Meido meido in ActiveMeidoList)
                {
                    if (meido.Slot != activeMeidoSlot)
                    {
                        meido.HairGravityActive = value && activeMeido.HairGravityActive;
                        meido.SkirtGravityActive = value && activeMeido.SkirtGravityActive;
                    }
                }
            }
        }

        static MeidoManager() => InputManager.Register(MpsKey.MeidoUndressing, KeyCode.H, "All maid undressing");

        public MeidoManager() => Activate();

        public void ChangeMaid(int index) => OnUpdateMeido(null, new MeidoUpdateEventArgs(index));

        public void Activate()
        {
            GameMain.Instance.CharacterMgr.ResetCharaPosAll();
            numberOfMeidos = characterMgr.GetStockMaidCount();
            Meidos = new Meido[numberOfMeidos];

            tempEditMaidIndex = -1;

            for (int stockMaidIndex = 0; stockMaidIndex < numberOfMeidos; stockMaidIndex++)
            {
                Meidos[stockMaidIndex] = new Meido(stockMaidIndex);
            }

            if (MeidoPhotoStudio.EditMode)
            {
                Maid editMaid = GameMain.Instance.CharacterMgr.GetMaid(0);
                EditMaidIndex = Array.FindIndex(Meidos, meido => meido.Maid.status.guid == editMaid.status.guid);
                EditMeido.IsEditMaid = true;

                var editOkCancel = UTY.GetChildObject(GameObject.Find("UI Root"), "OkCancel")
                    .GetComponent<EditOkCancel>();

                // Ensure MPS resets editor state before setting maid
                EditOkCancel.OnClick newEditOnClick = () => SetEditMaid(Meidos[EditMaidIndex]);
                newEditOnClick += OkCancelDelegate();

                Utility.SetFieldValue(editOkCancel, "m_dgOnClickOk", newEditOnClick);

                // Only for setting custom parts placement animation just in case body was changed before activating MPS
                SetEditMaid(Meidos[EditMaidIndex]);
            }

            ClearSelectList();
        }

        public void Deactivate()
        {
            foreach (Meido meido in Meidos)
            {
                meido.UpdateMeido -= OnUpdateMeido;
                meido.GravityMove -= OnGravityMove;
                meido.Deactivate();
            }

            ActiveMeidoList.Clear();

            if (MeidoPhotoStudio.EditMode)
            {
                Meido meido = Meidos[EditMaidIndex];
                meido.Maid.Visible = true;
                meido.Stop = false;
                meido.EyeToCam = true;

                SetEditMaid(meido);

                // Restore original OK button functionality
                GameObject okButton = UTY.GetChildObjectNoError(GameObject.Find("UI Root"), "OkCancel");
                if (okButton)
                {
                    EditOkCancel editOkCancel = okButton.GetComponent<EditOkCancel>();
                    Utility.SetFieldValue(editOkCancel, "m_dgOnClickOk", OkCancelDelegate());
                }
            }
        }

        private EditOkCancel.OnClick OkCancelDelegate()
        {
            return (EditOkCancel.OnClick)Delegate
                .CreateDelegate(typeof(EditOkCancel.OnClick), SceneEdit.Instance, "OnEditOk");
        }

        public void Update()
        {
            if (InputManager.GetKeyDown(MpsKey.MeidoUndressing)) UndressAll();
        }

        public void Serialize(System.IO.BinaryWriter binaryWriter)
        {
            binaryWriter.Write(header);
            // Only true for MM scenes converted to MPS scenes
            binaryWriter.Write(false);
            binaryWriter.Write(Meido.meidoDataVersion);
            binaryWriter.Write(ActiveMeidoList.Count);
            foreach (Meido meido in ActiveMeidoList)
            {
                meido.Serialize(binaryWriter);
            }
            // Global hair/skirt gravity
            binaryWriter.Write(GlobalGravity);
        }

        public void Deserialize(System.IO.BinaryReader binaryReader)
        {
            bool isMMScene = binaryReader.ReadBoolean();
            int dataVersion = binaryReader.ReadInt32();
            int numberOfMaids = binaryReader.ReadInt32();
            for (int i = 0; i < numberOfMaids; i++)
            {
                if (i >= ActiveMeidoList.Count)
                {
                    long skip = binaryReader.ReadInt64(); // meido buffer length
                    binaryReader.BaseStream.Seek(skip, System.IO.SeekOrigin.Current);
                    continue;
                }
                Meido meido = ActiveMeidoList[i];
                meido.Deserialize(binaryReader, dataVersion, isMMScene);
            }
            // Global hair/skirt gravity
            GlobalGravity = binaryReader.ReadBoolean();
        }

        private void UnloadMeidos()
        {
            SelectedMeido = 0;
            foreach (Meido meido in ActiveMeidoList)
            {
                meido.UpdateMeido -= OnUpdateMeido;
                meido.GravityMove -= OnGravityMove;
                meido.Unload();
            }
            ActiveMeidoList.Clear();
        }

        public void CallMeidos()
        {
            BeginCallMeidos?.Invoke(this, EventArgs.Empty);

            bool moreThanEditMaid = ActiveMeidoList.Count > 1;

            UnloadMeidos();

            if (SelectMeidoList.Count == 0)
            {
                OnEndCallMeidos(this, EventArgs.Empty);
                return;
            }

            void callMeidos() => GameMain.Instance.StartCoroutine(LoadMeidos());

            if (MeidoPhotoStudio.EditMode && !moreThanEditMaid && SelectMeidoList.Count == 1) callMeidos();
            else GameMain.Instance.MainCamera.FadeOut(0.01f, f_bSkipable: false, f_dg: callMeidos);
        }

        private System.Collections.IEnumerator LoadMeidos()
        {
            foreach (int slot in SelectMeidoList) ActiveMeidoList.Add(Meidos[slot]);

            for (int i = 0; i < ActiveMeidoList.Count; i++) ActiveMeidoList[i].Load(i);

            while (Busy) yield return null;

            yield return new WaitForEndOfFrame();

            OnEndCallMeidos(this, EventArgs.Empty);
        }

        public void SelectMeido(int index)
        {
            if (SelectedMeidoSet.Contains(index))
            {
                if (!MeidoPhotoStudio.EditMode || index != EditMaidIndex)
                {
                    SelectedMeidoSet.Remove(index);
                    SelectMeidoList.Remove(index);
                }
            }
            else
            {
                SelectedMeidoSet.Add(index);
                SelectMeidoList.Add(index);
            }
        }

        public void ClearSelectList()
        {
            SelectedMeidoSet.Clear();
            SelectMeidoList.Clear();
            if (MeidoPhotoStudio.EditMode)
            {
                SelectedMeidoSet.Add(EditMaidIndex);
                SelectMeidoList.Add(EditMaidIndex);
            }
        }

        public void SetEditMaid(Meido meido)
        {
            if (!MeidoPhotoStudio.EditMode) return;

            EditMeido.IsEditMaid = false;

            tempEditMaidIndex = meido.Maid.status.guid == Meidos[EditMaidIndex].Maid.status.guid
                ? -1
                : Array.FindIndex(Meidos, maid => maid.Maid.status.guid == meido.Maid.status.guid);

            EditMeido.IsEditMaid = true;

            Maid newEditMaid = EditMeido.Maid;

            GameObject uiRoot = GameObject.Find("UI Root");

            var presetCtrl = UTY.GetChildObjectNoError(uiRoot, "PresetPanel")?.GetComponent<PresetCtrl>();
            var presetButton = UTY.GetChildObjectNoError(uiRoot, "PresetButtonPanel")?.GetComponent<PresetButtonCtrl>();
            var profileCtrl = UTY.GetChildObjectNoError(uiRoot, "ProfilePanel")?.GetComponent<ProfileCtrl>();
            var customPartsWindow = UTY.GetChildObjectNoError(uiRoot, "Window/CustomPartsWindow")
                ?.GetComponent<SceneEditWindow.CustomPartsWindow>();

            if (!(presetCtrl || presetButton || profileCtrl || customPartsWindow)) return;

            // Preset application
            Utility.SetFieldValue(presetCtrl, "m_maid", newEditMaid);

            // Preset saving
            Utility.SetFieldValue(presetButton, "m_maid", newEditMaid);

            // Maid profile (name, description, experience etc)
            Utility.SetFieldValue(profileCtrl, "m_maidStatus", newEditMaid.status);

            // Accessory/Parts placement
            Utility.SetFieldValue(customPartsWindow, "maid", newEditMaid);

            // Stopping maid animation and head movement when customizing parts placement
            Utility.SetFieldValue(customPartsWindow, "animation", newEditMaid.GetAnimation());

            // Clothing/body in general and maybe other things
            Utility.SetFieldValue(SceneEdit.Instance, "m_maid", newEditMaid);

            // Body status, parts colours and maybe more
            Utility.GetFieldValue<CharacterMgr, Maid[]>(
                GameMain.Instance.CharacterMgr, "m_gcActiveMaid"
            )[0] = newEditMaid;
        }

        public Meido GetMeido(string guid)
        {
            return string.IsNullOrEmpty(guid) ? null : ActiveMeidoList.Find(meido => meido.Maid.status.guid == guid);
        }

        public Meido GetMeido(int activeIndex)
        {
            return activeIndex >= 0 && activeIndex < ActiveMeidoList.Count ? ActiveMeidoList[activeIndex] : null;
        }

        public void PlaceMeidos(string placementType)
        {
            MaidPlacementUtility.ApplyPlacement(placementType, ActiveMeidoList);
        }

        private void UndressAll()
        {
            if (!HasActiveMeido) return;
            undress = Utility.Wrap(undress + 1, 0, 3);
            TBody.MaskMode maskMode = TBody.MaskMode.None;
            switch (undress)
            {
                case 0: maskMode = TBody.MaskMode.None; break;
                case 1: maskMode = TBody.MaskMode.Underwear; break;
                case 2: maskMode = TBody.MaskMode.Nude; break;
            }

            foreach (Meido activeMeido in ActiveMeidoList)
            {
                activeMeido.SetMaskMode(maskMode);
            }

            UpdateMeido?.Invoke(ActiveMeido, new MeidoUpdateEventArgs(SelectedMeido));
        }

        private void OnUpdateMeido(object sender, MeidoUpdateEventArgs args)
        {
            if (!args.IsEmpty) SelectedMeido = args.SelectedMeido;
            UpdateMeido?.Invoke(ActiveMeido, args);
        }

        private void OnEndCallMeidos(object sender, EventArgs args)
        {
            GameMain.Instance.MainCamera.FadeIn(1f);
            EndCallMeidos?.Invoke(this, EventArgs.Empty);
            foreach (Meido meido in ActiveMeidoList)
            {
                meido.UpdateMeido += OnUpdateMeido;
                meido.GravityMove += OnGravityMove;
            }

            if (MeidoPhotoStudio.EditMode && tempEditMaidIndex >= 0 && !SelectedMeidoSet.Contains(tempEditMaidIndex))
            {
                SetEditMaid(Meidos[EditMaidIndex]);
            }
        }

        private void OnGravityMove(object sender, GravityEventArgs args)
        {
            if (!GlobalGravity) return;

            foreach (Meido meido in ActiveMeidoList)
            {
                meido.ApplyGravity(args.LocalPosition, args.IsSkirt);
            }
        }
    }

    public class MeidoUpdateEventArgs : EventArgs
    {
        public static new MeidoUpdateEventArgs Empty { get; } = new MeidoUpdateEventArgs(-1);
        public bool IsEmpty => (this == Empty) || (SelectedMeido == -1 && !FromMeido && IsBody);
        public int SelectedMeido { get; }
        public bool IsBody { get; }
        public bool FromMeido { get; }
        public MeidoUpdateEventArgs(int meidoIndex = -1, bool fromMaid = false, bool isBody = true)
        {
            SelectedMeido = meidoIndex;
            IsBody = isBody;
            FromMeido = fromMaid;
        }
    }
}
