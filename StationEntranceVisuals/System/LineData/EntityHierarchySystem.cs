using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Game.Common;

namespace StationEntranceVisuals.Systems.LineData;

/// <summary>
/// System responsible for traversing entity hierarchies to find the root building
/// </summary>
public partial class EntityHierarchySystem : SystemBase
{
    private EntityQuery _ownerQuery;

    protected override void OnCreate()
    {
        base.OnCreate();
        _ownerQuery = GetEntityQuery(ComponentType.ReadOnly<Owner>());
    }

    protected override void OnUpdate()
    {
        // This system is manually executed via jobs, not through OnUpdate
    }

    /// <summary>
    /// Finds the root entity by traversing up the Owner hierarchy
    /// </summary>
    public Entity FindRootEntity(Entity startEntity)
    {
        var job = new EntityHierarchyTraversalJob
        {
            StartEntity = startEntity,
            OwnerLookup = GetComponentLookup<Owner>(),
            Result = new NativeReference<Entity>(Allocator.TempJob)
        };

        job.Schedule().Complete();
        var result = job.Result.Value;
        job.Result.Dispose();
        return result;
    }

    [BurstCompile]
    private struct EntityHierarchyTraversalJob : IJob
    {
        public Entity StartEntity;
        [ReadOnly] public ComponentLookup<Owner> OwnerLookup;
        public NativeReference<Entity> Result;

        public void Execute()
        {
            Entity current = StartEntity;
            int maxIterations = 100; // Safety limit to prevent infinite loops
            int iterations = 0;

            while (iterations < maxIterations && OwnerLookup.TryGetComponent(current, out var owner) && owner.m_Owner != Entity.Null)
            {
                current = owner.m_Owner;
                iterations++;
            }

            Result.Value = current;
        }
    }
}
