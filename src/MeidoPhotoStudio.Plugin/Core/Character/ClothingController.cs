using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Database.Props.Menu;
using MeidoPhotoStudio.Plugin.Framework;

using CostumeType = MaidExtension.MaidCostumeChangeController.CostumeType;
using MaskMode = TBody.MaskMode;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class ClothingController(CharacterController characterController, TransformWatcher transformWatcher)
    : INotifyPropertyChanged
{
    private const float DefaultFloorHeight = -1000f;

    private readonly CharacterController characterController = characterController
        ?? throw new ArgumentNullException(nameof(characterController));

    private readonly TransformWatcher transformWatcher = transformWatcher
        ? transformWatcher : throw new ArgumentNullException(nameof(transformWatcher));

    private readonly Dictionary<SlotID, KeyedPropertyChangeEventArgs<SlotID>> clothingChangeEventArgsCache =
        new(EnumEqualityComparer<SlotID>.Instance);

    private bool customFloorHeight;
    private float floorHeight;
    private GravityController hairGravityController;
    private GravityController clothingGravityController;
    private MenuFilePropModel attachedLowerAccessory;
    private MenuFilePropModel attachedUpperAccessory;

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

        if (accessoryModel.CategoryMpn is not (MPN.kousoku_lower or MPN.kousoku_upper))
            return;

        Maid.SetProp(accessoryModel.CategoryMpn, accessoryModel.Filename, 0, true);
        Maid.AllProcProp();

        if (accessoryModel.CategoryMpn is MPN.kousoku_lower)
            AttachedLowerAccessory = accessoryModel;
        else
            AttachedUpperAccessory = accessoryModel;
    }

    public void DetachAccessory(MPN category)
    {
        if (category is not (MPN.kousoku_lower or MPN.kousoku_upper))
            throw new ArgumentException($"'{nameof(category)}' is not a valid accessory category");

        Maid.ResetProp(category, false);
        Maid.AllProcProp();

        if (category is MPN.kousoku_lower)
            AttachedLowerAccessory = null;
        else
            AttachedUpperAccessory = null;
    }

    public void DetachAllAccessories()
    {
        Maid.ResetProp(MPN.kousoku_upper, false);
        Maid.ResetProp(MPN.kousoku_lower, false);

        Maid.AllProcProp();

        AttachedLowerAccessory = null;
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
        if (!clothingChangeEventArgsCache.TryGetValue(slot, out var e))
            e = clothingChangeEventArgsCache[slot] = new(slot);

        ClothingChanged?.Invoke(this, e);
    }
}
