using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Entities.EntityCommandBuffer;
using static Unity.Mathematics.math;

[DisableAutoCreation]
public class BallGoalCheckSystem : JobComponentSystem
{
    // EndFrameBarrier provides the CommandBuffer
    BeginInitializationEntityCommandBufferSystem m_EntityCommandBufferSystem;
    EntityQuery m_Group;
    protected override void OnCreate()
    {
        // Cache the EndFrameBarrier in a field, so we don't have to get it every frame
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        m_Group = GetEntityQuery(typeof(PlayerScoreData));
    }

    // [BurstCompile]
    [RequireComponentTag(typeof(BallTag))]
    struct BallGoalCheckSystemJob : IJobForEachWithEntity<Translation>
    {
        public Concurrent CommandBuffer;
        public NativeArray<PlayerScoreData> PlayersScore;
        public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation)
        {
            float3 pos = translation.Value;
            float bound = GameManager.main.xBound;
            if (pos.x >= 10)
            {
                PlayersScore.ToArray()[0].score += 1;
                /* GameManager.main.PlayerScored (0); */
                CommandBuffer.DestroyEntity(index, entity);
            }
            else if (pos.x <= -10)
            {
                PlayersScore.ToArray()[1].score += 1;
                /*   GameManager.main.PlayerScored (1); */
                CommandBuffer.DestroyEntity(index, entity);
            }

        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var playersScore = m_Group.ToComponentDataArray<PlayerScoreData>(Allocator.TempJob);
        var job = new BallGoalCheckSystemJob()
        {
            CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
            PlayersScore = playersScore
        };
        inputDependencies = job.Run(this, inputDependencies);
        m_EntityCommandBufferSystem.AddJobHandleForProducer(inputDependencies);
        inputDependencies.Complete();
        playersScore.Dispose();

        return inputDependencies;
    }
}