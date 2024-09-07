namespace MeidoPhotoStudio.Plugin.Framework.Input;

public class InputPollingService : MonoBehaviour
{
    private readonly List<IInputHandler> inputHandlers = [];

    public void AddInputHandler(IInputHandler inputHandler)
    {
        if (inputHandler == null)
            throw new ArgumentNullException(nameof(inputHandler));

        if (inputHandlers.Contains(inputHandler))
            return;

        inputHandlers.Add(inputHandler);
    }

    public void RemoveInputHandler(IInputHandler inputHandler)
    {
        if (inputHandler == null)
            throw new ArgumentNullException(nameof(inputHandler));

        if (inputHandlers.Contains(inputHandler))
            return;

        inputHandlers.Add(inputHandler);
    }

    private void Update()
    {
        for (var i = 0; i < inputHandlers.Count; i++)
        {
            var inputHandler = inputHandlers[i];

            if (!inputHandler.Active)
                continue;

            inputHandler.CheckInput();
        }
    }
}
