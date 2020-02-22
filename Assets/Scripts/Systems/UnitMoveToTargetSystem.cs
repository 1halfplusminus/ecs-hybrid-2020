using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

[DisableAutoCreation]
public class UnitMoveToTargetSystem : JobComponentSystem
{
    [BurstCompile]
    [RequireComponentTag(typeof(UnitData))]
    struct UnitMoveToTargetSystemJob : IJobForEachWithEntity<HasTarget>
    {

        [ReadOnly] public ComponentDataFromEntity<Translation> componentDataFromEntity;
        public float deltaTime;
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        public void Execute(Entity entity, int index, ref HasTarget hasTarget)
        {
            if (componentDataFromEntity.Exists(hasTarget.target) && componentDataFromEntity.Exists(entity))
            {
                var translation = componentDataFromEntity[entity];
                var targetTranslation = componentDataFromEntity[hasTarget.target];

                float3 targetDir = math.normalize(targetTranslation.Value - translation.Value);
                float moveSpeed = 1f;
                translation.Value += targetDir * moveSpeed * deltaTime;
                entityCommandBuffer.SetComponent(index, entity, new Translation() { Value = translation.Value });
                if (math.distance(translation.Value, targetTranslation.Value) < 0.2f)
                {
                    entityCommandBuffer.DestroyEntity(index, hasTarget.target);
                    entityCommandBuffer.RemoveComponent<HasTarget>(index, entity);
                }
            }
            else
            {
                entityCommandBuffer.RemoveComponent<HasTarget>(index, entity);
            }
        }
    }

    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBuffer;
    protected override void OnCreate()
    {
        endSimulationEntityCommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new UnitMoveToTargetSystemJob()
        {
            componentDataFromEntity = GetComponentDataFromEntity<Translation>(true),
            deltaTime = Time.DeltaTime,
            entityCommandBuffer = endSimulationEntityCommandBuffer.CreateCommandBuffer().ToConcurrent()
        };
        var schedule = job.Schedule(this, inputDependencies);
        endSimulationEntityCommandBuffer.AddJobHandleForProducer(schedule);
        return schedule;
    }
}