using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Game.Buildings;
using Game.Common;
using Game.Prefabs;
using Game.Routes;
using SubObject = Game.Objects.SubObject;

namespace StationEntranceVisuals.Systems.LineData;

/// <summary>
/// System responsible for extracting line data from connected routes
/// </summary>
public partial class LineExtractionSystem : SystemBase
{
    private EntityQuery _connectedRouteQuery;

    protected override void OnCreate()
    {
        base.OnCreate();
        _connectedRouteQuery = GetEntityQuery(ComponentType.ReadOnly<ConnectedRoute>());
    }

    protected override void OnUpdate()
    {
        // This system is manually executed via jobs, not through OnUpdate
    }

    /// <summary>
    /// Extracts all lines connected to the given entity and its sub-objects/upgrades
    /// </summary>
    public NativeList<NativeLineDescriptor> ExtractLines(Entity rootEntity, Allocator allocator)
    {
        var results = new NativeList<NativeLineDescriptor>(allocator);
        var processedEntities = new NativeHashSet<Entity>(16, Allocator.TempJob);
        var entitiesToProcess = new NativeQueue<Entity>(Allocator.TempJob);

        entitiesToProcess.Enqueue(rootEntity);

        var job = new LineExtractionJob
        {
            EntitiesToProcess = entitiesToProcess,
            ProcessedEntities = processedEntities,
            Results = results,
            OwnerLookup = SystemAPI.GetComponentLookup<Owner>(true),
            PrefabRefLookup = SystemAPI.GetComponentLookup<PrefabRef>(true),
            TransportLineDataLookup = SystemAPI.GetComponentLookup<TransportLineData>(true),
            ColorLookup = SystemAPI.GetComponentLookup<Game.Routes.Color>(true),
            RouteNumberLookup = SystemAPI.GetComponentLookup<RouteNumber>(true),
            ConnectedRouteLookup = SystemAPI.GetBufferLookup<ConnectedRoute>(true),
            SubObjectLookup = SystemAPI.GetBufferLookup<SubObject>(true),
            InstalledUpgradeLookup = SystemAPI.GetBufferLookup<InstalledUpgrade>(true)
        };

        job.Schedule().Complete();

        processedEntities.Dispose();
        entitiesToProcess.Dispose();

        return results;
    }

    [BurstCompile]
    private struct LineExtractionJob : IJob
    {
        public NativeQueue<Entity> EntitiesToProcess;
        public NativeHashSet<Entity> ProcessedEntities;
        public NativeList<NativeLineDescriptor> Results;

        [ReadOnly] public ComponentLookup<Owner> OwnerLookup;
        [ReadOnly] public ComponentLookup<PrefabRef> PrefabRefLookup;
        [ReadOnly] public ComponentLookup<TransportLineData> TransportLineDataLookup;
        [ReadOnly] public ComponentLookup<Game.Routes.Color> ColorLookup;
        [ReadOnly] public ComponentLookup<RouteNumber> RouteNumberLookup;
        [ReadOnly] public BufferLookup<ConnectedRoute> ConnectedRouteLookup;
        [ReadOnly] public BufferLookup<SubObject> SubObjectLookup;
        [ReadOnly] public BufferLookup<InstalledUpgrade> InstalledUpgradeLookup;

        public void Execute()
        {
            while (EntitiesToProcess.TryDequeue(out var entity))
            {
                if (!ProcessedEntities.Add(entity))
                {
                    continue; // Already processed
                }

                // Extract lines from this entity
                ExtractLinesFromEntity(entity);

                // Add sub-objects to queue
                if (SubObjectLookup.TryGetBuffer(entity, out var subObjects))
                {
                    for (int i = 0; i < subObjects.Length; i++)
                    {
                        EntitiesToProcess.Enqueue(subObjects[i].m_SubObject);
                    }
                }

                // Add upgrades to queue
                if (InstalledUpgradeLookup.TryGetBuffer(entity, out var upgrades))
                {
                    for (int i = 0; i < upgrades.Length; i++)
                    {
                        EntitiesToProcess.Enqueue(upgrades[i].m_Upgrade);
                    }
                }
            }
        }

        private void ExtractLinesFromEntity(Entity entity)
        {
            if (!ConnectedRouteLookup.TryGetBuffer(entity, out var routes))
            {
                return;
            }

            for (int i = 0; i < routes.Length; i++)
            {
                var route = routes[i];
                
                if (!OwnerLookup.TryGetComponent(route.m_Waypoint, out var owner))
                    continue;
                if (!PrefabRefLookup.TryGetComponent(owner.m_Owner, out var prefabRef))
                    continue;
                if (!TransportLineDataLookup.TryGetComponent(prefabRef.m_Prefab, out var lineData))
                    continue;
                if (!ColorLookup.TryGetComponent(owner.m_Owner, out var lineColor))
                    continue;
                if (!RouteNumberLookup.TryGetComponent(owner.m_Owner, out var lineNumber))
                    continue;

                var descriptor = new NativeLineDescriptor(
                    owner.m_Owner,
                    lineData.m_TransportType,
                    lineData.m_CargoTransport,
                    lineData.m_PassengerTransport,
                    lineNumber.m_Number,
                    lineColor.m_Color
                );

                // Check if already exists (using entity as unique identifier)
                bool exists = false;
                for (int j = 0; j < Results.Length; j++)
                {
                    if (Results[j].Entity == descriptor.Entity)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    Results.Add(descriptor);
                }
            }
        }
    }
}
