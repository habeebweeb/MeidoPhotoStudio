using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Database.Props.Menu;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Service;

using CostumeType = MaidExtension.MaidCostumeChangeController.CostumeType;
using MaskMode = TBody.MaskMode;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class ClothingController : INotifyPropertyChanged
{
    private const float DefaultFloorHeight = -1000f;

    private static readonly Dictionary<SlotID, KeyedPropertyChangeEventArgs<SlotID>> ClothingChangeEventArgsCache =
        new(EnumEqualityComparer<SlotID>.Instance);

    private static readonly MPN[] AttachedAccessoryMpn = [SafeMpn.kousoku_upper, SafeMpn.kousoku_lower];

    private readonly CharacterController characterController;
    private readonly TransformWatcher transformWatcher;
    private readonly HashSet<SlotID> changedSlots = [];

    private bool customFloorHeight;
    private float floorHeight;
    private GravityController hairGravityController;
    private GravityController clothingGravityController;
    private MenuFilePropModel attachedLowerAccessory;
    private MenuFilePropModel attachedUpperAccessory;

    public ClothingController(CharacterController characterController, TransformWatcher transformWatcher)
    {
        this.characterController = characterController ?? throw new ArgumentNullException(nameof(characterController));
        this.transformWatcher = transformWatcher ? transformWatcher : throw new ArgumentNullException(nameof(transformWatcher));

        this.characterController.ProcessingCharacterProps += OnCharacterPropsProcessing;
        this.characterController.ProcessedCharacterProps += OnCharacterPropsProcessed;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public event EventHandler<KeyedPropertyChangeEventArgs<SlotID>> ClothingChanged;

    public event EventHandler<KeyedPropertyChangeEventArgs<Curling>> CurlingChanged;

    public enum Curling
    {
        Front,
        Back,
        Shift,
    }

    public bool BodyVisible
    {
        get =>
            new[]
            {
                SlotID.body, SlotID.head, SlotID.eye, SlotID.hairF, SlotID.hairR, SlotID.hairS, SlotID.hairT,
                SlotID.hairAho, SlotID.chikubi, SlotID.underhair, SlotID.moza, SlotID.accHa,
            }
            .Where(SlotLoaded)
            .All(Body.GetMask);

        set
        {
            var bodySlots = new[]
            {
                SlotID.body, SlotID.head, SlotID.eye, SlotID.hairF, SlotID.hairR, SlotID.hairS, SlotID.hairT,
                SlotID.hairAho, SlotID.chikubi, SlotID.underhair, SlotID.moza, SlotID.accHa,
            };

            foreach (var slot in bodySlots)
                Body.m_hFoceHide[slot] = value;

            Body.FixMaskFlag();
            Body.FixVisibleFlag(false);

            RaisePropertyChanged(nameof(BodyVisible));
        }
    }

    public MaskMode DressingMode
    {
        get => Body.m_eMaskMode;
        set
        {
            var bodyWasNotVisible = !BodyVisible;

            Body.SetMaskMode(value);

            if (bodyWasNotVisible)
                BodyVisible = false;

            RaisePropertyChanged(nameof(DressingMode));
        }
    }

    public bool CustomFloorHeight
    {
        get => customFloorHeight;
        set
        {
            customFloorHeight = value;

            Body.BoneHitHeightY = customFloorHeight ? floorHeight : DefaultFloorHeight;

            RaisePropertyChanged(nameof(CustomFloorHeight));
        }
    }

    public float FloorHeight
    {
        get => floorHeight;
        set
        {
            floorHeight = value;

            if (CustomFloorHeight)
                Body.BoneHitHeightY = floorHeight;

            RaisePropertyChanged(nameof(FloorHeight));
        }
    }

    public MenuFilePropModel AttachedLowerAccessory
    {
        get => attachedLowerAccessory;
        private set
        {
            attachedLowerAccessory = value;

            RaisePropertyChanged(nameof(AttachedLowerAccessory));
        }
    }

    public MenuFilePropModel AttachedUpperAccessory
    {
        get => attachedUpperAccessory;
        private set
        {
            attachedUpperAccessory = value;

            RaisePropertyChanged(nameof(AttachedUpperAccessory));
        }
    }

    public GravityController HairGravityController =>
        hairGravityController ??= new HairGravityController(characterController, transformWatcher);

    public GravityController ClothingGravityController =>
        clothingGravityController ??= new ClothingGravityController(characterController, transformWatcher);

    private Maid Maid =>
        characterController.Maid;

    private TBody Body =>
        Maid.body0;

    public bool this[SlotID slot]
    {
        get => SlotLoaded(slot) && Body.GetMask(slot);
        set
        {
            Body.SetMask(slot, value);

            RaiseClothingChanged(slot);
        }
    }

    public bool this[Curling curling]
    {
        get =>
            SupportsCurlingType(curling) && Maid.mekureController.IsEnabledCostumeType(CurlingToCostumeType(curling));
        set
        {
            Maid.mekureController.SetEnabledCostumeType(CurlingToCostumeType(curling), value);

            CurlingChanged?.Invoke(this, new(curling));
        }
    }

    public bool SupportsCurlingType(Curling curling) =>
        Maid.mekureController.IsSupportedCostumeType(CurlingToCostumeType(curling));

    public bool SlotLoaded(SlotID slot) =>
        Body.GetSlotLoaded(slot);

    public void AttachAccessory(MenuFilePropModel accessoryModel)
    {
        _ = accessoryModel ?? throw new ArgumentNullException(nameof(accessoryModel));

        if (!IsAccessory(accessoryModel.CategoryMpn))
            return;

        Maid.SetProp(accessoryModel.CategoryMpn, accessoryModel.Filename, 0, true);
        Maid.AllProcProp();

        if (accessoryModel.CategoryMpn == SafeMpn.kousoku_lower)
            AttachedLowerAccessory = accessoryModel;
        else
            AttachedUpperAccessory = accessoryModel;
    }

    public void DetachLowerAccessory() =>
        DetachAccessory(SafeMpn.kousoku_lower);

    public void DetachUpperAccessory() =>
        DetachAccessory(SafeMpn.kousoku_upper);

    public void DetachAllAccessories()
    {
        Maid.ResetProp(SafeMpn.kousoku_lower, false);
        Maid.ResetProp(SafeMpn.kousoku_upper, false);

        Maid.AllProcProp();

        AttachedLowerAccessory = null;
        AttachedUpperAccessory = null;
    }

    private static bool IsAccessory(MPN value) =>
        AttachedAccessoryMpn.Any(mpn => mpn == value);

    private void OnCharacterPropsProcessing(object sender, CharacterProcessingEventArgs e)
    {
        var mpnStart = (MPN)Enum.Parse(typeof(MPN_TYPE_RANGE), nameof(MPN_TYPE_RANGE.WEAR_START));
        var mpnEnd = (MPN)Enum.Parse(typeof(MPN_TYPE_RANGE), nameof(MPN_TYPE_RANGE.WEAR_END));

        changedSlots.Clear();

        changedSlots.UnionWith(
            from mpn in e.ChangingSlots
            where mpn >= mpnStart && mpn <= mpnEnd
            from skin in Body.goSlot
            where skin.m_ParentMPN == mpn
            select skin.SlotId);
    }

    private void OnCharacterPropsProcessed(object sender, CharacterProcessingEventArgs e)
    {
        var mpnStart = (MPN)Enum.Parse(typeof(MPN_TYPE_RANGE), nameof(MPN_TYPE_RANGE.WEAR_START));
        var mpnEnd = (MPN)Enum.Parse(typeof(MPN_TYPE_RANGE), nameof(MPN_TYPE_RANGE.WEAR_END));

        changedSlots.UnionWith(
            from mpn in e.ChangingSlots
            where mpn >= mpnStart && mpn <= mpnEnd
            from skin in Body.goSlot
            where skin.m_ParentMPN == mpn
            select skin.SlotId);

        foreach (var slot in changedSlots)
            RaiseClothingChanged(slot);
    }

    private void DetachAccessory(MPN category)
    {
        Maid.ResetProp(category, false);
        Maid.AllProcProp();

        if (category == SafeMpn.kousoku_lower)
            AttachedLowerAccessory = null;
        else
            AttachedUpperAccessory = null;
    }

    private CostumeType CurlingToCostumeType(Curling curling) =>
        curling switch
        {
            Curling.Front => CostumeType.MekureFront,
            Curling.Back => CostumeType.MekureBack,
            Curling.Shift => CostumeType.Zurasi,
            _ => throw new ArgumentOutOfRangeException(nameof(curling)),
        };

    private void RaisePropertyChanged(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

        PropertyChanged?.Invoke(this, new(name));
    }

    private void RaiseClothingChanged(SlotID slot)
    {
        if (!ClothingChangeEventArgsCache.TryGetValue(slot, out var e))
            e = ClothingChangeEventArgsCache[slot] = new(slot);

        ClothingChanged?.Invoke(this, e);
    }
}
