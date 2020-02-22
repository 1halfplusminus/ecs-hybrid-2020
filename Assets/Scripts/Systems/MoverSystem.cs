using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

[DisableAutoCreation]
public class MoverSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {

        float time = Time.DeltaTime;
        // Assign values to the fields on your job here, so that it has
        // everything it needs to do its work when it runs later.
        // For example,
        //     job.deltaTime = UnityEngine.Time.deltaTime;
        Entities.ForEach((ref Translation translation, ref MoveSpeedComponent moveSpeed) =>
        {
            translation.Value.y += moveSpeed.moveSpeed * time;
            if (translation.Value.y > 5f)
            {
                moveSpeed.moveSpeed = -math.abs(moveSpeed.moveSpeed);
            }
            if (translation.Value.y < -5f)
            {
                moveSpeed.moveSpeed = +math.abs(moveSpeed.moveSpeed);
            }
        }).Run();
        // Now that the job is set up, schedule it to be run. 
        return inputDependencies;
    }
}