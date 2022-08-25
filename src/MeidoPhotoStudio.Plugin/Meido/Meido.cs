using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

using UnityEngine;

using static TBody;

using Object = UnityEngine.Object;

namespace MeidoPhotoStudio.Plugin;

public class Meido
{
    public static readonly string DefaultFaceBlendSet = "通常";

    public static readonly string[] FaceKeys =
        new string[24]
        {
            "eyeclose", "eyeclose2", "eyeclose3", "eyebig", "eyeclose6", "eyeclose5", "hitomih",
            "hitomis", "mayuha", "mayuw", "mayuup", "mayuv", "mayuvhalf", "moutha", "mouths",
            "mouthc", "mouthi", "mouthup", "mouthdw", "mouthhe", "mouthuphalf", "tangout",
            "tangup", "tangopen",
        };

    public static readonly string[] FaceToggleKeys =
        new string[12]
        {
            // blush, shade, nose up, tears, drool, teeth
            "hoho2", "shock", "nosefook", "namida", "yodare", "toothoff",

            // cry 1, cry 2, cry 3, blush 1, blush 2, blush 3
            "tear1", "tear2", "tear3", "hohos", "hoho", "hohol",
        };

#pragma warning disable SA1308

    // TODO: Refactor reflection to using private members directly
    private readonly FieldInfo m_eMaskMode = Utility.GetFieldInfo<TBody>("m_eMaskMode");
#pragma warning restore SA1308

    private bool initialized;
    private float[] blendSetValueBackup;
    private bool freeLook;

    public Meido(int stockMaidIndex)
    {
        StockNo = stockMaidIndex;
        Maid = GameMain.Instance.CharacterMgr.GetStockMaid(stockMaidIndex);

        IKManager = new(this);
        IKManager.SelectMaid += (_, args) =>
            OnUpdateMeido(args);
    }

    public event EventHandler<GravityEventArgs> GravityMove;

    public event EventHandler<MeidoUpdateEventArgs> UpdateMeido;

    public enum Curl
    {
        Front,
        Back,
        Shift,
    }

    public enum Mask
    {
        All,
        Underwear,
        Nude,
    }

    public MaskMode CurrentMaskMode =>
        !Body.isLoadedBody ? default : (MaskMode)m_eMaskMode.GetValue(Body);

    public DragPointGravity HairGravityControl { get; private set; }

    public DragPointGravity SkirtGravityControl { get; private set; }

    public Quaternion DefaultEyeRotL { get; private set; }

    public Quaternion DefaultEyeRotR { get; private set; }

    public bool Active { get; private set; }

    public bool IsEditMaid { get; set; }

    public PoseInfo CachedPose { get; private set; } = PoseInfo.DefaultPose;

    public string CurrentFaceBlendSet { get; private set; } = DefaultFaceBlendSet;

    public int Slot { get; private set; }

    public bool Loading { get; private set; }

    public int StockNo { get; }

    public Maid Maid { get; }

    public MeidoDragPointManager IKManager { get; }

    public TBody Body =>
        Maid.body0;

    public Texture2D Portrait =>
        Maid.GetThumIcon();

    public string FirstName =>
        Maid.status.firstName;

    public string LastName =>
        Maid.status.lastName;

    public bool Busy =>
        Maid.IsBusy || Loading;

    public bool CurlingFront =>
        Maid.IsItemChange("skirt", "めくれスカート") || Maid.IsItemChange("onepiece", "めくれスカート");

    public bool CurlingBack =>
        Maid.IsItemChange("skirt", "めくれスカート後ろ") || Maid.IsItemChange("onepiece", "めくれスカート後ろ");

    public bool PantsuShift =>
        Maid.IsItemChange("panz", "パンツずらし") || Maid.IsItemChange("mizugi", "パンツずらし");

    public bool HairGravityActive
    {
        get => HairGravityControl.Active;
        set
        {
            if (HairGravityControl.Valid)
                HairGravityControl.gameObject.SetActive(value);
        }
    }

    public bool SkirtGravityActive
    {
        get => SkirtGravityControl.Active;
        set
        {
            if (SkirtGravityControl.Valid)
                SkirtGravityControl.gameObject.SetActive(value);
        }
    }

    public bool FreeLook
    {
        get => freeLook;
        set
        {
            if (freeLook == value)
                return;

            freeLook = value;
            Body.trsLookTarget = freeLook ? null : GameMain.Instance.MainCamera.transform;

            OnUpdateMeido();
        }
    }

    public bool HeadToCam
    {
        get => Body.isLoadedBody && Body.boHeadToCam;
        set
        {
            if (!Body.isLoadedBody || HeadToCam == value)
                return;

            Body.HeadToCamPer = 0f;
            Body.boHeadToCam = value;

            if (!HeadToCam && !EyeToCam)
                FreeLook = false;

            OnUpdateMeido();
        }
    }

    public bool EyeToCam
    {
        get => Body.isLoadedBody && Body.boEyeToCam;
        set
        {
            if (!Body.isLoadedBody || EyeToCam == value)
                return;

            Body.boEyeToCam = value;

            if (!HeadToCam && !EyeToCam)
                FreeLook = false;

            OnUpdateMeido();
        }
    }

    public bool Stop
    {
        get => !Body.isLoadedBody || !Maid.GetAnimation().isPlaying;
        set
        {
            if (!Body.isLoadedBody || value == Stop)
                return;

            if (value)
            {
                Maid.GetAnimation().Stop();
            }
            else
            {
                Body.boEyeToCam = true;
                Body.boHeadToCam = true;
                SetPose(CachedPose);
            }

            OnUpdateMeido();
        }
    }

    public bool IK
    {
        get => IKManager.Active;
        set
        {
            if (value == IKManager.Active)
                return;

            IKManager.Active = value;
        }
    }

    public bool Bone
    {
        get => IKManager.IsBone;
        set
        {
            if (value == Bone)
                return;

            IKManager.IsBone = value;
            OnUpdateMeido();
        }
    }

    public void Load(int slot)
    {
        if (Busy)
            return;

        Slot = slot;

        if (Active)
            return;

        FreeLook = false;
        Maid.Visible = true;
        Body.boHeadToCam = true;
        Body.boEyeToCam = true;
        Body.SetBoneHitHeightY(-1000f);

        if (!Body.isLoadedBody)
        {
            Maid.DutPropAll();
            Maid.AllProcPropSeqStart();
        }

        StartLoad(OnBodyLoad);
    }

    public void Unload()
    {
        if (Body.isLoadedBody && Maid.Visible)
        {
            DetachAllMpnAttach();

            Body.jbMuneL.enabled = true;
            Body.jbMuneR.enabled = true;

            Body.quaDefEyeL = DefaultEyeRotL;
            Body.quaDefEyeR = DefaultEyeRotR;

            if (HairGravityControl)
            {
                HairGravityControl.Move -= OnGravityEvent;
                HairGravityActive = false;
            }

            if (SkirtGravityControl)
            {
                SkirtGravityControl.Move -= OnGravityEvent;
                SkirtGravityActive = false;
            }

            ApplyGravity(Vector3.zero, skirt: false);
            ApplyGravity(Vector3.zero, skirt: true);

            SetFaceBlendSet(DefaultFaceBlendSet);
        }

        AllProcPropSeqStartPatcher.SequenceStart -= ReinitializeBody;

        Body.MuneYureL(1f);
        Body.MuneYureR(1f);

        Body.SetMaskMode(MaskMode.None);
        Body.SetBoneHitHeightY(0f);

        Maid.Visible = false;

        IKManager.Destroy();

        Active = false;
    }

    public void Deactivate()
    {
        Unload();

        if (HairGravityControl)
            Object.Destroy(HairGravityControl.gameObject);

        if (SkirtGravityControl)
            Object.Destroy(SkirtGravityControl.gameObject);

        Maid.SetPos(Vector3.zero);
        Maid.SetRot(Vector3.zero);
        Maid.SetPosOffset(Vector3.zero);
        Body.transform.localScale = Vector3.one;
        Maid.ResetAll();
        Maid.MabatakiUpdateStop = false;
        Maid.ActiveSlotNo = -1;
    }

    public void SetPose(PoseInfo poseInfo)
    {
        if (!Body.isLoadedBody)
            return;

        var pose = poseInfo.Pose;
        var custom = poseInfo.CustomPose;

        if (custom)
        {
            var poseFilename = Path.GetFileNameWithoutExtension(pose);

            try
            {
                var poseBuffer = File.ReadAllBytes(pose);
                var hash = Path.GetFileName(pose).GetHashCode().ToString();

                Body.CrossFade(hash, poseBuffer, loop: true, fade: 0f);
            }
            catch (Exception e) when (e is DirectoryNotFoundException or FileNotFoundException)
            {
                Utility.LogWarning($"Could not open '{poseFilename}' because {e.Message}");
                Constants.InitializeCustomPoses();
                SetDefaultPose();
                OnUpdateMeido();

                return;
            }
            catch (Exception e)
            {
                Utility.LogWarning($"Could not apply pose '{poseFilename}' because {e.Message}");
                SetDefaultPose();
                OnUpdateMeido();

                return;
            }

            SetMune(true, left: true);
            SetMune(true, left: false);
        }
        else
        {
            var poseComponents = pose.Split(',');
            var poseFilename = poseComponents[0] + ".anm";

            var tag = Maid.CrossFade(poseFilename, loop: true, val: 0f);

            if (string.IsNullOrEmpty(tag))
            {
                Utility.LogWarning($"Pose could not be loaded: {poseFilename}");
                SetDefaultPose();

                return;
            }

            Maid.GetAnimation().Play();

            if (poseComponents.Length > 1)
            {
                var animation = Maid.GetAnimation()[poseFilename];
                var time = float.Parse(poseComponents[1]);

                animation.time = time;
                animation.speed = 0f;
            }

            SetPoseMune(poseFilename);
        }

        Maid.SetAutoTwistAll(true);

        CachedPose = poseInfo;

        void SetDefaultPose() =>
            SetPose(PoseInfo.DefaultPose);

        void SetPoseMune(string pose)
        {
            var momiOrPaizuri = pose.Contains("_momi") || pose.Contains("paizuri_");

            SetMune(!momiOrPaizuri, left: true);
            SetMune(!momiOrPaizuri, left: false);
        }
    }

    public KeyValuePair<bool, bool> SetFrameBinary(byte[] poseBuffer) =>
        GetCacheBoneData().SetFrameBinary(poseBuffer);

    public void CopyPose(Meido fromMeido)
    {
        Stop = true;

        SetFrameBinary(fromMeido.SerializePose(frameBinary: true));
        SetMune(fromMeido.Body.GetMuneYureL() is not 0f, left: true);
        SetMune(fromMeido.Body.GetMuneYureR() is not 0f, left: false);
    }

    public void SetMune(bool enabled, bool left = false)
    {
        var value = enabled ? 1f : 0f;

        if (left)
        {
            Body.MuneYureL(value);
            Body.jbMuneL.enabled = enabled;
        }
        else
        {
            Body.MuneYureR(value);
            Body.jbMuneR.enabled = enabled;
        }
    }

    public void SetHandPreset(string filename, bool right)
    {
        var faceFilename = Path.GetFileNameWithoutExtension(filename);

        try
        {
            var handDocument = XDocument.Load(filename);
            var handElement = handDocument.Element("FingerData");

            if (handElement?.Elements().Any(element => element?.IsEmpty ?? true) ?? true)
            {
                Utility.LogWarning($"{faceFilename}: Could not apply hand preset because it is invalid.");

                return;
            }

            Stop = true;

            var rightData = bool.Parse(handElement.Element("RightData").Value);
            var base64Data = handElement.Element("BinaryData").Value;

            var handData = Convert.FromBase64String(base64Data);

            IKManager.DeserializeHand(handData, right, rightData != right);
        }
        catch (System.Xml.XmlException e)
        {
            Utility.LogWarning($"{faceFilename}: Hand preset data is malformed because {e.Message}");
        }
        catch (Exception e) when (e is DirectoryNotFoundException or FileNotFoundException)
        {
            Utility.LogWarning($"{faceFilename}: Could not open hand preset because {e.Message}");
            Constants.InitializeHandPresets();
        }
        catch (Exception e)
        {
            Utility.LogWarning($"{faceFilename}: Could not parse hand preset because {e.Message}");
        }
    }

    public byte[] SerializePose(bool frameBinary = false)
    {
        var cache = GetCacheBoneData();
        var muneL = Body.GetMuneYureL() is 0f;
        var muneR = Body.GetMuneYureR() is 0f;

        return frameBinary
            ? cache.GetFrameBinary(muneL, muneR)
            : cache.GetAnmBinary(true, true);
    }

    public Dictionary<string, float> SerializeFace()
    {
        var faceData = new Dictionary<string, float>();

        foreach (var hash in FaceKeys.Concat(FaceToggleKeys))
        {
            try
            {
                var value = GetFaceBlendValue(hash);

                faceData.Add(hash, value);
            }
            catch
            {
                // Ignored
            }
        }

        return faceData;
    }

    public void SetFaceBlendSet(string blendSet)
    {
        if (blendSet.StartsWith(Constants.CustomFacePath))
        {
            var blendSetFileName = Path.GetFileNameWithoutExtension(blendSet);

            try
            {
                var faceDocument = XDocument.Load(blendSet, LoadOptions.SetLineInfo);
                var faceDataElement = faceDocument.Element("FaceData");

                if (faceDataElement?.IsEmpty ?? true)
                {
                    Utility.LogWarning($"{blendSetFileName}: Could not apply face preset because it is invalid.");

                    return;
                }

                var hashKeys = new HashSet<string>(FaceKeys.Concat(FaceToggleKeys));

                foreach (var element in faceDataElement.Elements())
                {
                    System.Xml.IXmlLineInfo info = element;

                    var line = info.HasLineInfo() ? info.LineNumber : -1;
                    string key;

                    if ((key = (string)element.Attribute("name")) is null)
                    {
                        Utility.LogWarning($"{blendSetFileName}: Could not read face blend key at line {line}.");

                        continue;
                    }

                    if (!hashKeys.Contains(key))
                    {
                        Utility.LogWarning($"{blendSetFileName}: Invalid face blend key '{key}' at line {line}.");

                        continue;
                    }

                    if (float.TryParse(element.Value, out var value))
                    {
                        try
                        {
                            SetFaceBlendValue(key, value);
                        }
                        catch
                        {
                            // Ignored.
                        }
                    }
                    else
                    {
                        Utility.LogWarning(
                                $"{blendSetFileName}: Could not parse value '{element.Value}' of '{key}' at line {line}");
                    }
                }
            }
            catch (System.Xml.XmlException e)
            {
                Utility.LogWarning($"{blendSetFileName}: Face preset data is malformed because {e.Message}");

                return;
            }
            catch (Exception e) when (e is DirectoryNotFoundException or FileNotFoundException)
            {
                Utility.LogWarning($"{blendSetFileName}: Could not open face preset because {e.Message}");
                Constants.InitializeCustomFaceBlends();

                return;
            }
            catch (Exception e)
            {
                Utility.LogWarning($"{blendSetFileName}: Could not parse face preset because {e.Message}");

                return;
            }
        }
        else
        {
            ApplyBackupBlendSet();

            CurrentFaceBlendSet = blendSet;

            BackupBlendSetValues();

            Maid.FaceAnime(blendSet, 0f);

            var morph = Body.Face.morph;

            foreach (var faceKey in FaceKeys)
            {
                var hash = Utility.GP01FbFaceHash(morph, faceKey);

                if (!morph.Contains(hash))
                    continue;

                var blendIndex = (int)morph.hash[hash];
                var value = faceKey is "nosefook"
                    ? Maid.boNoseFook || morph.boNoseFook ? 1f : 0f
                    : morph.dicBlendSet[CurrentFaceBlendSet][blendIndex];

                morph.SetBlendValues(blendIndex, value);
            }

            morph.FixBlendValues();
        }

        StopBlink();
        OnUpdateMeido();
    }

    public void SetFaceBlendValue(string faceKey, float value)
    {
        var morph = Body.Face.morph;
        var hash = Utility.GP01FbFaceHash(morph, faceKey);

        if (!morph.Contains(hash))
            return;

        var blendIndex = (int)morph.hash[hash];

        if (faceKey is "nosefook")
            Maid.boNoseFook = morph.boNoseFook = value > 0f;
        else
            morph.dicBlendSet[CurrentFaceBlendSet][blendIndex] = value;

        morph.SetBlendValues(blendIndex, value);
        morph.FixBlendValues();
    }

    public float GetFaceBlendValue(string hash)
    {
        var morph = Body.Face.morph;

        if (hash is "nosefook")
            return (Maid.boNoseFook || morph.boNoseFook) ? 1f : 0f;

        hash = Utility.GP01FbFaceHash(morph, hash);

        return morph.dicBlendSet[CurrentFaceBlendSet][(int)morph.hash[hash]];
    }

    public void StopBlink()
    {
        Maid.MabatakiUpdateStop = true;
        Body.Face.morph.EyeMabataki = 0f;
        Utility.SetFieldValue(Maid, "MabatakiVal", 0f);
    }

    public void SetMaskMode(Mask maskMode) =>
        SetMaskMode(maskMode is Mask.Nude ? MaskMode.Nude : (MaskMode)maskMode);

    public void SetMaskMode(MaskMode maskMode)
    {
        var invisibleBody = !Body.GetMask(SlotID.body);

        Body.SetMaskMode(maskMode);

        if (invisibleBody)
            SetBodyMask(false);
    }

    public void SetBodyMask(bool enabled)
    {
        var table = Utility.GetFieldValue<TBody, Hashtable>(Body, "m_hFoceHide");

        foreach (var bodySlot in MaidDressingPane.BodySlots)
            table[bodySlot] = enabled;

        Body.FixMaskFlag();
        Body.FixVisibleFlag(false);
    }

    public void SetCurling(Curl curling, bool enabled)
    {
        var name = curling is Curl.Shift
            ? new[] { "panz", "mizugi" }
            : new[] { "skirt", "onepiece" };

        if (enabled)
        {
            var action = curling switch
            {
                Curl.Shift => "パンツずらし",
                Curl.Front => "めくれスカート",
                _ => "めくれスカート後ろ",
            };

            Maid.ItemChangeTemp(name[0], action);
            Maid.ItemChangeTemp(name[1], action);
        }
        else
        {
            Maid.ResetProp(name[0]);
            Maid.ResetProp(name[1]);
        }

        Maid.AllProcProp();
        HairGravityControl.Control.OnChangeMekure();
        SkirtGravityControl.Control.OnChangeMekure();
    }

    public void SetMpnProp(MpnAttachProp prop, bool detach)
    {
        if (detach)
            Maid.ResetProp(prop.Tag, false);
        else
            Maid.SetProp(prop.Tag, prop.MenuFile, 0, true);

        Maid.AllProcProp();
    }

    public void DetachAllMpnAttach()
    {
        Maid.ResetProp(MPN.kousoku_lower, false);
        Maid.ResetProp(MPN.kousoku_upper, false);
        Maid.AllProcProp();
    }

    public void ApplyGravity(Vector3 position, bool skirt = false)
    {
        var dragPoint = skirt ? SkirtGravityControl : HairGravityControl;

        if (dragPoint.Valid)
            dragPoint.Control.transform.localPosition = position;
    }

    private void StartLoad(Action callback)
    {
        if (Loading)
            return;

        GameMain.Instance.StartCoroutine(Load(callback));
    }

    private IEnumerator Load(Action callback)
    {
        Loading = true;

        while (Maid.IsBusy)
            yield return null;

        yield return new WaitForEndOfFrame();

        callback();
        Loading = false;
    }

    private void OnBodyLoad()
    {
        if (!initialized)
        {
            DefaultEyeRotL = Body.quaDefEyeL;
            DefaultEyeRotR = Body.quaDefEyeR;

            initialized = true;
        }

        if (blendSetValueBackup is null)
            BackupBlendSetValues();

        if (!HairGravityControl)
            InitializeGravityControls();

        HairGravityControl.Move += OnGravityEvent;
        SkirtGravityControl.Move += OnGravityEvent;

        if (MeidoPhotoStudio.EditMode)
            AllProcPropSeqStartPatcher.SequenceStart += ReinitializeBody;

        IKManager.Initialize();

        SetFaceBlendSet(DefaultFaceBlendSet);

        IK = true;
        Stop = false;
        Bone = false;

        Active = true;
    }

    private void ReinitializeBody(object sender, ProcStartEventArgs args)
    {
        if (Loading || !Body.isLoadedBody)
            return;

        if (args.Maid.status.guid != Maid.status.guid)
            return;

        var gravityControlProps =
            new[]
            {
                MPN.skirt, MPN.onepiece, MPN.mizugi, MPN.panz, MPN.set_maidwear, MPN.set_mywear, MPN.set_underwear,
                MPN.hairf, MPN.hairr, MPN.hairs, MPN.hairt,
            };

        Action action = null;

        // Change body
        if (Maid.GetProp(MPN.body).boDut)
        {
            IKManager.Destroy();
            action += ReinitializeBody;
        }

        // Change face
        if (Maid.GetProp(MPN.head).boDut)
        {
            SetFaceBlendSet(DefaultFaceBlendSet);
            action += ReinitializeFace;
        }

        // Gravity control clothing/hair change
        if (gravityControlProps.Any(prop => Maid.GetProp(prop).boDut))
        {
            if (HairGravityControl)
                Object.Destroy(HairGravityControl.gameObject);

            if (SkirtGravityControl)
                Object.Destroy(SkirtGravityControl.gameObject);

            action += ReinitializeGravity;
        }

        // Clothing/accessory changes
        // Includes null_mpn too but any button click results in null_mpn bodut I think
        action ??= Default;

        StartLoad(action);

        void ReinitializeBody()
        {
            IKManager.Initialize();
            Stop = false;

            // Maid animation needs to be set again for custom parts edit
            var uiRoot = GameObject.Find("UI Root");
            var customPartsWindow = UTY.GetChildObject(uiRoot, "Window/CustomPartsWindow")
                .GetComponent<SceneEditWindow.CustomPartsWindow>();

            Utility.SetFieldValue(customPartsWindow, "animation", Maid.GetAnimation());
        }

        void ReinitializeFace()
        {
            DefaultEyeRotL = Body.quaDefEyeL;
            DefaultEyeRotR = Body.quaDefEyeR;
            BackupBlendSetValues();
        }

        void ReinitializeGravity()
        {
            InitializeGravityControls();
            OnUpdateMeido();
        }

        void Default() =>
            OnUpdateMeido();
    }

    private void BackupBlendSetValues()
    {
        var values = Body.Face.morph.dicBlendSet[CurrentFaceBlendSet];

        blendSetValueBackup = new float[values.Length];
        values.CopyTo(blendSetValueBackup, 0);
    }

    private void ApplyBackupBlendSet()
    {
        blendSetValueBackup.CopyTo(Body.Face.morph.dicBlendSet[CurrentFaceBlendSet], 0);
        Maid.boNoseFook = false;
    }

    private CacheBoneDataArray GetCacheBoneData()
    {
        var cache = Maid.gameObject.GetComponent<CacheBoneDataArray>();

        if (!cache)
        {
            cache = Maid.gameObject.AddComponent<CacheBoneDataArray>();
            CreateCache();
        }

        if (!cache.bone_data?.transform)
        {
            Utility.LogDebug("Cache bone_data is null");
            CreateCache();
        }

        void CreateCache() =>
            cache.CreateCache(Body.GetBone("Bip01"));

        return cache;
    }

    private void InitializeGravityControls()
    {
        HairGravityControl = MakeGravityControl(skirt: false);
        SkirtGravityControl = MakeGravityControl(skirt: true);
    }

    private DragPointGravity MakeGravityControl(bool skirt = false)
    {
        var gravityDragpoint = DragPoint.Make<DragPointGravity>(PrimitiveType.Cube, Vector3.one * 0.12f);
        var control = DragPointGravity.MakeGravityControl(Maid, skirt);

        gravityDragpoint.Initialize(() => control.transform.position, () => Vector3.zero);
        gravityDragpoint.Set(control.transform);

        gravityDragpoint.gameObject.SetActive(false);

        return gravityDragpoint;
    }

    private void OnUpdateMeido(MeidoUpdateEventArgs args = null) =>
        UpdateMeido?.Invoke(this, args ?? MeidoUpdateEventArgs.Empty);

    private void OnGravityEvent(object sender, EventArgs args) =>
        OnGravityChange((DragPointGravity)sender);

    private void OnGravityChange(DragPointGravity dragPoint)
    {
        var args =
            new GravityEventArgs(dragPoint == SkirtGravityControl, dragPoint.MyObject.transform.localPosition);

        GravityMove?.Invoke(this, args);
    }
}
