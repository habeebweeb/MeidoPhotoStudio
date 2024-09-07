using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Character.Pose;
using MeidoPhotoStudio.Plugin.Core.Database.Character;
using MeidoPhotoStudio.Plugin.Core.Database.Props.Menu;
using MeidoPhotoStudio.Plugin.Core.Schema;
using MeidoPhotoStudio.Plugin.Core.Schema.Character;
using MeidoPhotoStudio.Plugin.Core.Schema.Props;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class CharacterSchemaBuilder(
    FacialExpressionBuilder facialExpressionBuilder,
    ISchemaBuilder<IAnimationModelSchema, IAnimationModel> animationModelSchemaBuilder,
    ISchemaBuilder<IBlendSetModelSchema, IBlendSetModel> blendSetSchemaBuilder,
    ISchemaBuilder<MenuFilePropModelSchema, MenuFilePropModel> menuFilePropModelSchemaBuilder,
    ISchemaBuilder<TransformSchema, Transform> transformSchemaBuilder)
    : ISchemaBuilder<CharacterSchema, CharacterController>
{
    private readonly FacialExpressionBuilder facialExpressionBuilder = facialExpressionBuilder
        ?? throw new ArgumentNullException(nameof(facialExpressionBuilder));

    private readonly ISchemaBuilder<IAnimationModelSchema, IAnimationModel> animationSchemaBuilder =
        animationModelSchemaBuilder ?? throw new ArgumentNullException(nameof(animationModelSchemaBuilder));

    private readonly ISchemaBuilder<IBlendSetModelSchema, IBlendSetModel> blendSetSchemaBuilder =
        blendSetSchemaBuilder ?? throw new ArgumentNullException(nameof(blendSetSchemaBuilder));

    private readonly ISchemaBuilder<MenuFilePropModelSchema, MenuFilePropModel> propModelSchemaBuilder =
        menuFilePropModelSchemaBuilder ?? throw new ArgumentNullException(nameof(menuFilePropModelSchemaBuilder));

    private readonly ISchemaBuilder<TransformSchema, Transform> transformSchemaBuilder = transformSchemaBuilder
        ?? throw new ArgumentNullException(nameof(transformSchemaBuilder));

    public CharacterSchema Build(CharacterController character)
    {
        return new()
        {
            ID = character.ID,
            Slot = character.Slot,
            Transform = transformSchemaBuilder.Build(character.Transform),
            Head = MakeHeadSchema(character.Head),
            Face = MakeFaceSchema(character.Face),
            Pose = MakePoseSchema(character.IK),
            Clothing = MakeClothingSchema(character.Clothing),
        };

        HeadSchema MakeHeadSchema(HeadController head) =>
            new()
            {
                MMConverted = false,
                LeftEyeRotationDelta = head.LeftEyeRotationDelta,
                RightEyeRotationDelta = head.RightEyeRotationDelta,
                FreeLook = head.FreeLook,
                OffsetLookTarget = head.OffsetLookTarget,
                HeadLookRotation = head.HeadRotation,
                HeadToCamera = head.HeadToCamera,
                EyeToCamera = head.EyeToCamera,
            };

        FaceSchema MakeFaceSchema(FaceController face) =>
            new()
            {
                Blink = face.Blink,
                BlendSet = blendSetSchemaBuilder.Build(face.BlendSet),
                FacialExpressionSet = facialExpressionBuilder.Build(face).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            };

        ClothingSchema MakeClothingSchema(ClothingController clothing) =>
            new()
            {
                MMConverted = false,
                BodyVisible = clothing.BodyVisible,
                ClothingSet = new[]
                {
                    SlotID.wear, SlotID.mizugi, SlotID.onepiece, SlotID.skirt, SlotID.bra, SlotID.panz, SlotID.headset,
                    SlotID.megane, SlotID.accUde, SlotID.glove, SlotID.accSenaka, SlotID.stkg,
                    SlotID.shoes, SlotID.accAshi, SlotID.accHana, SlotID.accHat, SlotID.accHeso, SlotID.accKamiSubL,
                    SlotID.accKamiSubR, SlotID.accKami_1_, SlotID.accKami_2_, SlotID.accKami_3_, SlotID.accKubi,
                    SlotID.accKubiwa, SlotID.accMiMiL, SlotID.accMiMiR, SlotID.accNipL, SlotID.accNipR,
                    SlotID.accShippo, SlotID.accXXX,
                }.ToDictionary(
                    slot => slot,
                    slot => !clothing.SlotLoaded(slot) || clothing[slot]),
                CurlingFront = clothing[ClothingController.Curling.Front],
                CurlingBack = clothing[ClothingController.Curling.Back],
                PantsuShift = clothing[ClothingController.Curling.Shift],
                AttachedLowerAccessory = clothing.AttachedLowerAccessory is null
                    ? null
                    : propModelSchemaBuilder.Build(clothing.AttachedLowerAccessory),
                AttachedUpperAccessory = clothing.AttachedUpperAccessory is null
                    ? null
                    : propModelSchemaBuilder.Build(clothing.AttachedUpperAccessory),
                HairGravityEnabled = clothing.HairGravityController.Enabled,
                HairGravityPosition = clothing.HairGravityController.Position,
                ClothingGravityEnabled = clothing.ClothingGravityController.Enabled,
                ClothingGravityPosition = clothing.ClothingGravityController.Position,
                CustomFloorHeight = clothing.CustomFloorHeight,
                FloorHeight = clothing.FloorHeight,
            };

        PoseSchema MakePoseSchema(IKController ik)
        {
            return new()
            {
                AnimationFrameBinary = ik.GetAnimationFrameData(),
                Animation = MakeAnimationSchema(character.Animation),
                MuneSubL = ik.GetBone("Mune_L_sub").localRotation,
                MuneSubR = ik.GetBone("Mune_R_sub").localRotation,
                LimbsLimited = ik.LimitLimbRotations,
                DigitsLimited = ik.LimitDigitRotations,
            };

            AnimationSchema MakeAnimationSchema(AnimationController animation) =>
                new()
                {
                    Animation = animationSchemaBuilder.Build(animation.Animation),
                    Time = animation.Time,
                    Playing = animation.Playing,
                };
        }
    }
}
