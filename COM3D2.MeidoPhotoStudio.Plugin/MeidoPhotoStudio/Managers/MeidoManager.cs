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
        public bool IsFade { get; set; } = false;
        public int numberOfMeidos;
        public event EventHandler<MeidoChangeEventArgs> SelectMeido;
        public event EventHandler CalledMeidos;
        public event EventHandler AnimeChange;
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
                meido.Unload();
                meido.SelectMeido -= ChangeMeido;
                meido.BodyLoad -= EndCallMeidos;
                meido.AnimeChange -= OnAnimeChangeEvent;
            }
            ActiveMeidoList.Clear();
        }

        public void DeactivateMeidos()
        {
            foreach (Meido meido in meidos)
            {
                meido.Deactivate();
                meido.SelectMeido -= ChangeMeido;
                meido.BodyLoad -= EndCallMeidos;
                meido.AnimeChange -= OnAnimeChangeEvent;
            }
            ActiveMeidoList.Clear();
        }

        public void CallMeidos(List<int> selectedMaids)
        {
            IsFade = true;

            UnloadMeidos();

            foreach (int slot in selectedMaids)
            {
                Meido meido = meidos[slot];
                ActiveMeidoList.Add(meido);
                meido.SelectMeido += ChangeMeido;
                meido.BodyLoad += EndCallMeidos;
                meido.AnimeChange += OnAnimeChangeEvent;
            }

            for (int i = 0; i < ActiveMeidoList.Count; i++)
            {
                Meido meido = ActiveMeidoList[i];
                meido.Load(i);
            }

            SelectedMeido = 0;
            OnSelectMeido(new MeidoChangeEventArgs(SelectedMeido));

            if (selectedMaids.Count == 0) EndCallMeidos(this, EventArgs.Empty);
        }

        private void OnAnimeChangeEvent(object sender, EventArgs args)
        {
            this.AnimeChange?.Invoke(this.ActiveMeido, EventArgs.Empty);
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

        private void EndCallMeidos(object sender, EventArgs args)
        {
            if (!IsBusy)
            {
                IsFade = false;
                GameMain.Instance.MainCamera.FadeIn(1f);
                CalledMeidos?.Invoke(this, EventArgs.Empty);
                foreach (Meido meido in ActiveMeidoList)
                {
                    meido.BodyLoad -= EndCallMeidos;
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
