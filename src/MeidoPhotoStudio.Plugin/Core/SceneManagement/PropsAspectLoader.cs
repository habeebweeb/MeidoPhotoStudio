using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Core.Schema;
using MeidoPhotoStudio.Plugin.Core.Schema.Props;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin.Core.SceneManagement;

public class PropsAspectLoader(
    PropService propService,
    PropDragHandleService propDragHandleService,
    PropAttachmentService propAttachmentService,
    CharacterService characterService,
    PropSchemaToPropModelMapper propSchemaMapper)
    : ISceneAspectLoader<PropsSchema>
{
    private readonly PropService propService = propService
        ?? throw new ArgumentNullException(nameof(propService));

    private readonly PropDragHandleService propDragHandleService = propDragHandleService
        ?? throw new ArgumentNullException(nameof(propDragHandleService));

    private readonly PropAttachmentService propAttachmentService = propAttachmentService
        ?? throw new ArgumentNullException(nameof(propAttachmentService));

    private readonly CharacterService characterService = characterService
        ?? throw new ArgumentNullException(nameof(characterService));

    private readonly PropSchemaToPropModelMapper propSchemaMapper = propSchemaMapper
        ?? throw new ArgumentNullException(nameof(propSchemaMapper));

    public void Load(PropsSchema propsSchema, LoadOptions loadOptions)
    {
        if (!loadOptions.Props)
            return;

        propService.Clear();

        var currentPropIndex = 0;

        propService.AddedProp += OnAddedProp;

        for (; currentPropIndex < propsSchema.Props.Count; currentPropIndex++)
        {
            var propSchema = propsSchema.Props[currentPropIndex];
            var propModel = propSchemaMapper.Resolve(propSchema.PropModel);

            if (propModel is null)
                continue;

            propService.Add(propModel);
        }

        propService.AddedProp -= OnAddedProp;

        void OnAddedProp(object sender, PropServiceEventArgs e)
        {
            ApplyPropSchema(e.PropController, propsSchema.Props[currentPropIndex]);
            ApplyDragHandleSettings(e.PropController, propsSchema.DragHandleSettings[currentPropIndex]);
            ApplyPropAttachment(
                e.PropController,
                propsSchema.PropAttachment[currentPropIndex],
                propsSchema.Props[currentPropIndex].Transform);
            ApplyShapeKeys(e.PropController, propsSchema.Props[currentPropIndex].ShapeKeys);

            void ApplyPropSchema(PropController propController, PropControllerSchema propSchema)
            {
                propController.GameObject.transform.SetPositionAndRotation(
                    propSchema.Transform.Position, propSchema.Transform.Rotation);
                propController.GameObject.transform.localScale = propSchema.Transform.LocalScale;
                propController.Visible = propSchema.Visible;
                propController.ShadowCasting = propSchema.ShadowCasting;
            }

            void ApplyDragHandleSettings(PropController propController, DragHandleSchema dragHandleSchema)
            {
                var propDragHandle = propDragHandleService[propController];

                propDragHandle.Enabled = dragHandleSchema.HandleEnabled;
                propDragHandle.GizmoEnabled = dragHandleSchema.GizmoEnabled;
                propDragHandle.GizmoMode = dragHandleSchema.GizmoSpace;
            }

            void ApplyPropAttachment(
                PropController propController, AttachPointSchema attachPointSchema, TransformSchema transformSchema)
            {
                if (characterService.Busy)
                {
                    characterService.CalledCharacters += OnCharactersLoaded;

                    void OnCharactersLoaded(object sender, CharacterServiceEventArgs e)
                    {
                        characterService.CalledCharacters -= OnCharactersLoaded;

                        Attach();
                    }
                }
                else
                {
                    Attach();
                }

                void Attach()
                {
                    if (characterService.Count is 0)
                        return;

                    if (attachPointSchema.CharacterIndex >= characterService.Count)
                        return;

                    if (attachPointSchema.AttachPoint is AttachPoint.None)
                        return;

                    var character = characterService[attachPointSchema.CharacterIndex];

                    propAttachmentService.AttachPropTo(propController, character, attachPointSchema.AttachPoint, false);

                    if (propsSchema.Version is 1)
                    {
                        propController.GameObject.transform.SetPositionAndRotation(
                            transformSchema.Position, transformSchema.Rotation);
                    }
                    else
                    {
                        propController.GameObject.transform.localPosition = transformSchema.LocalPosition;
                        propController.GameObject.transform.localRotation = transformSchema.LocalRotation;
                    }
                }
            }

            void ApplyShapeKeys(PropController propController, PropShapeKeySchema shapeKeySchema)
            {
                if (propController.ShapeKeyController is null || shapeKeySchema is null)
                    return;

                foreach (var (hashKey, blendValue) in shapeKeySchema.BlendValues)
                    propController.ShapeKeyController[hashKey] = blendValue;
            }
        }
    }
}
