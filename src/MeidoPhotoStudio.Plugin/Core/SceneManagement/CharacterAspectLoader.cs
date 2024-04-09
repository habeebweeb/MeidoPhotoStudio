using MeidoPhotoStudio.Database.Character;
using MeidoPhotoStudio.Database.Props.Menu;
using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Character.Pose;
using MeidoPhotoStudio.Plugin.Core.Schema;
using MeidoPhotoStudio.Plugin.Core.Schema.Character;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin.Core.SceneManagement;

public class CharacterAspectLoader(
    CharacterService characterService,
    GlobalGravityService globalGravityService,
    GameAnimationRepository gameAnimationRepository,
    CustomAnimationRepository customAnimationRepository,
    GameBlendSetRepository gameBlendSetRepository,
    CustomBlendSetRepository customBlendSetRepository,
    MenuPropRepository menuPropRepository)
    : ISceneAspectLoader<CharactersSchema>
{
    private readonly CharacterService characterService = characterService
        ?? throw new ArgumentNullException(nameof(characterService));

    private readonly GlobalGravityService globalGravityService = globalGravityService
        ?? throw new ArgumentNullException(nameof(globalGravityService));

    private readonly GameAnimationRepository gameAnimationRepository = gameAnimationRepository
        ?? throw new ArgumentNullException(nameof(gameAnimationRepository));

    private readonly CustomAnimationRepository customAnimationRepository = customAnimationRepository
        ?? throw new ArgumentNullException(nameof(customAnimationRepository));

    private readonly GameBlendSetRepository gameBlendSetRepository = gameBlendSetRepository
        ?? throw new ArgumentNullException(nameof(gameBlendSetRepository));

    private readonly CustomBlendSetRepository customBlendSetRepository = customBlendSetRepository
        ?? throw new ArgumentNullException(nameof(customBlendSetRepository));

    private readonly MenuPropRepository menuPropRepository = menuPropRepository
        ?? throw new ArgumentNullException(nameof(menuPropRepository));

    public void Load(CharactersSchema schema, LoadOptions loadOptions)
    {
        if (!loadOptions.Maids)
            return;

        if (schema is null)
            return;

        foreach (var (character, characterSchema) in characterService.Zip(schema.Characters))
            ApplyCharacter(character, characterSchema);

        ApplyGlobalGravity(globalGravityService, schema.GlobalGravity);
    }

    private void ApplyCharacter(CharacterController character, CharacterSchema schema)
    {
        ApplyTransform(character.Transform, schema.Transform);
        ApplyHead(character.Head, schema.Head);
        ApplyFace(character.Face, schema.Face);
        ApplyPose(character.IK, character.Animation, schema.Pose);
        ApplyClothing(character.Clothing, schema.Clothing);

        void ApplyTransform(Transform transform, TransformSchema schema)
        {
            transform.SetPositionAndRotation(schema.Position, schema.Rotation);
            transform.localScale = schema.LocalScale;
        }

        void ApplyHead(HeadController head, HeadSchema schema)
        {
            if (schema.MMConverted)
            {
                head.LeftEyeRotation = schema.LeftEyeRotationDelta;
                head.RightEyeRotation = schema.RightEyeRotationDelta;
            }
            else
            {
                head.LeftEyeRotationDelta = schema.LeftEyeRotationDelta;
                head.RightEyeRotationDelta = schema.RightEyeRotationDelta;
            }

            head.FreeLook = schema.FreeLook;
            head.OffsetLookTarget = schema.OffsetLookTarget;

            if (!schema.MMConverted)
                head.HeadRotation = schema.HeadLookRotation;

            head.HeadToCamera = schema.HeadToCamera;
            head.EyeToCamera = schema.EyeToCamera;
        }

        void ApplyFace(FaceController face, FaceSchema schema)
        {
            face.Blink = schema.Blink;

            var blendSet = GetBlendSetModel(schema.BlendSet) ?? face.BlendSet;

            face.ApplyBlendSet(blendSet);

            foreach (var (hash, value) in schema.FacialExpressionSet.Where(kvp => face.ContainsExpressionKey(kvp.Key)))
                face[hash] = value;

            IBlendSetModel GetBlendSetModel(IBlendSetModelSchema blendSetModelSchema) =>
                blendSetModelSchema switch
                {
                    GameBlendSetSchema gameBlendSet => gameBlendSetRepository.GetByID(gameBlendSet.ID),
                    CustomBlendSetSchema customBlendSet => customBlendSetRepository.GetByID(customBlendSet.ID),
                    _ => null,
                };
        }

        void ApplyPose(IKController ik, AnimationController animation, PoseSchema schema)
        {
            ApplyAnimation(animation, schema.Animation);

            if (!animation.Playing)
            {
                var muneSetting = (Left: true, Right: true);

                if (schema.MMPose is not null)
                    ApplyMMPose(ik, schema.MMPose);
                else
                    muneSetting = ik.ApplyAnimationFrameBinary(schema.AnimationFrameBinary);

                ik.MuneLEnabled = !muneSetting.Left;
                ik.MuneREnabled = !muneSetting.Right;

                if (schema.Version >= 2)
                {
                    if (ik.MuneLEnabled)
                        ik.GetBone("Mune_L_sub").localRotation = schema.MuneSubL;
                    if (ik.MuneREnabled)
                        ik.GetBone("Mune_R_sub").localRotation = schema.MuneSubR;
                }
            }

            ik.LimitLimbRotations = schema.LimbsLimited;
            ik.LimitDigitRotations = schema.DigitsLimited;

            static void ApplyMMPose(IKController ik, MMPoseSchema mmPose)
            {
                var sixtyFour = mmPose.SixtyFourFlag;

                var fingerBones = new string[]
                {
                    "Bip01 L Finger0", "Bip01 L Finger01", "Bip01 L Finger02", "Bip01 L Finger0Nub", "Bip01 L Finger1",
                    "Bip01 L Finger11", "Bip01 L Finger12", "Bip01 L Finger1Nub", "Bip01 L Finger2", "Bip01 L Finger21",
                    "Bip01 L Finger22", "Bip01 L Finger2Nub", "Bip01 L Finger3", "Bip01 L Finger31", "Bip01 L Finger32",
                    "Bip01 L Finger3Nub", "Bip01 L Finger4", "Bip01 L Finger41", "Bip01 L Finger42", "Bip01 L Finger4Nub",
                    "Bip01 R Finger0", "Bip01 R Finger01", "Bip01 R Finger02", "Bip01 R Finger0Nub", "Bip01 R Finger1",
                    "Bip01 R Finger11", "Bip01 R Finger12", "Bip01 R Finger1Nub", "Bip01 R Finger2", "Bip01 R Finger21",
                    "Bip01 R Finger22", "Bip01 R Finger2Nub", "Bip01 R Finger3", "Bip01 R Finger31", "Bip01 R Finger32",
                    "Bip01 R Finger3Nub", "Bip01 R Finger4", "Bip01 R Finger41", "Bip01 R Finger42", "Bip01 R Finger4Nub",
                    "Bip01 L Toe0", "Bip01 L Toe01", "Bip01 L Toe0Nub", "Bip01 L Toe1", "Bip01 L Toe11", "Bip01 L Toe1Nub",
                    "Bip01 L Toe2", "Bip01 L Toe21", "Bip01 L Toe2Nub", "Bip01 R Toe0", "Bip01 R Toe01", "Bip01 R Toe0Nub",
                    "Bip01 R Toe1", "Bip01 R Toe11", "Bip01 R Toe1Nub", "Bip01 R Toe2", "Bip01 R Toe21", "Bip01 R Toe2Nub",
                };

                for (var i = 0; i < mmPose.FingerToeRotations.Length; i++)
                {
                    var bone = ik.GetBone(fingerBones[i]);
                    var rotation = mmPose.FingerToeRotations[i];

                    bone.localRotation = rotation;
                }

                var boneNames = sixtyFour
                    ? new[]
                    {
                        "Bip01 Pelvis", "Bip01 Spine", "Bip01 Spine0a", "Bip01 Spine1", "Bip01 Spine1a", "Bip01 Neck",
                        "Bip01 L UpperArm", "Bip01 R UpperArm", "Bip01 L Forearm", "Bip01 R Forearm", "Bip01 L Thigh",
                        "Bip01 R Thigh", "Bip01 L Calf", "Bip01 R Calf", "Bip01 L Hand", "Bip01 R Hand", "Bip01 L Foot",
                        "Bip01 R Foot",
                    }
                    : new[]
                    {
                        "Bip01", "Bip01 Pelvis", "Bip01 Spine", "Bip01 Spine0a", "Bip01 Spine1", "Bip01 Spine1a",
                        "Bip01 Neck", "Bip01 L Clavicle", "Bip01 R Clavicle", "Bip01 L UpperArm", "Bip01 R UpperArm",
                        "Bip01 L Forearm", "Bip01 R Forearm", "Bip01 L Thigh", "Bip01 R Thigh", "Bip01 L Calf",
                        "Bip01 R Calf", "Mune_L", "Mune_R", "Mune_L_sub", "Mune_R_sub", "Bip01 L Hand", "Bip01 R Hand",
                        "Bip01 L Foot", "Bip01 R Foot",
                    };

                var localRotationIndex = Array.IndexOf(boneNames, "Bip01 R Calf");
                var clavicleLIndex = Array.IndexOf(boneNames, "Bip01 L Clavicle");

                for (var i = 0; i < mmPose.BoneRotations.Length; i++)
                {
                    var rotation = mmPose.BoneRotations[i];

                    if (!sixtyFour && i == clavicleLIndex && !mmPose.ProperClavicle)
                        continue;

                    if (sixtyFour || i > localRotationIndex)
                        ik.GetBone(boneNames[i]).localRotation = rotation;
                    else
                        ik.GetBone(boneNames[i]).rotation = rotation;
                }

                new CoroutineRunner(() => SetHipPosition(ik.GetBone("Bip01"), mmPose.HipPosition)).Start();

                // NOTE: MM apparently also does this too (search for haraPosition in the source) and there
                // isn't a smarter way to do this. MM should've saved the localPosition instead.
                static IEnumerator SetHipPosition(Transform bone, Vector3 hipPosition)
                {
                    for (var i = 0; i < 3; i++)
                    {
                        bone.position = hipPosition;

                        yield return new WaitForEndOfFrame();
                    }
                }
            }

            void ApplyAnimation(AnimationController animation, AnimationSchema schema)
            {
                var animationModel = GetAnimationModel(schema.Animation);

                if (animationModel is not null)
                {
                    animation.Apply(animationModel);
                    animation.Time = schema.Time;
                    animation.Playing = schema.Playing;
                }
                else
                {
                    animation.Time = 0f;
                    animation.Playing = false;
                }

                IAnimationModel GetAnimationModel(IAnimationModelSchema animationModelSchema) =>
                    animationModelSchema switch
                    {
                        GameAnimationSchema gameAnimation => gameAnimationRepository.GetByID(gameAnimation.ID),
                        CustomAnimationSchema customAnimation => customAnimationRepository.GetByID(customAnimation.ID),
                        _ => null,
                    };
            }
        }

        void ApplyClothing(ClothingController clothing, ClothingSchema schema)
        {
            clothing.BodyVisible = schema.BodyVisible;
            clothing[ClothingController.Curling.Front] = schema.CurlingFront;
            clothing[ClothingController.Curling.Back] = schema.CurlingBack;
            clothing[ClothingController.Curling.Shift] = schema.PantsuShift;

            foreach (var (slot, visible) in schema.ClothingSet)
            {
                if (schema.Version >= 2)
                {
                    clothing[slot] = visible;
                }
                else
                {
                    if (slot is SlotID.wear)
                    {
                        clothing[SlotID.wear] = visible;
                        clothing[SlotID.mizugi] = visible;
                        clothing[SlotID.onepiece] = visible;
                    }
                    else if (slot is SlotID.megane)
                    {
                        clothing[SlotID.megane] = visible;
                        clothing[SlotID.accHead] = visible;
                    }
                    else
                    {
                        clothing[slot] = visible;
                    }
                }
            }

            clothing.DetachAllAccessories();

            if (schema.AttachedLowerAccessory is not null)
            {
                var accessory = menuPropRepository.GetByID(schema.AttachedLowerAccessory.ID);

                clothing.AttachAccessory(accessory);
            }

            if (schema.AttachedUpperAccessory is not null)
            {
                var accessory = menuPropRepository.GetByID(schema.AttachedUpperAccessory.ID);

                clothing.AttachAccessory(accessory);
            }

            clothing.HairGravityController.Enabled = schema.HairGravityEnabled;
            clothing.HairGravityController.SetPositionWithoutNotify(schema.HairGravityPosition);

            clothing.ClothingGravityController.Enabled = schema.ClothingGravityEnabled;
            clothing.ClothingGravityController.SetPositionWithoutNotify(schema.ClothingGravityPosition);

            clothing.CustomFloorHeight = schema.CustomFloorHeight;
            clothing.FloorHeight = schema.FloorHeight;
        }
    }

    private void ApplyGlobalGravity(GlobalGravityService globalGravityService, GlobalGravitySchema schema)
    {
        globalGravityService.Enabled = schema.Enabled;
        globalGravityService.HairGravityPosition = schema.HairGravityPosition;
        globalGravityService.ClothingGravityPosition = schema.ClothingGravityPosition;
    }
}
