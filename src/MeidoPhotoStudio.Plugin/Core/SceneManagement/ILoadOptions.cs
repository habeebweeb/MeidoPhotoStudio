namespace MeidoPhotoStudio.Plugin.Core.SceneManagement;

public interface ILoadOptions : IEnumerable<LoadOption>
{
    event EventHandler<LoadOptionsChangedEventArgs> AddedOption;

    event EventHandler<LoadOptionsChangedEventArgs> RemovedOption;

    LoadOption this[string tag] { get; }

    bool TryGetOption(string tag, out LoadOption option);

    bool OptionEnabled(string tag);
}
