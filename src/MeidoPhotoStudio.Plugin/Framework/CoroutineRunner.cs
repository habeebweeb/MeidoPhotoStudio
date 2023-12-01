using System;
using System.Collections;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Framework;

public class CoroutineRunner
{
    private static GameObject coroutineRunnerParent;

    private readonly Func<IEnumerator> coroutine;

    private string name;

    public CoroutineRunner(Func<IEnumerator> coroutine) =>
        this.coroutine = coroutine ?? throw new ArgumentNullException(nameof(coroutine));

    public string Name
    {
        get => name;
        set => name = string.IsNullOrEmpty(value) ? "[Coroutine Runner]" : value;
    }

    private static GameObject CoroutineRunnerParent
    {
        get
        {
            if (coroutineRunnerParent)
                return coroutineRunnerParent;

            coroutineRunnerParent = new("[MPS Coroutine Runner Parent]");

            UnityEngine.Object.DontDestroyOnLoad(coroutineRunnerParent);

            return coroutineRunnerParent;
        }
    }

    public void Start()
    {
        var coroutineContainer = new GameObject(Name)
        {
            hideFlags = HideFlags.HideAndDontSave,
        };

        coroutineContainer.transform.SetParent(CoroutineRunnerParent.transform);

        var coroutineRunner = coroutineContainer.AddComponent<CoroutineBehaviour>();

        coroutineRunner.StartCoroutine(RunCoroutine());

        IEnumerator RunCoroutine()
        {
            yield return coroutine();

            UnityEngine.Object.Destroy(coroutineContainer);
        }
    }

    private class CoroutineBehaviour : MonoBehaviour
    {
    }
}