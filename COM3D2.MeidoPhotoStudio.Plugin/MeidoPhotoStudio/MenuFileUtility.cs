using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public static partial class MenuFileUtility
    {
        private static byte[] fileBuffer;
        public const string noCategory = "noCategory";

        public static readonly string[] MenuCategories =
        {
            noCategory, "acchat", "headset", "wear", "skirt", "onepiece", "mizugi", "bra", "panz", "stkg", "shoes",
            "acckami", "megane", "acchead", "acchana", "accmimi", "glove", "acckubi", "acckubiwa", "acckamisub",
            "accnip", "accude", "accheso", "accashi", "accsenaka", "accshippo", "accxxx"
        };

        private static readonly HashSet<string> accMpn = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        private enum IMode
        {
            None,
            ItemChange,
            TexChange
        }

        public static event EventHandler MenuFilesReadyChange;
        public static bool MenuFilesReady { get; private set; }

        static MenuFileUtility()
        {
            accMpn.UnionWith(MenuCategories.Skip(1));
            GameMain.Instance.StartCoroutine(CheckMenuDataBaseJob());
        }

        private static IEnumerator CheckMenuDataBaseJob()
        {
            if (MenuFilesReady) yield break;
            while (!GameMain.Instance.MenuDataBase.JobFinished()) yield return null;
            MenuFilesReady = true;
            MenuFilesReadyChange?.Invoke(null, EventArgs.Empty);
        }

        private static ref byte[] GetFileBuffer(long size)
        {
            if (fileBuffer == null) fileBuffer = new byte[Math.Max(500000, size)];
            else if (fileBuffer.Length < size) fileBuffer = new byte[size];

            return ref fileBuffer;
        }

        private static byte[] ReadAFileBase(string filename)
        {
            using AFileBase aFileBase = GameUty.FileOpen(filename);

            if (aFileBase.IsValid() && aFileBase.GetSize() != 0)
            {
                ref byte[] buffer = ref GetFileBuffer(aFileBase.GetSize());

                aFileBase.Read(ref buffer, aFileBase.GetSize());

                return buffer;
            }

            Utility.LogError($"AFileBase '{filename}' is invalid");
            return null;
        }

        private static byte[] ReadOfficialMod(string filename)
        {
            using var fileStream = new FileStream(filename, FileMode.Open);

            if (fileStream.Length != 0)
            {
                ref byte[] buffer = ref GetFileBuffer(fileStream.Length);

                fileStream.Read(buffer, 0, (int) fileStream.Length);

                return buffer;
            }

            Utility.LogWarning($"Mod menu file '{filename}' is invalid");
            return null;
        }

        private static IEnumerable<Renderer> GetRenderers(GameObject gameObject)
            => gameObject.transform.GetComponentsInChildren<Transform>(true)
                .Select(transform => transform.GetComponent<Renderer>())
                .Where(renderer => renderer && renderer.material).ToList();

        private static bool ProcScriptBin(byte[] menuBuf, out ModelInfo modelInfo)
        {
            modelInfo = null;
            using var binaryReader = new BinaryReader(new MemoryStream(menuBuf), Encoding.UTF8);

            if (binaryReader.ReadString() != "CM3D2_MENU") return false;

            modelInfo = new ModelInfo();

            binaryReader.ReadInt32(); // file version
            binaryReader.ReadString(); // txt path
            binaryReader.ReadString(); // name
            binaryReader.ReadString(); // category
            binaryReader.ReadString(); // description
            binaryReader.ReadInt32(); // idk (as long)

            try
            {
                while (true)
                {
                    int numberOfProps = binaryReader.ReadByte();
                    var menuPropString = string.Empty;

                    if (numberOfProps != 0)
                    {
                        for (var i = 0; i < numberOfProps; i++)
                        {
                            menuPropString = $"{menuPropString}\"{binaryReader.ReadString()}\"";
                        }

                        if (menuPropString != string.Empty)
                        {
                            var header = UTY.GetStringCom(menuPropString);
                            string[] menuProps = UTY.GetStringList(menuPropString);

                            if (header == "end") break;

                            switch (header)
                            {
                                case "マテリアル変更":
                                {
                                    var matNo = int.Parse(menuProps[2]);
                                    var materialFile = menuProps[3];
                                    modelInfo.MaterialChanges.Add(new MaterialChange(matNo, materialFile));
                                    break;
                                }
                                case "additem":
                                    modelInfo.ModelFile = menuProps[1];
                                    break;
                            }
                        }
                    }
                    else break;
                }
            }
            catch { return false; }

            return true;
        }

        private static void ProcModScriptBin(byte[] cd, GameObject go)
        {
            var matDict = new Dictionary<string, byte[]>();
            string modData;

            using (var binaryReader = new BinaryReader(new MemoryStream(cd), Encoding.UTF8))
            {
                if (binaryReader.ReadString() != "CM3D2_MOD") return;
                binaryReader.ReadInt32();
                binaryReader.ReadString();
                binaryReader.ReadString();
                binaryReader.ReadString();
                binaryReader.ReadString();
                binaryReader.ReadString();
                var mpnValue = binaryReader.ReadString();
                var mpn = MPN.null_mpn;
                try { mpn = (MPN) Enum.Parse(typeof(MPN), mpnValue, true); }
                catch { /* ignored */ }

                if (mpn != MPN.null_mpn) binaryReader.ReadString();

                modData = binaryReader.ReadString();
                var entryCount = binaryReader.ReadInt32();
                for (var i = 0; i < entryCount; i++)
                {
                    var key = binaryReader.ReadString();
                    var count = binaryReader.ReadInt32();
                    byte[] value = binaryReader.ReadBytes(count);
                    matDict.Add(key, value);
                }
            }

            var mode = IMode.None;
            var materialChange = false;

            Material material = null;
            var materialIndex = 0;

            using var stringReader = new StringReader(modData);

            string line;

            List<Renderer> renderers = null;

            while ((line = stringReader.ReadLine()) != null)
            {
                string[] data = line.Split(new[] {'\t', ' '}, StringSplitOptions.RemoveEmptyEntries);

                switch (data[0])
                {
                    case "アイテム変更":
                    case "マテリアル変更":
                        mode = IMode.ItemChange;
                        break;
                    case "テクスチャ変更":
                        mode = IMode.TexChange;
                        break;
                }

                switch (mode)
                {
                    case IMode.ItemChange:
                    {
                        if (data[0] == "スロット名") materialChange = true;

                        if (materialChange)
                        {
                            if (data[0] == "マテリアル番号")
                            {
                                materialIndex = int.Parse(data[1]);

                                renderers ??= GetRenderers(go).ToList();

                                foreach (Renderer renderer in renderers)
                                {
                                    if (materialIndex < renderer.materials.Length)
                                        material = renderer.materials[materialIndex];
                                }
                            }

                            if (!material) continue;

                            switch (data[0])
                            {
                                case "テクスチャ設定":
                                    ChangeTex(materialIndex, data[1], data[2].ToLower());
                                    break;
                                case "色設定":
                                    material.SetColor(data[1],
                                        new Color(
                                            float.Parse(data[2]) / 255f, float.Parse(data[3]) / 255f,
                                            float.Parse(data[4]) / 255f, float.Parse(data[5]) / 255f
                                        )
                                    );
                                    break;
                                case "数値設定":
                                    material.SetFloat(data[1], float.Parse(data[2]));
                                    break;
                            }
                        }

                        break;
                    }
                    case IMode.TexChange:
                    {
                        var matno = int.Parse(data[2]);
                        ChangeTex(matno, data[3], data[4].ToLower());
                        break;
                    }
                    case IMode.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            void ChangeTex(int matno, string prop, string filename)
            {
                byte[] buf = matDict[filename.ToLowerInvariant()];
                var textureResource = new TextureResource(2, 2, TextureFormat.ARGB32, null, buf);
                renderers ??= GetRenderers(go).ToList();
                foreach (Renderer r in renderers)
                {
                    r.materials[matno].SetTexture(prop, null);
                    Texture2D texture2D = textureResource.CreateTexture2D();
                    texture2D.name = filename;
                    r.materials[matno].SetTexture(prop, texture2D);
                }
            }
        }

        public static GameObject LoadModel(string menuFile) => LoadModel(new ModItem(menuFile));

        public static GameObject LoadModel(ModItem modItem)
        {
            var menu = modItem.IsOfficialMod ? modItem.BaseMenuFile : modItem.MenuFile;

            byte[] modelBuffer;

            try { modelBuffer = ReadAFileBase(menu); }
            catch (Exception e)
            {
                Utility.LogError($"Could not read menu file '{menu}' because {e.Message}\n{e.StackTrace}");
                return null;
            }

            if (ProcScriptBin(modelBuffer, out ModelInfo modelInfo))
            {
                if (InstantiateModel(modelInfo.ModelFile, out GameObject finalModel))
                {
                    IEnumerable<Renderer> renderers = GetRenderers(finalModel).ToList();

                    foreach (MaterialChange matChange in modelInfo.MaterialChanges)
                    {
                        foreach (Renderer renderer in renderers)
                        {
                            if (matChange.MaterialIndex < renderer.materials.Length)
                            {
                                renderer.materials[matChange.MaterialIndex] = ImportCM.LoadMaterial(
                                    matChange.MaterialFile, null, renderer.materials[matChange.MaterialIndex]
                                );
                            }
                        }
                    }

                    if (!modItem.IsOfficialMod) return finalModel;

                    try { modelBuffer = ReadOfficialMod(modItem.MenuFile); }
                    catch (Exception e)
                    {
                        Utility.LogError(
                            $"Could not read mod menu file '{modItem.MenuFile}' because {e.Message}\n{e.StackTrace}"
                        );
                        return null;
                    }

                    ProcModScriptBin(modelBuffer, finalModel);

                    return finalModel;
                }
            }

            Utility.LogMessage($"Could not load model '{modItem.MenuFile}'");

            return null;
        }

        public static bool ParseNativeMenuFile(int menuIndex, ModItem modItem)
        {
            MenuDataBase menuDataBase = GameMain.Instance.MenuDataBase;
            menuDataBase.SetIndex(menuIndex);
            if (menuDataBase.GetBoDelOnly()) return false;
            modItem.Category = menuDataBase.GetCategoryMpnText();
            if (!accMpn.Contains(modItem.Category)) return false;
            modItem.MenuFile = menuDataBase.GetMenuFileName().ToLower();
            if (!ValidBG2MenuFile(modItem.MenuFile)) return false;
            modItem.Name = menuDataBase.GetMenuName();
            modItem.IconFile = menuDataBase.GetIconS();
            modItem.Priority = menuDataBase.GetPriority();
            return true;
        }

        public static void ParseMenuFile(string menuFile, ModItem modItem)
        {
            if (!ValidBG2MenuFile(menuFile)) return;

            byte[] buffer;

            try { buffer = ReadAFileBase(menuFile); }
            catch (Exception e)
            {
                Utility.LogError($"Could not read menu file '{menuFile}' because {e.Message}");
                return ;
            }

            try
            {
                using var binaryReader = new BinaryReader(new MemoryStream(buffer), Encoding.UTF8);

                if (binaryReader.ReadString() != "CM3D2_MENU") return;
                binaryReader.ReadInt32(); // file version
                binaryReader.ReadString(); // txt path
                modItem.Name = binaryReader.ReadString(); // name
                binaryReader.ReadString(); // category
                binaryReader.ReadString(); // description
                binaryReader.ReadInt32(); // idk (as long)

                while (true)
                {
                    int numberOfProps = binaryReader.ReadByte();
                    var menuPropString = string.Empty;

                    if (numberOfProps == 0) break;

                    for (var i = 0; i < numberOfProps; i++)
                    {
                        menuPropString = $"{menuPropString}\"{binaryReader.ReadString()}\"";
                    }

                    if (string.IsNullOrEmpty(menuPropString)) continue;

                    var header = UTY.GetStringCom(menuPropString);
                    string[] menuProps = UTY.GetStringList(menuPropString);

                    if (header == "end") break;

                    if (header == "category")
                    {
                        modItem.Category = menuProps[1];
                        if (!accMpn.Contains(modItem.Category)) return;
                    }
                    else if (header == "icons" || header == "icon")
                    {
                        modItem.IconFile = menuProps[1];
                        break;
                    }
                    else if (header == "priority") modItem.Priority = float.Parse(menuProps[1]);
                }
            }
            catch (Exception e)
            {
                Utility.LogWarning($"Could not parse menu file '{menuFile}' because {e.Message}");
            }
        }

        public static bool ParseModMenuFile(string modMenuFile, ModItem modItem)
        {
            if (!ValidBG2MenuFile(modMenuFile)) return false;

            byte[] modBuffer;

            try { modBuffer = ReadOfficialMod(modMenuFile); }
            catch (Exception e)
            {
                Utility.LogError($"Could not read mod menu file '{modMenuFile} because {e.Message}'");
                return false;
            }

            try
            {
                using var binaryReader = new BinaryReader(new MemoryStream(modBuffer), Encoding.UTF8);

                if (binaryReader.ReadString() != "CM3D2_MOD") return false;

                binaryReader.ReadInt32();
                var iconName = binaryReader.ReadString();
                var baseItemPath = binaryReader.ReadString().Replace(":", " ");
                modItem.BaseMenuFile = Path.GetFileName(baseItemPath);
                modItem.Name = binaryReader.ReadString();
                modItem.Category = binaryReader.ReadString();
                if (!accMpn.Contains(modItem.Category)) return false;
                binaryReader.ReadString();

                var mpnValue = binaryReader.ReadString();
                var mpn = MPN.null_mpn;
                try { mpn = (MPN) Enum.Parse(typeof(MPN), mpnValue, true); }
                catch { /* ignored */ }

                if (mpn != MPN.null_mpn) binaryReader.ReadString();

                binaryReader.ReadString();

                var entryCount = binaryReader.ReadInt32();
                for (var i = 0; i < entryCount; i++)
                {
                    var key = binaryReader.ReadString();
                    var count = binaryReader.ReadInt32();
                    byte[] data = binaryReader.ReadBytes(count);

                    if (!string.Equals(key, iconName, StringComparison.InvariantCultureIgnoreCase)) continue;

                    var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    tex.LoadImage(data);
                    modItem.Icon = tex;
                    break;
                }
            }
            catch (Exception e)
            {
                Utility.LogWarning($"Could not parse mod menu file '{modMenuFile}' because {e}");
                return false;
            }

            return true;
        }

        public static bool ValidBG2MenuFile(ModItem modItem)
            => accMpn.Contains(modItem.Category) && ValidBG2MenuFile(modItem.MenuFile);

        public static bool ValidBG2MenuFile(string menu)
        {
            menu = Path.GetFileNameWithoutExtension(menu).ToLower();
            return !(menu.EndsWith("_del") || menu.Contains("zurashi") || menu.Contains("mekure")
                || menu.Contains("porori") || menu.Contains("moza") || menu.Contains("folder"));
        }

        public abstract class MenuItem
        {
            public string IconFile { get; set; }
            public Texture2D Icon { get; set; }
        }

        public class ModItem : MenuItem
        {
            public string MenuFile { get; set; }
            public string BaseMenuFile { get; set; }
            public string Name { get; set; }
            public string Category { get; set; }
            public float Priority { get; set; }
            public bool IsMod { get; private set; }
            public bool IsOfficialMod { get; private set; }

            public static ModItem OfficialMod(string menuFile) => new ModItem()
            {
                MenuFile = menuFile,
                IsMod = true,
                IsOfficialMod = true,
                Priority = 1000f
            };

            public static ModItem Mod(string menuFile) => new ModItem()
            {
                MenuFile = menuFile,
                IsMod = true
            };

            public ModItem() { }

            public ModItem(string menuFile) => MenuFile = menuFile;

            public override string ToString()
                => IsOfficialMod ? $"{Path.GetFileName(MenuFile)}#{BaseMenuFile}" : MenuFile;

            public static ModItem Deserialize(BinaryReader binaryReader)
                => new ModItem()
                {
                    MenuFile = binaryReader.ReadNullableString(),
                    BaseMenuFile = binaryReader.ReadNullableString(),
                    IconFile = binaryReader.ReadNullableString(),
                    Name = binaryReader.ReadNullableString(),
                    Category = binaryReader.ReadNullableString(),
                    Priority = float.Parse(binaryReader.ReadNullableString()),
                    IsMod = binaryReader.ReadBoolean(),
                    IsOfficialMod = binaryReader.ReadBoolean()
                };

            public void Serialize(BinaryWriter binaryWriter)
            {
                if (IsOfficialMod) return;
                binaryWriter.WriteNullableString(MenuFile);
                binaryWriter.WriteNullableString(BaseMenuFile);
                binaryWriter.WriteNullableString(IconFile);
                binaryWriter.WriteNullableString(Name);
                binaryWriter.WriteNullableString(Category);
                binaryWriter.WriteNullableString(Priority.ToString(CultureInfo.InvariantCulture));
                binaryWriter.Write(IsMod);
                binaryWriter.Write(IsOfficialMod);
            }
        }

        public class MyRoomItem : MenuItem
        {
            public int ID { get; set; }
            public string PrefabName { get; set; }

            public override string ToString() => $"MYR_{ID}#{PrefabName}";
        }

        private class ModelInfo
        {
            public List<MaterialChange> MaterialChanges { get; } = new List<MaterialChange>();
            public string ModelFile { get; set; }
        }

        private readonly struct MaterialChange
        {
            public int MaterialIndex { get; }
            public string MaterialFile { get; }

            public MaterialChange(int materialIndex, string materialFile)
            {
                MaterialIndex = materialIndex;
                MaterialFile = materialFile;
            }
        }
    }
}
