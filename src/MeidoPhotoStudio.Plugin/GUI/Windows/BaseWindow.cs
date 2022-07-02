using System.Collections.Generic;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin
{
    public abstract class BaseWindow : BasePane
    {
        private static int id = 765;
        private static int ID => id++;
        public readonly int windowID = ID;
        protected readonly List<BasePane> Panes = new();
        protected Vector2 scrollPos;
        public bool ActiveWindow { get; set; }
        protected Rect windowRect = new(0f, 0f, 480f, 270f);
        public virtual Rect WindowRect
        {
            get => windowRect;
            set
            {
                value.x = Mathf.Clamp(
                    value.x, -value.width + Utility.GetPix(20), Screen.width - Utility.GetPix(20)
                );

                value.y = Mathf.Clamp(
                    value.y, -value.height + Utility.GetPix(20), Screen.height - Utility.GetPix(20)
                );

                windowRect = value;
            }
        }
        protected Vector2 MiddlePosition => new(
            (float)Screen.width / 2 - windowRect.width / 2, (float)Screen.height / 2 - windowRect.height / 2
        );

        protected T AddPane<T>(T pane) where T : BasePane
        {
            Panes.Add(pane);
            pane.SetParent(this);
            return pane;
        }

        public override void SetParent(BaseWindow window)
        {
            foreach (var pane in Panes) 
                pane.SetParent(window);
        }

        private void HandleZoom()
        {
            if (Input.mouseScrollDelta.y == 0f || !Visible) return;

            var mousePos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);

            if (WindowRect.Contains(mousePos))
                Input.ResetInputAxes();
        }

        public virtual void Update() =>
            HandleZoom();

        public virtual void GUIFunc(int id)
        {
            Draw();
            GUI.DragWindow();
        }

        public virtual void UpdatePanes()
        {
            foreach (var pane in Panes)
                pane.UpdatePane();
        }

        public override void Activate()
        {
            base.Activate();

            foreach (var pane in Panes)
                pane.Activate();
        }

        public override void Deactivate()
        {
            base.Deactivate();

            foreach (var pane in Panes)
                pane.Deactivate();
        }
    }
}
