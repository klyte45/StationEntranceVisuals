using Game;
using Game.Common;
using Game.Prefabs;
using Game.Routes;
using Game.Tools;
using Game.UI.InGame;
using Unity.Collections;
using Unity.Entities;
using TransportStation = Game.Buildings.TransportStation;

namespace AutoStationNaming.System;

public partial class StationsSystem : GameSystemBase
{
    private EntityQuery m_linesQueue;
    private PrefabSystem m_prefabSystem;

    protected override void OnUpdate()
    {
        m_linesQueue = GetEntityQuery(new EntityQueryDesc
        {
            All =
            [
                ComponentType.ReadOnly<Route>(),
                ComponentType.ReadWrite<RouteNumber>(),
                ComponentType.ReadWrite<TransportLine>(),
                ComponentType.ReadOnly<RouteWaypoint>(),
                ComponentType.ReadOnly<PrefabRef>()
            ],
            None =
            [
                ComponentType.ReadOnly<Deleted>(),
                ComponentType.ReadOnly<Temp>()
            ]
        });
        RequireForUpdate(m_linesQueue);
    }

    public NativeArray<UITransportLineData> GetLinesQueue()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        m_prefabSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<PrefabSystem>();
        
        return TransportUIUtils.GetSortedLines(m_linesQueue, entityManager, m_prefabSystem);
    }
}