using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class MeidoManager
    {
        private static CharacterMgr characterMgr = GameMain.Instance.CharacterMgr;
        private int undress = 0;
        public Meido[] meidos { get; private set; }
        public List<Meido> ActiveMeidoList { get; private set; }
        public Meido ActiveMeido => ActiveMeidoList.Count > 0 ? ActiveMeidoList[selectedMeido] : null;
        public bool HasActiveMeido => ActiveMeido != null;
        public int numberOfMeidos;
        public event EventHandler<MeidoChangeEventArgs> SelectMeido;
        public event EventHandler EndCallMeidos;
        public event EventHandler BeginCallMeidos;
        public event EventHandler AnimeChange;
        public event EventHandler FreeLookChange;
        private int selectedMeido = 0;
        public int SelectedMeido
        {
            get => selectedMeido;
            private set => selectedMeido = Mathf.Clamp(value, 0, ActiveMeidoList.Count);
        }
        public bool IsBusy
        {
            get
            {
                foreach (Meido meido in ActiveMeidoList)
                {
                    if (meido.Maid.IsBusy)
                    {
                        Debug.Log(meido.NameEN + " is busy!");
                        return true;
                    }
                }
                return false;
            }
        }

        public MeidoManager()
        {
            numberOfMeidos = characterMgr.GetStockMaidCount();
            ActiveMeidoList = new List<Meido>();
            meidos = new Meido[numberOfMeidos];

            MaidSwitcherPane.MaidChange += ChangeMeido;
            MaidSwitcherPane.meidoManager = this;

            for (int stockMaidIndex = 0; stockMaidIndex < numberOfMeidos; stockMaidIndex++)
            {
                meidos[stockMaidIndex] = new Meido(stockMaidIndex);
            }
        }

        ~MeidoManager()
        {
            MaidSwitcherPane.MaidChange -= ChangeMeido;
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.H)) UndressAll();

            foreach (Meido activeMeido in ActiveMeidoList)
            {
                activeMeido.Update();
            }
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
            OnSelectMeido(new MeidoChangeEventArgs(SelectedMeido));
        }

        public void UnloadMeidos()
        {
            foreach (Meido meido in ActiveMeidoList)
            {
                meido.SelectMeido -= ChangeMeido;
                meido.BodyLoad -= OnEndCallMeidos;
                meido.AnimeChange -= OnAnimeChangeEvent;
                meido.FreeLookChange -= OnFreeLookChangeEvent;
                meido.Unload();
            }
            ActiveMeidoList.Clear();
        }

        public void Deactivate()
        {
            foreach (Meido meido in meidos)
            {
                meido.SelectMeido -= ChangeMeido;
                meido.BodyLoad -= OnEndCallMeidos;
                meido.AnimeChange -= OnAnimeChangeEvent;
                meido.FreeLookChange -= OnFreeLookChangeEvent;
                meido.Deactivate();
            }
            ActiveMeidoList.Clear();
        }

        public void CallMeidos(List<int> selectedMaids)
        {
            UnloadMeidos();

            foreach (int slot in selectedMaids)
            {
                Meido meido = meidos[slot];
                ActiveMeidoList.Add(meido);
                meido.SelectMeido += ChangeMeido;
                meido.BodyLoad += OnEndCallMeidos;
                meido.AnimeChange += OnAnimeChangeEvent;
                meido.FreeLookChange += OnFreeLookChangeEvent;
            }

            for (int i = 0; i < ActiveMeidoList.Count; i++)
            {
                Meido meido = ActiveMeidoList[i];
                meido.Load(i, selectedMaids[i]);
            }

            SelectedMeido = 0;
            OnSelectMeido(new MeidoChangeEventArgs(SelectedMeido));

            if (selectedMaids.Count == 0) OnEndCallMeidos(this, EventArgs.Empty);
        }

        private void OnAnimeChangeEvent(object sender, EventArgs args)
        {
            this.AnimeChange?.Invoke(this.ActiveMeido, EventArgs.Empty);
        }

        private void OnFreeLookChangeEvent(object sender, EventArgs args)
        {
            this.FreeLookChange?.Invoke(this.ActiveMeido, args);
        }

        private void OnSelectMeido(MeidoChangeEventArgs args)
        {
            SelectMeido?.Invoke(this, args);
        }

        private void ChangeMeido(object sender, MeidoChangeEventArgs args)
        {
            SelectedMeido = args.selected;
            OnSelectMeido(args);
        }

        public void OnBeginCallMeidos(List<int> selectList)
        {
            this.BeginCallMeidos?.Invoke(this, EventArgs.Empty);
            GameMain.Instance.MainCamera.FadeOut(0.01f, false, () => CallMeidos(selectList), false);
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
    public class MeidoChangeEventArgs : EventArgs
    {
        public int selected;
        public bool isBody;
        public bool fromMeido = false;
        public MeidoChangeEventArgs(int selected, bool fromMaid = false, bool isBody = true)
        {
            this.selected = selected;
            this.isBody = isBody;
            this.fromMeido = fromMaid;
        }
    }
}
