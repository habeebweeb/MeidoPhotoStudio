using System;
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
        public Meido ActiveMeido { get; private set; }
        public bool HasActiveMeido { get => ActiveMeido != null; }
        public bool IsFade { get; set; } = false;
        public int numberOfMeidos;
        public event EventHandler<MeidoChangeEventArgs> SelectMeido;
        public event EventHandler CalledMeidos;
        private int selectedMeido = 0;
        public int SelectedMeido
        {
            get => selectedMeido;
            set
            {
                int max = Math.Max(ActiveMeidoList.Count, 0);
                selectedMeido = Mathf.Clamp(value, 0, max);
                ActiveMeido = ActiveMeidoList.Count > 0 ? ActiveMeidoList[selectedMeido] : null;
            }
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

            ActiveMeido = null;

            for (int stockMaidIndex = 0; stockMaidIndex < numberOfMeidos; stockMaidIndex++)
            {
                meidos[stockMaidIndex] = new Meido(stockMaidIndex);
                meidos[stockMaidIndex].SelectMeido += ChangeMeido;
                meidos[stockMaidIndex].BodyLoad += EndCallMeidos;
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
                activeMeido.Maid.body0.SetMaskMode(maskMode);
            }
        }

        public void UnloadMeidos()
        {
            foreach (Meido meido in ActiveMeidoList)
            {
                meido.Unload();
            }
            ActiveMeidoList.Clear();
        }

        public void DeactivateMeidos()
        {
            foreach (Meido meido in meidos)
            {
                meido.Deactivate();
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
            }

            for (int i = 0; i < ActiveMeidoList.Count; i++)
            {
                Meido meido = ActiveMeidoList[i];
                meido.Load(i);
            }

            if (selectedMaids.Count == 0)
            {
                EndCallMeidos(this, EventArgs.Empty);
                return;
            }

            SelectedMeido = 0;
        }

        public void SetMeidoPose(string pose, int meidoIndex = -1)
        {
            Meido meido = meidoIndex == -1 ? ActiveMeido : ActiveMeidoList[meidoIndex];
            meido.SetPose(pose);
        }

        private void ChangeMeido(object sender, MeidoChangeEventArgs args)
        {
            SelectedMeido = args.selected;
            // if (args.fromMeido)
            // {
            EventHandler<MeidoChangeEventArgs> handler = SelectMeido;
            if (handler != null) handler(this, args);
            // }
        }

        private void EndCallMeidos(object sender, EventArgs args)
        {
            if (!IsBusy)
            {
                IsFade = false;
                GameMain.Instance.MainCamera.FadeIn(1f);
                EventHandler handler = CalledMeidos;
                if (handler != null)
                    handler(this, EventArgs.Empty);
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
