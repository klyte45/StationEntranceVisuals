using Colossal.Entities;
using Colossal.IO.AssetDatabase.Internal;
using Game.Buildings;
using Game.Common;
using Game.Prefabs;
using StationEntranceVisuals.BridgeWE;
using System;
using System.Collections.Generic;
using System.Linq;
using Game.Routes;
using Game.UI;
using StationEntranceVisuals.Utils;
using Unity.Entities;
using UnityEngine;
using Color = UnityEngine.Color;
using SubObject = Game.Objects.SubObject;

namespace StationEntranceVisuals.Formulas;

public static class LinesUtils
{
    private const string SUBBUILDING_ONLY_VAR = "SUBBUILDING_ONLY";
    private const string INVERSE_ORDER = "INVERSE_ORDER";
    internal const string LINETYPE_VAR = "lineType";
    private const string CURRENT_INDEX_VAR = "$idx";
    
    private static NameSystem _nameSystem;

    private static readonly Dictionary<Entity, (HashSet<LineDescriptor> desc, int frameCalcuated)> m_cacheData = [];

    private static HashSet<LineDescriptor> GetLines(EntityManager entityManager, Entity selectedEntity, bool iterateToOwner)
    {
        if (iterateToOwner)
        {
            while (entityManager.TryGetComponent<Owner>(selectedEntity, out var buildingOwner))
            {
                selectedEntity = buildingOwner.m_Owner;
            }
        }

        if (m_cacheData.TryGetValue(selectedEntity, out var data) && Time.frameCount - data.frameCalcuated < 120)
        {
            return data.desc;
        }

        var lineNumberList = new HashSet<LineDescriptor>();
        m_cacheData[selectedEntity] = (lineNumberList, Time.frameCount);
        ExtractLines(entityManager, selectedEntity, lineNumberList);
        if (entityManager.TryGetBuffer(selectedEntity, true, out DynamicBuffer<SubObject> subObjects))
        {
            foreach (var subObject in subObjects)
            {
                ExtractLines(entityManager, subObject.m_SubObject, lineNumberList);
            }
        }
        if (entityManager.TryGetBuffer(selectedEntity, true, out DynamicBuffer<InstalledUpgrade> upgrades))
        {
            foreach (var upgrade in upgrades)
            {
                GetLines(entityManager, upgrade.m_Upgrade, false).ForEach((x) => lineNumberList.Add(x));
            }
        }
        return lineNumberList;
    }

    private static void ExtractLines(EntityManager entityManager, Entity selectedEntity, HashSet<LineDescriptor> lineNumberList)
    {
        if (entityManager.TryGetBuffer(selectedEntity, true, out DynamicBuffer<ConnectedRoute> routes))
        {
            foreach (var route in routes)
            {
                if (entityManager.TryGetComponent<Owner>(route.m_Waypoint, out var owner)
                    && entityManager.TryGetComponent<PrefabRef>(owner.m_Owner, out var prefabRef)
                    && entityManager.TryGetComponent<TransportLineData>(prefabRef.m_Prefab, out var lineData)
                    && entityManager.TryGetComponent<Game.Routes.Color>(owner.m_Owner, out var lineColor)
                    && entityManager.TryGetComponent<RouteNumber>(owner.m_Owner, out var lineNumber)
                    )
                {
                    lineNumberList.Add(
                        new LineDescriptor(
                            owner.m_Owner,
                            lineData.m_TransportType,
                            lineData.m_CargoTransport,
                            lineData.m_PassengerTransport, 
                            WERouteFn.GetTransportLineNumber(owner.m_Owner),
                            lineNumber.m_Number,
                            GetSmallLineName(owner, lineNumber),
                            lineColor.m_Color
                        )
                    );
                }
            }
        }
    }

    private static string GetSmallLineName(Owner owner, RouteNumber routeNumber)
    {
        var lineName = _nameSystem.GetRenderedLabelName(owner.m_Owner).Split(' ').LastOrDefault();
        return lineName is { Length: >= 1 and <= 3 } ? lineName : routeNumber.m_Number.ToString();
    }

    internal static HashSet<LineDescriptor> GetFilteredLinesList(Entity buildingRef, string lineType, bool iterateToOwner)
    {
        if (Mod.m_Setting.EnableLayoutValidation)
        {
            return MockLineUtils.GetMockLineDescriptors(buildingRef, lineType);
        }
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        _nameSystem ??= World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<NameSystem>();
        var lineNumberList = GetLines(entityManager, buildingRef, iterateToOwner);

        var lineTypes = lineType.Split(',')
            .Select(x => Enum.TryParse<TransportType>(lineType, out var transportType) ? transportType as TransportType? : null)
            .Where(x => x.HasValue)
            .Select(x => x.Value)
            .ToList();
        return lineTypes.Count > 0
            ? lineNumberList
                .Where(x => lineTypes.Contains(x.TransportType))
                .ToHashSet()
            : lineNumberList;
    }

    private static LineDescriptor GetLine(Entity buildingRef, int index, string lineType, bool iterateToOwner, bool inverse)
    {
        var filteredLine = GetFilteredLinesList(buildingRef, lineType, iterateToOwner);
        if (inverse)
        {
            return filteredLine.OrderByDescending(t => t.GetOrderingIndex()).ElementAtOrDefault(index);
        }

        return filteredLine.OrderBy(t => t.GetOrderingIndex()).ElementAtOrDefault(index);
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
}
