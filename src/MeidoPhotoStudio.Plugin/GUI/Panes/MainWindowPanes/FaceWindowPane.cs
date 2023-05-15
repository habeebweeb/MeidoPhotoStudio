using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class FaceWindowPane : BaseMainWindowPane
{
    private readonly MeidoManager meidoManager;
    private readonly MaidFaceSliderPane maidFaceSliderPane;
    private readonly MaidFaceBlendPane maidFaceBlendPane;
    private readonly MaidSwitcherPane maidSwitcherPane;
    private readonly SaveFacePane saveFacePane;
    private readonly Toggle saveFaceToggle;

    private bool saveFaceMode;

    public FaceWindowPane(MeidoManager meidoManager, MaidSwitcherPane maidSwitcherPane)
    {
        this.meidoManager = meidoManager;
        this.maidSwitcherPane = maidSwitcherPane;

        this.maidSwitcherPane.MaidPortraitClicked += MaidPortraitClickedEventHandler;

        maidFaceSliderPane = AddPane(new MaidFaceSliderPane(this.meidoManager));
        maidFaceBlendPane = AddPane(new MaidFaceBlendPane(this.meidoManager));
        saveFacePane = AddPane(new SaveFacePane(this.meidoManager));

        saveFaceToggle = new(Translation.Get("maidFaceWindow", "savePaneToggle"));
        saveFaceToggle.ControlEvent += (_, _) =>
            saveFaceMode = !saveFaceMode;
    }

    public override void Draw()
    {
        tabsPane.Draw();
        maidSwitcherPane.Draw();

        maidFaceBlendPane.Draw();

        scrollPos = GUILayout.BeginScrollView(scrollPos);

        maidFaceSliderPane.Draw();

        GUI.enabled = meidoManager.HasActiveMeido;
        saveFaceToggle.Draw();
        GUI.enabled = true;

        if (saveFaceMode)
            saveFacePane.Draw();

        GUILayout.EndScrollView();
    }

    public override void UpdatePanes()
    {
        if (!meidoManager.HasActiveMeido)
            return;

        if (!ActiveWindow)
            return;

        meidoManager.ActiveMeido.StopBlink();

        base.UpdatePanes();
    }

    protected override void ReloadTranslation() =>
        saveFaceToggle.Label = Translation.Get("maidFaceWindow", "savePaneToggle");

    private void MaidPortraitClickedEventHandler(object sender, System.EventArgs args)
    {
        if (!ActiveWindow)
            return;

        if (!meidoManager.HasActiveMeido)
            return;

        var meido = meidoManager.ActiveMeido;

        meido.FocusOnFace();
    }
}
