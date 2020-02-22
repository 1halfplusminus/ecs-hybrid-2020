using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
[DisableAutoCreation]
public class PlayerMovementSystem : JobComponentSystem
{

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        float deltaTime = Time.DeltaTime;
        var jobHandle = inputDependencies = Entities.ForEach((ref Translation translation, in MoveDirection moveDirection) =>
        {
            translation.Value.x += moveDirection.Value * deltaTime;
        }).Schedule(inputDependencies);
        return jobHandle;
    }
}