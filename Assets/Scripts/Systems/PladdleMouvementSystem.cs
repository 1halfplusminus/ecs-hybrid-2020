using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
[DisableAutoCreation]
public class PladdleMouvementSystem : JobComponentSystem
{
    [BurstCompile]
    struct PladdleMouvementSystemJob : IJobForEach<Translation, PaddleMovementData>
    {
        // Add fields here that your job needs to do its work.
        // For example,
        public float deltaTime;
        public float yBound;
        public void Execute(ref Translation translation, [ReadOnly] ref PaddleMovementData paddleMovementData)
        {
            translation.Value.y = math.clamp(translation.Value.y + (paddleMovementData.speed * paddleMovementData.direction * deltaTime), -yBound, yBound);
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        if (GameManager.main)
        {
            var job = new PladdleMouvementSystemJob() { yBound = GameManager.main.yBound, deltaTime = UnityEngine.Time.deltaTime };
            inputDependencies = job.Schedule(this, inputDependencies);
            inputDependencies.Complete();
        }
        return inputDependencies;
    }
}