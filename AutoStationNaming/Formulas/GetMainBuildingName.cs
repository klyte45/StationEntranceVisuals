using System;
using Colossal.Entities;
using Game.Common;
using Game.UI;
using Unity.Entities;

namespace AutoStationNaming.Formulas;

internal static class BuildingName
{

    private static NameSystem _nameSystem;

    public static readonly Func<Entity, string> GetMainBuildingNameBinding = (buildingRef) =>
    {
        _nameSystem ??= World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<NameSystem>();
        World.DefaultGameObjectInjectionWorld.EntityManager.TryGetComponent<Owner>(buildingRef, out var owner);

        return _nameSystem.GetRenderedLabelName(owner.m_Owner);
    };

    public static readonly Func<Entity, string> GetBuildingNameBinding = (buildingRef) =>
    {
        _nameSystem ??= World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<NameSystem>();

        return _nameSystem.GetRenderedLabelName(buildingRef);
    };

    public static string GetBuildingName(Entity buildingRef) => GetBuildingNameBinding?.Invoke(buildingRef) ?? "<???>";

    public static string GetMainBuildingName(Entity buildingRef) => GetMainBuildingNameBinding?.Invoke(buildingRef) ?? "<???>";
}