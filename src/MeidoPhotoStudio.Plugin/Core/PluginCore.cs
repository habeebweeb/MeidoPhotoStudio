using BepInEx.Configuration;
using MeidoPhotoStudio.Plugin.Core.Background;
using MeidoPhotoStudio.Plugin.Core.Camera;
using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Character.Pose;
using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.Database.Background;
using MeidoPhotoStudio.Plugin.Core.Database.Character;
using MeidoPhotoStudio.Plugin.Core.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Database.Props.Menu;
using MeidoPhotoStudio.Plugin.Core.Database.Scenes;
using MeidoPhotoStudio.Plugin.Core.Effects;
using MeidoPhotoStudio.Plugin.Core.Lighting;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Core.Message;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Core.SceneManagement;
using MeidoPhotoStudio.Plugin.Core.Scenes;
using MeidoPhotoStudio.Plugin.Core.Serialization;
using MeidoPhotoStudio.Plugin.Core.StartupPreset;
using MeidoPhotoStudio.Plugin.Core.UI.Legacy;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Core.UndoRedo;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.Input;
using MeidoPhotoStudio.Plugin.Framework.Menu;
using MeidoPhotoStudio.Plugin.Framework.Service;
using MeidoPhotoStudio.Plugin.Framework.UI;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;
using UnityEngine.SceneManagement;

namespace MeidoPhotoStudio.Plugin.Core;

/// <summary>Core plugin.</summary>
public partial class PluginCore : MonoBehaviour
{
    private readonly IconCache iconCache = new();
    private readonly List<IActivateable> activateables = [];

    private ConfigFile configuration;
    private CustomMaidSceneService customMaidSceneService;
    private CharacterService characterService;
    private DragHandle.ClickHandler dragHandleClickHandler;
    private CustomGizmo.ClickHandler gizmoClickHandler;
    private TransformWatcher transformWatcher;
    private Translation translation;
    private ScreenSizeChecker screenSizeChecker;
    private MenuPropRepository menuPropRepository;

    public Api.Api Api { get; private set; }

    internal bool Active { get; private set; }

    private void Awake()
    {
        DontDestroyOnLoad(this);

        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDestroy()
    {
        if (Active)
            Deactivate(true);

        GameMain.Instance.MainCamera.ResetCalcNearClip();

        SceneManager.sceneUnloaded -= OnSceneUnloaded;

        iconCache.Destroy();

        DragHandle.Builder.DestroyParent();
        LightService.DestroyParent();
        Framework.CoroutineRunner.DestroyParent();
        IKController.DestroyParent();
        WfCameraMoveSupportUtility.Destroy();
        Framework.UI.Legacy.UIUtility.Destroy();
        FloorHeightDragHandleService.DestroyParent();

        menuPropRepository?.Destroy();
    }

    private void Start()
    {
        // Configuration
        var configRoot = Path.Combine(BepInEx.Paths.ConfigPath, Plugin.PluginName);
        var presetsPath = Path.Combine(configRoot, "Presets");
        var databasePath = Path.Combine(configRoot, "Database");

        configuration = new ConfigFile(Path.Combine(configRoot, $"{Plugin.PluginName}.cfg"), false);

        var inputConfiguration = new InputConfiguration(configuration);

        var translationConfiguration = new TranslationConfiguration(configuration);
        var faceShapeKeyConfiguration = new FaceShapeKeyConfiguration(configuration);
        var faceShapekeyRangeConfiguration = new ShapeKeyRangeConfiguration(
            new ShapeKeyRangeSerializer(Path.Combine(databasePath, "face_shapekey_range.json")));

        var bodyShapeKeyConfiguration = new BodyShapeKeyConfiguration(configuration);
        var bodyShapeKeyRangeConfiguration = new ShapeKeyRangeConfiguration(new ShapeKeyRangeSerializer(Path.Combine(databasePath, "body_shapekey_range.json")));
        var dragHandleConfiguration = new DragHandleConfiguration(configuration);
        var propsConfiguration = new PropsConfiguration(configuration);
        var autoSaveConfiguration = new AutoSaveConfiguration(configuration);
        var uiConfiguration = new UIConfiguration(configuration);
        var characterConfiguration = new CharacterConfiguration(configuration);
        var startupPresetConfiguration = new StartupPresetConfiguration(configuration);

        // Translation
        translation = new Translation(
            Path.Combine(configRoot, "Translations"),
            translationConfiguration.CurrentLanguage.Value)
        {
            LogMissingTranslations = translationConfiguration.LogMissingTranslations.Value,
        };

        // Utilities
        screenSizeChecker = gameObject.AddComponent<ScreenSizeChecker>();
        screenSizeChecker.enabled = false;

        transformWatcher = gameObject.AddComponent<TransformWatcher>();

        customMaidSceneService = new CustomMaidSceneService();

        var tabSelectionController = new TabSelectionController();

        // Input
        var inputPollingService = gameObject.AddComponent<InputPollingService>();

        var inputRemapper = gameObject.AddComponent<InputRemapper>();
        inputRemapper.InputPollingService = inputPollingService;

        inputPollingService.AddInputHandler(new InputHandler(this, inputConfiguration, customMaidSceneService));

        // UI
        dragHandleClickHandler = gameObject.AddComponent<DragHandle.ClickHandler>();
        dragHandleClickHandler.enabled = false;

        gizmoClickHandler = gameObject.AddComponent<CustomGizmo.ClickHandler>();
        gizmoClickHandler.enabled = false;

        var windowManager = gameObject.AddComponent<WindowManager>();

        windowManager.PluginCore = this;

        var generalDragHandleInputService = new GeneralDragHandleInputHandler(inputConfiguration);

        AddPluginActiveInputHandler(generalDragHandleInputService);

        ColourPickerButton.ColourPickerModal = new ColourPickerModal(translation);

        // Screenshot
        var screenshotService = gameObject.AddComponent<ScreenshotService>();

        screenshotService.WindowManager = windowManager;

        AddPluginActiveInputHandler(new ScreenshotServiceInputHandler(screenshotService, inputConfiguration));

        // Undo/Redo
        var undoRedoService = new UndoRedoService();

        AddPluginActiveInputHandler(new UndoRedoInputHandler(undoRedoService, inputConfiguration));

        // Characters
        var gameBlendSetRepository = new GameBlendSetRepository(translation);
        var customBlendSetRepository = new CustomBlendSetRepository(Path.Combine(presetsPath, "Face Presets"));
        var gameAnimationRepository = new GameAnimationRepository(databasePath);
        var customAnimationRepository = new CustomAnimationRepository(Path.Combine(presetsPath, "Custom Poses"));
        var customAnimationRepositorySorter = new CustomAnimationRepositorySorter(customAnimationRepository.RootCategoryName);

        var characterRepository = new CharacterRepository();

        characterService = new CharacterService(customMaidSceneService, transformWatcher, undoRedoService);

        var editModeMaidService = new EditModeMaidService(customMaidSceneService, characterRepository, characterService);

        windowManager.CharacterService = characterService;

        var characterSelectionController = new SelectionController<CharacterController>(characterService);
        var characterCallController = new CallController(
            characterRepository,
            characterService,
            characterSelectionController,
            customMaidSceneService,
            editModeMaidService);

        var facialExpressionBuilder = new FacialExpressionBuilder(faceShapeKeyConfiguration);
        var autoEditMaidService = new AutoEditMaidService(
            customMaidSceneService, editModeMaidService, characterSelectionController)
        {
            Enabled = true,
        };

        AddPluginActiveInputHandler(new CharacterDressingCycler(characterService, inputConfiguration));

        var gravityDragHandleInputService = new GravityDragHandleInputService(inputConfiguration);

        AddPluginActiveInputHandler(gravityDragHandleInputService);

        var gravityDragHandleService = new GravityDragHandleService(
            gravityDragHandleInputService, characterService, characterSelectionController, tabSelectionController)
        {
            SmallHandle = dragHandleConfiguration.SmallTransformCube.Value,
            AutoSelect = dragHandleConfiguration.AutomaticSelection.Value,
            AutoSelectTab = dragHandleConfiguration.AutomaticTabSelection.Value,
            HairDragHandleColour = dragHandleConfiguration.HairDragHandleColour.Value,
            ClothingDragHandleColour = dragHandleConfiguration.ClothingDragHandleColour.Value,
        };

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
            tabSelectionController)
        {
            SmallHandle = dragHandleConfiguration.SmallTransformCube.Value,
            CubeEnabled = dragHandleConfiguration.CharacterTransformCube.Value,
            AutoSelect = dragHandleConfiguration.AutomaticSelection.Value,
            AutoSelectTab = dragHandleConfiguration.AutomaticTabSelection.Value,
            UpperBoneColour = dragHandleConfiguration.UpperLimbDragHandleColour.Value,
            MiddleBoneColour = dragHandleConfiguration.MiddleLimbDragHandleColour.Value,
            LowerBoneColour = dragHandleConfiguration.LowerLimbDragHandleColour.Value,
            SpineColour = dragHandleConfiguration.SpineDragHandleColour.Value,
            RootColour = dragHandleConfiguration.RootDragHandleColour.Value,
            BaseDigitJointColour = dragHandleConfiguration.BaseDigitJointColour.Value,
            MiddleDigitJointColour = dragHandleConfiguration.MiddleDigitJointColour.Value,
            TipDigitJointColour = dragHandleConfiguration.TipDigitJointColour.Value,
        };

        AddPluginActiveInputHandler(new AnimationCycler(
            characterService,
            new AnimationCyclingService(
                characterService, characterUndoRedoService, gameAnimationRepository, customAnimationRepository, customAnimationRepositorySorter),
            inputConfiguration));

        var floorHeightDragHandleInputHandler = new FloorHeightDragHandleInputHandler(inputConfiguration);

        AddPluginActiveInputHandler(floorHeightDragHandleInputHandler);

        var floorHeightDragHandleService = new FloorHeightDragHandleService(
            floorHeightDragHandleInputHandler,
            characterService,
            characterSelectionController,
            tabSelectionController)
        {
            AutoSelect = dragHandleConfiguration.AutomaticSelection.Value,
            AutoSelectTab = dragHandleConfiguration.AutomaticTabSelection.Value,
            DragHandleColour = dragHandleConfiguration.FloorHeightDragHandleColour.Value,
        };
        _ = new CharacterConfigurationController(characterService, ikDragHandleService, characterConfiguration);

        var characterPlacementService = new PlacementService(characterService);

        var automaticCharacterPlacementController = new AutomaticCharacterPlacementController(
            customMaidSceneService, characterService, characterPlacementService)
        {
            Enabled = characterConfiguration.AutomaticallyApplyPlacement.Value,
            PlacementType = characterConfiguration.PlacementPreset.Value,
        };

        // Message
        var messageWindowManager = new MessageWindowManager();

        // Camera
        var cameraController = new CameraController();

        var cameraSaveSlotController = new CameraSaveSlotController(cameraController);
        var cameraSpeedController = new CameraSpeedController();

        AddPluginActiveInputHandler(
            new CameraInputHandler(
                cameraController, cameraSpeedController, cameraSaveSlotController, inputConfiguration));

        // Backgrounds
        var backgroundRepository = new BackgroundRepository(translation);
        var backgroundService = new BackgroundService(backgroundRepository);
        var backgroundDragHandleService = new BackgroundDragHandleService(generalDragHandleInputService, backgroundService)
        {
            SmallHandle = dragHandleConfiguration.SmallTransformCube.Value,
        };

        // Lights
        var lightService = new LightService(transformWatcher);

        var lightSelectionController = new SelectionController<LightController>(lightService);

        var lightDragHandleRepository = new LightDragHandleService(
            generalDragHandleInputService, lightService, lightSelectionController, tabSelectionController)
        {
            SmallHandle = dragHandleConfiguration.SmallTransformCube.Value,
            AutoSelect = dragHandleConfiguration.AutomaticSelection.Value,
            AutoSelectTab = dragHandleConfiguration.AutomaticTabSelection.Value,
        };

        // Effects
        var bloomController = new BloomController(GameMain.Instance.MainCamera.camera);
        var depthOfFieldController = new DepthOfFieldController(GameMain.Instance.MainCamera.camera);
        var vignetteController = new VignetteController(GameMain.Instance.MainCamera.camera);
        var fogController = new FogController(GameMain.Instance.MainCamera.camera);
        var blurController = new BlurController(GameMain.Instance.MainCamera.camera);
        var sepiaToneController = new SepiaToneController(GameMain.Instance.MainCamera.camera);

        // Props
        var gamePropRepository = new PhotoBgPropRepository(translation);
        var deskPropRepository = new DeskPropRepository(translation);
        var otherPropRepository = new OtherPropRepository(translation, backgroundRepository);
        var backgroundPropRepository = new BackgroundPropRepository(backgroundRepository);
        var myRoomPropRepository = new MyRoomPropRepository(translation);

        IModRefreshHandler modRefreshHandler = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("COM3D2.MaidLoader")
            ? new MaidLoaderModRefreshHandler()
            : new EmptyModRefreshHandler();

        IRevealModInFileManagerHandler revealModHandler = BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue("ShiftClickExplorer", out var shiftClickExplorer)
            && shiftClickExplorer.Metadata.Version >= new Version(1, 4)
                ? new ShiftClickExplorerModHandler()
                : new EmptyRevealModHandler();

        menuPropRepository = new MenuPropRepository(
            translation,
            propsConfiguration,
            new MenuFileCacheSerializer(Path.Combine(BepInEx.Paths.ConfigPath, Plugin.PluginName)),
            modRefreshHandler);

        var propService = new PropService(transformWatcher);

        var propSelectionController = new SelectionController<PropController>(propService);

        var propDragHandleService = new PropDragHandleService(
            generalDragHandleInputService,
            propService,
            propSelectionController,
            tabSelectionController)
        {
            SmallHandle = dragHandleConfiguration.SmallTransformCube.Value,
            AutoSelect = dragHandleConfiguration.AutomaticSelection.Value,
            AutoSelectTab = dragHandleConfiguration.AutomaticTabSelection.Value,
        };

        var propAttachmentService = new PropAttachmentService(characterService, propService);

        var propSchemaMapper = new PropSchemaToPropModelMapper(
            translation,
            backgroundPropRepository,
            deskPropRepository,
            myRoomPropRepository,
            gamePropRepository,
            menuPropRepository,
            otherPropRepository);

        var propModelSchemaBuilder = new PropModelSchemaBuilder();

        var favouritePropRepository = new FavouritePropRepository(
            new FavouritePropListSerializer(
                Path.Combine(BepInEx.Paths.ConfigPath, Plugin.PluginName),
                propModelSchemaBuilder,
                propSchemaMapper));

        // Scenes
        var extensionSchemaBuilder = new ExtensionSchemaBuilder();
        var transformSchemaBuilder = new TransformSchemaBuilder();

        // TODO: This is kinda stupid tbf. Maybe look into writing a code generator and attributes to create these
        // "schema" things instead of manually building it.
        // Would that even be possible? idk.
        var sceneSchemaBuilder = new SceneSchemaBuilder(
            new CharacterServiceSchemaBuilder(
                characterService,
                globalGravityService,
                new CharacterSchemaBuilder(
                    facialExpressionBuilder,
                    bodyShapeKeyConfiguration,
                    new AnimationModelSchemaBuilder(),
                    new BlendSetModelSchemaBuilder(),
                    propModelSchemaBuilder,
                    transformSchemaBuilder),
                new GlobalGravitySchemaBuilder()),
            new MessageWindowSchemaBuilder(messageWindowManager),
            new CameraSchemaBuilder(cameraSaveSlotController, new CameraInfoSchemaBuilder()),
            new LightsSchemaBuilder(
                lightService, new LightSchemaBuilder(new LightPropertiesSchemaBuilder())),
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
                new PropControllerSchemaBuilder(
                    propModelSchemaBuilder,
                    transformSchemaBuilder,
                    new PropShapeKeySchemaBuilder()),
                new PropDragHandleSchemaBuilder(),
                new AttachPointSchemaBuilder()),
            extensionSchemaBuilder);

        var extensionAspectLoader = new ExtensionAspectLoader();

        var sceneLoader = new SceneLoader(
            undoRedoService,
            new CharacterAspectLoader(
                characterService,
                characterRepository,
                editModeMaidService,
                characterCallController,
                customMaidSceneService,
                globalGravityService,
                gameAnimationRepository,
                customAnimationRepository,
                gameBlendSetRepository,
                customBlendSetRepository,
                menuPropRepository,
                faceShapeKeyConfiguration,
                bodyShapeKeyConfiguration),
            new MessageAspectLoader(messageWindowManager),
            new CameraAspectLoader(cameraSaveSlotController),
            new LightAspectLoader(lightService, backgroundService),
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
                propSchemaMapper),
            extensionAspectLoader);

        var loadOptionsService = new LoadOptionsService();
        var extensionDataConverter = new ExtensionDataConverter();
        var sceneSerializer = new WrappedSerializer(new([extensionDataConverter]), new());
        var quickSaveService = new QuickSaveService(
            configRoot, characterService, sceneSchemaBuilder, sceneSerializer, sceneLoader, loadOptionsService);

        AddPluginActiveInputHandler(new QuickSaveInputHandler(
            quickSaveService,
            inputConfiguration));

        var sceneRepository = new SceneRepository(Path.Combine(configRoot, "Scenes"), sceneSerializer);

        var autoSaveService = new AutoSaveService(characterService, sceneRepository, screenshotService, sceneSchemaBuilder)
        {
            Enabled = autoSaveConfiguration.Enabled.Value,
            AutoSaveInterval = autoSaveConfiguration.Frequency.Value,
            Slots = autoSaveConfiguration.Slots.Value,
        };

        var startupPresetService = new StartupPresetService(
            configRoot, characterService, screenshotService, sceneSchemaBuilder, sceneSerializer, sceneLoader, loadOptionsService)
        {
            Enabled = startupPresetConfiguration.Enabled.Value,
            UseCustomPreset = startupPresetConfiguration.UseCustomPreset.Value,
        };

        // Windows
        var sceneBrowser = new SceneBrowserWindow(
            translation,
            sceneRepository,
            sceneSchemaBuilder,
            screenshotService,
            sceneSerializer,
            sceneLoader,
            loadOptionsService,
            new(configuration),
            inputRemapper);

        AddPluginActiveInputHandler(new SceneBrowserWindowInputHandler(sceneBrowser, inputConfiguration));

        var messageWindow = new MessageWindow(translation, messageWindowManager, inputRemapper);

        AddPluginActiveInputHandler(new MessageWindow.InputHandler(messageWindow, inputConfiguration));

        var settingsWindow = new SettingsWindow(translation, inputRemapper)
        {
            [SettingsWindow.SettingType.Controls] = new InputSettingsPane(
                translation, inputConfiguration, inputRemapper),
            [SettingsWindow.SettingType.DragHandle] = new DragHandleSettingsPane(
                translation,
                dragHandleConfiguration,
                ikDragHandleService,
                propDragHandleService,
                gravityDragHandleService,
                lightDragHandleRepository,
                backgroundDragHandleService,
                floorHeightDragHandleService),
            [SettingsWindow.SettingType.ShapeKeys] = new ShapeKeysSettingsPane(
                translation,
                faceShapeKeyConfiguration,
                faceShapekeyRangeConfiguration,
                bodyShapeKeyConfiguration,
                bodyShapeKeyRangeConfiguration),
            [SettingsWindow.SettingType.AutoSave] = new AutoSaveSettingsPane(
                translation, autoSaveConfiguration, autoSaveService),
            [SettingsWindow.SettingType.Translation] = new TranslationSettingsPane(translationConfiguration, translation),
            [SettingsWindow.SettingType.Character] = new CharacterSettingsPane(
                translation, characterConfiguration, automaticCharacterPlacementController),
            [SettingsWindow.SettingType.StartupPreset] = new StartupPresetSettingsPane(
                translation, startupPresetConfiguration, startupPresetService),
        };

        TransformClipboard transformClipboard = new();

        var characterPoseTabHeaderGroup = new HeaderGroup();
        var characterFaceTabHeaderGroup = new HeaderGroup();
        var propsHeaderGroup = new HeaderGroup();
        var environmentHeaderGroup = new HeaderGroup();

        var mainWindow = new MainWindow(
            translation, tabSelectionController, customMaidSceneService, inputRemapper, settingsWindow)
        {
            WindowWidth = uiConfiguration.WindowWidth.Value,

            [MainWindow.Tab.Call] = new CallWindowPane()
            {
                new CharacterPlacementPane(translation, characterPlacementService),
                new CharacterCallPane(translation, characterCallController),
            },
            [MainWindow.Tab.Character] = new CharacterWindowPane()
            {
                new CharacterSwitcherPane(
                    translation,
                    characterService,
                    characterSelectionController,
                    customMaidSceneService,
                    editModeMaidService,
                    autoEditMaidService),
                new CharacterPane(translation, tabSelectionController, characterSelectionController)
                {
                    [CharacterPane.CharacterWindowTab.Pose] =
                    [
                        new PaneGroup(
                            new LocalizableGUIContent(translation, "characterPoseSubTabPaneGroups", "presets"),
                            group: characterPoseTabHeaderGroup)
                        {
                            new SubPaneGroup(
                                new LocalizableGUIContent(translation, "presetsSubPaneGroup", "animationPresets"),
                                true)
                            {
                                new AnimationSelectorPane(
                                    translation,
                                    gameAnimationRepository,
                                    customAnimationRepository,
                                    characterUndoRedoService,
                                    characterSelectionController,
                                    customAnimationRepositorySorter),
                                new AnimationPane(translation, characterUndoRedoService, characterSelectionController),
                            },
                            new SubPaneGroup(
                                new LocalizableGUIContent(translation, "presetsSubPaneGroup", "handPresets"))
                            {
                                new HandPresetSelectorPane(
                                    translation,
                                    new(Path.Combine(presetsPath, "Hand Presets")),
                                    characterUndoRedoService,
                                    characterSelectionController),
                            },
                        },
                        new PaneGroup(
                            new LocalizableGUIContent(translation, "characterPoseSubTabPaneGroups", "posing"),
                            group: characterPoseTabHeaderGroup)
                        {
                            new IKPane(
                                translation,
                                ikDragHandleService,
                                characterUndoRedoService,
                                characterSelectionController,
                                transformClipboard),
                            new SubPaneGroup(new LocalizableGUIContent(translation, "posingSubPaneGroup", "freeLook"))
                            {
                                new FreeLookPane(translation, characterSelectionController),
                            },
                            new SubPaneGroup(new LocalizableGUIContent(translation, "posingSubPaneGroup", "copy"))
                            {
                                new CopyPosePane(
                                    translation,
                                    characterService,
                                    characterUndoRedoService,
                                    characterSelectionController),
                            },
                        },
                        new PaneGroup(
                            new LocalizableGUIContent(translation, "characterPoseSubTabPaneGroups", "clothing"),
                            group: characterPoseTabHeaderGroup)
                        {
                            new DressingPane(translation, characterSelectionController),
                        },
                        new PaneGroup(
                            new LocalizableGUIContent(translation, "characterPoseSubTabPaneGroups", "attachedAccessories"),
                            group: characterPoseTabHeaderGroup)
                        {
                            new AttachedAccessoryPane(translation, menuPropRepository, characterSelectionController),
                        },
                        new PaneGroup(
                            new LocalizableGUIContent(translation, "characterPoseSubTabPaneGroups", "gravity"),
                            group: characterPoseTabHeaderGroup)
                        {
                            new GravityControlPane(
                                translation,
                                gravityDragHandleService,
                                globalGravityService,
                                characterSelectionController),
                            new SubPaneGroup(
                                new LocalizableGUIContent(translation, "gravitySubPaneGroup", "floorHeight"))
                            {
                                new CustomFloorHeightPane(
                                    translation,
                                    floorHeightDragHandleService,
                                    characterSelectionController),
                            },
                        }
                    ],
                    [CharacterPane.CharacterWindowTab.Face] =
                    [
                        new PaneGroup(
                            new LocalizableGUIContent(translation, "characterTabFaceSubTabPaneGroups", "presets"),
                            group: characterFaceTabHeaderGroup)
                        {
                            new BlendSetSelectorPane(
                                translation,
                                gameBlendSetRepository,
                                customBlendSetRepository,
                                facialExpressionBuilder,
                                characterSelectionController),
                            new SubPaneGroup(
                                new LocalizableGUIContent(translation, "facePresetsSubPaneGroup", "copyFacialExpression"))
                            {
                                new CopyFacialExpressionPane(translation, facialExpressionBuilder, characterService, characterSelectionController),
                            },
                        },
                        new PaneGroup(
                            new LocalizableGUIContent(translation, "characterTabFaceSubTabPaneGroups", "facialExpression"),
                            group: characterFaceTabHeaderGroup)
                        {
                            new ExpressionPane(
                                translation,
                                characterSelectionController,
                                faceShapeKeyConfiguration,
                                faceShapekeyRangeConfiguration),
                        }
                    ],
                    [CharacterPane.CharacterWindowTab.Body] =
                    [
                        new PaneGroup(
                            new LocalizableGUIContent(translation, "characterTabBodySubTabPaneGroups", "bodyShapeKeys"))
                        {
                            new BodyShapeKeyPane(
                                translation,
                                characterSelectionController,
                                bodyShapeKeyConfiguration,
                                bodyShapeKeyRangeConfiguration),
                        },
                    ],
                },
            },
            [MainWindow.Tab.Environment] = new BGWindowPane()
            {
                new PaneGroup(
                    new LocalizableGUIContent(translation, "environmentTabPaneGroups", "sceneManagement"),
                    group: environmentHeaderGroup)
                {
                    new SceneManagementPane(translation, sceneBrowser, quickSaveService),
                },
                new PaneGroup(
                    new LocalizableGUIContent(translation, "environmentTabPaneGroups", "backgrounds"),
                    group: environmentHeaderGroup)
                {
                    new BackgroundsPane(
                        translation, backgroundService, backgroundRepository, backgroundDragHandleService),
                },
                new PaneGroup(
                    new LocalizableGUIContent(translation, "environmentTabPaneGroups", "camera"),
                    group: environmentHeaderGroup)
                {
                    new CameraPane(translation, cameraController, cameraSaveSlotController),
                },
                new PaneGroup(
                    new LocalizableGUIContent(translation, "environmentTabPaneGroups", "lights"),
                    group: environmentHeaderGroup)
                {
                    new LightsPane(translation, lightService, lightSelectionController, transformClipboard),
                },
                new PaneGroup(
                    new LocalizableGUIContent(translation, "environmentTabPaneGroups", "effects"),
                    group: environmentHeaderGroup)
                {
                    new EffectsPane(translation)
                    {
                        [EffectsPane.EffectType.Bloom] = new BloomPane(translation, bloomController),
                        [EffectsPane.EffectType.DepthOfField] = new DepthOfFieldPane(
                            translation, depthOfFieldController),
                        [EffectsPane.EffectType.Vignette] = new VignettePane(translation, vignetteController),
                        [EffectsPane.EffectType.Fog] = new FogPane(translation, fogController),
                        [EffectsPane.EffectType.Blur] = new BlurPane(translation, blurController),
                        [EffectsPane.EffectType.SepiaTone] = new SepiaTonePane(translation, sepiaToneController),
                    },
                },
            },
            [MainWindow.Tab.Props] = new PropsWindowPane()
            {
                new PaneGroup(
                    new LocalizableGUIContent(translation, "propsTabPaneGroups", "spawnProps"), group: propsHeaderGroup)
                {
                    new PropsPane(translation)
                    {
                        [PropsPane.PropCategory.Game] = new GamePropsPane(translation, propService, gamePropRepository),
                        [PropsPane.PropCategory.Desk] = new DeskPropsPane(translation, propService, deskPropRepository),
                        [PropsPane.PropCategory.Other] = new OtherPropsPane(
                            translation, propService, otherPropRepository),
                        [PropsPane.PropCategory.HandItem] = new HandItemPropsPane(
                            translation, propService, menuPropRepository),
                        [PropsPane.PropCategory.Background] = new BackgroundPropsPane(
                            translation,
                            propService,
                            backgroundPropRepository),
                        [PropsPane.PropCategory.Menu] = new MenuPropsPane(
                            translation,
                            propService,
                            menuPropRepository,
                            propsConfiguration,
                            iconCache,
                            revealModHandler),
                        [PropsPane.PropCategory.MyRoom] = new MyRoomPropsPane(
                            translation, propService, myRoomPropRepository, iconCache),
                        [PropsPane.PropCategory.Favourite] = new FavouritePropsPane(
                            translation, propService, favouritePropRepository, iconCache),
                    },
                },
                new PaneGroup(
                    new LocalizableGUIContent(translation, "propsTabPaneGroups", "manageProps"),
                    group: propsHeaderGroup)
                {
                    new PropManagerPane(
                        translation,
                        propService,
                        favouritePropRepository,
                        propDragHandleService,
                        propSelectionController,
                        transformClipboard),
                    new SubPaneGroup(new LocalizableGUIContent(translation, "managePropSubPaneGroup", "shapeKeys"))
                    {
                        new PropShapeKeyPane(translation, propSelectionController),
                    },
                    new SubPaneGroup(new LocalizableGUIContent(translation, "managePropSubPaneGroup", "attachment"))
                    {
                        new AttachPropPane(translation, characterService, propAttachmentService, propSelectionController),
                    },
                },
            },
        };

        settingsWindow[SettingsWindow.SettingType.UI] = new UISettingsPane(translation, uiConfiguration, mainWindow);

        AddPluginActiveInputHandler(new MainWindow.InputHandler(mainWindow, inputConfiguration));

        windowManager[WindowManager.Window.Main] = mainWindow;
        windowManager[WindowManager.Window.Message] = messageWindow;
        windowManager[WindowManager.Window.Save] = sceneBrowser;
        windowManager[WindowManager.Window.Settings] = settingsWindow;

        dragHandleClickHandler.WindowManager = windowManager;
        gizmoClickHandler.WindowManager = windowManager;

        AddActivateable(cameraController);

        AddActivateable(characterRepository);
        AddActivateable(characterService);
        AddActivateable(editModeMaidService);
        AddActivateable(characterCallController);
        AddActivateable(automaticCharacterPlacementController);

        AddActivateable(cameraSaveSlotController);
        AddActivateable(cameraSpeedController);

        AddActivateable(messageWindowManager);

        AddActivateable(backgroundRepository);
        AddActivateable(backgroundService);

        AddActivateable(lightService);

        AddActivateable(bloomController);
        AddActivateable(depthOfFieldController);
        AddActivateable(vignetteController);
        AddActivateable(fogController);
        AddActivateable(blurController);
        AddActivateable(sepiaToneController);

        AddActivateable(propService);
        AddActivateable(favouritePropRepository);

        AddActivateable(autoSaveService);
        AddActivateable(startupPresetService);

        AddActivateable(windowManager);

        Api = new(
            this,
            new(
                this,
                characterService,
                editModeMaidService,
                ikDragHandleService,
                characterSelectionController),
            new(
                this,
                propService,
                propAttachmentService,
                propSelectionController),
            new(this, lightService, lightSelectionController),
            new(this, windowManager),
            new(extensionSchemaBuilder, extensionAspectLoader, extensionDataConverter, loadOptionsService));

        void AddPluginActiveInputHandler<T>(T inputHandler)
            where T : IInputHandler =>
            inputPollingService.AddInputHandler(new PluginActiveInputHandler<T>(this, inputHandler));

        T AddActivateable<T>(T activateable)
            where T : IActivateable
        {
            activateables.Add(activateable);

            return activateable;
        }
    }

    private void Activate()
    {
        if (!GameMain.Instance.SysDlg.IsDecided)
            return;

        Active = true;

        Api.RaiseActivating();

        dragHandleClickHandler.enabled = true;
        transformWatcher.enabled = true;
        gizmoClickHandler.enabled = true;
        screenSizeChecker.enabled = true;

        foreach (var activateable in activateables)
            activateable.Activate();

        SetDailyPanelActive(false);

        Api.RaiseActivated();
    }

    private void Deactivate(bool force = false)
    {
        if (characterService.Busy || !GameMain.Instance.SysDlg.IsDecided && !force)
            return;

        if (force)
        {
            Exit();

            return;
        }

        GameMain.Instance.SysDlg.Show(
            string.Format(translation["systemMessage", "exitConfirm"], Plugin.PluginName),
            SystemDialog.TYPE.OK_CANCEL,
            Exit,
            GameMain.Instance.SysDlg.Close);

        void Exit()
        {
            GameMain.Instance.SysDlg.Close();

            Api.RaiseDeactivating();

            dragHandleClickHandler.enabled = false;
            transformWatcher.enabled = false;
            gizmoClickHandler.enabled = false;
            screenSizeChecker.enabled = false;

            transformWatcher.Clear();

            foreach (var activateable in activateables)
                activateable.Deactivate();

            // TODO: Should this deactivation stuff be somewhere else?
            if (customMaidSceneService.EditScene)
            {
                SceneEditWindow.PoseIconData.GetItemData(SceneEdit.Instance.pauseIconWindow.selectedIconId).ExecScript();

                if (SceneEdit.Instance.viewReset.GetVisibleEyeToCam())
                    SceneEdit.Instance.maid.EyeToCamera(Maid.EyeMoveType.目と顔を向ける, 0.8f);
                else
                    SceneEdit.Instance.maid.EyeToCamera(Maid.EyeMoveType.無視する, 0.8f);
            }

            configuration.Save();

            SetDailyPanelActive(true);

            Active = false;

            Api.RaiseDeactivated();
        }
    }

    private void OnSceneUnloaded(Scene arg0)
    {
        if (Active)
            Deactivate(true);

        GameMain.Instance.MainCamera.ResetCalcNearClip();
    }

    private void ToggleActive()
    {
        if (Active)
            Deactivate();
        else
            Activate();
    }

    private void SetDailyPanelActive(bool active)
    {
        if (!customMaidSceneService.OfficeScene)
            return;

        var uiRoot = GameObject.Find("UI Root");

        if (!uiRoot)
            return;

        var dailyPanel = uiRoot.transform.Find("DailyPanel");

        if (!dailyPanel)
            return;

        dailyPanel.gameObject.SetActive(active);
    }
}
