using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct SelectedTag : IComponentData
{

}
public class UnitControlSystem : JobComponentSystem
{

    private float3 startPosition;
    private float3 endPosition;

    private float3 lowerLeftPosition;

    private float3 upperRightPosition;

    private bool selection;
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBuffer;
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
    protected override void OnCreate()
    {
        EntityManager.CreateEntity(typeof(SelectionAreaData));
        endSimulationEntityCommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        HandleInput();
        if (selection && SelectionEnded())
        {
            var job = new UnitControlSystemJob()
            {
                selection = true,
                lowerLeftPosition = lowerLeftPosition,
                upperRightPosition = upperRightPosition,
                entityCommandBuffer = endSimulationEntityCommandBuffer.CreateCommandBuffer().ToConcurrent()
            };
            var schedule = job.Schedule(this, inputDependencies);
            endSimulationEntityCommandBuffer.AddJobHandleForProducer(schedule);
            return schedule;
        }
        return inputDependencies;
    }
    private bool SelectionEnded()
    {
        return math.any(endPosition != float3.zero);
    }
    private void SetSelectionData()
    {
        var area = (lowerLeftPosition - upperRightPosition) * math.sign(startPosition - GetWorldMousePoint());
        SetSingleton<SelectionAreaData>(new SelectionAreaData()
        {
            isSelecting = selection,
            lowerLeftPosition = lowerLeftPosition,
            upperRightPosition = upperRightPosition,
            startPosition = startPosition,
            selectionAreaSize = new float3(area.x, area.y, 1),
        });
    }
    private float3 GetWorldMousePoint()
    {
        var cameraPos = (float3)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cameraPos.z = 0;
        return cameraPos;
    }
    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            selection = true;
            startPosition = GetWorldMousePoint();
            return;
        }
        if (Input.GetMouseButtonUp(0))
        {
            endPosition = GetWorldMousePoint();
            lowerLeftPosition = new float3(math.min(startPosition.x, endPosition.x), math.min(startPosition.y, endPosition.y), 0);
            upperRightPosition = new float3(math.max(startPosition.x, endPosition.x), math.max(startPosition.y, endPosition.y), 0);
            SetSelectionData();
            return;
        }
        if (Input.GetMouseButton(0))
        {
            var currentPosition = GetWorldMousePoint();
            lowerLeftPosition = new float3(math.min(startPosition.x, currentPosition.x), math.min(startPosition.y, currentPosition.y), 0);
            upperRightPosition = new float3(math.max(startPosition.x, currentPosition.x), math.max(startPosition.y, currentPosition.y), 0);
            SetSelectionData();
            return;
        }
        else if (selection)
        {
            Reset();
            SetSelectionData();
            return;
        }
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
            /*             entityCommandBuffer.AddComponent(index, instance, new NonUniformScale() { Value = new float3(1f, 1f, 1f) }); */
            entityCommandBuffer.AddComponent(index, instance, new LocalToWorld());
            /*  entityCommandBuffer.AddComponent(index, instance, new CompositeScale()); */
            /*  entityCommandBuffer.AddComponent(index, instance, new Translation()
             {
                 Value = float3.zero,
             }); */
            entityCommandBuffer.AddComponent(index, entity, new HasSelectionCircle());
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
