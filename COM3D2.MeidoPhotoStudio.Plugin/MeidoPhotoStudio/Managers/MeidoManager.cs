using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class MeidoManager : IManager, ISerializable
    {
        public const string header = "MEIDO";
        private static CharacterMgr characterMgr = GameMain.Instance.CharacterMgr;
        private int undress = 0;
        private int numberOfMeidos;
        public Meido[] meidos { get; private set; }
        public List<int> SelectMeidoList { get; private set; } = new List<int>();
        public List<Meido> ActiveMeidoList { get; private set; } = new List<Meido>();
        public Meido ActiveMeido => ActiveMeidoList.Count > 0 ? ActiveMeidoList[SelectedMeido] : null;
        public bool HasActiveMeido => ActiveMeido != null;
        public event EventHandler<MeidoUpdateEventArgs> UpdateMeido;
        public event EventHandler EndCallMeidos;
        public event EventHandler BeginCallMeidos;
        private int selectedMeido = 0;
        public int SelectedMeido
        {
            get => selectedMeido;
            private set => selectedMeido = Utility.Bound(value, 0, ActiveMeidoList.Count - 1);
        }
        public bool Busy => ActiveMeidoList.Any(meido => meido.Busy);

        public void ChangeMaid(int index)
        {
            OnUpdateMeido(null, new MeidoUpdateEventArgs(index));
        }

        public void Activate()
        {
            GameMain.Instance.CharacterMgr.ResetCharaPosAll();
            numberOfMeidos = characterMgr.GetStockMaidCount();
            meidos = new Meido[numberOfMeidos];

            for (int stockMaidIndex = 0; stockMaidIndex < numberOfMeidos; stockMaidIndex++)
            {
                meidos[stockMaidIndex] = new Meido(stockMaidIndex);
            }
        }

        public void Deactivate()
        {
            foreach (Meido meido in meidos)
            {
                meido.UpdateMeido -= OnUpdateMeido;
                meido.Deactivate();
            }
            SelectMeidoList.Clear();
            ActiveMeidoList.Clear();
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.H)) UndressAll();
        }

        public void Serialize(System.IO.BinaryWriter binaryWriter)
        {
            binaryWriter.Write(header);
            // Only true for MM scenes converted to MPS scenes
            binaryWriter.Write(false);
            binaryWriter.Write(ActiveMeidoList.Count);
            foreach (Meido meido in ActiveMeidoList)
            {
                meido.Serialize(binaryWriter);
            }
        }

        public void Deserialize(System.IO.BinaryReader binaryReader)
        {
            bool isMMScene = binaryReader.ReadBoolean();
            int numberOfMaids = binaryReader.ReadInt32();
            for (int i = 0; i < numberOfMaids; i++)
            {
                if (i >= ActiveMeidoList.Count)
                {
                    Int64 skip = binaryReader.ReadInt64(); // meido buffer length
                    binaryReader.BaseStream.Seek(skip, System.IO.SeekOrigin.Current);
                    continue;
                }
                Meido meido = ActiveMeidoList[i];
                meido.Deserialize(binaryReader, isMMScene);
            }
        }

        private void UnloadMeidos()
        {
            SelectedMeido = 0;
            foreach (Meido meido in ActiveMeidoList)
            {
                meido.UpdateMeido -= OnUpdateMeido;
                meido.Unload();
            }
            ActiveMeidoList.Clear();
        }

        public void CallMeidos()
        {
            this.BeginCallMeidos?.Invoke(this, EventArgs.Empty);

            bool hadActiveMeidos = HasActiveMeido;

            UnloadMeidos();

            if (SelectMeidoList.Count == 0)
            {
                OnEndCallMeidos(this, EventArgs.Empty);
                return;
            }

            GameMain.Instance.MainCamera.FadeOut(0.01f, f_bSkipable: false, f_dg: () =>
            {
                GameMain.Instance.StartCoroutine(LoadMeidos());
            });
        }

        private System.Collections.IEnumerator LoadMeidos()
        {
            foreach (int slot in this.SelectMeidoList)
            {
                Meido meido = meidos[slot];
                ActiveMeidoList.Add(meido);
                meido.BeginLoad();
            }

            for (int i = 0; i < ActiveMeidoList.Count; i++)
            {
                ActiveMeidoList[i].Load(i);
            }

            while (Busy) yield return null;

            yield return new WaitForEndOfFrame();

            OnEndCallMeidos(this, EventArgs.Empty);
        }

        public Meido GetMeido(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return null;
            else return ActiveMeidoList.FirstOrDefault(meido => meido.Maid.status.guid == guid);
        }

        public Meido GetMeido(int activeIndex)
        {
            if (activeIndex >= 0 && activeIndex < ActiveMeidoList.Count) return ActiveMeidoList[activeIndex];
            else return null;
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

            this.UpdateMeido?.Invoke(ActiveMeido, new MeidoUpdateEventArgs(SelectedMeido));
        }

        private void OnUpdateMeido(object sender, MeidoUpdateEventArgs args)
        {
            if (!args.IsEmpty) this.SelectedMeido = args.SelectedMeido;
            this.UpdateMeido?.Invoke(ActiveMeido, args);
        }

        private void OnEndCallMeidos(object sender, EventArgs args)
        {
            GameMain.Instance.MainCamera.FadeIn(1f);
            EndCallMeidos?.Invoke(this, EventArgs.Empty);
            foreach (Meido meido in ActiveMeidoList)
            {
                meido.UpdateMeido += OnUpdateMeido;
            }
        }
    }

    public class MeidoUpdateEventArgs : EventArgs
    {
        public static new MeidoUpdateEventArgs Empty { get; } = new MeidoUpdateEventArgs(-1);
        public bool IsEmpty
        {
            get
            {
                return (this == MeidoUpdateEventArgs.Empty) ||
                    (this.SelectedMeido == -1 && !this.FromMeido && this.IsBody);
            }
        }
        public int SelectedMeido { get; }
        public bool IsBody { get; }
        public bool FromMeido { get; }
        public MeidoUpdateEventArgs(int meidoIndex = -1, bool fromMaid = false, bool isBody = true)
        {
            this.SelectedMeido = meidoIndex;
            this.IsBody = isBody;
            this.FromMeido = fromMaid;
        }
    }
}
