using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using MeidoPhotoStudio.Plugin.Framework.Extensions;
using Newtonsoft.Json;

namespace MeidoPhotoStudio.Database.Character;

public class GameAnimationRepository : IEnumerable<GameAnimationModel>
{
    private readonly string databaseDirectory;

    private Dictionary<string, IList<GameAnimationModel>> animations;

    public GameAnimationRepository(string databaseDirectory)
    {
        if (string.IsNullOrEmpty(databaseDirectory))
            throw new ArgumentException($"'{nameof(databaseDirectory)}' cannot be null or empty.", nameof(databaseDirectory));

        this.databaseDirectory = databaseDirectory;

        InitializeAnimations(databaseDirectory);
    }

    public event EventHandler InitializingAnimations;

    public event EventHandler InitializedAnimations;

    public bool Busy { get; private set; }

    public IEnumerable<string> Categories =>
        Animations.Keys;

    private Dictionary<string, IList<GameAnimationModel>> Animations =>
        Busy
            ? throw new InvalidOperationException()
            : animations;

    public IList<GameAnimationModel> this[string category] =>
        Animations[category];

    public bool ContainsCategory(string category) =>
        Animations.ContainsKey(category);

    public GameAnimationModel GetByID(string id) =>
        this.FirstOrDefault(animation => string.Equals(animation.ID, id, StringComparison.OrdinalIgnoreCase));

    public void Refresh()
    {
        if (Busy)
            throw new InvalidOperationException();

        InitializeAnimations(databaseDirectory);
    }

    public IEnumerator<GameAnimationModel> GetEnumerator() =>
        Animations.Values.SelectMany(list => list).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    private void InitializeAnimations(string databaseDirectory)
    {
        Busy = true;

        InitializingAnimations?.Invoke(this, EventArgs.Empty);

        Task<Dictionary<string, IList<GameAnimationModel>>>.Factory
            .StartNew(() => Initialize(databaseDirectory))
            .ContinueWith(task =>
            {
                animations = task.Result;

                Busy = false;

                InitializedAnimations?.Invoke(this, EventArgs.Empty);
            });

        static Dictionary<string, IList<GameAnimationModel>> Initialize(string databaseDirectory)
        {
            var animations = new List<GameAnimationModel>();

            InitializeMMAnimationss();
            InitializeGameAnimations();

            return animations
                .GroupBy(model => model.Category, model => model)
                .ToDictionary(group => group.Key, group => (IList<GameAnimationModel>)group.ToList().AsReadOnly());

            void InitializeMMAnimationss()
            {
                var animationListPath = Path.Combine(databaseDirectory, "mm_animation_list.json");

                try
                {
                    var animationListJson = File.ReadAllText(animationListPath);
                    var mmAnimations = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(animationListJson);

                    foreach (var (category, animationList) in mmAnimations)
                        animations.AddRange(animationList.Select(animation => new GameAnimationModel(category, animation)));
                }
                catch
                {
                }
            }

            // TODO: These need to be further categorized than ero2 and normal2
            void InitializeGameAnimations()
            {
                var motionList = GameUty.FileSystem.GetList("motion", AFileSystemBase.ListType.AllFile);
                var animationSet = new HashSet<string>(animations.Select(animation => animation.Filename));

                foreach (var path in motionList)
                {
                    if (Path.GetExtension(path) is not ".anm")
                        continue;

                    var file = Path.GetFileNameWithoutExtension(path);

                    if (animationSet.Contains(file))
                        continue;

                    if (file.StartsWith("edit_"))
                    {
                        animations.Add(new("normal", file));
                    }
                    else if (file is not ("dance_cm3d2_001_zoukin" or "dance_cm3d2_001_mop" or "aruki_1_idougo_f"
                        or "sleep2" or "stand_akire2") && !file.EndsWith("_3_") && !file.EndsWith("_5_")
                        && !file.StartsWith("vr_") && !file.StartsWith("dance_mc") && !file.Contains("_kubi_")
                        && !file.Contains("a01_") && !file.Contains("b01_") && !file.Contains("b02_")
                        && !file.EndsWith("_m2") && !file.EndsWith("_m2_once_") && !file.StartsWith("h_")
                        && !file.StartsWith("event_") && !file.StartsWith("man_") && !file.EndsWith("_m")
                        && !file.Contains("_m_") && !file.Contains("_man_"))
                    {
                        if (path.Contains(@"\sex\"))
                            animations.Add(new("ero2", file));
                        else
                            animations.Add(new("normal2", file));
                    }
                }
            }
        }
    }
}
