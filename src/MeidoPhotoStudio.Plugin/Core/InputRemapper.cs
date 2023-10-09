using System;
using System.Collections.Generic;
using System.Linq;

using MeidoPhotoStudio.Plugin.Input;
using MeidoPhotoStudio.Plugin.Service.Input;
using UnityEngine;

using UInput = UnityEngine.Input;

namespace MeidoPhotoStudio.Plugin.Core;

public class InputRemapper : MonoBehaviour
{
    private const KeyCode UpperKeyCode = KeyCode.RightApple;

    private static readonly KeyCode[] ValidKeyCodes = ((KeyCode[])Enum.GetValues(typeof(KeyCode)))
        .Where(keyCode => keyCode is not KeyCode.None and < UpperKeyCode)
        .ToArray();

    private BindingType bindingType;
    private Action<KeyboardShortcut> newShortcutCallback;
    private Action<KeyboardHotkey> newHotkeyCallback;

    private enum BindingType
    {
        Shortcut,
        Hotkey,
    }

    public bool Listening { get; private set; }

    public InputPollingService InputPollingService { get; set; }

    public void ListenForShortcut(Action<KeyboardShortcut> newShortcutCallback)
    {
        this.newShortcutCallback = newShortcutCallback ?? throw new ArgumentNullException(nameof(newShortcutCallback));
        bindingType = BindingType.Shortcut;

        StartListening();
    }

    public void ListenForHotkey(Action<KeyboardHotkey> newHotkeyCallback)
    {
        this.newHotkeyCallback = newHotkeyCallback ?? throw new ArgumentNullException(nameof(newHotkeyCallback));
        bindingType = BindingType.Hotkey;

        StartListening();
    }

    public void Cancel() =>
        StopListening();

    private void Start()
    {
        if (!InputPollingService)
            throw new InvalidOperationException($"{nameof(InputPollingService)} cannot be null");

        enabled = false;
    }

    private void Update()
    {
        if (!Listening)
            return;

        foreach (var keyCode in ValidKeyCodes)
        {
            if (!UInput.GetKeyUp(keyCode))
                continue;

            if (bindingType is BindingType.Shortcut)
                ResolveNewShortcut(keyCode, ValidKeyCodes.Where(UInput.GetKey));
            else
                ResolveNewHotkey(new[] { keyCode }.Concat(ValidKeyCodes.Where(UInput.GetKey)));
        }
    }

    private void ResolveNewShortcut(KeyCode mainKey, IEnumerable<KeyCode> modifiers)
    {
        newShortcutCallback(new KeyboardShortcut(mainKey, modifiers.ToArray()));
        StopListening();
    }

    private void ResolveNewHotkey(IEnumerable<KeyCode> keys)
    {
        newHotkeyCallback(new KeyboardHotkey(keys.ToArray()));
        StopListening();
    }

    private void StartListening()
    {
        InputPollingService.enabled = false;
        enabled = true;
        Listening = true;

        UInput.ResetInputAxes();
    }

    private void StopListening()
    {
        InputPollingService.enabled = true;
        enabled = false;
        Listening = false;

        UInput.ResetInputAxes();
    }
}
