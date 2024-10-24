using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class ExpressionPane : BasePane
{
    private static readonly string[] EyeHashes =
    [
        "eyeclose", "eyeclose2", "eyeclose3", "eyebig", "eyeclose6", "eyeclose5", "eyeclose8", "eyeclose7", "hitomih",
        "hitomis", "mayuha", "mayuw", "mayuup", "mayuv", "mayuvhalf",
    ];

    private static readonly string[] MouthHashes =
    [
        "moutha", "mouths", "mouthc", "mouthi", "mouthup", "mouthdw", "mouthhe", "mouthuphalf", "tangout", "tangup",
        "tangopen",
    ];

    private static readonly string[] FaceHashes =
    [
        "hoho2", "shock", "nosefook", "namida", "yodare", "toothoff", "tear1", "tear2", "tear3", "hohos", "hoho",
        "hohol",
    ];

    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly FaceShapeKeyConfiguration faceShapeKeyConfiguration;
    private readonly ShapeKeyRangeConfiguration shapeKeyRangeConfiguration;
    private readonly Dictionary<string, BaseControl> controls = new(StringComparer.Ordinal);
    private readonly HashSet<string> customShapeKeys = new(StringComparer.Ordinal);
    private readonly Toggle blinkToggle;
    private readonly Toggle modifyShapeKeysToggle;
    private readonly Button refreshRangeButton;
    private readonly Toggle deleteShapeKeysToggle;
    private readonly PaneHeader paneHeader;
    private readonly Framework.UI.Legacy.ComboBox addShapeKeyComboBox;
    private readonly Button addShapeKeyButton;
    private readonly PaneHeader baseGameShapeKeyHeader;
    private readonly PaneHeader customShapeKeyHeader;
    private readonly LazyStyle deleteShapeKeyButtonStyle = new(13, () => new(GUI.skin.button));
    private readonly LazyStyle shapeKeyLabelStyle = new(13, () => new(GUI.skin.label));

    private bool validCustomShapeKey;
    private string[] shapeKeys;
    private bool hasShapeKeys;

    public ExpressionPane(
        SelectionController<CharacterController> characterSelectionController,
        FaceShapeKeyConfiguration faceShapeKeyConfiguration,
        ShapeKeyRangeConfiguration shapeKeyRangeConfiguration)
    {
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));
        this.faceShapeKeyConfiguration = faceShapeKeyConfiguration ?? throw new ArgumentNullException(nameof(faceShapeKeyConfiguration));
        this.shapeKeyRangeConfiguration = shapeKeyRangeConfiguration ?? throw new ArgumentNullException(nameof(shapeKeyRangeConfiguration));

        this.shapeKeyRangeConfiguration.Refreshed += OnFaceShapeKeyRangeConfigurationRefreshed;
        this.characterSelectionController.Selecting += OnCharacterSelectionChanging;
        this.characterSelectionController.Selected += OnCharacterSelectionChanged;

        this.faceShapeKeyConfiguration.AddedCustomShapeKey += OnShapeKeyAdded;
        this.faceShapeKeyConfiguration.RemovedCustomShapeKey += OnShapeKeyRemoved;

        deleteShapeKeysToggle = new(Translation.Get("expressionPane", "deleteShapeKeysToggle"));

        shapeKeys = [.. this.faceShapeKeyConfiguration.CustomShapeKeys];

        modifyShapeKeysToggle = new(Translation.Get("expressionPane", "modifyShapeKeysToggle"));
        modifyShapeKeysToggle.ControlEvent += OnModifyShapeKeysToggleChanged;

        addShapeKeyComboBox = new(shapeKeys)
        {
            Placeholder = Translation.Get("expressionPane", "searchShapeKeyPlaceholder"),
        };

        addShapeKeyComboBox.ChangedValue += OnAddShapeKeyComboBoxValueChanged;

        addShapeKeyButton = new(Translation.Get("expressionPane", "addShapeKeyButton"));
        addShapeKeyButton.ControlEvent += OnAddShapeKeyButtonPushed;

        baseGameShapeKeyHeader = new(Translation.Get("expressionPane", "baseGameExpressionKeys"));
        customShapeKeyHeader = new(Translation.Get("expressionPane", "customExpressionKeys"));

        foreach (var hashKey in EyeHashes.Concat(MouthHashes).Concat(shapeKeys))
        {
            if (!shapeKeyRangeConfiguration.TryGetRange(hashKey, out var range))
                range = new(0f, 1f);

            var slider = new Slider(Translation.Get("faceBlendValues", hashKey, false), range.Lower, range.Upper);

            slider.ControlEvent += OnControlChanged(hashKey);

            controls.Add(hashKey, slider);
        }

        foreach (var hashKey in FaceHashes)
        {
            var toggle = new Toggle(Translation.Get("faceBlendValues", hashKey));

            toggle.ControlEvent += OnControlChanged(hashKey);

            controls.Add(hashKey, toggle);
        }

        blinkToggle = new(Translation.Get("expressionPane", "blinkToggle"), true);
        blinkToggle.ControlEvent += OnBlinkToggleChanged;

        refreshRangeButton = new(Translation.Get("expressionPane", "refreshShapeKeyRangeButton"));
        refreshRangeButton.ControlEvent += OnRefreshRangeButtonPushed;

        paneHeader = new(Translation.Get("expressionPane", "header"), true);
    }

    private FaceController CurrentFace =>
        characterSelectionController.Current?.Face;

    public override void Draw()
    {
        var guiEnabled = CurrentFace is not null;

        GUI.enabled = guiEnabled;

        paneHeader.Draw();

        if (!paneHeader.Enabled)
            return;

        GUILayout.BeginHorizontal();

        blinkToggle.Draw();

        refreshRangeButton.Draw(GUILayout.ExpandWidth(false));

        GUILayout.EndHorizontal();

        if (!guiEnabled)
            return;

        MpsGui.BlackLine();

        baseGameShapeKeyHeader.Draw();

        if (baseGameShapeKeyHeader.Enabled)
            DrawBuiltinTab();

        if (hasShapeKeys)
        {
            customShapeKeyHeader.Draw();

            if (customShapeKeyHeader.Enabled)
                DrawShapeKeyTab();
        }

        void DrawBuiltinTab()
        {
            const int SliderColumnCount = 2;
            const int ToggleColumnCount = 3;

            var maxWidth = GUILayout.MaxWidth(parent.WindowRect.width - 10f);
            var sliderWidth = GUILayout.MaxWidth(parent.WindowRect.width / SliderColumnCount - 10f);

            foreach (var chunk in EyeHashes
                .Where(CurrentFace.ContainsExpressionKey)
                .Select(hash => controls[hash])
                .Chunk(SliderColumnCount))
            {
                GUILayout.BeginHorizontal(maxWidth);

                foreach (var slider in chunk)
                    slider.Draw(sliderWidth);

                GUILayout.EndHorizontal();
            }

            foreach (var chunk in MouthHashes
                .Where(CurrentFace.ContainsExpressionKey)
                .Select(hash => controls[hash])
                .Chunk(SliderColumnCount))
            {
                GUILayout.BeginHorizontal(maxWidth);

                foreach (var slider in chunk)
                    slider.Draw(sliderWidth);

                GUILayout.EndHorizontal();
            }

            MpsGui.BlackLine();

            foreach (var chunk in FaceHashes
                .Where(CurrentFace.ContainsExpressionKey)
                .Select(hash => controls[hash])
                .Chunk(ToggleColumnCount))
            {
                GUILayout.BeginHorizontal(maxWidth);

                foreach (var toggle in chunk)
                    toggle.Draw();

                GUILayout.EndHorizontal();
            }
        }

        void DrawShapeKeyTab()
        {
            modifyShapeKeysToggle.Draw();

            if (modifyShapeKeysToggle.Value)
            {
                MpsGui.BlackLine();

                GUI.enabled = guiEnabled && hasShapeKeys && !deleteShapeKeysToggle.Value;

                DrawComboBox(addShapeKeyComboBox);

                GUILayout.BeginHorizontal();

                GUI.enabled = guiEnabled;

                deleteShapeKeysToggle.Draw();

                GUI.enabled = guiEnabled && hasShapeKeys && !deleteShapeKeysToggle.Value && validCustomShapeKey;

                addShapeKeyButton.Draw(GUILayout.ExpandWidth(false));

                GUILayout.EndHorizontal();
            }

            MpsGui.BlackLine();

            if (deleteShapeKeysToggle.Value)
                DrawDeleteShapeKeys();
            else
                DrawShapeKeySliders();

            void DrawDeleteShapeKeys()
            {
                GUI.enabled = guiEnabled;

                var noExpandWidth = GUILayout.ExpandWidth(false);
                var maxWidth = GUILayout.MaxWidth(parent.WindowRect.width);

                foreach (var shapeKey in shapeKeys.Where(CurrentFace.ContainsExpressionKey))
                {
                    GUILayout.BeginHorizontal(maxWidth);

                    GUILayout.Label(shapeKey, shapeKeyLabelStyle);

                    if (GUILayout.Button("X", deleteShapeKeyButtonStyle, noExpandWidth))
                        faceShapeKeyConfiguration.RemoveCustomShapeKey(shapeKey);

                    GUILayout.EndHorizontal();
                }
            }

            void DrawShapeKeySliders()
            {
                GUI.enabled = guiEnabled;

                const int SliderColumnCount = 2;

                var maxWidth = GUILayout.MaxWidth(parent.WindowRect.width - 10f);
                var sliderWidth = GUILayout.MaxWidth(parent.WindowRect.width / SliderColumnCount - 10f);

                foreach (var chunk in shapeKeys.Where(CurrentFace.ContainsExpressionKey).Select(hash => controls[hash]).Chunk(2))
                {
                    GUILayout.BeginHorizontal(maxWidth);

                    foreach (var slider in chunk)
                        slider.Draw(sliderWidth);

                    GUILayout.EndHorizontal();
                }
            }
        }
    }

    protected override void ReloadTranslation()
    {
        deleteShapeKeysToggle.Label = Translation.Get("expressionPane", "deleteShapeKeysToggle");

        foreach (var (hashKey, control) in EyeHashes.Concat(MouthHashes).Concat(FaceHashes).Select(hashKey => (hashKey, controls[hashKey])))
        {
            var translation = Translation.Get("faceBlendValues", hashKey);

            if (control is Slider slider)
                slider.Label = translation;
            else if (control is Toggle toggle)
                toggle.Label = translation;
        }

        blinkToggle.Label = Translation.Get("expressionPane", "blinkToggle");
        paneHeader.Label = Translation.Get("expressionPane", "header");
        refreshRangeButton.Label = Translation.Get("expressionPane", "refreshShapeKeyRangeButton");
        addShapeKeyComboBox.Placeholder = Translation.Get("expressionPane", "searchShapeKeyPlaceholder");
        addShapeKeyButton.Label = Translation.Get("expressionPane", "addShapeKeyButton");
        baseGameShapeKeyHeader.Label = Translation.Get("expressionPane", "baseGameExpressionKeys");
        customShapeKeyHeader.Label = Translation.Get("expressionPane", "customExpressionKeys");
    }

    private EventHandler OnControlChanged(string hashKey) =>
        (object sender, EventArgs e) =>
        {
            if (CurrentFace is null)
                return;

            var value = sender switch
            {
                Slider slider => slider.Value,
                Toggle toggle => Convert.ToSingle(toggle.Value),
                _ => throw new NotSupportedException($"'{sender.GetType()} is not supported'"),
            };

            CurrentFace[hashKey] = value;
        };

    private void OnRefreshRangeButtonPushed(object sender, EventArgs e) =>
        shapeKeyRangeConfiguration.Refresh();

    private void OnBlinkToggleChanged(object sender, EventArgs e)
    {
        if (CurrentFace is null)
            return;

        CurrentFace.Blink = blinkToggle.Value;
    }

    private void OnShapeKeyAdded(object sender, FaceShapeKeyConfigurationEventArgs e)
    {
        if (!controls.TryGetValue(e.ChangedShapeKey, out var control))
        {
            if (!shapeKeyRangeConfiguration.TryGetRange(e.ChangedShapeKey, out var range))
                range = new(0f, 1f);

            control = new Slider(e.ChangedShapeKey, range.Lower, range.Upper);

            control.ControlEvent += OnControlChanged(e.ChangedShapeKey);

            controls.Add(e.ChangedShapeKey, control);
        }

        if (CurrentFace is not null && CurrentFace.ContainsExpressionKey(e.ChangedShapeKey))
        {
            var slider = (Slider)control;

            slider.SetValueWithoutNotify(CurrentFace[e.ChangedShapeKey]);
        }

        UpdateShapekeyList();

        shapeKeys = [.. faceShapeKeyConfiguration.CustomShapeKeys];
    }

    private void OnFaceShapeKeyRangeConfigurationRefreshed(object sender, EventArgs e)
    {
        foreach (var (key, slider) in controls.Where(kvp => kvp.Value is Slider).Select(kvp => (kvp.Key, (Slider)kvp.Value)))
        {
            if (!shapeKeyRangeConfiguration.TryGetRange(key, out var range))
                range = new(0f, 1f);

            slider.Left = range.Lower;
            slider.Right = range.Upper;
        }
    }

    private void OnShapeKeyRemoved(object sender, FaceShapeKeyConfigurationEventArgs e)
    {
        if (controls.ContainsKey(e.ChangedShapeKey))
            controls.Remove(e.ChangedShapeKey);

        if (CurrentFace is not null && CurrentFace.ContainsExpressionKey(e.ChangedShapeKey))
            CurrentFace[e.ChangedShapeKey] = 0f;

        UpdateShapekeyList();

        shapeKeys = [.. faceShapeKeyConfiguration.CustomShapeKeys];
    }

    private void OnCharacterSelectionChanging(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (e.Selected is null)
            return;

        var face = e.Selected.Face;

        face.PropertyChanged -= OnFacePropertyChanged;
        face.BlendValueChanged -= OnFaceBlendValueChanged;
    }

    private void OnCharacterSelectionChanged(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (e.Selected is null)
            return;

        var face = e.Selected.Face;

        face.PropertyChanged += OnFacePropertyChanged;
        face.BlendValueChanged += OnFaceBlendValueChanged;

        UpdateShapekeyList();

        UpdateControls();
    }

    private void OnFaceBlendValueChanged(object sender, KeyedPropertyChangeEventArgs<string> e)
    {
        if (!controls.TryGetValue(e.Key, out var control))
            return;

        var face = (FaceController)sender;

        if (control is Slider slider)
            slider.SetValueWithoutNotify(face[e.Key]);
        else if (control is Toggle toggle)
            toggle.SetEnabledWithoutNotify(Convert.ToBoolean(face[e.Key]));
    }

    private void OnFacePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var face = (FaceController)sender;

        if (e.PropertyName is nameof(FaceController.Blink))
            blinkToggle.SetEnabledWithoutNotify(face.Blink);
        else if (e.PropertyName is nameof(FaceController.BlendSet))
            UpdateControls();
    }

    private void OnAddShapeKeyComboBoxValueChanged(object sender, EventArgs e)
    {
        if (CurrentFace is null)
            return;

        validCustomShapeKey = customShapeKeys.Contains(addShapeKeyComboBox.Value);
    }

    private void OnAddShapeKeyButtonPushed(object sender, EventArgs e)
    {
        if (CurrentFace is null)
            return;

        if (string.IsNullOrEmpty(addShapeKeyComboBox.Value))
            return;

        if (!customShapeKeys.Contains(addShapeKeyComboBox.Value))
            return;

        faceShapeKeyConfiguration.AddCustomShapeKey(addShapeKeyComboBox.Value);

        addShapeKeyComboBox.Value = string.Empty;
    }

    private void OnModifyShapeKeysToggleChanged(object sender, EventArgs e) =>
        deleteShapeKeysToggle.Value = false;

    private void UpdateShapekeyList()
    {
        var shapeKeyList = CurrentFace.ExpressionKeys
            .Except(faceShapeKeyConfiguration.BlockedShapeKeys)
            .Except(faceShapeKeyConfiguration.CustomShapeKeys)
            .ToArray();

        hasShapeKeys = shapeKeyList.Length is not 0;

        customShapeKeys.Clear();
        customShapeKeys.UnionWith(shapeKeyList);

        validCustomShapeKey = customShapeKeys.Contains(addShapeKeyComboBox.Value);

        addShapeKeyComboBox.SetItems(shapeKeyList);
    }

    private void UpdateControls()
    {
        if (CurrentFace is null)
            return;

        UpdateBaseGameSliders();
        UpdateShapekeySliders();

        blinkToggle.SetEnabledWithoutNotify(CurrentFace.Blink);

        void UpdateBaseGameSliders()
        {
            var hashKeyAndSliders = EyeHashes
                .Concat(MouthHashes)
                .Where(CurrentFace.ContainsExpressionKey)
                .Select(hashKey => (hashKey, (Slider)controls[hashKey]));

            foreach (var (hashKey, slider) in hashKeyAndSliders)
                slider.SetValueWithoutNotify(CurrentFace[hashKey]);

            var hashKeyAndToggles = FaceHashes
                .Where(CurrentFace.ContainsExpressionKey)
                .Select(hashKey => (hashKey, (Toggle)controls[hashKey]));

            foreach (var (hashKey, toggle) in hashKeyAndToggles)
                toggle.SetEnabledWithoutNotify(Convert.ToBoolean(CurrentFace[hashKey]));
        }

        void UpdateShapekeySliders()
        {
            if (CurrentFace is null)
                return;

            var hashKeyAndSliders = shapeKeys
                .Where(CurrentFace.ContainsExpressionKey)
                .Select(hashKey => (hashKey, (Slider)controls[hashKey]));

            foreach (var (hashKey, slider) in hashKeyAndSliders)
                slider.SetValueWithoutNotify(CurrentFace[hashKey]);
        }
    }
}
