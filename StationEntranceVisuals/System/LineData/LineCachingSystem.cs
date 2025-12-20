using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using StationEntranceVisuals.Formulas;

namespace StationEntranceVisuals.Systems.LineData;

/// <summary>
/// Caching system for line data to avoid redundant calculations
/// Uses native collections for better performance
/// </summary>
public partial class LineCachingSystem : SystemBase
{
    private const int CACHE_LIFETIME_FRAMES = 120;

    private struct CachedLineData
    {
        public NativeList<NativeLineDescriptor> Lines;
        public int FrameCalculated;
        public bool IsValid => Lines.IsCreated;
    }

    private NativeHashMap<Entity, CachedLineData> _nativeCache;
    private bool _isInitialized;

    protected override void OnCreate()
    {
        base.OnCreate();
        _nativeCache = new NativeHashMap<Entity, CachedLineData>(64, Allocator.Persistent);
        _isInitialized = true;
    }

    protected override void OnDestroy()
    {
        if (_isInitialized)
        {
            // Dispose all cached lists
            var keys = _nativeCache.GetKeyArray(Allocator.Temp);
            for (int i = 0; i < keys.Length; i++)
            {
                if (_nativeCache.TryGetValue(keys[i], out var cached) && cached.IsValid)
                {
                    cached.Lines.Dispose();
                }
            }
            keys.Dispose();
            _nativeCache.Dispose();
        }
        base.OnDestroy();
    }

    protected override void OnUpdate()
    {
        // Periodic cleanup of expired cache entries
        int currentFrame = UnityEngine.Time.frameCount;
        var keys = _nativeCache.GetKeyArray(Allocator.Temp);
        
        for (int i = 0; i < keys.Length; i++)
        {
            var key = keys[i];
            if (_nativeCache.TryGetValue(key, out var cached))
            {
                if (currentFrame - cached.FrameCalculated > CACHE_LIFETIME_FRAMES)
                {
                    if (cached.IsValid)
                    {
                        cached.Lines.Dispose();
                    }
                    _nativeCache.Remove(key);
                }
            }
        }
        
        keys.Dispose();
    }

    /// <summary>
    /// Tries to get cached line data
    /// </summary>
    public bool TryGetCached(Entity entity, out NativeArray<NativeLineDescriptor> lines)
    {
        if (_nativeCache.TryGetValue(entity, out var cached))
        {
            int currentFrame = UnityEngine.Time.frameCount;
            if (currentFrame - cached.FrameCalculated < CACHE_LIFETIME_FRAMES && cached.IsValid)
            {
                lines = cached.Lines.AsArray();
                return true;
            }
            else if (cached.IsValid)
            {
                // Expired, dispose and remove
                cached.Lines.Dispose();
                _nativeCache.Remove(entity);
            }
        }

        lines = default;
        return false;
    }

    /// <summary>
    /// Caches line data for an entity
    /// </summary>
    public void Cache(Entity entity, NativeList<NativeLineDescriptor> lines)
    {
        // Remove old cache if exists
        if (_nativeCache.TryGetValue(entity, out var oldCached) && oldCached.IsValid)
        {
            oldCached.Lines.Dispose();
        }

        // Create a persistent copy
        var persistentLines = new NativeList<NativeLineDescriptor>(lines.Length, Allocator.Persistent);
        persistentLines.AddRange(lines.AsArray());

        var cachedData = new CachedLineData
        {
            Lines = persistentLines,
            FrameCalculated = UnityEngine.Time.frameCount
        };

        _nativeCache[entity] = cachedData;
    }

    /// <summary>
    /// Clears all cached data
    /// </summary>
    public void ClearCache()
    {
        var keys = _nativeCache.GetKeyArray(Allocator.Temp);
        for (int i = 0; i < keys.Length; i++)
        {
            if (_nativeCache.TryGetValue(keys[i], out var cached) && cached.IsValid)
            {
                cached.Lines.Dispose();
            }
        }
        keys.Dispose();
        _nativeCache.Clear();
    }

    /// <summary>
    /// Invalidates cache for a specific entity
    /// </summary>
    public void InvalidateCache(Entity entity)
    {
        if (_nativeCache.TryGetValue(entity, out var cached) && cached.IsValid)
        {
            cached.Lines.Dispose();
            _nativeCache.Remove(entity);
        }
    }
}
