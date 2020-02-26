
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

public struct SelectedTag : IComponentData
{

}
public class UnitControlSystem : JobComponentSystem
{

    private float3 startPosition;
    private float3 endPosition;
    private float3 currentPosition;
    private float3 lowerLeftPosition;

    private float3 upperRightPosition;

    private float3 selectionAreaSize;

    private bool selection;

    private float selectionAreaMinSize = 10f;
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBuffer;

    private BuildPhysicsWorld physicsWorldSystem;

    [BurstCompile]
    [RequireComponentTag(typeof(SelectableTag))]
    struct UnitControlSystemJob : IJobForEachWithEntity<Translation>
    {
        public float3 lowerLeftPosition;

        public float3 upperRightPosition;

        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        public bool selection;
        public void Execute(Entity entity, int index, ref Translation translation)
        {
            float3 entityPosition = translation.Value;
            if (entityPosition.x >= lowerLeftPosition.x &&
                entityPosition.y >= lowerLeftPosition.y &&
                entityPosition.x <= upperRightPosition.x &&
                entityPosition.y <= upperRightPosition.y)
            {
                entityCommandBuffer.AddComponent(index, entity, new SelectedTag());
            }
        }
    }

    [BurstCompile]
    struct SelectionJob : IJobFor
    {
        public float3 lowerLeftPosition;

        public float3 upperRightPosition;

        public float3 startPosition;

        public float3 endPosition;

        public float3 selectionAreaSize;

        public EntityCommandBuffer.Concurrent entityCommandBuffer;

        [Unity.Collections.ReadOnly] public CollisionWorld collisionWorld;
        [ReadOnly] public PhysicsWorld physicsWorld;
        unsafe public void Execute(int index)
        {
            var center = math.lerp(upperRightPosition, lowerLeftPosition, 0.5f);
            var size = new float3(startPosition.x - endPosition.x,
                startPosition.y - endPosition.y,
                1);
            var filter = new CollisionFilter()
            {
                BelongsTo = ~0u,
                CollidesWith = ~0u, // all 1s, so all layers, collide with everything
                GroupIndex = 0
            };

            BoxGeometry boxGeometry = new BoxGeometry()
            {
                Center = new float3(0, 0, 0) - new float3(startPosition.x - endPosition.x,
                   startPosition.y - endPosition.y,
                    1) / 2,
                Size = (size * math.sign(size)) + new float3(1, 1, 1),
                Orientation = quaternion.identity,
                BevelRadius = 0,
            };
            BlobAssetReference<Unity.Physics.Collider> boxColliser = Unity.Physics.BoxCollider.Create(boxGeometry, filter);
            ColliderCastInput input = new ColliderCastInput()
            {
                Collider = (Unity.Physics.Collider*)boxColliser.GetUnsafePtr(),
                Orientation = quaternion.identity,
                Start = startPosition,
                End = startPosition
            };
            NativeList<ColliderCastHit> allHits = new NativeList<ColliderCastHit>(500, Allocator.Temp);
            bool haveHit = collisionWorld.CastCollider(input, ref allHits);
            if (haveHit)
            {
                for (int i = 0; i < allHits.Length; i++)
                {
                    var hit = allHits[i];
                    Entity e = physicsWorld.Bodies[hit.RigidBodyIndex].Entity;
                    entityCommandBuffer.AddComponent(index, e, new SelectedTag());
                }
            }
        }
    }

    [BurstCompile]
    struct DeselectJob : IJobForEachWithEntity<HasSelectionCircle>
    {
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        public void Execute(Entity entity, int index, [Unity.Collections.ReadOnly] ref HasSelectionCircle sec)
        {
            entityCommandBuffer.RemoveComponent<SelectedTag>(index, entity);
            entityCommandBuffer.RemoveComponent<HasSelectionCircle>(index, entity);
            entityCommandBuffer.DestroyEntity(index, sec.circle);
        }
    }
    protected override void OnCreate()
    {
        EntityManager.CreateEntity(typeof(SelectionAreaData));
        endSimulationEntityCommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        physicsWorldSystem = World.GetExistingSystem<BuildPhysicsWorld>();
    }
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var handleSchedule = HandleInput(inputDependencies);
        if (selection && SelectionEnded())
        {
            /* var job = new UnitControlSystemJob () {
                selection = true,
                lowerLeftPosition = lowerLeftPosition,
                upperRightPosition = upperRightPosition,
                entityCommandBuffer = endSimulationEntityCommandBuffer.CreateCommandBuffer ().ToConcurrent ()
            }; 
             var schedule = job.Schedule (this, handleSchedule);
            */
            var job = new SelectionJob()
            {
                physicsWorld = physicsWorldSystem.PhysicsWorld,
                collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld,
                entityCommandBuffer = endSimulationEntityCommandBuffer.CreateCommandBuffer().ToConcurrent(),
                lowerLeftPosition = lowerLeftPosition,
                upperRightPosition = upperRightPosition,
                startPosition = startPosition,
                endPosition = endPosition,
                selectionAreaSize = selectionAreaSize
            };
            var schedule = job.Schedule(1, handleSchedule);
            endSimulationEntityCommandBuffer.AddJobHandleForProducer(schedule);
            schedule.Complete();
            return schedule;
        }
        return handleSchedule;
    }
    private bool SelectionEnded()
    {
        return math.any(endPosition != float3.zero);
    }
    private void SetSelectionData()
    {
        selectionAreaSize = (lowerLeftPosition - upperRightPosition) * math.sign(startPosition - GetWorldMousePoint()) * new float3(1, 1, 0);
        SetSingleton<SelectionAreaData>(new SelectionAreaData()
        {
            isSelecting = selection,
            lowerLeftPosition = lowerLeftPosition,
            upperRightPosition = upperRightPosition,
            startPosition = startPosition,
            selectionAreaSize = selectionAreaSize,
            endPosition = endPosition,
            currentPosition = currentPosition
        });
    }
    private float3 GetWorldMousePoint()
    {
        var cameraPos = (float3)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cameraPos.z = 0;
        return cameraPos;
    }
    private JobHandle HandleInput(JobHandle inputDependencies)
    {
        if (Input.GetMouseButtonDown(0))
        {
            selection = true;
            startPosition = GetWorldMousePoint();

            return inputDependencies;
        }
        if (Input.GetMouseButtonUp(0))
        {
            var deselectJob = new DeselectJob() { entityCommandBuffer = endSimulationEntityCommandBuffer.CreateCommandBuffer().ToConcurrent() };
            var handle = deselectJob.Schedule(this, inputDependencies);
            endSimulationEntityCommandBuffer.AddJobHandleForProducer(handle);
            handle.Complete();
            endPosition = GetWorldMousePoint();
            lowerLeftPosition = new float3(math.min(startPosition.x, endPosition.x), math.min(startPosition.y, endPosition.y), 0);
            upperRightPosition = new float3(math.max(startPosition.x, endPosition.x), math.max(startPosition.y, endPosition.y), 0);
            SetSelectionData();
            return handle;
        }
        if (Input.GetMouseButton(0))
        {
            currentPosition = GetWorldMousePoint();
            lowerLeftPosition = new float3(math.min(startPosition.x, currentPosition.x), math.min(startPosition.y, currentPosition.y), 0);
            upperRightPosition = new float3(math.max(startPosition.x, currentPosition.x), math.max(startPosition.y, currentPosition.y), 0);
            SetSelectionData();
            return inputDependencies;
        }
        else if (selection)
        {
            Reset();
            SetSelectionData();
            return inputDependencies;
        }
        return inputDependencies;
    }

    private void Reset()
    {
        selection = false;
        endPosition = float3.zero;
        startPosition = float3.zero;
        upperRightPosition = float3.zero;
        lowerLeftPosition = float3.zero;
    }
}

public struct HasSelectionCircle : IComponentData
{
    public Entity circle;
}
public class UnitSelectRenderer : JobComponentSystem
{

    public Entity circleEntity;
    public float3 offset;
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBuffer;

    [BurstCompile]
    [RequireComponentTag(typeof(SelectedTag))]
    [ExcludeComponent(typeof(HasSelectionCircle))]
    struct UnitSelecRendererJob : IJobForEachWithEntity<Translation>
    {

        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        public Entity circleEntity;
        public float3 offset;
        public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation)
        {
            var instance = entityCommandBuffer.Instantiate(index, circleEntity);
            entityCommandBuffer.AddComponent(index, instance, new Parent() { Value = entity });
            entityCommandBuffer.AddComponent(index, instance, new LocalToParent() { Value = float4x4.identity });
            entityCommandBuffer.AddComponent(index, instance, new LocalToWorld());
            entityCommandBuffer.AddComponent(index, entity, new HasSelectionCircle() { circle = instance });
        }
    }

    protected override void OnCreate()
    {
        endSimulationEntityCommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var offset = EntityManager.GetComponentData<Translation>(circleEntity);
        var job = new UnitSelecRendererJob()
        {
            entityCommandBuffer = endSimulationEntityCommandBuffer.CreateCommandBuffer().ToConcurrent(),
            circleEntity = circleEntity,
            offset = offset.Value,
        };
        var schedule = job.Schedule(this, inputDependencies);
        endSimulationEntityCommandBuffer.AddJobHandleForProducer(schedule);
        return schedule;
    }
}