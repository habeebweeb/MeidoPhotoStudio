using System;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class MeidoManager : IManager
{
    public const string Header = "MEIDO";

    private static readonly CharacterMgr CharacterMgr = GameMain.Instance.CharacterMgr;

    private static bool active;

    private int selectedMeido;
    private bool globalGravity;
    private int undress;
    private int tempEditMaidIndex = -1;

    static MeidoManager() =>
        InputManager.Register(MpsKey.MeidoUndressing, KeyCode.H, "All maid undressing");


    public MeidoManager()
    {
        if (SceneEdit.Instance)
            SceneEditStartPostfix();

        Activate();
    }

    public event EventHandler<MeidoUpdateEventArgs> UpdateMeido;

    public event EventHandler EndCallMeidos;

    public event EventHandler BeginCallMeidos;

    public Meido[] Meidos { get; private set; }

    public HashSet<int> SelectedMeidoSet { get; } = new();

    public List<int> SelectMeidoList { get; } = new();

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

    public Meido EditMeido =>
        tempEditMaidIndex >= 0 ? Meidos[tempEditMaidIndex] : Meidos[EditMaidIndex];

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

    private static int EditMaidIndex { get; set; }

    public void ChangeMaid(int index) =>
        OnUpdateMeido(null, new(index));

    public void Activate()
    {
        CharacterMgr.ResetCharaPosAll();

        if (!MeidoPhotoStudio.EditMode)
            CharacterMgr.DeactivateMaid(0);

        Meidos = CharacterMgr.GetStockMaidList()
            .Select((_, stockNo) => new Meido(stockNo))
            .ToArray();

        tempEditMaidIndex = -1;

        if (MeidoPhotoStudio.EditMode && EditMaidIndex >= 0)
            Meidos[EditMaidIndex].IsEditMaid = true;

        ClearSelectList();
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

        if (MeidoPhotoStudio.EditMode && !GameMain.Instance.MainCamera.IsFadeOut())
        {
            var meido = Meidos[EditMaidIndex];

            meido.Maid.Visible = true;
            meido.Stop = false;
            meido.EyeToCam = true;

            SetEditorMaid(meido.Maid);
        }

        active = false;
    }

    public void Update()
    {
        if (InputManager.GetKeyDown(MpsKey.MeidoUndressing))
            UndressAll();
    }

    public void CallMeidos()
    {
        BeginCallMeidos?.Invoke(this, EventArgs.Empty);

        var moreThanEditMaid = ActiveMeidoList.Count > 1;

        UnloadMeidos();

        if (SelectMeidoList.Count is 0)
        {
            OnEndCallMeidos(this, EventArgs.Empty);

            return;
        }

        void LoadMeido() =>
            GameMain.Instance.StartCoroutine(LoadMeidos());

        if (MeidoPhotoStudio.EditMode && !moreThanEditMaid && SelectMeidoList.Count is 1)
            LoadMeido();
        else
            GameMain.Instance.MainCamera.FadeOut(0.01f, f_bSkipable: false, f_dg: LoadMeido);
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
        if (!MeidoPhotoStudio.EditMode)
            return;

        EditMeido.IsEditMaid = false;

        tempEditMaidIndex = meido.StockNo == EditMaidIndex ? -1 : meido.StockNo;

        EditMeido.IsEditMaid = true;

        SetEditorMaid(EditMeido.Maid);
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
        EditMaidIndex = -1;

        if (!SceneEdit.Instance.maid)
            return;

        var originalEditingMaid = SceneEdit.Instance.maid;

        EditMaidIndex = GameMain.Instance.CharacterMgr.GetStockMaidList()
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

    private void UnloadMeidos()
    {
        SelectedMeido = 0;

        var commonMeidoIDs = new HashSet<int>(
            ActiveMeidoList.Where(meido => SelectedMeidoSet.Contains(meido.StockNo)).Select(meido => meido.StockNo));

        foreach (var meido in ActiveMeidoList)
        {
            meido.UpdateMeido -= OnUpdateMeido;
            meido.GravityMove -= OnGravityMove;

            if (!commonMeidoIDs.Contains(meido.StockNo))
                meido.Unload();
        }

        ActiveMeidoList.Clear();
    }

    private System.Collections.IEnumerator LoadMeidos()
    {
        foreach (var slot in SelectMeidoList)
            ActiveMeidoList.Add(Meidos[slot]);

        for (var i = 0; i < ActiveMeidoList.Count; i++)
            ActiveMeidoList[i].Load(i);

        while (Busy)
            yield return null;

        yield return new WaitForEndOfFrame();

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

        if (MeidoPhotoStudio.EditMode && tempEditMaidIndex >= 0 && !SelectedMeidoSet.Contains(tempEditMaidIndex))
            SetEditMaid(Meidos[EditMaidIndex]);
    }

    private void OnGravityMove(object sender, GravityEventArgs args)
    {
        if (!GlobalGravity)
            return;

        foreach (var meido in ActiveMeidoList)
            meido.ApplyGravity(args.LocalPosition, args.IsSkirt);
    }
}
