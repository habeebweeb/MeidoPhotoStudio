using MeidoPhotoStudio.Plugin.Core.UndoRedo;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin.Core.Props;

public class PropUndoRedoService
{
    private readonly PropService propService;
    private readonly UndoRedoService undoRedoService;
    private readonly Dictionary<int, PropUndoRedoController> undoRedoControllerByID = [];
    private readonly Dictionary<int, PropController> propControllers = [];
    private readonly Dictionary<PropController, PropUndoRedoController> undoRedoControllers = [];
    private readonly PropResolver propResolver;

    private bool changeFromUndoRedo;
    private PropState undoRedoState;

    public PropUndoRedoService(PropService propService, UndoRedoService undoRedoService)
    {
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        this.undoRedoService = undoRedoService ?? throw new ArgumentNullException(nameof(undoRedoService));

        this.propService.AddedProp += OnPropAdded;
        this.propService.RemovingProp += OnPropRemoving;

        propResolver = new(this);
    }

    public PropUndoRedoController this[PropController propController] =>
        propController is null
            ? throw new ArgumentNullException(nameof(propController))
            : undoRedoControllers[propController];

    private void OnPropAdded(object sender, PropServiceEventArgs e)
    {
        if (changeFromUndoRedo)
            ApplyUndoRedoState(e.PropController, undoRedoState);
        else
            PushNewUndoRedo(e.PropController);

        void ApplyUndoRedoState(PropController propController, PropState undoRedoState)
        {
            var (id, backup) = undoRedoState;
            var undoRedoController = new PropUndoRedoController(id, propResolver, propController, undoRedoService);

            undoRedoControllerByID[id] = undoRedoControllers[propController] = undoRedoController;
            propControllers[id] = propController;

            backup.Apply(propController);
        }

        void PushNewUndoRedo(PropController propController)
        {
            var model = propController.PropModel;
            var id = propController.GameObject.GetInstanceID();
            var undoRedoController = new PropUndoRedoController(id, propResolver, propController, undoRedoService);
            var propState = new PropState(id, PropBackup.Create(propController));

            undoRedoControllerByID[id] = undoRedoControllers[propController] = undoRedoController;
            propControllers[id] = propController;

            undoRedoService.Push(new UndoRedoAction(UndoAdd, RedoAdd));

            void UndoAdd()
            {
                changeFromUndoRedo = true;

                var controller = propControllers[id];

                propService.Remove(controller);

                changeFromUndoRedo = false;
            }

            void RedoAdd()
            {
                changeFromUndoRedo = true;

                undoRedoState = propState;

                propService.Add(model);

                changeFromUndoRedo = false;
            }
        }
    }

    private void OnPropRemoving(object sender, PropServiceEventArgs e)
    {
        if (changeFromUndoRedo)
            return;

        PushNewUndoRedo(e.PropController);

        void PushNewUndoRedo(PropController propController)
        {
            var model = propController.PropModel;
            var id = undoRedoControllers[propController].ID;

            undoRedoControllers.Remove(propController);
            undoRedoControllerByID.Remove(id);
            propControllers.Remove(id);

            var propState = new PropState(id, PropBackup.Create(propController));

            undoRedoService.Push(new UndoRedoAction(UndoDelete, RedoDelete));

            void UndoDelete()
            {
                changeFromUndoRedo = true;

                undoRedoState = propState;

                propService.Add(model);

                changeFromUndoRedo = false;
            }

            void RedoDelete()
            {
                changeFromUndoRedo = true;

                var controller = propControllers[id];

                propService.Remove(controller);

                changeFromUndoRedo = false;
            }
        }
    }

    private readonly record struct PropState(int ID, PropBackup Backup);

    private readonly record struct PropBackup(TransformBackup TransformBackup, bool ShadowCasting, bool Visible, Dictionary<string, float> ShapeKeys)
    {
        public static PropBackup Create(PropController propController)
        {
            _ = propController ?? throw new ArgumentNullException(nameof(propController));

            return new(
                new(propController.GameObject.transform),
                propController.ShadowCasting,
                propController.Visible,
                propController.ShapeKeyController?.ToDictionary(kvp => kvp.HashKey, kvp => kvp.BlendValue) ?? []);
        }

        public void Apply(PropController propController)
        {
            _ = propController ?? throw new ArgumentNullException(nameof(propController));

            TransformBackup.Apply(propController.GameObject.transform);

            propController.ShadowCasting = ShadowCasting;
            propController.Visible = Visible;

            if (propController.ShapeKeyController is null)
                return;

            foreach (var (hashKey, blendValue) in ShapeKeys)
                propController.ShapeKeyController[hashKey] = blendValue;
        }
    }

    public class PropResolver(PropUndoRedoService propUndoRedoService)
    {
        public PropController ResolveProp(int id) =>
            propUndoRedoService.propControllers[id];
    }
}
