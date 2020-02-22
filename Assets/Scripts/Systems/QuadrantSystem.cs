using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


public struct QuandrantData
{
    public Entity entity;
    public Translation translation;

    public QuadrantEntity quadrantEntity;
}
[DisableAutoCreation]
public class QuadrantSystem : JobComponentSystem
{
    public NativeMultiHashMap<int, QuandrantData> quandrantMultiHashMap;
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBuffer;
    struct QuadrantSystemJob : IJobForEachWithEntity<Translation, QuadrantEntity>
    {
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        public NativeMultiHashMap<int, QuandrantData>.ParallelWriter quandrantMultiHashMap;
        public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation, [ReadOnly] ref QuadrantEntity quadrantEntity)
        {
            int hashMapKey = GetPositionHasMapKey(translation.Value);
            quandrantMultiHashMap.Add(hashMapKey, new QuandrantData() { entity = entity, translation = translation, quadrantEntity = quadrantEntity });
        }
    }
    protected override void OnCreate()
    {
        quandrantMultiHashMap = new NativeMultiHashMap<int, QuandrantData>(0, Allocator.Persistent);
        endSimulationEntityCommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }
    protected override void OnDestroy()
    {
        quandrantMultiHashMap.Dispose();
        base.OnDestroy();
    }
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        quandrantMultiHashMap.Clear();
        EntityQuery query = GetEntityQuery(typeof(QuadrantEntity));
        int quandrantEntityLength = query.CalculateEntityCount();
        if (quandrantEntityLength > quandrantMultiHashMap.Capacity)
        {
            Debug.Log("Setting Capacity " + quandrantEntityLength);
            quandrantMultiHashMap.Capacity = quandrantEntityLength;
        }
        var job = new QuadrantSystemJob()
        {
            entityCommandBuffer = endSimulationEntityCommandBuffer.CreateCommandBuffer().ToConcurrent(),
            quandrantMultiHashMap = quandrantMultiHashMap.AsParallelWriter(),
        };
        var schedule = job.Schedule(this, inputDependencies);
        endSimulationEntityCommandBuffer.AddJobHandleForProducer(schedule);
        schedule.Complete();
        var currentMousePosition = Camera.main.ScreenToWorldPoint(
                 new Vector3(Input.mousePosition.x,
                 Input.mousePosition.y,
                 Camera.main.nearClipPlane));
        DebugDrawQuadrant(currentMousePosition);
        //Debug.Log(quandrantMultiHashMap.CountValuesForKey(GetPositionHasMapKey(currentMousePosition)));
        /*   quandrantMultiHashMap.Dispose(); */
        return schedule;
    }

    public const int quadrantYMultiplier = 1000;
    const int quadrantCellSize = 5;
    private static void DebugDrawQuadrant(float3 position)
    {
        Vector3 lowerLeft = new Vector3(math.floor(position.x / quadrantCellSize) * quadrantCellSize, (position.y / quadrantCellSize) * quadrantCellSize, (position.z / quadrantCellSize) * quadrantCellSize);
        Debug.DrawLine(lowerLeft, lowerLeft + new Vector3(+1, +0) * quadrantCellSize);
        Debug.DrawLine(lowerLeft, lowerLeft + new Vector3(+0, +1) * quadrantCellSize);
        Debug.DrawLine(lowerLeft + new Vector3(+1, +0) * quadrantCellSize, lowerLeft + new Vector3(+1, +1) * quadrantCellSize);
        Debug.DrawLine(lowerLeft + new Vector3(+0, +1) * quadrantCellSize, lowerLeft + new Vector3(+1, +1) * quadrantCellSize);

        // Debug.Log(GetPositionHasMapKey(position) + " " + position);
    }
    public static int GetPositionHasMapKey(float3 position)
    {
        return (int)(math.floor(position.x / quadrantCellSize) + (quadrantYMultiplier * math.floor(position.y / quadrantCellSize)));
    }

}