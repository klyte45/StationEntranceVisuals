using BridgeWE;
using Colossal.Core;
using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Colossal.Reflection;
using Game;
using Game.Modding;
using Game.SceneFlow;
using HarmonyLib;
using StationEntranceVisuals.BridgeWE;
using StationEntranceVisuals.Systems;
using StationEntranceVisuals.Utils;
using StationEntranceVisuals.WE_TFMBridge;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.Entities;
using static StationEntranceVisuals.Settings;

namespace StationEntranceVisuals
{
    public class Mod : IMod
    {
        public static ILog log = LogManager.GetLogger($"{nameof(StationEntranceVisuals)}.{nameof(Mod)}").SetShowsErrorsInUI(false);
        public static readonly BindingFlags allFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.GetField | BindingFlags.GetProperty;
        public static Settings m_Setting;

        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Info(nameof(OnLoad));

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
                log.Info($"Current mod asset at {asset.path}");
            m_Setting = new Settings(this);
            m_Setting.RegisterInOptionsUI();
            GameManager.instance.localizationManager.AddSource("en-US", new LocaleEn(m_Setting));
            AssetDatabase.global.LoadSettings(nameof(StationEntranceVisuals), m_Setting, new Settings(this));

            // Initialize burst-optimized line data systems
            InitializeLineDataSystems();

            var bw = new BackgroundWorker();
            bw.DoWork += DeleteOldFiles;
            bw.RunWorkerAsync();

            MainThreadDispatcher.RegisterUpdater(DoWhenLoaded);
            (AssetDatabase<ParadoxMods>.instance.dataSource as ParadoxModsDataSource).onAfterActivePlaysetOrModStatusChanged += DoWhenLoaded;
        }

        private void InitializeLineDataSystems()
        {
            log.Info("Initializing burst-optimized line data systems...");
            var world = World.DefaultGameObjectInjectionWorld;
            log.Info("Line data systems initialized successfully.");
        }

        private bool isLoaded = false;
        private void DoWhenLoaded()
        {
            if (isLoaded) return;
            log.Info($"Loading patches");
            if (!DoPatches()) return;
            RegisterFilesToWe();
            isLoaded = true;
            (AssetDatabase<ParadoxMods>.instance.dataSource as ParadoxModsDataSource).onAfterActivePlaysetOrModStatusChanged -= DoWhenLoaded;
        }

        private static void DeleteOldFiles(object sender, DoWorkEventArgs e)
        {
            FileUtils.DeleteOldFiles();
        }


        private static void RegisterFilesToWe()
        {
            string modPath = Path.GetDirectoryName(GameManager.instance.modManager.FirstOrDefault(x => x.asset.assembly == typeof(Mod).Assembly).asset.path);

            var imagesDirectory = Path.Combine(modPath, "atlases");
            var atlases = Directory.GetDirectories(imagesDirectory, "*", SearchOption.TopDirectoryOnly);
            foreach (var atlasFolder in atlases)
            {
                WEImageManagementBridge.RegisterImageAtlas(typeof(Mod).Assembly, Path.GetFileName(atlasFolder), Directory.GetFiles(atlasFolder, "*.png"));
            }
            var localLayoutsDirectory = Path.Combine(modPath, "weLayouts");
            WETemplatesManagementBridge.RegisterCustomTemplates(typeof(Mod).Assembly, localLayoutsDirectory);
            WETemplatesManagementBridge.RegisterLoadableTemplatesFolder(typeof(Mod).Assembly, localLayoutsDirectory);

            var dataSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<SEV_SettingSystem>();
            WEModuleOptionsBridge.CreateBuilder(typeof(Mod).Assembly, "StationEntranceVisuals.weOptions")
                .Dropdown("SubwayLineIndicatorShape",
                    () => dataSystem.SubwayLineIndicatorShape.ToString(),
                    x =>
                    {
                        log.DebugFormat("Setting SubwayLineIndicatorShape to {0}", x);
                        dataSystem.SubwayLineIndicatorShape = Enum.TryParse<LineIndicatorShapeOptions>(x, out var result) ? result : LineIndicatorShapeOptions.Square;
                    },
                    () => Enum.GetNames(typeof(LineIndicatorShapeOptions)).ToDictionary(x => x, x => $"StationEntranceVisuals.weOptions[LineIndicatorShapeOptions.{x}]"))
                .Dropdown("TrainLineIndicatorShape",
                    () => dataSystem.TrainLineIndicatorShape.ToString(),
                    x =>
                    {
                        log.DebugFormat("Setting TrainLineIndicatorShape to {0}", x);
                        dataSystem.TrainLineIndicatorShape = Enum.TryParse<LineIndicatorShapeOptions>(x, out var result) ? result : LineIndicatorShapeOptions.Square;
                    },
                    () => Enum.GetNames(typeof(LineIndicatorShapeOptions)).ToDictionary(x => x, x => $"StationEntranceVisuals.weOptions[LineIndicatorShapeOptions.{x}]"))
                .Dropdown("BusLineIndicatorShape",
                    () => dataSystem.BusLineIndicatorShape.ToString(),
                    x =>
                    {
                        log.DebugFormat("Setting BusLineIndicatorShape to {0}", x);
                        dataSystem.BusLineIndicatorShape = Enum.TryParse<LineIndicatorShapeOptions>(x, out var result) ? result : LineIndicatorShapeOptions.Diamond;
                    },
                    () => Enum.GetNames(typeof(LineIndicatorShapeOptions)).ToDictionary(x => x, x => $"StationEntranceVisuals.weOptions[LineIndicatorShapeOptions.{x}]"))
                .Dropdown("TramLineIndicatorShape",
                    () => dataSystem.TramLineIndicatorShape.ToString(),
                    x =>
                    {
                        log.DebugFormat("Setting TramLineIndicatorShape to {0}", x);
                        dataSystem.TramLineIndicatorShape = Enum.TryParse<LineIndicatorShapeOptions>(x, out var result) ? result : LineIndicatorShapeOptions.Pentagon;
                    },
                    () => Enum.GetNames(typeof(LineIndicatorShapeOptions)).ToDictionary(x => x, x => $"StationEntranceVisuals.weOptions[LineIndicatorShapeOptions.{x}]"))
                .Spacer("______")
                .Dropdown("LineOperatorCity",
                    () => dataSystem.LineOperatorCity.ToString(),
                    x =>
                    {
                        log.DebugFormat("Setting LineOperatorCity to {0}", x);
                        dataSystem.LineOperatorCity = Enum.TryParse<LineOperatorCityOptions>(x, out var result) ? result : LineOperatorCityOptions.Generic;
                    },
                    () => Enum.GetNames(typeof(LineOperatorCityOptions)).ToDictionary(x => x, x => $"StationEntranceVisuals.weOptions[LineOperatorCityOptions.{x}]"))
                .Dropdown("LineDisplayName",
                    () => dataSystem.LineDisplayName.ToString(),
                    x =>
                    {
                        log.DebugFormat("Setting LineDisplayName to {0}", x);
                        dataSystem.LineDisplayName = Enum.TryParse<LineDisplayNameOptions>(x, out var result) ? result : LineDisplayNameOptions.Generated;
                    },
                    () => Enum.GetNames(typeof(LineDisplayNameOptions)).ToDictionary(x => x, x => $"StationEntranceVisuals.weOptions[LineDisplayNameOptions.{x}]"))
                .Register();
        }

        private bool DoPatches()
        {
            ConnectBridge("BelzontWE",
            [
                (typeof(WEFontManagementBridge), "FontManagementBridge"),
                (typeof(WEImageManagementBridge), "ImageManagementBridge"),
                (typeof(WETemplatesManagementBridge), "TemplatesManagementBridge"),
                (typeof(WERouteFn), "WERouteFn"),
                (typeof(WEModuleOptionsBridge), "ModuleOptionsBridge")
            ]);
            ConnectBridge("WE_TFM",
            [
                (typeof(WE_TFMBuildingLineCacheBridge), "WE_TFMBuildingLineCacheBridge"),
            ]);
            return true;
        }

        private static bool ConnectBridge(string dllName, List<(Type, string)> typesMapping)
        {
            try
            {
                if (AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == dllName) is Assembly weAssembly)
                {
                    var exportedTypes = weAssembly.ExportedTypes;
                    foreach (var (type, sourceClassName) in typesMapping)
                    {
                        var targetType = exportedTypes.First(x => x.Name == sourceClassName);
                        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                        {
                            MethodInfo srcMethod;
                            if (method.TryGetAttribute<PatchGenericMethod>(out var attribute))
                            {
                                var targetMethodName = attribute.OriginalMethodName ?? method.Name;
                                MethodInfo[] methods = targetType.GetMethods(allFlags);
                                srcMethod = methods.FirstOrDefault(x => x.Name == targetMethodName && x.IsGenericMethod && x.GetGenericArguments().Length == attribute.Types.Length);
                                if (srcMethod == null)
                                {
                                    log.Warn($"Method not found while patching {dllName}: {targetType.FullName} {targetMethodName}({string.Join(", ", method.GetParameters().Select(x => $"{x.ParameterType}"))}) with generic arguments [{string.Join(", ", attribute.Types.Select(x => x.FullName))}] - Searched for {targetMethodName}");
                                    continue;
                                }
                                if (!srcMethod.IsGenericMethod || srcMethod.GetGenericArguments().Length != attribute.Types.Length)
                                {
                                    log.Warn($"Method not found while patching {dllName}: {targetType.FullName} {srcMethod.Name}({string.Join(", ", method.GetParameters().Select(x => $"{x.ParameterType}"))}) with generic arguments [{string.Join(", ", [.. attribute.Types.Select(x => x.FullName)])}] - Incompatible types (found: [{string.Join(", ", [.. srcMethod.GetGenericArguments().Select(x => x.FullName)])}])");
                                    continue;
                                }
                                srcMethod = srcMethod.MakeGenericMethod(attribute.Types);
                                if (!srcMethod.GetParameters().Types().SequenceEqual(method.GetParameters().Types()) || srcMethod.ReturnType != method.ReturnType)
                                {
                                    log.Warn($"Method not found while patching {dllName}: {targetType.FullName} {srcMethod.Name}({string.Join(", ", method.GetParameters().Select(x => $"{x.ParameterType}"))}) with generic arguments [{string.Join(", ", attribute.Types.Select(x => x.FullName))}] - Parameter or return type mismatch (found: ({string.Join(", ", srcMethod.GetParameters().Select(x => $"{x.ParameterType}"))}) => {srcMethod.ReturnType.FullName})");
                                    continue;
                                }
                            }
                            else
                            {
                                srcMethod = targetType.GetMethod(method.Name, allFlags, null, [.. method.GetParameters().Select(x => x.ParameterType)], null);
                                if (srcMethod == null)
                                {
                                    log.Warn($"Method not found while patching {dllName}: {targetType.FullName} {method.Name}({string.Join(", ", method.GetParameters().Select(x => $"{x.ParameterType}"))})");
                                    continue;
                                }
                                if (srcMethod.IsGenericMethod)
                                {
                                    log.Warn($"Method {srcMethod} is generic but doesn't have {nameof(PatchGenericMethod)} attribute while patching {dllName}: {targetType.FullName} {srcMethod.Name}({string.Join(", ", method.GetParameters().Select(x => $"{x.ParameterType}"))})");
                                    continue;
                                }
                            }
                            if (srcMethod != null)
                            {
                                Harmony.ReversePatch(srcMethod, new HarmonyMethod(method));
                                log.Info($"Reverse Patched: {srcMethod} => {method}");
                            }
                            else
                            {
                                log.Warn($"Method not found while patching {dllName}: {targetType.FullName} {srcMethod.Name}({string.Join(", ", method.GetParameters().Select(x => $"{x.ParameterType}"))})");
                            }
                        }
                    }
                    return true;
                }
                else
                {
                    log.Warn($"{dllName}.dll file required for using this mod! Check if it's enabled.");
                    return false;
                }
            }
            catch (Exception e)
            {
                log.Warn($"{dllName}.dll file required for using this mod! Check if it's enabled. Error loading.\n{e}");
                return false;
            }
        }

        public void OnDispose()
        {
            log.Info(nameof(OnDispose));
        }

    }
}
