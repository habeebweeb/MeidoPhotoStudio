using MeidoPhotoStudio.Database.Character;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Service;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class CharacterService(
    CustomMaidSceneService customMaidSceneService,
    EditModeMaidService editModeMaidService,
    TransformWatcher transformWatcher)
    : IEnumerable<CharacterController>, IIndexableCollection<CharacterController>
{
    private const int MaidCount = 12;

    private readonly List<CharacterController> activeCharacters = [];
    private readonly List<CharacterModel> activeCharacterModels = [];
    private readonly Dictionary<CharacterModel, CharacterController> characterControllerCache = [];

    private readonly CustomMaidSceneService customMaidSceneService = customMaidSceneService
        ?? throw new ArgumentNullException(nameof(customMaidSceneService));

    private readonly EditModeMaidService editModeMaidService = editModeMaidService
        ?? throw new ArgumentNullException(nameof(editModeMaidService));

    private readonly TransformWatcher transformWatcher = transformWatcher
        ? transformWatcher : throw new ArgumentNullException(nameof(transformWatcher));

    private bool calling;

    public event EventHandler<CharacterServiceEventArgs> CallingCharacters;

    public event EventHandler<CharacterServiceEventArgs> CalledCharacters;

    public event EventHandler Deactivating;

    public bool Busy =>
        calling || activeCharacters.Any(character => character.Busy);

    public int Count =>
        activeCharacters.Count;

    public IEnumerable<CharacterModel> ActiveCharacterModels =>
        activeCharacterModels;

    public CharacterController this[int index] =>
        GetCharacterController(index);

    public CharacterController GetCharacterController(int index) =>
        (uint)index >= Count
            ? throw new ArgumentOutOfRangeException(nameof(index))
            : activeCharacters[index];

    public CharacterController GetCharacterControllerByID(string id) =>
        string.IsNullOrEmpty(id)
            ? throw new ArgumentException($"'{nameof(id)}' cannot be null or empty.", nameof(id))
            : activeCharacters.FirstOrDefault(character =>
                string.Equals(character.ID, id, StringComparison.OrdinalIgnoreCase));

    public CharacterModel GetCharacterModel(int index) =>
        (uint)index >= Count
            ? throw new ArgumentOutOfRangeException(nameof(index))
            : activeCharacterModels[index];

    public int IndexOf(CharacterController character) =>
        character is null
            ? throw new ArgumentNullException(nameof(character))
            : activeCharacters.IndexOf(character);

    public int IndexOf(CharacterModel character) =>
        character is null
            ? throw new ArgumentNullException(nameof(character))
            : activeCharacterModels.IndexOf(character);

    public IEnumerator<CharacterController> GetEnumerator() =>
        activeCharacters.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public void Call(IEnumerable<CharacterModel> characters)
    {
        if (Busy)
            return;

        var charactersToCall = GetCharactersToCall(characters);

        calling = true;

        CallingCharacters?.Invoke(this, new(charactersToCall));

        UnloadActiveCharacters(charactersToCall);

        UpdateActiveCharacters(charactersToCall);

        CallCharacters(charactersToCall);

        CharacterController[] GetCharactersToCall(IEnumerable<CharacterModel> charactersToCall)
        {
            foreach (var character in charactersToCall.Where(character => !characterControllerCache.ContainsKey(character)))
                characterControllerCache.Add(character, new(character, transformWatcher));

            return charactersToCall.Select(character => characterControllerCache[character]).ToArray();
        }

        void UnloadActiveCharacters(IEnumerable<CharacterController> charactersToCall)
        {
            foreach (var character in activeCharacters.Except(charactersToCall))
                character.Unload();

            var cmg = GameMain.Instance.CharacterMgr;

            Array.Clear(cmg.m_gcActiveMaid, 0, MaidCount);
            Array.Clear(cmg.m_objActiveMaid, 0, MaidCount);
        }

        void UpdateActiveCharacters(IEnumerable<CharacterController> charactersToCall)
        {
            activeCharacters.Clear();
            activeCharacters.AddRange(charactersToCall);

            activeCharacterModels.Clear();
            activeCharacterModels.AddRange(characters);

            var cmg = GameMain.Instance.CharacterMgr;

            foreach (var (index, maid) in charactersToCall.Take(MaidCount).Select(character => character.Maid).WithIndex())
            {
                cmg.m_gcActiveMaid[index] = maid;
                cmg.m_objActiveMaid[index] = maid.gameObject;
            }
        }

        void CallCharacters(CharacterController[] charactersToCall)
        {
            var runner = new CoroutineRunner(Call)
            {
                Name = "[MPS Character Caller]",
            };

#if DEBUG
            runner.Start();
#else
            GameMain.Instance.MainCamera.FadeOut(0.2f, f_bSkipable: false, f_dg: runner.Start);
#endif

            IEnumerator Call()
            {
                yield return new WaitForEndOfFrame();

                foreach (var (index, character) in charactersToCall.WithIndex())
                    character.Load(index);

                yield return new WaitForEndOfFrame();

                calling = false;

                var wait = new WaitForSeconds(0.2f);

                while (Busy)
                    yield return wait;

                yield return new WaitForEndOfFrame();

#if DEBUG
                EmitCharactersCalled();
#else
                GameMain.Instance.MainCamera.FadeIn(0.2f);
                EmitCharactersCalled();
#endif

                void EmitCharactersCalled()
                {
#if DEBUG
                    if (CalledCharacters is null)
                        return;

                    var args = new CharacterServiceEventArgs(charactersToCall);

                    foreach (var callback in CalledCharacters.GetInvocationList())
                    {
                        try
                        {
                            callback.DynamicInvoke(this, args);
                        }
                        catch (Exception e)
                        {
                            Utility.LogError(e);
                        }
                    }
#else
                    CalledCharacters?.Invoke(this, new CharacterServiceEventArgs(charactersToCall));
#endif
                }
            }
        }
    }

    internal void Activate()
    {
        if (!customMaidSceneService.EditScene)
            return;

        Call(new CharacterModel[] { editModeMaidService.OriginalEditingCharacter });
    }

    internal void Deactivate()
    {
        if (Busy)
            return;

        activeCharacters.Clear();

        Deactivating?.Invoke(this, EventArgs.Empty);

        foreach (var loadedCharacter in characterControllerCache.Values)
        {
            var keepLoaded = customMaidSceneService.EditScene && loadedCharacter.CharacterModel == editModeMaidService.OriginalEditingCharacter;

            loadedCharacter.Deactivate(keepLoaded);
        }

        characterControllerCache.Clear();

        var maidStart = customMaidSceneService.EditScene ? 1 : 0;
        var cmg = GameMain.Instance.CharacterMgr;

        Array.Clear(cmg.m_gcActiveMaid, maidStart, MaidCount - maidStart);
        Array.Clear(cmg.m_objActiveMaid, maidStart, MaidCount - maidStart);
    }
}
