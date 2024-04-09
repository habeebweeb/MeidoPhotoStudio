using System;
using System.Collections.Generic;
using System.Linq;

using MeidoPhotoStudio.Plugin.Core.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class CharacterDragHandleInputService : IDragHandleInputHandler<ICharacterDragHandleController>
{
    private readonly Dictionary<Type, IDragHandleInputHandler<ICharacterDragHandleController>> inputHandlers = [];
    private readonly GeneralDragHandleInputHandler generalDragHandleInputService;

    public CharacterDragHandleInputService(
        GeneralDragHandleInputHandler generalDragHandleInputService,
        params IDragHandleInputHandler<ICharacterDragHandleController>[] inputHandlers)
    {
        this.generalDragHandleInputService = generalDragHandleInputService ?? throw new ArgumentNullException(nameof(generalDragHandleInputService));

        _ = inputHandlers ?? throw new ArgumentNullException(nameof(inputHandlers));

        foreach (var inputHandler in inputHandlers)
        {
            var interfaces = inputHandler.GetType().GetInterfaces();

            var genericArgument = interfaces.Where(@interface => @interface.IsGenericType)
                .SelectMany(@interface => @interface.GetGenericArguments())
                .FirstOrDefault(argument => argument != typeof(ICharacterDragHandleController));

            if (genericArgument is null)
                continue;

            this.inputHandlers[genericArgument] = inputHandler;
        }
    }

    public bool Active =>
        true;

    public void AddController(ICharacterDragHandleController controller)
    {
        _ = controller ?? throw new ArgumentNullException(nameof(controller));

        if (controller is CharacterGeneralDragHandleController generalController)
        {
            generalDragHandleInputService.AddController(generalController);

            return;
        }

        if (!inputHandlers.ContainsKey(controller.GetType()))
            throw new ArgumentException(nameof(controller), $"{controller.GetType()} is not supported");

        inputHandlers[controller.GetType()].AddController(controller);
    }

    public void RemoveController(ICharacterDragHandleController controller)
    {
        _ = controller ?? throw new ArgumentNullException(nameof(controller));

        if (controller is CharacterGeneralDragHandleController generalController)
        {
            generalDragHandleInputService.RemoveController(generalController);

            return;
        }

        if (!inputHandlers.ContainsKey(controller.GetType()))
            throw new ArgumentException(nameof(controller), $"{controller.GetType()} is not supported");

        inputHandlers[controller.GetType()].RemoveController(controller);
    }

    public void CheckInput()
    {
        foreach (var inputHandler in inputHandlers.Values.Where(inputHandler => inputHandler.Active))
            inputHandler.CheckInput();
    }
}
