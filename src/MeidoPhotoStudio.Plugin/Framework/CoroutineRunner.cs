namespace MeidoPhotoStudio.Plugin.Framework;

public class CoroutineRunner(Func<IEnumerator> coroutine)
{
    private static GameObject coroutineRunnerParent;

    private readonly Func<IEnumerator> coroutine = coroutine ?? throw new ArgumentNullException(nameof(coroutine));

    private string name;

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

            Object.DontDestroyOnLoad(coroutineRunnerParent);

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
            IEnumerator result;

            try
            {
                result = coroutine();
            }
            catch
            {
                Object.Destroy(coroutineContainer);

                throw;
            }

            yield return result;

            Object.Destroy(coroutineContainer);
        }
    }

    internal static void DestroyParent()
    {
        if (!coroutineRunnerParent)
            return;

        Object.Destroy(coroutineRunnerParent);
    }

    private class CoroutineBehaviour : MonoBehaviour
    {
    }
}
