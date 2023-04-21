using System.Collections.Generic;

namespace MeidoPhotoStudio.Plugin;

public class ModelInfo
{
    private List<MaterialChange> materialChanges;
    private ModelAnimeInfo animeInfo;

    public List<MaterialChange> MaterialChanges =>
        materialChanges ??= new();

    public ModelAnimeInfo AnimeInfo =>
        animeInfo ??= new();

    public string ModelFile { get; set; }
}
