using MeidoPhotoStudio.Plugin.Core.UndoRedo;
using MeidoPhotoStudio.Plugin.Framework;

namespace MeidoPhotoStudio.Plugin.Core.Lighting;

public class LightUndoRedoService
{
    private readonly LightRepository lightRepository;
    private readonly UndoRedoService undoRedoService;
    private readonly Dictionary<int, LightController> lightControllers = [];
    private readonly Dictionary<int, LightUndoRedoController> undoRedoControllerByID = [];
    private readonly Dictionary<LightController, LightUndoRedoController> undoRedoControllers = [];
    private readonly LightResolver lightResolver;

    private bool changeFromUndoRedo;
    private LightState undoRedoState;

    public LightUndoRedoService(LightRepository lightRepository, UndoRedoService undoRedoService)
    {
        this.lightRepository = lightRepository ?? throw new ArgumentNullException(nameof(lightRepository));
        this.undoRedoService = undoRedoService ?? throw new ArgumentNullException(nameof(undoRedoService));

        this.lightRepository.AddedLight += OnLightAdded;
        this.lightRepository.RemovingLight += OnLightRemoving;

        lightResolver = new(this);
    }

    public LightUndoRedoController this[LightController lightController] =>
        lightController is null
            ? throw new ArgumentNullException(nameof(lightController))
            : undoRedoControllers[lightController];

    private void OnLightAdded(object sender, LightRepositoryEventArgs e)
    {
        if (changeFromUndoRedo)
            ApplyUndoRedoState(e.LightController, undoRedoState);
        else
            PushNewUndoRedo(e.LightController);

        void ApplyUndoRedoState(LightController lightController, LightState undoRedoState)
        {
            var (id, backup) = undoRedoState;
            var undoRedoController = new LightUndoRedoController(id, lightResolver, lightController, undoRedoService);

            undoRedoControllerByID[id] = undoRedoControllers[lightController] = undoRedoController;
            lightControllers[id] = lightController;

            backup.Apply(lightController);
        }

        void PushNewUndoRedo(LightController lightController)
        {
            var id = lightController.Light.GetInstanceID();
            var undoRedoController = new LightUndoRedoController(id, lightResolver, lightController, undoRedoService);
            var lightState = new LightState(id, LightBackup.Create(lightController));

            undoRedoControllerByID[id] = undoRedoControllers[lightController] = undoRedoController;
            lightControllers[id] = lightController;

            if (lightController.Light == GameMain.Instance.MainLight.m_light)
                return;

            undoRedoService.Push(new UndoRedoAction(UndoAdd, RedoAdd));

            void UndoAdd()
            {
                changeFromUndoRedo = true;

                var controller = lightControllers[id];

                lightRepository.RemoveLight(controller);

                changeFromUndoRedo = false;
            }

            void RedoAdd()
            {
                changeFromUndoRedo = true;

                undoRedoState = lightState;

                lightRepository.AddLight();

                changeFromUndoRedo = false;
            }
        }
    }

    private void OnLightRemoving(object sender, LightRepositoryEventArgs e)
    {
        if (changeFromUndoRedo)
            return;

        PushNewUndoRedo(e.LightController);

        void PushNewUndoRedo(LightController lightController)
        {
            var id = undoRedoControllers[lightController].ID;

            undoRedoControllers.Remove(lightController);
            undoRedoControllerByID.Remove(id);
            lightControllers.Remove(id);

            if (lightController.Light == GameMain.Instance.MainLight.m_light)
                return;

            var lightState = new LightState(id, LightBackup.Create(lightController));

            undoRedoService.Push(new UndoRedoAction(UndoDelete, RedoDelete));

            void UndoDelete()
            {
                changeFromUndoRedo = true;

                undoRedoState = lightState;

                lightRepository.AddLight();

                changeFromUndoRedo = false;
            }

            void RedoDelete()
            {
                changeFromUndoRedo = true;

                var controller = lightControllers[id];

                lightRepository.RemoveLight(controller);

                changeFromUndoRedo = false;
            }
        }
    }

    private readonly record struct LightState(int ID, LightBackup Backup);

    private readonly struct LightBackup
    {
        private readonly TransformBackup transformBackup;
        private readonly bool enabled;
        private readonly LightType lightType;
        private readonly LightProperties directionalProperties;
        private readonly LightProperties spotProperties;
        private readonly LightProperties pointProperties;

        private LightBackup(
            TransformBackup transformBackup,
            bool enabled,
            LightType lightType,
            LightProperties directionalProperties,
            LightProperties spotProperties,
            LightProperties pointProperties)
        {
            this.transformBackup = transformBackup;
            this.enabled = enabled;
            this.lightType = lightType;
            this.directionalProperties = directionalProperties;
            this.spotProperties = spotProperties;
            this.pointProperties = pointProperties;
        }

        public static LightBackup Create(LightController lightController)
        {
            _ = lightController ?? throw new ArgumentNullException(nameof(lightController));

            return new(
                new(lightController.Light.transform),
                lightController.Enabled,
                lightController.Type,
                lightController[LightType.Directional],
                lightController[LightType.Spot],
                lightController[LightType.Point]);
        }

        public void Apply(LightController lightController)
        {
            _ = lightController ?? throw new ArgumentNullException(nameof(lightController));

            transformBackup.Apply(lightController.Light.transform);
            lightController.Enabled = enabled;
            lightController.Type = lightType;
            lightController[LightType.Directional] = directionalProperties;
            lightController[LightType.Spot] = spotProperties;
            lightController[LightType.Point] = pointProperties;
        }
    }

    public class LightResolver(LightUndoRedoService lightUndoRedoService)
    {
        public LightController ResolveLight(int id) =>
            lightUndoRedoService.lightControllers[id];
    }
}
