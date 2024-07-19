using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core;
using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin;

public class ExpressionPane : BasePane
{
    private const int BaseGameIndex = 0;

    private static readonly string[] KeySourceGridTranslationKeys = ["baseTab", "shapeKeyTab"];

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
    private readonly Dictionary<string, BaseControl> controls = new(StringComparer.Ordinal);
    private readonly SelectionGrid keySourceGrid;
    private readonly Toggle blinkToggle;
    private readonly Toggle editShapeKeysToggle;
    private readonly PaneHeader paneHeader;
    private readonly Dropdown addShapeKeyDropdown;

    private string[] shapeKeys;
    private bool hasShapeKeys;

    public ExpressionPane(
        SelectionController<CharacterController> characterSelectionController,
        FaceShapeKeyConfiguration faceShapeKeyConfiguration)
    {
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));
        this.faceShapeKeyConfiguration = faceShapeKeyConfiguration ?? throw new ArgumentNullException(nameof(faceShapeKeyConfiguration));

        this.characterSelectionController.Selecting += OnCharacterSelectionChanging;
        this.characterSelectionController.Selected += OnCharacterSelectionChanged;

        this.faceShapeKeyConfiguration.AddedCustomShapeKey += OnShapeKeyAdded;
        this.faceShapeKeyConfiguration.RemovedCustomShapeKey += OnShapeKeyRemoved;

        editShapeKeysToggle = new(Translation.Get("expressionPane", "editShapeKeysToggle"));

        addShapeKeyDropdown = new([Translation.Get("expressionPane", "noShapeKeys")]);

        shapeKeys = [.. this.faceShapeKeyConfiguration.CustomShapeKeys];

        foreach (var hashKey in EyeHashes.Concat(MouthHashes).Concat(shapeKeys))
        {
            var slider = new Slider(Translation.Get("faceBlendValues", hashKey, false), 0f, 1f);

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

        keySourceGrid = new(Translation.GetArray("expressionPane", KeySourceGridTranslationKeys));

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

        blinkToggle.Draw();

        MpsGui.BlackLine();

        keySourceGrid.Draw();

        MpsGui.BlackLine();

        if (!guiEnabled)
            return;

        if (keySourceGrid.SelectedItemIndex is BaseGameIndex)
            DrawBuiltinTab();
        else
            DrawShapeKeyTab();

        void DrawBuiltinTab()
        {
            const int SliderColumnCount = 2;
            const int ToggleColumnCount = 3;

            var sliderWidth = GUILayout.Width(parent.WindowRect.width / SliderColumnCount - 15f);
            var toggleWidth = GUILayout.Width(parent.WindowRect.width / ToggleColumnCount - 15f);

            foreach (var chunk in EyeHashes
                .Where(CurrentFace.ContainsExpressionKey)
                .Select(hash => controls[hash])
                .Chunk(SliderColumnCount))
            {
                GUILayout.BeginHorizontal();

                foreach (var slider in chunk)
                    slider.Draw(sliderWidth);

                GUILayout.EndHorizontal();
            }

            foreach (var chunk in MouthHashes
                .Where(CurrentFace.ContainsExpressionKey)
                .Select(hash => controls[hash])
                .Chunk(SliderColumnCount))
            {
                GUILayout.BeginHorizontal();

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
                GUILayout.BeginHorizontal();

                foreach (var slider in chunk)
                    slider.Draw(toggleWidth);

                GUILayout.EndHorizontal();
            }
        }

        void DrawShapeKeyTab()
        {
            GUILayout.BeginHorizontal();

            GUI.enabled = guiEnabled && hasShapeKeys;

            addShapeKeyDropdown.Draw(GUILayout.Width(150f));

            if (GUILayout.Button("+", GUILayout.ExpandWidth(false)))
                faceShapeKeyConfiguration.AddCustomShapeKey(addShapeKeyDropdown.SelectedItem);

            GUI.enabled = guiEnabled;

            editShapeKeysToggle.Draw();

            GUILayout.EndHorizontal();

            MpsGui.BlackLine();

            if (editShapeKeysToggle.Value)
                DrawEditShapeKeys();
            else
                DrawShapeKeySliders();

            void DrawEditShapeKeys()
            {
                var noExpandWidth = GUILayout.ExpandWidth(false);
                var maxWidth = GUILayout.MaxWidth(parent.WindowRect.width);

                foreach (var shapeKey in shapeKeys.Where(CurrentFace.ContainsExpressionKey))
                {
                    GUILayout.BeginHorizontal(maxWidth);

                    if (GUILayout.Button("X", noExpandWidth))
                    {
                        faceShapeKeyConfiguration.RemoveCustomShapeKey(shapeKey);
                    }

                    GUILayout.Label(shapeKey);

                    GUILayout.EndHorizontal();
                }
            }

            void DrawShapeKeySliders()
            {
                const int SliderColumnCount = 2;

                var width = GUILayout.Width(parent.WindowRect.width / SliderColumnCount - 15f);

                foreach (var chunk in shapeKeys.Where(CurrentFace.ContainsExpressionKey).Select(hash => controls[hash]).Chunk(2))
                {
                    GUILayout.BeginHorizontal();

                    foreach (var slider in chunk)
                        slider.Draw(width);

                    GUILayout.EndHorizontal();
                }
            }
        }
    }

    protected override void ReloadTranslation()
    {
        editShapeKeysToggle.Label = Translation.Get("expressionPane", "editShapeKeysToggle");

        if (!hasShapeKeys)
            addShapeKeyDropdown.SetDropdownItemsWithoutNotify([Translation.Get("expressionPane", "noShapeKeys")]);

        foreach (var (hashKey, control) in EyeHashes.Concat(MouthHashes).Concat(FaceHashes).Select(hashKey => (hashKey, controls[hashKey])))
        {
            var translation = Translation.Get("faceBlendValues", hashKey);

            if (control is Slider slider)
                slider.Label = translation;
            else if (control is Toggle toggle)
                toggle.Label = translation;
        }

        blinkToggle.Label = Translation.Get("expressionPane", "blinkToggle");
        keySourceGrid.SetItemsWithoutNotify(Translation.GetArray("expressionPane", KeySourceGridTranslationKeys));
        paneHeader.Label = Translation.Get("expressionPane", "header");
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
            control = new Slider(e.ChangedShapeKey, 0f, 1f);

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

    private void UpdateShapekeyList()
    {
        var shapeKeyList = CurrentFace.ExpressionKeys
            .Except(faceShapeKeyConfiguration.BlockedShapeKeys)
            .Except(faceShapeKeyConfiguration.CustomShapeKeys)
            .ToArray();

        hasShapeKeys = shapeKeyList.Length is not 0;

        if (shapeKeyList.Length is 0)
            shapeKeyList = [Translation.Get("expressionPane", "noShapeKeys")];

        addShapeKeyDropdown.SetDropdownItems(shapeKeyList);
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
