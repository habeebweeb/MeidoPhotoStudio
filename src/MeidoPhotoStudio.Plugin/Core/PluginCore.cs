using com.workman.cm3d2.scene.dailyEtc;
using MeidoPhotoStudio.Database.Background;
using MeidoPhotoStudio.Database.Character;
using MeidoPhotoStudio.Database.Props;
using MeidoPhotoStudio.Database.Props.Menu;
using MeidoPhotoStudio.Database.Scenes;
using MeidoPhotoStudio.Plugin.Core.Background;
using MeidoPhotoStudio.Plugin.Core.Camera;
using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Character.Pose;
using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.Effects;
using MeidoPhotoStudio.Plugin.Core.Lighting;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Core.SceneManagement;
using MeidoPhotoStudio.Plugin.Core.Scenes;
using MeidoPhotoStudio.Plugin.Core.Serialization;
using MeidoPhotoStudio.Plugin.Core.UI.Legacy;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Core.UndoRedo;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.Menu;
using MeidoPhotoStudio.Plugin.Framework.UI;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;
using MeidoPhotoStudio.Plugin.Service;
using MeidoPhotoStudio.Plugin.Service.Input;
using UnityEngine.SceneManagement;

namespace MeidoPhotoStudio.Plugin.Core;

/// <summary>Core plugin.</summary>
public partial class PluginCore : MonoBehaviour
{
    private readonly IconCache iconCache = new();

    private WindowManager windowManager;
    private MessageWindowManager messageWindowManager;
    private PropService propService;
    private CameraController cameraController;
    private CameraSpeedController cameraSpeedController;
    private CameraSaveSlotController cameraSaveSlotController;
    private ScreenshotService screenshotService;
    private CustomMaidSceneService customMaidSceneService;
    private InputPollingService inputPollingService;
    private bool initialized;
    private InputConfiguration inputConfiguration;
    private InputRemapper inputRemapper;
    private bool active;
    private bool uiActive;
    private BackgroundRepository backgroundRepository;
    private BackgroundService backgroundService;
    private BackgroundDragHandleService backgroundDragHandleService;
    private LightRepository lightRepository;
    private CharacterService characterService;
    private EditModeMaidService editModeMaidService;
    private DragHandle.ClickHandler dragHandleClickHandler;
    private CustomGizmo.ClickHandler gizmoClickHandler;
    private CharacterRepository characterRepository;
    private BloomController bloomController;
    private DepthOfFieldController depthOfFieldController;
    private VignetteController vignetteController;
    private FogController fogController;
    private BlurController blurController;
    private SepiaToneController sepiaToneController;
    private TransformWatcher transformWatcher;
    private UndoRedoService undoRedoService;
    private ScreenSizeChecker screenSizeChecker;

    public bool UIActive
    {
        get => uiActive;
        set
        {
            var newValue = value;

            if (!active)
                newValue = false;

            uiActive = newValue;
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(this);

        customMaidSceneService = new();

        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void OnDestroy()
    {
        if (active)
            Deactivate(true);

        GameMain.Instance.MainCamera.ResetCalcNearClip();

        SceneManager.activeSceneChanged -= OnSceneChanged;

        iconCache.Destroy();

        Destroy(GameObject.Find("[MPS Drag Handle Parent]"));
        Destroy(GameObject.Find("[MPS Light Parent]"));
        Destroy(GameObject.Find("[MPS Coroutine Runner Parent]"));
        Destroy(GameObject.Find("[IK Solver Target Parent]"));
        WfCameraMoveSupportUtility.Destroy();
    }

    private void Start()
    {
        Translation.Initialize(Translation.CurrentLanguage);

        inputConfiguration = new InputConfiguration(MeidoPhotoStudio.Plugin.Configuration.Config);

        inputPollingService = gameObject.AddComponent<InputPollingService>();
        inputPollingService.AddInputHandler(new InputHandler(this, inputConfiguration));
        dragHandleClickHandler = gameObject.AddComponent<DragHandle.ClickHandler>();
        dragHandleClickHandler.enabled = false;
        gizmoClickHandler = gameObject.AddComponent<CustomGizmo.ClickHandler>();
        gizmoClickHandler.enabled = false;
        screenSizeChecker = gameObject.AddComponent<ScreenSizeChecker>();
        screenSizeChecker.enabled = false;
    }

    private void Update()
    {
        if (!customMaidSceneService.ValidScene)
            return;

        if (!active)
            return;

        windowManager.Update();

        if (Modal.Visible)
            Modal.Update();
    }

    private void OnGUI()
    {
        if (!uiActive)
            return;

        windowManager.DrawWindows();

        if (DropdownHelper.Visible)
            DropdownHelper.HandleDropdown();

        if (Modal.Visible)
            Modal.Draw();
    }

    // TODO: Clean this up.
    private void Initialize()
    {
        if (initialized)
            return;

        initialized = true;

        transformWatcher = gameObject.AddComponent<TransformWatcher>();

        screenshotService = gameObject.AddComponent<ScreenshotService>();
        screenshotService.PluginCore = this;

        inputRemapper = gameObject.AddComponent<InputRemapper>();
        inputRemapper.InputPollingService = inputPollingService;

        AddPluginActiveInputHandler(new ScreenshotService.InputHandler(screenshotService, inputConfiguration));

        undoRedoService = new();

        AddPluginActiveInputHandler(new UndoRedoInputHandler(undoRedoService, inputConfiguration));

        var generalDragHandleInputService = new GeneralDragHandleInputHandler(inputConfiguration);

        AddPluginActiveInputHandler(generalDragHandleInputService);

        var tabSelectionController = new TabSelectionController();

        characterRepository = new();

        editModeMaidService = new EditModeMaidService(customMaidSceneService, characterRepository);
        characterService = new CharacterService(customMaidSceneService, editModeMaidService, transformWatcher, undoRedoService);

        var characterSelectionController = new SelectionController<CharacterController>(characterService);

        AddPluginActiveInputHandler(new CharacterDressingCycler(characterService, inputConfiguration));

        var gravityDragHandleInputService = new GravityDragHandleInputService(inputConfiguration);

        AddPluginActiveInputHandler(gravityDragHandleInputService);

        var gravityDragHandleService = new GravityDragHandleService(gravityDragHandleInputService, characterService);

        var globalGravityService = new GlobalGravityService(characterService);

        var characterDragHandleInputService = new CharacterDragHandleInputService(
            generalDragHandleInputService,
            new UpperLimbDragHandleInputHandler(inputConfiguration),
            new MiddleLimbDragHandleInputHandler(inputConfiguration),
            new LowerLimbDragHandleInputHandler(inputConfiguration),
            new TorsoDragHandleInputHandler(inputConfiguration),
            new HeadDragHandleInputHandler(inputConfiguration),
            new PelvisDragHandleInputHandler(inputConfiguration),
            new SpineDragHandleInputHandler(inputConfiguration),
            new HipDragHandleInputHandler(inputConfiguration),
            new ThighDragHandleInputHandler(inputConfiguration),
            new ChestDragHandleInputHandler(inputConfiguration),
            new ChestSubGizmoInputHandler(inputConfiguration),
            new DigitBaseDragHandleInputHandler(inputConfiguration),
            new DigitDragHandleInputHandler(inputConfiguration),
            new EyeDragHandleInputHandler(inputConfiguration));

        AddPluginActiveInputHandler(characterDragHandleInputService);

        var characterUndoRedoService = new CharacterUndoRedoService(characterService, undoRedoService);

        var ikDragHandleService = new IKDragHandleService(
            characterDragHandleInputService,
            characterService,
            characterUndoRedoService,
            characterSelectionController,
            tabSelectionController);

        var configRoot = Path.Combine(BepInEx.Paths.ConfigPath, Plugin.PluginName);
        var presetsPath = Path.Combine(configRoot, "Presets");
        var databasePath = Path.Combine(configRoot, "Database");
        var customPosePath = Path.Combine(presetsPath, "Custom Poses");
        var customBlendSetPath = Path.Combine(presetsPath, "Face Presets");
        var customHandPresetPath = Path.Combine(presetsPath, "Hand Presets");

        var faceShapeKeyConfiguration = new FaceShapeKeyConfiguration(MeidoPhotoStudio.Plugin.Configuration.Config);
        var facialExpressionBuilder = new FacialExpressionBuilder(faceShapeKeyConfiguration);

        messageWindowManager = new();
        messageWindowManager.Activate();

        // TODO: Game hangs when first initializing. This happened before too but was hidden because MPS was initialized
        // while the game was starting up so you don't really notice.
        backgroundRepository = new BackgroundRepository();
        backgroundService = new BackgroundService(backgroundRepository);
        backgroundDragHandleService = new(generalDragHandleInputService, backgroundService);

        lightRepository = new LightRepository(transformWatcher);

        var lightSelectionController = new SelectionController<LightController>(lightRepository);

        // TODO: This reference is not used anywhere else.
        var lightDragHandleRepository = new LightDragHandleRepository(
            generalDragHandleInputService, lightRepository, lightSelectionController, tabSelectionController);

        cameraController = new(customMaidSceneService);

        cameraSpeedController = new();
        cameraSaveSlotController = new(cameraController);

        AddPluginActiveInputHandler(
            new CameraInputHandler(
                cameraController, cameraSpeedController, cameraSaveSlotController, inputConfiguration));

        bloomController = new BloomController(GameMain.Instance.MainCamera.camera);
        depthOfFieldController = new DepthOfFieldController(GameMain.Instance.MainCamera.camera);
        vignetteController = new VignetteController(GameMain.Instance.MainCamera.camera);
        fogController = new FogController(GameMain.Instance.MainCamera.camera);
        blurController = new BlurController(GameMain.Instance.MainCamera.camera);
        sepiaToneController = new SepiaToneController(GameMain.Instance.MainCamera.camera);

        propService = new(transformWatcher);

        var propAttachmentService = new PropAttachmentService(characterService, propService);

        var propSelectionController = new SelectionController<PropController>(propService);
        var propDragHandleService = new PropDragHandleService(
            generalDragHandleInputService, propService, propSelectionController, tabSelectionController);

        var gamePropRepository = new PhotoBgPropRepository();
        var deskPropRepository = new DeskPropRepository();
        var otherPropRepository = new OtherPropRepository(backgroundRepository);
        var backgroundPropRepository = new BackgroundPropRepository(backgroundRepository);
        var myRoomPropRepository = new MyRoomPropRepository();

        var menuPropsConfiguration = new MenuPropsConfiguration(MeidoPhotoStudio.Plugin.Configuration.Config);
        var menuPropRepository = new MenuPropRepository(
            menuPropsConfiguration,
            new MenuFileCacheSerializer(Path.Combine(BepInEx.Paths.ConfigPath, Plugin.PluginName)));

        var gameAnimationRepository = new GameAnimationRepository(databasePath);
        var customAnimationRepository = new CustomAnimationRepository(customPosePath);

        var gameBlendSetRepository = new GameBlendSetRepository();
        var customBlendSetRepository = new CustomBlendSetRepository(customBlendSetPath);

        var transformSchemaBuilder = new TransformSchemaBuilder();
        var propModelSchemaBuilder = new PropModelSchemaBuilder();

        // TODO: This is kinda stupid tbf. Maybe look into writing a code generator and attributes to create these
        // "schema" things instead of manually building it.
        // Would that even be possible? idk.
        var sceneSchemaBuilder = new SceneSchemaBuilder(
            new CharacterServiceSchemaBuilder(
                characterService,
                globalGravityService,
                new CharacterSchemaBuilder(
                    facialExpressionBuilder,
                    new AnimationModelSchemaBuilder(),
                    new BlendSetModelSchemaBuilder(),
                    propModelSchemaBuilder,
                    transformSchemaBuilder),
                new GlobalGravitySchemaBuilder()),
            new MessageWindowSchemaBuilder(messageWindowManager),
            new CameraSchemaBuilder(cameraSaveSlotController, new CameraInfoSchemaBuilder()),
            new LightRepositorySchemaBuilder(
                lightRepository, new LightSchemaBuilder(new LightPropertiesSchemaBuilder())),
            new EffectsSchemaBuilder(
                bloomController,
                depthOfFieldController,
                fogController,
                vignetteController,
                sepiaToneController,
                blurController,
                new BloomSchemaBuilder(),
                new DepthOfFieldSchemaBuilder(),
                new FogSchemaBuilder(),
                new VignetteSchemaBuilder(),
                new SepiaToneSchemaBuilder(),
                new BlurSchemaBuilder()),
            new BackgroundSchemaBuilder(
                backgroundService,
                new BackgroundModelSchemaBuilder(),
                transformSchemaBuilder),
            new PropsSchemaBuilder(
                propService,
                propDragHandleService,
                propAttachmentService,
                new PropControllerSchemaBuilder(new PropModelSchemaBuilder(), transformSchemaBuilder),
                new DragHandleSchemaBuilder(),
                new AttachPointSchemaBuilder()));

        var sceneLoader = new SceneLoader(
            undoRedoService,
            new CharacterAspectLoader(
                characterService,
                characterRepository,
                globalGravityService,
                gameAnimationRepository,
                customAnimationRepository,
                gameBlendSetRepository,
                customBlendSetRepository,
                menuPropRepository),
            new MessageAspectLoader(messageWindowManager),
            new CameraAspectLoader(cameraSaveSlotController),
            new LightAspectLoader(lightRepository, backgroundService),
            new EffectsAspectLoader(
                bloomController,
                depthOfFieldController,
                vignetteController,
                fogController,
                blurController,
                sepiaToneController),
            new BackgroundAspectLoader(backgroundService, backgroundRepository),
            new PropsAspectLoader(
                propService,
                propDragHandleService,
                propAttachmentService,
                characterService,
                backgroundRepository,
                deskPropRepository,
                myRoomPropRepository,
                gamePropRepository,
                menuPropRepository));

        var sceneSerializer = new WrappedSerializer(new(), new());

        var sceneRepository = new SceneRepository(Path.Combine(configRoot, "Scenes"), sceneSerializer);

        var sceneBrowser = new SceneBrowserWindow(
            sceneRepository,
            new(sceneRepository, screenshotService, sceneSchemaBuilder, sceneSerializer, sceneLoader),
            sceneSchemaBuilder,
            screenshotService,
            new(MeidoPhotoStudio.Plugin.Configuration.Config));

        AddPluginActiveInputHandler(new SceneBrowserWindowInputHandler(sceneBrowser, inputConfiguration));

        var quickSaveService = new QuickSaveService(configRoot, characterService, sceneSchemaBuilder, sceneSerializer, sceneLoader);

        AddPluginActiveInputHandler(new QuickSaveInputHandler(
            quickSaveService,
            inputConfiguration));

        var messageWindow = new MessageWindow(messageWindowManager);

        AddPluginActiveInputHandler(new MessageWindow.InputHandler(messageWindow, inputConfiguration));

        var characterWindowPane = new CharacterWindowPane(
            new CharacterSwitcherPane(
                characterService, characterSelectionController, customMaidSceneService, editModeMaidService),
            tabSelectionController)
        {
            [CharacterWindowPane.CharacterWindowTab.Pose] =
            [
                new AnimationSelectorPane(gameAnimationRepository, customAnimationRepository, characterUndoRedoService, characterSelectionController),
                new IKPane(ikDragHandleService, characterUndoRedoService,  characterSelectionController),
                new AnimationPane(characterUndoRedoService, characterSelectionController),
                new FreeLookPane(characterSelectionController),
                new DressingPane(characterSelectionController),
                new GravityControlPane(gravityDragHandleService, globalGravityService, characterSelectionController),
                new AttachedAccessoryPane(menuPropRepository, characterSelectionController),
                new HandPresetSelectorPane(new(customHandPresetPath), characterUndoRedoService, characterSelectionController),
                new CopyPosePane(characterService, characterUndoRedoService, characterSelectionController),
            ],
            [CharacterWindowPane.CharacterWindowTab.Face] =
            [
                new BlendSetSelectorPane(
                    gameBlendSetRepository,
                    customBlendSetRepository,
                    facialExpressionBuilder,
                    characterSelectionController),
                new ExpressionPane(characterSelectionController, faceShapeKeyConfiguration),
            ],
        };

        var mainWindow = new MainWindow(tabSelectionController, customMaidSceneService, inputRemapper)
        {
            [Constants.Window.Call] = new CallWindowPane()
            {
                new CharacterPlacementPane(new(characterService)),
                new CharacterCallPane(new(characterRepository, characterService, customMaidSceneService, editModeMaidService)),
            },
            [Constants.Window.Pose] = characterWindowPane,
            [Constants.Window.Face] = characterWindowPane,
            [Constants.Window.BG] = new BGWindowPane()
                {
                    new SceneManagementPane(sceneBrowser, quickSaveService),
                    new BackgroundsPane(backgroundService, backgroundRepository, backgroundDragHandleService),
                    new DragPointPane(propDragHandleService, ikDragHandleService),
                    new CameraPane(cameraController, cameraSaveSlotController),
                    new LightsPane(lightRepository, lightSelectionController),
                    new EffectsPane()
                    {
                        [EffectsPane.EffectType.Bloom] = new BloomPane(bloomController),
                        [EffectsPane.EffectType.DepthOfField] = new DepthOfFieldPane(depthOfFieldController),
                        [EffectsPane.EffectType.Vignette] = new VignettePane(vignetteController),
                        [EffectsPane.EffectType.Fog] = new FogPane(fogController),
                        [EffectsPane.EffectType.Blur] = new BlurPane(blurController),
                        [EffectsPane.EffectType.SepiaTone] = new SepiaTonePane(sepiaToneController),
                    },
                },
            [Constants.Window.BG2] = new PropsWindowPane()
                {
                    new PropsPane()
                    {
                        [PropsPane.PropCategory.Game] = new GamePropsPane(propService, gamePropRepository),
                        [PropsPane.PropCategory.Desk] = new DeskPropsPane(propService, deskPropRepository),
                        [PropsPane.PropCategory.Other] = new OtherPropsPane(propService, otherPropRepository),
                        [PropsPane.PropCategory.HandItem] = new HandItemPropsPane(propService, menuPropRepository),
                        [PropsPane.PropCategory.Background] = new BackgroundPropsPane(
                            propService,
                            backgroundPropRepository),
                        [PropsPane.PropCategory.Menu] = new MenuPropsPane(
                            propService,
                            menuPropRepository,
                            menuPropsConfiguration,
                            iconCache),
                        [PropsPane.PropCategory.MyRoom] = new MyRoomPropsPane(
                            propService, myRoomPropRepository, iconCache),
                    },
                    new PropShapeKeyPane(propSelectionController),
                    new PropManagerPane(propService, propDragHandleService, propSelectionController, new()),
                    new AttachPropPane(characterService, propAttachmentService, propSelectionController),
                },
            [Constants.Window.Settings] = new SettingsWindowPane(inputConfiguration, inputRemapper),
        };

        AddPluginActiveInputHandler(new MainWindow.InputHandler(mainWindow, inputConfiguration));

        windowManager = new()
        {
            [Constants.Window.Main] = mainWindow,
            [Constants.Window.Message] = messageWindow,
            [Constants.Window.Save] = sceneBrowser,
        };

        dragHandleClickHandler.WindowManager = windowManager;
        gizmoClickHandler.WindowManager = windowManager;

        void AddPluginActiveInputHandler<T>(T inputHandler)
            where T : IInputHandler =>
            inputPollingService.AddInputHandler(new PluginActiveInputHandler<T>(this, inputHandler));
    }

    private void Activate()
    {
        if (!GameMain.Instance.SysDlg.IsDecided)
            return;

        if (!initialized)
            Initialize();

        dragHandleClickHandler.enabled = true;
        transformWatcher.enabled = true;
        gizmoClickHandler.enabled = true;
        screenSizeChecker.enabled = true;

        // TODO: Move all this activation/deactivation stuff.
        backgroundRepository.Refresh();
        characterRepository.Refresh();

        cameraController.Activate();
        editModeMaidService.Activate();
        characterService.Activate();
        messageWindowManager.Activate();
        cameraSaveSlotController.Activate();

        screenshotService.enabled = true;

        lightRepository.AddLight(GameMain.Instance.MainLight.GetComponent<Light>());

        backgroundService.ChangeBackground(new(BackgroundCategory.COM3D2, "Theater"));

        bloomController.Activate();
        depthOfFieldController.Activate();
        vignetteController.Activate();
        fogController.Activate();
        blurController.Activate();
        sepiaToneController.Activate();

        windowManager.Activate();

        characterService.CallingCharacters += OnCallingCharacters;

        uiActive = true;
        active = true;

        if (!customMaidSceneService.EditScene)
        {
            // TODO: Rework this to not use null propagation (UNT008)
            var dailyPanel = GameObject.Find("UI Root")?.transform.Find("DailyPanel")?.gameObject;

            if (dailyPanel)
                dailyPanel.SetActive(false);
        }
    }

    private void OnCallingCharacters(object sender, CharacterServiceEventArgs e)
    {
#if DEBUG
        return;
#else
        if (!active)
            return;

        uiActive = false;

        characterService.CalledCharacters += OnCharactersCalled;

        void OnCharactersCalled(object sender, CharacterServiceEventArgs e)
        {
            uiActive = true;

            characterService.CalledCharacters -= OnCharactersCalled;
        }
#endif
    }

    private void Deactivate(bool force = false)
    {
        if (characterService.Busy)
            return;

        var sysDialog = GameMain.Instance.SysDlg;

        if (!sysDialog.IsDecided && !force)
            return;

        uiActive = false;
        active = false;

        if (force)
        {
            Exit();

            return;
        }

        sysDialog.Show(
            string.Format(Translation.Get("systemMessage", "exitConfirm"), Plugin.PluginName),
            SystemDialog.TYPE.OK_CANCEL,
            Exit,
            Resume);

        void Resume()
        {
            sysDialog.Close();
            uiActive = true;
            active = true;
        }

        void Exit()
        {
            sysDialog.Close();

            dragHandleClickHandler.enabled = false;
            transformWatcher.enabled = false;
            gizmoClickHandler.enabled = false;
            screenSizeChecker.enabled = false;

            characterService.Deactivate();
            cameraController.Deactivate();
            messageWindowManager.Deactivate();
            windowManager.Deactivate();
            cameraSpeedController.Deactivate();
            screenshotService.enabled = false;

            bloomController.Deactivate();
            depthOfFieldController.Deactivate();
            vignetteController.Deactivate();
            fogController.Deactivate();
            blurController.Deactivate();
            sepiaToneController.Deactivate();

            editModeMaidService.Deactivate();

            characterService.CallingCharacters -= OnCallingCharacters;

            // TODO: Should this deactivation stuff be somewhere else?
            if (customMaidSceneService.EditScene)
            {
                SceneEditWindow.BgIconData.GetItemData(SceneEdit.Instance.bgIconWindow.selectedIconId).Exec();
                SceneEditWindow.PoseIconData.GetItemData(SceneEdit.Instance.pauseIconWindow.selectedIconId).ExecScript();

                if (SceneEdit.Instance.viewReset.GetVisibleEyeToCam())
                    SceneEdit.Instance.maid.EyeToCamera(Maid.EyeMoveType.目と顔を向ける, 0.8f);
                else
                    SceneEdit.Instance.maid.EyeToCamera(Maid.EyeMoveType.無視する, 0.8f);
            }
            else
            {
                if (GameMain.Instance.CharacterMgr.status.isDaytime)
                    GameMain.Instance.BgMgr.ChangeBg(DailyAPI.dayBg);
                else
                    GameMain.Instance.BgMgr.ChangeBg(DailyAPI.nightBg);
            }

            lightRepository.RemoveAllLights();
            propService.Clear();
            undoRedoService.Clear();

            Modal.Close();

            MeidoPhotoStudio.Plugin.Configuration.Config.Save();

            if (customMaidSceneService.EditScene)
                return;

            // TODO: Rework this to not use null propagation (UNT008)
            var dailyPanel = GameObject.Find("UI Root")?.transform.Find("DailyPanel")?.gameObject;

            // NOTE: using is (not) for null checks on UnityEngine.Object does not work
            if (dailyPanel)
                dailyPanel.SetActive(true);
        }
    }

    private void OnSceneChanged(Scene current, Scene next)
    {
        if (active)
            Deactivate(true);

        GameMain.Instance.MainCamera.ResetCalcNearClip();
    }

    private void ToggleActive()
    {
        if (active)
            Deactivate();
        else
            Activate();
    }
}
