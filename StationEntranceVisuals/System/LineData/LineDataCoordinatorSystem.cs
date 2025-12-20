using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Game.Prefabs;
using Game.UI;
using StationEntranceVisuals.Formulas;
using StationEntranceVisuals.BridgeWE;
using UnityEngine;

namespace StationEntranceVisuals.Systems.LineData;

/// <summary>
/// Coordinator system that orchestrates the burst-optimized line data extraction
/// and provides high-level API for the Formulas namespace
/// </summary>
public partial class LineDataCoordinatorSystem : SystemBase
{
    private EntityHierarchySystem _hierarchySystem;
    private LineExtractionSystem _extractionSystem;
    private LineCachingSystem _cachingSystem;
    private NameSystem _nameSystem;

    public static LineDataCoordinatorSystem Instance { get; private set; }

    protected override void OnCreate()
    {
        base.OnCreate();
        Instance = this;
        
        _hierarchySystem = World.GetOrCreateSystemManaged<EntityHierarchySystem>();
        _extractionSystem = World.GetOrCreateSystemManaged<LineExtractionSystem>();
        _cachingSystem = World.GetOrCreateSystemManaged<LineCachingSystem>();
    }

    protected override void OnUpdate()
    {
        // This system is manually invoked, not through OnUpdate
    }

    /// <summary>
    /// Gets all lines for a building entity, using cache when possible
    /// </summary>
    public List<LineDescriptor> GetLines(Entity buildingEntity, bool iterateToOwner)
    {
        // Find root entity if requested
        Entity rootEntity = buildingEntity;
        if (iterateToOwner)
        {
            rootEntity = _hierarchySystem.FindRootEntity(buildingEntity);
        }

        // Try to get from cache
        if (_cachingSystem.TryGetCached(rootEntity, out var cachedLines))
        {
            return ConvertToLineDescriptors(cachedLines);
        }

        // Extract lines using burst-optimized system
        var nativeLines = _extractionSystem.ExtractLines(rootEntity, Allocator.TempJob);

        // Cache the results
        _cachingSystem.Cache(rootEntity, nativeLines);

        // Convert to managed type
        var result = ConvertToLineDescriptors(nativeLines.AsArray());

        nativeLines.Dispose();

        return result;
    }

    /// <summary>
    /// Gets filtered and sorted lines
    /// </summary>
    public List<LineDescriptor> GetFilteredLines(Entity buildingEntity, string lineType, bool iterateToOwner, bool sortDescending = false)
    {
        var allLines = GetLines(buildingEntity, iterateToOwner);

        // Apply filtering
        List<LineDescriptor> filtered;
        if (lineType == "All")
        {
            filtered = allLines;
        }
        else
        {
            var transportTypes = ParseTransportTypes(lineType);
            filtered = allLines.Where(x => transportTypes.Contains(x.TransportType)).ToList();
        }

        // Apply sorting
        if (sortDescending)
        {
            filtered = filtered.OrderByDescending(x => x.Number).ToList();
        }
        else
        {
            filtered = filtered.OrderBy(x => x.Number).ToList();
        }

        return filtered;
    }

    /// <summary>
    /// Gets a specific line by index after filtering and sorting
    /// </summary>
    public LineDescriptor? GetLineByIndex(Entity buildingEntity, string lineType, int index, bool iterateToOwner, bool sortDescending = false)
    {
        var filtered = GetFilteredLines(buildingEntity, lineType, iterateToOwner, sortDescending);
        
        if (index >= 0 && index < filtered.Count)
        {
            return filtered[index];
        }

        return null;
    }

    /// <summary>
    /// Invalidates cache for an entity (useful when routes change)
    /// </summary>
    public void InvalidateCache(Entity entity)
    {
        _cachingSystem.InvalidateCache(entity);
    }

    /// <summary>
    /// Converts native line descriptors to managed LineDescriptor structs
    /// </summary>
    private List<LineDescriptor> ConvertToLineDescriptors(NativeArray<NativeLineDescriptor> nativeLines)
    {
        _nameSystem ??= World.GetOrCreateSystemManaged<NameSystem>();
        
        var result = new List<LineDescriptor>(nativeLines.Length);
        
        for (int i = 0; i < nativeLines.Length; i++)
        {
            var nativeLine = nativeLines[i];
            var color = nativeLine.GetUnityColor();
            
            // Get string-based data that couldn't be in burst job
            var acronym = WERouteFn.GetTransportLineNumber(nativeLine.Entity);
            var smallName = GetSmallLineName(nativeLine.Entity, nativeLine.Number);

            var descriptor = new LineDescriptor(
                nativeLine.Entity,
                nativeLine.TransportType,
                nativeLine.IsCargo,
                nativeLine.IsPassenger,
                acronym,
                nativeLine.Number,
                smallName,
                color
            );

            result.Add(descriptor);
        }

        return result;
    }

    /// <summary>
    /// Gets a short line name based on the rendered label
    /// </summary>
    private string GetSmallLineName(Entity lineEntity, int routeNumber)
    {
        _nameSystem ??= World.GetOrCreateSystemManaged<NameSystem>();
        
        var lineName = _nameSystem.GetRenderedLabelName(lineEntity).Split(' ').LastOrDefault();
        return lineName is { Length: >= 1 and <= 3 } ? lineName : routeNumber.ToString();
    }

    /// <summary>
    /// Parses transport type string into list of transport types
    /// </summary>
    private List<TransportType> ParseTransportTypes(string lineType)
    {
        return lineType.Split(',')
            .Select(x => Enum.TryParse<TransportType>(x.Trim(), out var transportType) ? transportType as TransportType? : null)
            .Where(x => x.HasValue)
            .Select(x => x.Value)
            .ToList();
    }
}
