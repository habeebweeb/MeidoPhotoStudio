using MeidoPhotoStudio.Database.Props.Menu;

using CostumeType = MaidExtension.MaidCostumeChangeController.CostumeType;
using MaskMode = TBody.MaskMode;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class ClothingController(CharacterController characterController)
{
    private const float DefaultFloorHeight = -1000f;

    private readonly CharacterController characterController = characterController
        ?? throw new ArgumentNullException(nameof(characterController));

    private bool customFloorHeight;
    private float floorHeight;
    private GravityController hairGravityController;
    private GravityController clothingGravityController;

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
        }
    }

    public bool CustomFloorHeight
    {
        get => customFloorHeight;
        set
        {
            customFloorHeight = value;

            Body.BoneHitHeightY = customFloorHeight ? floorHeight : DefaultFloorHeight;
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
        }
    }

    public MenuFilePropModel AttachedLowerAccessory { get; private set; }

    public MenuFilePropModel AttachedUpperAccessory { get; private set; }

    public GravityController HairGravityController =>
        hairGravityController ??= new HairGravityController(characterController);

    public GravityController ClothingGravityController =>
        clothingGravityController ??= new ClothingGravityController(characterController);

    private Maid Maid =>
        characterController.Maid;

    private TBody Body =>
        Maid.body0;

    public bool this[SlotID slot]
    {
        get => SlotLoaded(slot) && Body.GetMask(slot);
        set => Body.SetMask(slot, value);
    }

    public bool this[Curling curling]
    {
        get =>
            SupportsCurlingType(curling) && Maid.mekureController.IsEnabledCostumeType(CurlingToCostumeType(curling));
        set =>
            Maid.mekureController.SetEnabledCostumeType(CurlingToCostumeType(curling), value);
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
}
