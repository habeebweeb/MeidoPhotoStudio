using System;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;
using MeidoPhotoStudio.Plugin.Service;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

/// <summary>Meido management.</summary>
public partial class MeidoManager : IManager
{
    public const string Header = "MEIDO";

    private static bool active;

    private readonly CustomMaidSceneService customMaidSceneService;
    private readonly GeneralDragPointInputService generalDragPointInputService;
    private readonly DragPointMeidoInputService dragPointMeidoInputService;

    private int selectedMeido;
    private bool globalGravity;
    private int undress;
    private Meido editingMeido;
    private Meido temporaryEditingMeido;

    public MeidoManager(
        CustomMaidSceneService customMaidSceneService,
        GeneralDragPointInputService generalDragPointInputService,
        DragPointMeidoInputService dragPointMeidoInputService)
    {
        this.customMaidSceneService = customMaidSceneService
            ?? throw new ArgumentNullException(nameof(customMaidSceneService));

        this.generalDragPointInputService = generalDragPointInputService
            ?? throw new ArgumentNullException(nameof(generalDragPointInputService));

        this.dragPointMeidoInputService = dragPointMeidoInputService
            ?? throw new ArgumentNullException(nameof(dragPointMeidoInputService));

        if (SceneEdit.Instance)
            SceneEditStartPostfix();
    }

    public event EventHandler<MeidoUpdateEventArgs> UpdateMeido;

    public event EventHandler EndCallMeidos;

    public event EventHandler BeginCallMeidos;

    public Meido[] Meidos { get; private set; }

    public List<Meido> ActiveMeidoList { get; } = new();

    public int SelectedMeido
    {
        get => selectedMeido;
        private set => selectedMeido = Utility.Bound(value, 0, ActiveMeidoList.Count - 1);
    }

    public bool Busy =>
        ActiveMeidoList.Any(meido => meido.Busy);

    public Meido ActiveMeido =>
        ActiveMeidoList.Count > 0 ? ActiveMeidoList[SelectedMeido] : null;

    public Meido EditingMeido
    {
        get => EditMode ? editingMeido : null;
        set
        {
            if (!EditMode || value is null)
                return;

            editingMeido = value;
            temporaryEditingMeido = editingMeido == OriginalEditingMeido ? null : editingMeido;

            SetEditorMaid(editingMeido.Maid);
        }
    }

    public Meido TemporaryEditingMeido =>
        EditMode ? temporaryEditingMeido : null;

    public Meido OriginalEditingMeido =>
        EditMode && OriginalEditingMaidIndex >= 0 ? Meidos[OriginalEditingMaidIndex] : null;

    public bool HasActiveMeido =>
        ActiveMeido is not null;

    public bool GlobalGravity
    {
        get => globalGravity;
        set
        {
            globalGravity = value;

            if (!HasActiveMeido)
                return;

            var activeMeido = ActiveMeido;
            var activeMeidoSlot = activeMeido.Slot;

            foreach (var meido in ActiveMeidoList)
            {
                if (meido.Slot == activeMeidoSlot)
                    continue;

                meido.HairGravityActive = value && activeMeido.HairGravityActive;
                meido.SkirtGravityActive = value && activeMeido.SkirtGravityActive;
            }
        }
    }

    private static CharacterMgr CharacterMgr =>
        GameMain.Instance.CharacterMgr;

    private static int OriginalEditingMaidIndex { get; set; }

    private bool EditMode =>
        customMaidSceneService.EditScene;

    public void ChangeMaid(int index) =>
        OnUpdateMeido(null, new(index));

    public void Activate()
    {
        Meidos = CharacterMgr.GetStockMaidList()
            .Select(maid => new Meido(
                maid, customMaidSceneService, generalDragPointInputService, dragPointMeidoInputService))
            .ToArray();

        CharacterMgr.ResetCharaPosAll();

        if (EditMode)
        {
            temporaryEditingMeido = null;
            editingMeido = OriginalEditingMeido;

            if (OriginalEditingMeido is not null)
                CallMeidos(new List<Meido>() { OriginalEditingMeido });
        }
        else
        {
            CharacterMgr.DeactivateMaid(0);
        }

        active = true;
    }

    public void Deactivate()
    {
        foreach (var meido in Meidos)
        {
            meido.UpdateMeido -= OnUpdateMeido;
            meido.GravityMove -= OnGravityMove;
            meido.Deactivate();
        }

        ActiveMeidoList.Clear();

        if (EditMode && !GameMain.Instance.MainCamera.IsFadeOut())
        {
            var meido = OriginalEditingMeido;

            meido.Maid.Visible = true;
            meido.Stop = false;
            meido.EyeToCam = true;

            SetEditorMaid(meido.Maid);
        }

        active = false;
    }

    public void Update()
    {
    }

    public void CallMeidos(IList<Meido> meidoToCall)
    {
        BeginCallMeidos?.Invoke(this, EventArgs.Empty);

        SelectedMeido = 0;

        if (EditMode && meidoToCall.Count is 0)
            meidoToCall.Add(OriginalEditingMeido);

        UnloadMeidos(meidoToCall);

        if (meidoToCall.Count is 0)
        {
            OnEndCallMeidos(this, EventArgs.Empty);

            return;
        }

        ActiveMeidoList.AddRange(meidoToCall);

        GameMain.Instance.StartCoroutine(LoadMeidos(meidoToCall));
    }

    public Meido GetMeido(string guid) =>
        string.IsNullOrEmpty(guid) ? null : ActiveMeidoList.Find(meido => meido.Maid.status.guid == guid);

    public Meido GetMeido(int activeIndex) =>
        activeIndex >= 0 && activeIndex < ActiveMeidoList.Count ? ActiveMeidoList[activeIndex] : null;

    public void PlaceMeidos(string placementType) =>
        MaidPlacementUtility.ApplyPlacement(placementType, ActiveMeidoList);

    private static void SetEditorMaid(Maid maid)
    {
        if (!maid)
        {
            Utility.LogWarning("Refusing to change editing maid because the new maid is null!");

            return;
        }

        if (SceneEdit.Instance.maid.status.guid == maid.status.guid)
        {
            Utility.LogDebug("Editing maid is the same as new maid");

            return;
        }

        var uiRoot = GameObject.Find("UI Root");

        if (!TryGetUIControl<PresetCtrl>(uiRoot, "PresetPanel", out var presetCtrl))
            return;

        if (!TryGetUIControl<PresetButtonCtrl>(uiRoot, "PresetButtonPanel", out var presetButtonCtrl))
            return;

        if (!TryGetUIControl<ProfileCtrl>(uiRoot, "ProfilePanel", out var profileCtrl))
            return;

        if (!TryGetUIControl<SceneEditWindow.CustomPartsWindow>(
            uiRoot, "Window/CustomPartsWindow", out var sceneEditWindow))
            return;

        // Preset application
        presetCtrl.m_maid = maid;

        // Preset saving
        presetButtonCtrl.m_maid = maid;

        // Maid profile (name, description, experience etc)
        profileCtrl.m_maidStatus = maid.status;

        // Accessory/Parts placement
        sceneEditWindow.maid = maid;

        // Stopping maid animation and head movement when customizing parts placement
        sceneEditWindow.animation = maid.GetAnimation();

        // Clothing/body in general and maybe other things
        SceneEdit.Instance.m_maid = maid;

        // Body status, parts colours and maybe more
        GameMain.Instance.CharacterMgr.m_gcActiveMaid[0] = maid;

        static bool TryGetUIControl<T>(GameObject root, string hierarchy, out T uiControl)
            where T : MonoBehaviour
        {
            uiControl = null;

            var uiElement = UTY.GetChildObjectNoError(root, hierarchy);

            if (!uiElement)
                return false;

            uiControl = uiElement.GetComponent<T>();

            return uiControl;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.Start))]
    private static void SceneEditStartPostfix()
    {
        if (!SceneEdit.Instance.maid)
            return;

        var originalEditingMaid = SceneEdit.Instance.maid;

        OriginalEditingMaidIndex = GameMain.Instance.CharacterMgr.GetStockMaidList()
            .FindIndex(maid => maid.status.guid == originalEditingMaid.status.guid);

        try
        {
            var editOkCancelButton = UTY.GetChildObject(GameObject.Find("UI Root"), "OkCancel")
                .GetComponent<EditOkCancel>();

            EditOkCancel.OnClick newEditOkCancelDelegate = RestoreOriginalEditingMaid;

            newEditOkCancelDelegate += editOkCancelButton.m_dgOnClickOk;

            editOkCancelButton.m_dgOnClickOk = newEditOkCancelDelegate;

            void RestoreOriginalEditingMaid()
            {
                // Only restore original editing maid when active.
                if (!active)
                    return;

                Utility.LogDebug($"Setting Editing maid back to '{originalEditingMaid.status.fullNameJpStyle}'");

                SetEditorMaid(originalEditingMaid);

                // Set SceneEdit's maid regardless of UI integration failing
                SceneEdit.Instance.m_maid = originalEditingMaid;
            }
        }
        catch (Exception e)
        {
            Utility.LogWarning($"Failed to hook onto Edit Mode OK button: {e}");
        }
    }

    private void UnloadMeidos(IList<Meido> meidoToCall)
    {
        foreach (var meido in ActiveMeidoList)
        {
            meido.UpdateMeido -= OnUpdateMeido;
            meido.GravityMove -= OnGravityMove;

            if (!meidoToCall.Contains(meido))
                meido.Unload();
        }

        ActiveMeidoList.Clear();
    }

    private System.Collections.IEnumerator LoadMeidos(IList<Meido> meidoToLoad)
    {
        GameMain.Instance.MainCamera.FadeOut(0.01f, f_bSkipable: false);

        yield return new WaitForSeconds(0.01f);

        for (var meidoSlot = 0; meidoSlot < meidoToLoad.Count; meidoSlot++)
            meidoToLoad[meidoSlot].Load(meidoSlot);

        yield return new WaitForEndOfFrame();

        var waitForSeconds = new WaitForSeconds(0.5f);

        while (Busy)
            yield return waitForSeconds;

        OnEndCallMeidos(this, EventArgs.Empty);
    }

    private void UndressAll()
    {
        if (!HasActiveMeido)
            return;

        undress = ++undress % Enum.GetNames(typeof(Meido.Mask)).Length;

        foreach (var activeMeido in ActiveMeidoList)
            activeMeido.SetMaskMode((Meido.Mask)undress);

        UpdateMeido?.Invoke(ActiveMeido, new(SelectedMeido));
    }

    private void OnUpdateMeido(object sender, MeidoUpdateEventArgs args)
    {
        if (!args.IsEmpty)
            SelectedMeido = args.SelectedMeido;

        UpdateMeido?.Invoke(ActiveMeido, args);
    }

    private void OnEndCallMeidos(object sender, EventArgs args)
    {
        GameMain.Instance.MainCamera.FadeIn(1f);
        EndCallMeidos?.Invoke(this, EventArgs.Empty);

        foreach (var meido in ActiveMeidoList)
        {
            meido.UpdateMeido += OnUpdateMeido;
            meido.GravityMove += OnGravityMove;
        }

        if (EditMode && !ActiveMeidoList.Contains(TemporaryEditingMeido))
            EditingMeido = OriginalEditingMeido;
    }

    private void OnGravityMove(object sender, GravityEventArgs args)
    {
        if (!GlobalGravity)
            return;

        foreach (var meido in ActiveMeidoList)
            meido.ApplyGravity(args.LocalPosition, args.IsSkirt);
    }
}
