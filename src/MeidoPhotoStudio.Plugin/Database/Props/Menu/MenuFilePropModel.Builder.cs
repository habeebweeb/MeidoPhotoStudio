namespace MeidoPhotoStudio.Database.Props.Menu;

/// <summary>MenuFile builder.</summary>
public partial class MenuFilePropModel
{
    public class Builder
    {
        private readonly string menuFilename;
        private readonly bool gameMenu;

        private string name = string.Empty;
        private MPN mpn;
        private string iconFilename;
        private float priority;
        private string modelFilename;
        private List<MaterialChange> materialChanges;
        private List<ModelAnimation> modelAnimations;
        private List<ModelMaterialAnimation> modelMaterialAnimations;

        public Builder(string menuFilename, bool gameMenu)
        {
            if (string.IsNullOrEmpty(menuFilename))
                throw new ArgumentException($"'{nameof(menuFilename)}' cannot be null or empty.", nameof(menuFilename));

            this.gameMenu = gameMenu;
            this.menuFilename = menuFilename;
        }

        public Builder WithName(string name)
        {
            this.name = name;

            return this;
        }

        public Builder WithMpn(MPN mpn)
        {
            this.mpn = mpn;

            return this;
        }

        public Builder WithIconFilename(string iconFilename)
        {
            this.iconFilename = iconFilename;

            return this;
        }

        public Builder WithPriority(float priority)
        {
            this.priority = priority;

            return this;
        }

        public Builder WithModelFilename(string modelFilename)
        {
            this.modelFilename = modelFilename;

            return this;
        }

        public Builder AddMaterialChange(MaterialChange materialChange)
        {
            materialChanges ??= [];
            materialChanges.Add(materialChange);

            return this;
        }

        public Builder AddModelAnime(ModelAnimation modelAnimation)
        {
            modelAnimations ??= [];
            modelAnimations.Add(modelAnimation);

            return this;
        }

        public Builder AddModelMaterialAnimation(ModelMaterialAnimation modelMaterialAnimation)
        {
            modelMaterialAnimations ??= [];
            modelMaterialAnimations.Add(modelMaterialAnimation);

            return this;
        }

        public MenuFilePropModel Build() =>
            new(menuFilename, gameMenu)
            {
                OriginalName = name,
                Name = name,
                CategoryMpn = mpn,
                IconFilename = iconFilename,
                Priority = priority,
                ModelFilename = modelFilename,
                MaterialChanges = materialChanges ?? Enumerable.Empty<MaterialChange>(),
                ModelAnimations = modelAnimations ?? Enumerable.Empty<ModelAnimation>(),
                ModelMaterialAnimations = modelMaterialAnimations ?? Enumerable.Empty<ModelMaterialAnimation>(),
            };
    }
}
