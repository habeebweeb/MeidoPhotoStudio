using MeidoPhotoStudio.Database.Background;
using MeidoPhotoStudio.Database.Props;
using MeidoPhotoStudio.Database.Props.Menu;
using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Core.Schema;
using MeidoPhotoStudio.Plugin.Core.Schema.Props;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin.Core.SceneManagement;

public class PropsAspectLoader : ISceneAspectLoader<PropsSchema>
{
    private readonly PropService propService;
    private readonly PropDragHandleService propDragHandleService;
    private readonly PropAttachmentService propAttachmentService;
    private readonly CharacterService characterService;
    private readonly BackgroundRepository backgroundRepository;
    private readonly DeskPropRepository deskPropRepository;
    private readonly MyRoomPropRepository myRoomPropRepository;
    private readonly PhotoBgPropRepository photoBgPropRepository;
    private readonly MenuPropRepository menuPropRepository;

    public PropsAspectLoader(
        PropService propService,
        PropDragHandleService propDragHandleService,
        PropAttachmentService propAttachmentService,
        CharacterService characterService,
        BackgroundRepository backgroundRepository,
        DeskPropRepository deskPropRepository,
        MyRoomPropRepository myRoomPropRepository,
        PhotoBgPropRepository photoBgPropRepository,
        MenuPropRepository menuPropRepository)
    {
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        this.propDragHandleService = propDragHandleService ?? throw new ArgumentNullException(nameof(propDragHandleService));
        this.propAttachmentService = propAttachmentService ?? throw new ArgumentNullException(nameof(propAttachmentService));
        this.characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));
        this.backgroundRepository = backgroundRepository ?? throw new ArgumentNullException(nameof(backgroundRepository));
        this.deskPropRepository = deskPropRepository ?? throw new ArgumentNullException(nameof(deskPropRepository));
        this.myRoomPropRepository = myRoomPropRepository ?? throw new ArgumentNullException(nameof(myRoomPropRepository));
        this.photoBgPropRepository = photoBgPropRepository ?? throw new ArgumentNullException(nameof(photoBgPropRepository));
        this.menuPropRepository = menuPropRepository ?? throw new ArgumentNullException(nameof(menuPropRepository));
    }

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
            var propModel = GetPropModel(propSchema.PropModel);

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
                    propController.GameObject.transform.SetPositionAndRotation(transformSchema.Position, transformSchema.Rotation);
                }
                else
                {
                    propController.GameObject.transform.localPosition = transformSchema.LocalPosition;
                    propController.GameObject.transform.localRotation = transformSchema.LocalRotation;
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

    private IPropModel GetPropModel(IPropModelSchema propModelSchema)
    {
        if (propModelSchema is BackgroundPropModelSchema backgroundPropModelSchema)
        {
            var model = backgroundRepository.GetByID(backgroundPropModelSchema.ID);

            if (model is not null)
                return new BackgroundPropModel(model);
        }
        else if (propModelSchema is DeskPropModelSchema deskPropModelSchema)
        {
            return deskPropRepository.GetByID(deskPropModelSchema.ID);
        }
        else if (propModelSchema is MyRoomPropModelSchema myRoomPropModelSchema)
        {
            return myRoomPropRepository.GetByID(myRoomPropModelSchema.ID);
        }
        else if (propModelSchema is OtherPropModelSchema otherPropModel)
        {
            // NOTE: Older versions saved desk/prop/dogu as the same thing so the repository cannot be reliably searched
            return new OtherPropModel(otherPropModel.AssetName, Translation.Get("propNames", otherPropModel.AssetName));
        }
        else if (propModelSchema is PhotoBgPropModelSchema photoBgPropModelSchema)
        {
            return photoBgPropRepository.GetByID(photoBgPropModelSchema.ID);
        }
        else if (propModelSchema is MenuFilePropModelSchema menuFilePropModelSchema)
        {
            if (menuPropRepository.Busy)
            {
                if (string.IsNullOrEmpty(menuFilePropModelSchema.Filename))
                    return null;

                var menuFile = new MenuFileParser().ParseMenuFile(menuFilePropModelSchema.Filename, false);

                if (menuFile is not null)
                    menuFile.Name = menuFile.CategoryMpn is MPN.handitem
                        ? Translation.Get("propNames", menuFile.Filename)
                        : menuFile.Filename;

                return menuFile;
            }

            if (string.IsNullOrEmpty(menuFilePropModelSchema.ID))
                return null;

            return menuPropRepository.GetByID(menuFilePropModelSchema.ID);
        }

        return null;
    }
}
