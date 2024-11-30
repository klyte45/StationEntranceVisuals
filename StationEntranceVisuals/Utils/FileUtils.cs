using Colossal.IO.AssetDatabase.Internal;
using Game.SceneFlow;
using StationEntranceVisuals.BridgeWE;
using System.IO;
using System.Linq;

namespace StationEntranceVisuals.Utils;

public static class FileUtils
{
    private static void InstallLayouts(string imageAtlasName, string layoutsPath)
    {
        var targetDirectory = layoutsPath + "_cache_" + WEImageManagementBridge.GetImageAtlasVersion();
        if (!Directory.Exists(targetDirectory))
        {
            string modPath = Path.GetDirectoryName(GameManager.instance.modManager.First(x => x.asset.assembly == typeof(Mod).Assembly).asset.path);
            Directory.GetDirectories(Path.GetDirectoryName(layoutsPath), Path.GetFileName(layoutsPath) + "_cache_*").ForEach(x => Directory.Delete(x, true));
            Directory.CreateDirectory(targetDirectory);
            var layoutsFiles = Directory.GetFiles(layoutsPath);
            foreach (var layoutsFile in layoutsFiles)
            {
                File.WriteAllText(Path.Combine(targetDirectory, Path.GetFileName(layoutsFile)), File.ReadAllText(layoutsFile).Replace("atlas=\"_StationEntranceVisuals\"", $"atlas=\"{imageAtlasName}\""));
            }
        }
        WETemplatesManagementBridge.RegisterCustomTemplates(typeof(Mod).Assembly, targetDirectory);
    }

    public static void RegisterFilesToWE()
    {
        string modPath = Path.GetDirectoryName(GameManager.instance.modManager.First(x => x.asset.assembly == typeof(Mod).Assembly).asset.path);
        var localImagesDirectory = Path.Combine(
                 modPath,
                 "weImageAtlases"
            );
        var localLayoutsDirectory = Path.Combine(
             modPath,
             "weLayouts"
        );
        WEImageManagementBridge.RegisterImageAtlas(typeof(Mod).Assembly, "main", Directory.GetFiles(localImagesDirectory), (atlasInternalName) => InstallLayouts(atlasInternalName, localLayoutsDirectory));

    }
}