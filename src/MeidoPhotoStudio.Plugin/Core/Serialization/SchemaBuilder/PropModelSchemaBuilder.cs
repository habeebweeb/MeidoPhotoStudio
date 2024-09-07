using MeidoPhotoStudio.Plugin.Core.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Database.Props.Menu;
using MeidoPhotoStudio.Plugin.Core.Schema.Props;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class PropModelSchemaBuilder
    : ISchemaBuilder<IPropModelSchema, IPropModel>,
    ISchemaBuilder<BackgroundPropModelSchema, BackgroundPropModel>,
    ISchemaBuilder<DeskPropModelSchema, DeskPropModel>,
    ISchemaBuilder<MyRoomPropModelSchema, MyRoomPropModel>,
    ISchemaBuilder<OtherPropModelSchema, OtherPropModel>,
    ISchemaBuilder<PhotoBgPropModelSchema, PhotoBgPropModel>,
    ISchemaBuilder<MenuFilePropModelSchema, MenuFilePropModel>
{
    public IPropModelSchema Build(IPropModel value) =>
        value switch
        {
            BackgroundPropModel backgroundPropModel => Build(backgroundPropModel),
            DeskPropModel deskPropModel => Build(deskPropModel),
            MyRoomPropModel myRoomPropModel => Build(myRoomPropModel),
            OtherPropModel otherPropModel => Build(otherPropModel),
            PhotoBgPropModel photoBgPropModel => Build(photoBgPropModel),
            MenuFilePropModel menuFile => Build(menuFile),
            _ => throw new NotImplementedException($"'{value.GetType()}' is not implemented"),
        };

    public BackgroundPropModelSchema Build(BackgroundPropModel propModel) =>
        new()
        {
            ID = propModel.ID,
        };

    public DeskPropModelSchema Build(DeskPropModel propModel) =>
        new()
        {
            ID = propModel.ID,
        };

    public MyRoomPropModelSchema Build(MyRoomPropModel propModel) =>
        new()
        {
            ID = propModel.ID,
        };

    public OtherPropModelSchema Build(OtherPropModel propModel) =>
        new()
        {
            AssetName = propModel.AssetName,
        };

    public PhotoBgPropModelSchema Build(PhotoBgPropModel propModel) =>
        new()
        {
            ID = propModel.ID,
        };

    public MenuFilePropModelSchema Build(MenuFilePropModel propModel) =>
        new()
        {
            ID = propModel.ID,
            Filename = propModel.Filename,
        };
}
