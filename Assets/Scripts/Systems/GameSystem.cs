using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
[DisableAutoCreation]
public class GameSystem : JobComponentSystem
{

    [BurstCompile]
    struct GameSystemJob : IJobForEach<Translation, Rotation>
    {
        // Add fields here that your job needs to do its work.
        // For example,
        //    public float deltaTime;

        public void Execute(ref Translation translation, [ReadOnly] ref Rotation rotation) { }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new GameSystemJob();
        return job.Schedule(this, inputDependencies);
    }
}