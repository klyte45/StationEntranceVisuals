using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using HarmonyLib;
using StationEntranceVisuals.BridgeWE;
using StationEntranceVisuals.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StationEntranceVisuals
{
    public class Mod : IMod
    {
        public static ILog log = LogManager.GetLogger($"{nameof(StationEntranceVisuals)}.{nameof(Mod)}").SetShowsErrorsInUI(false);

        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Info(nameof(OnLoad));

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
                log.Info($"Current mod asset at {asset.path}");

            DoPatches();
            WEImageManagementBridge.RegisterForAtlasCacheResetNotification(FileUtils.RegisterFilesToWE);
        }

        private void DoPatches()
        {
            if (AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == "BelzontWE") is Assembly weAssembly)
            {
                var exportedTypes = weAssembly.ExportedTypes;
                foreach (var (type, sourceClassName) in new List<(Type, string)>() {
                    (typeof(WEFontManagementBridge), "FontManagementBridge"),
                    (typeof(WEImageManagementBridge), "ImageManagementBridge"),
                    (typeof(WETemplatesManagementBridge), "TemplatesManagementBridge"),
                })
                {
                    var targetType = exportedTypes.First(x => x.Name == sourceClassName);
                    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    {
                        var srcMethod = targetType.GetMethod(method.Name, ReflectionUtils.allFlags, null, method.GetParameters().Select(x => x.ParameterType).ToArray(), null);
                        if (srcMethod != null) Harmony.ReversePatch(srcMethod, method);
                        else log.Warn($"Method not found while patching WE: {targetType.FullName} {srcMethod.Name}({string.Join(", ", method.GetParameters().Select(x => $"{x.ParameterType}"))})");
                    }
                }
            }
        }

        public void OnDispose()
        {
            log.Info(nameof(OnDispose));
            WEImageManagementBridge.UnregisterForAtlasCacheResetNotification(FileUtils.RegisterFilesToWE);
        }

    }
}
