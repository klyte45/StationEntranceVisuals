﻿using System;
using System.Reflection;

namespace StationEntranceVisuals.BridgeWE
{
    public static class WEImageManagementBridge
    {
        public static string GetImageAtlasVersion() => throw new NotImplementedException("Stub only!");
        public static void RegisterImageAtlas(Assembly mainAssembly, string atlasName, string[] imagePaths, Action<string> onCompleteLoading)
            => throw new NotImplementedException("Stub only!");
        public static void RegisterForAtlasCacheResetNotification(Action onLocalCacheAtlasReset) => throw new NotImplementedException("Stub only!");
        public static void UnregisterForAtlasCacheResetNotification(Action onLocalCacheAtlasReset) => throw new NotImplementedException("Stub only!");
        public static bool CheckImageAtlasExists(Assembly mainAssembly, string atlasName) => throw new NotImplementedException("Stub only!");
        public static void EnsureAtlasDeleted(Assembly mainAssembly, string atlasName) => throw new NotImplementedException("Stub only!");
    }
}