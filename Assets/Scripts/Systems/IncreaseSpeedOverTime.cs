using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using static Unity.Mathematics.math;

[DisableAutoCreation]
public class IncreaseSpeedOverTime : JobComponentSystem
{
    [BurstCompile]
    struct IncreaseSpeedOverTimeJob : IJobForEach<SpeedIncreaseOverTimeData, PhysicsVelocity>
    {
        public float deltaTime;

        public void Execute([ReadOnly] ref SpeedIncreaseOverTimeData speedIncreaseOverTime, ref PhysicsVelocity physicsVelocity)
        {
            var modifier = new float2(speedIncreaseOverTime.increasePerSeconds * deltaTime);

            float2 newVel = physicsVelocity.Linear.xy;

            newVel += math.lerp(-modifier, modifier, math.sign(newVel));

            physicsVelocity.Linear.xy = newVel;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new IncreaseSpeedOverTimeJob() { deltaTime = Time.DeltaTime };
        return job.Run(this, inputDependencies);
    }
}