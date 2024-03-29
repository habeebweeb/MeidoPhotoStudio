using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class PoseWindowPane : BaseMainWindowPane
{
    private readonly MeidoManager meidoManager;
    private readonly MaidPoseSelectorPane maidPosePane;
    private readonly SavePosePane savePosePane;
    private readonly MaidSwitcherPane maidSwitcherPane;
    private readonly MaidFreeLookPane maidFaceLookPane;
    private readonly MpnAttachPropPane mpnAttachPropPane;
    private readonly MaidDressingPane maidDressingPane;
    private readonly GravityControlPane gravityControlPane;
    private readonly CopyPosePane copyPosePane;
    private readonly HandPresetPane handPresetPane;
    private readonly SaveHandPane saveHandPane;
    private readonly MaidIKPane maidIKPane;
    private readonly Toggle freeLookToggle;
    private readonly Toggle savePoseToggle;
    private readonly Toggle saveHandToggle;
    private readonly Button flipButton;

    private bool savePoseMode;
    private bool saveHandMode;
    private string handPresetHeader;
    private string flipIKHeader;

    public PoseWindowPane(MeidoManager meidoManager, MaidSwitcherPane maidSwitcherPane)
    {
        this.meidoManager = meidoManager;
        this.maidSwitcherPane = maidSwitcherPane;

        this.maidSwitcherPane.MaidPortraitClicked += MaidPortraitClickedEventHandler;

        maidPosePane = AddPane(new MaidPoseSelectorPane(meidoManager));
        savePosePane = AddPane(new SavePosePane(meidoManager));

        maidFaceLookPane = AddPane(new MaidFreeLookPane(meidoManager));
        maidFaceLookPane.Enabled = false;

        freeLookToggle = new(Translation.Get("freeLookPane", "freeLookToggle"), false);
        freeLookToggle.ControlEvent += (_, _) =>
            SetMaidFreeLook();

        savePoseToggle = new(Translation.Get("posePane", "saveToggle"));
        savePoseToggle.ControlEvent += (_, _) =>
            savePoseMode = !savePoseMode;

        // TODO: Why are only some panes added to the pane list?
        mpnAttachPropPane = new(this.meidoManager);

        maidDressingPane = AddPane(new MaidDressingPane(this.meidoManager));
        maidIKPane = AddPane(new MaidIKPane(this.meidoManager));
        gravityControlPane = AddPane(new GravityControlPane(this.meidoManager));
        copyPosePane = AddPane(new CopyPosePane(this.meidoManager));

        saveHandToggle = new(Translation.Get("handPane", "saveToggle"));
        saveHandToggle.ControlEvent += (_, _) =>
            saveHandMode = !saveHandMode;

        handPresetPane = AddPane(new HandPresetPane(meidoManager));
        saveHandPane = AddPane(new SaveHandPane(meidoManager));

        flipButton = new(Translation.Get("flipIK", "flipButton"));
        flipButton.ControlEvent += (_, _) =>
            this.meidoManager.ActiveMeido.IKManager.Flip();

        handPresetHeader = Translation.Get("handPane", "header");
        flipIKHeader = Translation.Get("flipIK", "header");
    }

    public override void Draw()
    {
        tabsPane.Draw();

        maidSwitcherPane.Draw();
        maidPosePane.Draw();

        maidIKPane.Draw();

        MpsGui.WhiteLine();

        scrollPos = GUILayout.BeginScrollView(scrollPos);

        GUI.enabled = meidoManager.HasActiveMeido;
        GUILayout.BeginHorizontal();
        freeLookToggle.Draw();
        savePoseToggle.Draw();
        GUILayout.EndHorizontal();
        GUI.enabled = true;

        if (savePoseMode)
            savePosePane.Draw();
        else
            maidFaceLookPane.Draw();

        mpnAttachPropPane.Draw();

        maidDressingPane.Draw();

        GUI.enabled = meidoManager.HasActiveMeido;
        MpsGui.Header(handPresetHeader);
        MpsGui.WhiteLine();
        saveHandToggle.Draw();
        GUI.enabled = true;

        if (saveHandMode)
            saveHandPane.Draw();
        else
            handPresetPane.Draw();

        gravityControlPane.Draw();

        copyPosePane.Draw();

        GUILayout.BeginHorizontal();
        GUI.enabled = meidoManager.HasActiveMeido;
        GUILayout.Label(flipIKHeader, GUILayout.ExpandWidth(false));
        flipButton.Draw(GUILayout.ExpandWidth(false));
        GUI.enabled = true;
        GUILayout.EndHorizontal();

        GUILayout.EndScrollView();
    }

    public override void UpdatePanes()
    {
        if (meidoManager.ActiveMeido is null)
            return;

        if (ActiveWindow)
        {
            updating = true;
            freeLookToggle.Value = meidoManager.ActiveMeido?.FreeLook ?? false;
            updating = false;

            base.UpdatePanes();
        }
    }

    protected override void ReloadTranslation()
    {
        freeLookToggle.Label = Translation.Get("freeLookPane", "freeLookToggle");
        savePoseToggle.Label = Translation.Get("posePane", "saveToggle");
        saveHandToggle.Label = Translation.Get("handPane", "saveToggle");
        flipButton.Label = Translation.Get("flipIK", "flipButton");
        handPresetHeader = Translation.Get("handPane", "header");
        flipIKHeader = Translation.Get("flipIK", "header");
    }

    private void SetMaidFreeLook()
    {
        if (updating)
            return;

        meidoManager.ActiveMeido.FreeLook = freeLookToggle.Value;
    }

    private void MaidPortraitClickedEventHandler(object sender, System.EventArgs args)
    {
        if (!ActiveWindow)
            return;

        if (!meidoManager.HasActiveMeido)
            return;

        var meido = meidoManager.ActiveMeido;

        meido.FocusOnBody();
    }
}
