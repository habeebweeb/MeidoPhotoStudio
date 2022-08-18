using System;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class KeyRebindButton : BaseControl
{
    private readonly Button button;

    private bool listening;
    private KeyCode keyCode;

    public KeyRebindButton(KeyCode code)
    {
        button = new(code.ToString());
        button.ControlEvent += (_, _) =>
            StartListening();
    }

    public KeyCode KeyCode
    {
        get => keyCode;
        set
        {
            keyCode = value;
            button.Label = keyCode.ToString();
        }
    }

    public override void Draw(params GUILayoutOption[] layoutOptions)
    {
        var buttonStyle = new GUIStyle(GUI.skin.button);

        Draw(buttonStyle, layoutOptions);
    }

    public void Draw(GUIStyle buttonStyle, params GUILayoutOption[] layoutOptions)
    {
        GUI.enabled = !listening && !InputManager.Listening;
        button.Draw(buttonStyle, layoutOptions);
        GUI.enabled = true;
    }

    private void StartListening()
    {
        listening = true;
        button.Label = string.Empty;
        InputManager.StartListening();
        InputManager.KeyChange += KeyChange;
    }

    private void KeyChange(object sender, EventArgs args)
    {
        listening = false;

        KeyCode = InputManager.CurrentKeyCode is not KeyCode.Escape ? InputManager.CurrentKeyCode : KeyCode;

        InputManager.KeyChange -= KeyChange;

        OnControlEvent(EventArgs.Empty);
    }
}
