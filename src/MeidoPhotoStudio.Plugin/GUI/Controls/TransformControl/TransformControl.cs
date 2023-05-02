using System;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class TransformControl : BaseControl
{
    private readonly Button copyButton;
    private readonly Button pasteButton;
    private readonly Button resetButton;
    private readonly NumericalTextField xTextField;
    private readonly NumericalTextField yTextField;
    private readonly NumericalTextField zTextField;

    public TransformControl(string header, Vector3 defaultValue)
    {
        Header = header;
        DefaultValue = defaultValue;

        copyButton = new("C");
        pasteButton = new("P");
        resetButton = new("R");

        copyButton.ControlEvent += (_, _) =>
            Copy();

        pasteButton.ControlEvent += (_, _) =>
            Paste();

        resetButton.ControlEvent += (_, _) =>
            Reset();

        xTextField = new(defaultValue[0]);
        yTextField = new(defaultValue[1]);
        zTextField = new(defaultValue[2]);

        xTextField.ControlEvent += (_, _) =>
            TextFieldChangedEventHandler(
                this, new(TransformComponentChangeEventArgs.TransformComponent.X, xTextField.Value));

        yTextField.ControlEvent += (_, _) =>
            TextFieldChangedEventHandler(
                this, new(TransformComponentChangeEventArgs.TransformComponent.Y, yTextField.Value));

        zTextField.ControlEvent += (_, _) =>
            TextFieldChangedEventHandler(
                this, new(TransformComponentChangeEventArgs.TransformComponent.Z, zTextField.Value));
    }

    public new event EventHandler<TransformComponentChangeEventArgs> ControlEvent;

    public TransformClipboard.TransformType TransformType { get; set; }

    public Vector3 DefaultValue { get; set; }

    public TransformClipboard Clipboard { get; set; }

    public string Header { get; set; }

    public string CopyButtonLabel
    {
        get => copyButton.Label;
        set => copyButton.Label = value;
    }

    public string PasteButtonLabel
    {
        get => pasteButton.Label;
        set => pasteButton.Label = value;
    }

    public string ResetButtonLabel
    {
        get => resetButton.Label;
        set => resetButton.Label = value;
    }

    public Vector3 Value
    {
        get => new(xTextField.Value, yTextField.Value, zTextField.Value);
        set => SetValue(value);
    }

    public override void Draw(params GUILayoutOption[] layoutoptions)
    {
        var noExpandWidth = GUILayout.ExpandWidth(false);
        var textFieldWidth = GUILayout.Width(60f);

        GUILayout.BeginHorizontal();
        MpsGui.Header(Header);
        GUILayout.FlexibleSpace();

        if (Clipboard is not null)
        {
            copyButton.Draw(noExpandWidth);
            pasteButton.Draw(noExpandWidth);
        }

        resetButton.Draw(noExpandWidth);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("X", noExpandWidth);
        xTextField.Draw(textFieldWidth);
        GUILayout.Label("Y", noExpandWidth);
        yTextField.Draw(textFieldWidth);
        GUILayout.Label("Z", noExpandWidth);
        zTextField.Draw(textFieldWidth);
        GUILayout.EndHorizontal();
    }

    public void SetButtonLabels(
        string copyButtonLabel = "C", string pasteButtonLabel = "P", string resetButtonLabel = "R")
    {
        CopyButtonLabel = copyButtonLabel;
        PasteButtonLabel = pasteButtonLabel;
        ResetButtonLabel = resetButtonLabel;
    }

    public void SetValueWithoutNotify(Vector3 value) =>
        SetValue(value, false);

    private void Copy()
    {
        if (Clipboard is null)
            return;

        Clipboard[TransformType] = Value;
    }

    private void Paste()
    {
        if (Clipboard?[TransformType] is not Vector3 value)
            return;

        Value = value;
    }

    private void Reset() =>
        Value = DefaultValue;

    private void SetValue(Vector3 value, bool notify = true)
    {
        if (notify)
        {
            xTextField.Value = value[0];
            yTextField.Value = value[1];
            zTextField.Value = value[2];
        }
        else
        {
            xTextField.SetValueWithoutNotify(value[0]);
            yTextField.SetValueWithoutNotify(value[1]);
            zTextField.SetValueWithoutNotify(value[2]);
        }
    }

    private void TextFieldChangedEventHandler(object sender, TransformComponentChangeEventArgs e) =>
        ControlEvent?.Invoke(sender, e);
}
