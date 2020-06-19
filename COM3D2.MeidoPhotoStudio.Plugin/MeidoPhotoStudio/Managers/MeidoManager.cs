using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class MeidoManager
    {
        private static CharacterMgr characterMgr = GameMain.Instance.CharacterMgr;
        private int undress = 0;
        public Meido[] meidos { get; private set; }
        public List<int> SelectMeidoList { get; private set; } = new List<int>();
        public List<Meido> ActiveMeidoList { get; private set; } = new List<Meido>();
        public Meido ActiveMeido => ActiveMeidoList.Count > 0 ? ActiveMeidoList[SelectedMeido] : null;
        public bool HasActiveMeido => ActiveMeido != null;
        public int numberOfMeidos;
        public event EventHandler<MeidoUpdateEventArgs> UpdateMeido;
        public event EventHandler EndCallMeidos;
        public event EventHandler BeginCallMeidos;
        private int selectedMeido = 0;
        public int SelectedMeido
        {
            get => selectedMeido;
            private set => selectedMeido = Mathf.Clamp(value, 0, ActiveMeidoList.Count - 1);
        }
        public bool IsBusy
        {
            get
            {
                foreach (Meido meido in ActiveMeidoList)
                {
                    if (meido.Maid.IsBusy) return true;
                }
                return false;
            }
        }

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

            foreach (Meido activeMeido in ActiveMeidoList)
            {
                activeMeido.Update();
            }
        }

        public void CallMeidos()
        {
            this.BeginCallMeidos?.Invoke(this, EventArgs.Empty);
            GameMain.Instance.MainCamera.FadeOut(0.01f, false, () =>
            {
                UnloadMeidos();

                foreach (int slot in this.SelectMeidoList)
                {
                    Meido meido = meidos[slot];
                    ActiveMeidoList.Add(meido);
                    meido.BodyLoad += OnEndCallMeidos;
                    meido.UpdateMeido += OnUpdateMeido;
                }

                for (int i = 0; i < ActiveMeidoList.Count; i++)
                {
                    Meido meido = ActiveMeidoList[i];
                    meido.Load(i, this.SelectMeidoList[i]);
                }

                SelectedMeido = 0;

                if (this.SelectMeidoList.Count == 0) OnEndCallMeidos(this, EventArgs.Empty);
            }, false);
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

        private void UnloadMeidos()
        {
            foreach (Meido meido in ActiveMeidoList)
            {
                meido.UpdateMeido -= OnUpdateMeido;
                meido.Unload();
            }
            ActiveMeidoList.Clear();
        }

        private void OnUpdateMeido(object sender, MeidoUpdateEventArgs args)
        {
            if (!args.IsEmpty) this.SelectedMeido = args.SelectedMeido;
            this.UpdateMeido?.Invoke(ActiveMeido, args);
        }

        private void OnEndCallMeidos(object sender, EventArgs args)
        {
            if (!IsBusy)
            {
                GameMain.Instance.MainCamera.FadeIn(1f);
                EndCallMeidos?.Invoke(this, EventArgs.Empty);
                foreach (Meido meido in ActiveMeidoList)
                {
                    meido.BodyLoad -= OnEndCallMeidos;
                }
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
                    (this.SelectedMeido == -1 && !this.FromMeido && !this.IsBody);
            }
        }
        public int SelectedMeido { get; }
        public bool IsBody { get; }
        public bool FromMeido { get; } = false;
        public MeidoUpdateEventArgs(int meidoIndex, bool fromMaid = false, bool isBody = true)
        {
            this.SelectedMeido = meidoIndex;
            this.IsBody = isBody;
            this.FromMeido = fromMaid;
        }
    }
}
