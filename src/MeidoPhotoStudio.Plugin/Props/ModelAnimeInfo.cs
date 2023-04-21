using System.Collections.Generic;
using System.IO;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class ModelAnimeInfo
{
    private List<ModelAnime> anime;
    private List<ModelMaterialAnime> materialAnime;

    public List<ModelAnime> Anime =>
        anime ??= new();

    public List<ModelMaterialAnime> MaterialAnime =>
        materialAnime ??= new();

    public void ApplyModelAnime(GameObject model)
    {
        if (anime is null)
            return;

        var animation = model.GetOrAddComponent<Animation>();

        // TODO: I think this will just override the previous animations if there are multiple animations.
        // Find a mod that has animations for different slots.
        foreach (var modelAnime in anime)
        {
            LoadAnimation(animation, modelAnime.AnimationName);
            PlayAnimation(animation, modelAnime.AnimationName, modelAnime.Loop);
        }

        Animation LoadAnimation(Animation animation, string animationName)
        {
            if (!animation.GetClip(animationName))
            {
                var animationFilename = animationName;

                if (string.IsNullOrEmpty(Path.GetExtension(animationName)))
                    animationFilename += ".anm";

                var animationClip = ImportCM.LoadAniClipNative(GameUty.FileSystem, animationFilename, true, true, true);

                if (!animationClip)
                    return animation;

                animation.Stop();
                animation.AddClip(animationClip, animationName);
                animation.clip = animationClip;
                animation.playAutomatically = true;
            }

            animation.Stop();

            return animation;
        }

        void PlayAnimation(Animation animation, string animationName, bool loop)
        {
            if (!animation)
                return;

            animation.Stop();
            animation.wrapMode = loop ? WrapMode.Loop : WrapMode.Once;

            if (!animation.GetClip(animationName))
                return;

            animation[animationName].time = 0f;
            animation.Play(animationName);
        }
    }

    public void ApplyModelAnimeMaterial(GameObject model)
    {
        if (materialAnime is null)
            return;

        var renderer = model.GetComponentInChildren<Renderer>();

        if (!renderer)
            return;

        // TODO: This doesn't make sense. This will just override the previous material animations.
        // TODO: Find a mod that has multiple "animematerial" tags to test.
        foreach (var modelMaterialAnime in materialAnime)
        {
            var materialAnimator = renderer.gameObject.GetOrAddComponent<MaterialAnimator>();

            materialAnimator.m_nMateNo = modelMaterialAnime.MaterialNumber;
            materialAnimator.Init();
        }
    }
}
