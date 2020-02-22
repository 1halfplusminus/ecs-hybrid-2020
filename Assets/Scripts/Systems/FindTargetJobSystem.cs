using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisableAutoCreation]
public class FindTargetJobSystem : JobComponentSystem
{
    [BurstCompile]
    [RequireComponentTag(typeof(UnitData))]
    [ExcludeComponent(typeof(HasTarget))]
    struct FindTargetJobSystemJob : IJobForEachWithEntity<Translation>
    {
        // Add fields here that your job needs to do its work.
        // For example,
        //    public float deltaTime;


        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<EntityWithPosition> targetArray;
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        public void Execute(Entity entity, int index, [ReadOnly] ref Translation unitTranslation)
        {
            /* Debug.Log("FindTargetJobSystemJob :" + entity.ToString()); */
            Entity closestTargetEntity = Entity.Null;
            float3 unitPosition = unitTranslation.Value;
            float3 closestTargetPosition = new float3(0);
            for (int i = 0; i < targetArray.Length; i++)
            {
                var targetEntity = targetArray[i].entity;
                var targetTranslation = targetArray[i].position;
                if (closestTargetEntity == Entity.Null)
                {
                    closestTargetEntity = targetEntity;
                    closestTargetPosition = targetTranslation;
                }
                else
                {
                    if (math.distance(unitPosition, targetTranslation) <= math.distance(unitPosition, closestTargetPosition))
                    {
                        closestTargetEntity = targetEntity;
                        closestTargetPosition = targetTranslation;
                    }
                }
            }
            if (closestTargetEntity != Entity.Null)
            {
                entityCommandBuffer.AddComponent(index, entity, new HasTarget() { target = closestTargetEntity });
            }
        }
    }
    [BurstCompile]
    [RequireComponentTag(typeof(UnitData))]
    [ExcludeComponent(typeof(HasTarget))]
    struct FindTargetQuandrantJob : IJobForEachWithEntity<Translation, QuadrantEntity>
    {
        // Add fields here that your job needs to do its work.
        // For example,
        //    public float deltaTime;


        [ReadOnly] public NativeMultiHashMap<int, QuandrantData> quandrantMultiHashMap;
        public EntityCommandBuffer.Concurrent entityCommandBuffer;

        private void FindTarget(int hashMapKey, float3 unitPosition, QuadrantEntity unitQuandrantEntity, ref Entity closestTargetEntity, ref float3 closestTargetPosition)
        {
            QuandrantData quandrantData;
            NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
            if (quandrantMultiHashMap.TryGetFirstValue(hashMapKey, out quandrantData, out nativeMultiHashMapIterator))
            {
                do
                {
                    if (quandrantData.quadrantEntity.typeEnum != unitQuandrantEntity.typeEnum)
                    {
                        var targetEntity = quandrantData.entity;
                        var targetTranslation = quandrantData.translation.Value;
                        if (closestTargetEntity == Entity.Null)
                        {
                            closestTargetEntity = targetEntity;
                            closestTargetPosition = targetTranslation;
                        }
                        else
                        {
                            if (math.distance(unitPosition, targetTranslation) <= math.distance(unitPosition, closestTargetPosition))
                            {
                                closestTargetEntity = targetEntity;
                                closestTargetPosition = targetTranslation;
                            }
                        }
                    }
                } while (quandrantMultiHashMap.TryGetNextValue(out quandrantData, ref nativeMultiHashMapIterator));
            }
        }
        public void Execute(Entity entity, int index, [ReadOnly] ref Translation unitTranslation, [ReadOnly] ref QuadrantEntity unitQuandrantEntity)
        {
            Entity closestTargetEntity = Entity.Null;
            float3 unitPosition = unitTranslation.Value;
            float3 closestTargetPosition = new float3(0);
            int hashMapKey = QuadrantSystem.GetPositionHasMapKey(unitTranslation.Value);
            FindTarget(hashMapKey, unitPosition, unitQuandrantEntity, ref closestTargetEntity, ref closestTargetPosition);
            for (int i = 1; i <= 2; i++)
            {
                FindTarget(hashMapKey + i, unitPosition, unitQuandrantEntity, ref closestTargetEntity, ref closestTargetPosition);
                FindTarget(hashMapKey - i, unitPosition, unitQuandrantEntity, ref closestTargetEntity, ref closestTargetPosition);
                FindTarget(hashMapKey + i * QuadrantSystem.quadrantYMultiplier, unitPosition, unitQuandrantEntity, ref closestTargetEntity, ref closestTargetPosition);
                FindTarget(hashMapKey - i * QuadrantSystem.quadrantYMultiplier, unitPosition, unitQuandrantEntity, ref closestTargetEntity, ref closestTargetPosition);
            }

            if (closestTargetEntity != Entity.Null)
            {
                entityCommandBuffer.AddComponent(index, entity, new HasTarget() { target = closestTargetEntity });
            }
        }
    }
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBuffer;
    private QuadrantSystem quadrantSystem;
    protected override void OnCreate()
    {
        endSimulationEntityCommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        quadrantSystem = World.GetOrCreateSystem<QuadrantSystem>();
        base.OnCreate();
    }
    private struct EntityWithPosition
    {
        public Entity entity;
        public float3 position;
    }
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        /* EntityQuery targetQuery = GetEntityQuery(typeof(TargetData), ComponentType.ReadOnly<Translation>());
        NativeArray<Entity> targetEntityArray = targetQuery.ToEntityArray(Allocator.TempJob);
        NativeArray<Translation> targetTranslation = targetQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
        NativeArray<EntityWithPosition> targetArray = new NativeArray<EntityWithPosition>(targetEntityArray.Length, Allocator.TempJob);

        for (int i = 0; i < targetEntityArray.Length; i++)
        {
            targetArray[i] = new EntityWithPosition { entity = targetEntityArray[i], position = targetTranslation[i].Value };
        }

        targetEntityArray.Dispose();
        targetTranslation.Dispose();

        var job = new FindTargetJobSystemJob()
        {
            targetArray = targetArray,
            entityCommandBuffer = endSimulationEntityCommandBuffer.CreateCommandBuffer().ToConcurrent()
        }; */
        var job = new FindTargetQuandrantJob()
        {
            quandrantMultiHashMap = quadrantSystem.quandrantMultiHashMap,
            entityCommandBuffer = endSimulationEntityCommandBuffer.CreateCommandBuffer().ToConcurrent()
        };
        var JobHandle = job.Schedule(this, inputDependencies);
        endSimulationEntityCommandBuffer.AddJobHandleForProducer(JobHandle);
        return JobHandle;
    }
}