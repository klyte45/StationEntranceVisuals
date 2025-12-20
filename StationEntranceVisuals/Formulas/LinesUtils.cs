using Game.Prefabs;
using System;
using System.Collections.Generic;
using System.Linq;
using StationEntranceVisuals.Utils;
using Unity.Entities;
using UnityEngine;
using Color = UnityEngine.Color;
using StationEntranceVisuals.Systems;

namespace StationEntranceVisuals.Formulas;

public static class LinesUtils
{
    private const string SUBBUILDING_ONLY_VAR = "SUBBUILDING_ONLY";
    private const string INVERSE_ORDER = "INVERSE_ORDER";
    internal const string LINETYPE_VAR = "lineType";
    private const string CURRENT_INDEX_VAR = "$idx";

    /// <summary>
    /// Gets lines using the burst-optimized system
    /// </summary>
    private static HashSet<LineDescriptor> GetLines(Entity selectedEntity, bool iterateToOwner)
    {
        var coordinatorSystem = SEV_BuildingLineCacheSystem.Instance;
        var lines = coordinatorSystem.GetLines(selectedEntity, iterateToOwner);
        return [.. lines];
    }

    internal static HashSet<LineDescriptor> GetFilteredLinesList(Entity buildingRef, string lineType, bool iterateToOwner)
    {
        if (Mod.m_Setting.EnableLayoutValidation)
        {
            return MockLineUtils.GetMockLineDescriptors(buildingRef, lineType);
        }

        var coordinatorSystem = SEV_BuildingLineCacheSystem.Instance;
        var lines = coordinatorSystem.GetFilteredLines(buildingRef, lineType, iterateToOwner);
        return lines.ToHashSet();
    }

    private static LineDescriptor GetLine(Entity buildingRef, int index, string lineType, bool iterateToOwner, bool inverse)
    {
        var coordinatorSystem = SEV_BuildingLineCacheSystem.Instance;
        var line = coordinatorSystem.GetLineByIndex(buildingRef, lineType, index, iterateToOwner, inverse);
        return line ?? default;
    }

    public static int GetLineCount(Entity buildingRef, Dictionary<string, string> vars)
        => vars.TryGetValue(LINETYPE_VAR, out var lineType) ? GetFilteredLinesList(buildingRef, lineType, !vars.ContainsKey(SUBBUILDING_ONLY_VAR)).Count : -1;
    public static int WillShowNthLine(Entity buildingRef, Dictionary<string, string> vars)
        => vars.TryGetValue(CURRENT_INDEX_VAR, out var idxStr) && int.TryParse(idxStr, out int idx) && vars.TryGetValue(LINETYPE_VAR, out var lineType)
            ? GetFilteredLinesList(buildingRef, lineType, !vars.ContainsKey(SUBBUILDING_ONLY_VAR)).Count - idx
            : -1;

    public static LineDescriptor GetLineData(Entity buildingRef, Dictionary<string, string> vars)
        => vars.TryGetValue(CURRENT_INDEX_VAR, out var idxStr) && int.TryParse(idxStr, out int idx) && vars.TryGetValue(LINETYPE_VAR, out var lineType)
        ? GetLine(buildingRef, idx, lineType, !vars.ContainsKey(SUBBUILDING_ONLY_VAR), vars.ContainsKey(INVERSE_ORDER))
        : default;

    public static Color GetMainColor(Entity buildingRef, Dictionary<string, string> vars)
    {
        var lineList = GetLines(buildingRef, true);
        return lineList.Count != 1 ? Color.white : lineList.First().Color;
    }
}
