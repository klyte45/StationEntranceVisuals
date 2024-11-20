using System;
using Colossal.Entities;
using Game.Common;
using Game.SceneFlow;
using Game.UI;
using Unity.Entities;

namespace StationEntranceVisuals.Formulas;

internal static class BuildingName
{

    private static NameSystem _nameSystem;

    private static readonly Func<Entity, string> GetMainBuildingNameBinding = (buildingRef) =>
    {
        _nameSystem ??= World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<NameSystem>();
        World.DefaultGameObjectInjectionWorld.EntityManager.TryGetComponent<Owner>(buildingRef, out var owner);

        return _nameSystem.GetRenderedLabelName(owner.m_Owner);
    };

    private static readonly Func<Entity, string> GetBuildingNameBinding = (buildingRef) =>
    {
        _nameSystem ??= World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<NameSystem>();

        return _nameSystem.GetRenderedLabelName(buildingRef);
    };

    private static readonly Func<Entity, string> GetEntranceLocalizedNameBinding = (_) =>
    {
        GameManager.instance.localizationManager.activeDictionary.TryGetValue("StationEntranceVisuals.Entrance", out var result);
        return result.Length > 0 ? result : "Entrance";
    };

    public static string GetBuildingName(Entity buildingRef) => GetBuildingNameBinding?.Invoke(buildingRef) ?? "<???>";

    public static string GetMainBuildingName(Entity buildingRef) => GetMainBuildingNameBinding?.Invoke(buildingRef) ?? "<???>";
    
    public static string GetEntranceALocalizedName(Entity buildingRef) => GetEntranceLocalizedNameBinding?.Invoke(buildingRef) + " A";
    
    public static string GetEntranceBLocalizedName(Entity buildingRef) => GetEntranceLocalizedNameBinding?.Invoke(buildingRef) + " B";
    
    public static string GetEntranceCLocalizedName(Entity buildingRef) => GetEntranceLocalizedNameBinding?.Invoke(buildingRef) + " C";
}