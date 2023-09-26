using System;
using System.Collections.Generic;
using System.Linq;

namespace MeidoPhotoStudio.Plugin;

public class DragPointMeidoInputService : IDragPointInputRepository<DragPointMeido>
{
    private readonly Dictionary<Type, IDragPointInputRepository<DragPointMeido>> dragpointMeidoInputServices = new();

    public DragPointMeidoInputService(params IDragPointInputRepository<DragPointMeido>[] services)
    {
        if (services is null)
            throw new ArgumentNullException(nameof(services));

        foreach (var service in services)
        {
            var interfaces = service.GetType().GetInterfaces();

            var genericArgument = interfaces.Where(@interface => @interface.IsGenericType)
                .SelectMany(@interface => @interface.GetGenericArguments())
                .First(argument => argument != typeof(DragPointMeido));

            dragpointMeidoInputServices[genericArgument] = service;
        }
    }

    public bool Active { get; } = true;

    public void AddDragHandle(DragPointMeido dragHandle)
    {
        if (dragHandle == null)
            throw new ArgumentNullException(nameof(dragHandle));

        if (!dragpointMeidoInputServices.ContainsKey(dragHandle.GetType()))
            throw new ArgumentException(nameof(dragHandle), $"{dragHandle.GetType()} is not supported");

        dragpointMeidoInputServices[dragHandle.GetType()].AddDragHandle(dragHandle);
    }

    public void CheckInput()
    {
        foreach (var service in dragpointMeidoInputServices.Values.Where(service => service.Active))
            service.CheckInput();
    }

    public void RemoveDragHandle(DragPointMeido dragHandle)
    {
        if (dragHandle == null)
            throw new ArgumentNullException(nameof(dragHandle));

        if (!dragpointMeidoInputServices.ContainsKey(dragHandle.GetType()))
            throw new ArgumentException(nameof(dragHandle), $"{dragHandle.GetType()} is not supported");

        dragpointMeidoInputServices[dragHandle.GetType()].RemoveDragHandle(dragHandle);
    }
}
