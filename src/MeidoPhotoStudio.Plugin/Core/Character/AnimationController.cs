using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Database.Character;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class AnimationController : INotifyPropertyChanged
{
    private readonly CharacterController character;
    private IAnimationModel animation;

    public AnimationController(CharacterController character)
    {
        this.character = character ?? throw new ArgumentNullException(nameof(character));

        this.character.ProcessedCharacterProps += OnCharacterProcessed;
    }

    public event EventHandler ChangedAnimation;

    public event PropertyChangedEventHandler PropertyChanged;

    public IAnimationModel Animation
    {
        get => animation;
        private set
        {
            animation = value;

            RaisePropertyChanged(nameof(Animation));
        }
    }

    public float Time
    {
        get => AnimationState is var state ? state.time % state.length : -1f;

        set
        {
            if (AnimationState is null)
                return;

            var newTime = value % AnimationState.length;

            if (newTime == AnimationState.time)
                return;

            AnimationState.time = newTime;

            if (!AnimationState.enabled)
            {
                AnimationState.enabled = true;

                Body.GetAnimation().Sample();

                AnimationState.enabled = false;
            }

            RaisePropertyChanged(nameof(Time));

            if (!AnimationState.enabled)
                character.IK.Dirty = false;
        }
    }

    public float Length =>
        AnimationState?.length ?? -1f;

    public bool Playing
    {
        get => AnimationState?.enabled ?? false;
        set
        {
            if (AnimationState is null)
                return;

            if (value == AnimationState.enabled)
                return;

            AnimationState.enabled = value;

            RaisePropertyChanged(nameof(Playing));

            if (value)
                character.IK.Dirty = false;
        }
    }

    private Maid Maid =>
        character.Maid;

    private TBody Body =>
        character.Maid.body0;

    private AnimationState AnimationState =>
        Body.isLoadedBody ? Body.GetAnimation()[Body.LastAnimeFN] : null;

    public void Apply(IAnimationModel animation)
    {
        try
        {
            if (animation.Custom)
                ApplyCustomAnimation(animation);
            else
                ApplyGameAnimation(animation);

            Maid.SetAutoTwistAll(true);
        }
        catch
        {
            Utility.LogError($"Could not load animation: {animation.Filename}");

            return;
        }

        Animation = animation;

        character.IK.Dirty = false;

        ChangedAnimation?.Invoke(this, EventArgs.Empty);

        void ApplyGameAnimation(IAnimationModel animation)
        {
            var animationComponents = animation.Filename.Split(',');
            var animationFilename = animationComponents[0];

            if (Path.GetExtension(animationFilename) is not ".anm")
                animationFilename += ".anm";

            var tag = Maid.CrossFade(animationFilename, loop: true, val: 0f);

            if (string.IsNullOrEmpty(tag))
                return;

            var characterAnimation = Maid.GetAnimation();

            characterAnimation.Play();

            if (animationComponents.Length > 1)
            {
                var animationState = characterAnimation[Maid.body0.LastAnimeFN];

                try
                {
                    var time = float.Parse(animationComponents[1]);

                    animationState.enabled = true;
                    animationState.time = time;
                    characterAnimation.Sample();
                    animationState.enabled = false;
                }
                catch
                {
                    Utility.LogWarning($"Time is not a valid format for {animation.Filename}");
                }
            }

            var momiOrPaizuri = animationFilename.Contains("_momi") || animationFilename.Contains("paizuri_");

            Body.SetMuneYureLWithEnable(!momiOrPaizuri);
            Body.SetMuneYureRWithEnable(!momiOrPaizuri);
        }

        void ApplyCustomAnimation(IAnimationModel animation)
        {
            var animationData = File.ReadAllBytes(animation.Filename);
            var hash = Path.GetFileName(animation.Filename).GetHashCode().ToString();

            Body.CrossFade(hash, animationData, loop: true, fade: 0f);
            Body.SetMuneYureLWithEnable(true);
            Body.SetMuneYureRWithEnable(true);
        }
    }

    private void OnCharacterProcessed(object sender, CharacterProcessingEventArgs e)
    {
        if (!e.ChangingSlots.Contains(SafeMpn.GetValue(nameof(MPN.body))))
            return;

        Apply(Animation);
    }

    private void RaisePropertyChanged(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            throw new ArgumentException($"'{nameof(propertyName)}' cannot be null or empty.", nameof(propertyName));

        PropertyChanged?.Invoke(this, new(propertyName));
    }
}
