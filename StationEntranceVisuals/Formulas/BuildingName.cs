using Colossal.Entities;
using Game.Common;
using Game.SceneFlow;
using Game.UI;
using System;
using System.Collections.Generic;
using Unity.Entities;

namespace StationEntranceVisuals.Formulas;

internal static class BuildingName
{

    private static NameSystem _nameSystem;

    private static readonly Func<Entity, string> GetMainBuildingNameBinding = (buildingRef) =>
    {
        _nameSystem ??= World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<NameSystem>();
        while (World.DefaultGameObjectInjectionWorld.EntityManager.TryGetComponent<Owner>(buildingRef, out var owner) && owner.m_Owner != Entity.Null)
        {
            buildingRef = owner.m_Owner;
        }

        return _nameSystem.GetRenderedLabelName(buildingRef);
    };

    private static readonly Func<Entity, string> GetBuildingNameBinding = (buildingRef) =>
    {
        _nameSystem ??= World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<NameSystem>();

        return _nameSystem.GetRenderedLabelName(buildingRef);
    };

    private static string GetEntranceLocalizedName()
    {
        return GameManager.instance.localizationManager.activeDictionary.TryGetValue("StationEntranceVisuals.Entrance", out var result) && result.Length > 0 ? result : "Entrance";
    }

    public static string GetBuildingName(Entity buildingRef) => GetBuildingNameBinding?.Invoke(buildingRef) ?? "<???>";

    public static string GetMainBuildingName(Entity buildingRef) => GetMainBuildingNameBinding?.Invoke(buildingRef) ?? "<???>";

    public static string GetEntranceLocalizedName(Entity buildingRef, Dictionary<string, string> vars) => $"{GetEntranceLocalizedName()} {(vars.TryGetValue("entrance#", out var entrance) ? entrance : "?")}";
}