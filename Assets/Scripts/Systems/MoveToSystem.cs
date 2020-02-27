using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class MoveToSystem : JobComponentSystem
{
    [BurstCompile]
    struct MoveToSystemJob : IJobForEachWithEntity<Translation, MoveTo, UnitData>
    {

        public float deltaTime;

        public void Execute(Entity entity, int jobIndex, ref Translation translation, ref MoveTo moveTo, ref UnitData unitData)
        {
            if (moveTo.move)
            {
                float reachedPositionDistance = 1f;
                if (math.distance(translation.Value, moveTo.position) > reachedPositionDistance)
                {
                    float3 moveDir = math.normalize(moveTo.position - translation.Value);
                    translation.Value += moveDir * moveTo.moveSpeed * deltaTime;
                    unitData.animation = UnitData.AnimationType.Walking;
                }
                else
                {
                    unitData.animation = UnitData.AnimationType.Idle;
                    moveTo.move = false;
                }
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new MoveToSystemJob() { deltaTime = UnityEngine.Time.deltaTime };

        // Assign values to the fields on your job here, so that it has
        // everything it needs to do its work when it runs later.
        // For example,
        //     job.deltaTime = UnityEngine.Time.deltaTime;

        // Now that the job is set up, schedule it to be run. 
        return job.Schedule(this, inputDependencies);
    }
}