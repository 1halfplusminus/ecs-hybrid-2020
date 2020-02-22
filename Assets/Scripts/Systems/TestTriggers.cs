using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using static Unity.Mathematics.math;

[DisableAutoCreation]
public class TestTriggers : JobComponentSystem
{
    // This declares a new kind of job, which is a unit of work to do.
    // The job is declared as an IJobForEach<Translation, Rotation>,
    // meaning it will process all entities in the world that have both
    // Translation and Rotation components. Change it to process the component
    // types you want.
    //
    // The job is also tagged with the BurstCompile attribute, which means
    // that the Burst compiler will optimize it for the best performance.
    [BurstCompile]
    struct TestTriggersJob : ITriggerEventsJob
    {
        // Add fields here that your job needs to do its work.
        // For example,
        //    public float deltaTime;
        public ComponentDataFromEntity<PhysicsVelocity> physicsVelocityEntities;
        public void Execute(TriggerEvent triggerEvent)
        {
            if (physicsVelocityEntities.HasComponent(triggerEvent.Entities.EntityA))
            {
                PhysicsVelocity physicsVelocity = physicsVelocityEntities[triggerEvent.Entities.EntityA];
                physicsVelocity.Linear.y = 5f;
                physicsVelocityEntities[triggerEvent.Entities.EntityA] = physicsVelocity;
            }
            if (physicsVelocityEntities.HasComponent(triggerEvent.Entities.EntityB))
            {
                PhysicsVelocity physicsVelocity = physicsVelocityEntities[triggerEvent.Entities.EntityB];
                physicsVelocity.Linear.y = 5f;
                physicsVelocityEntities[triggerEvent.Entities.EntityB] = physicsVelocity;
            }
        }
    }
    private BuildPhysicsWorld buildPhsicsWorld;
    private StepPhysicsWorld stepPhysicsWorld;
    protected override void OnCreate()
    {
        buildPhsicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
    }
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new TestTriggersJob()
        {
            physicsVelocityEntities = GetComponentDataFromEntity<PhysicsVelocity>()
        };
        return job.Schedule(stepPhysicsWorld.Simulation, ref buildPhsicsWorld.PhysicsWorld, inputDependencies);
    }
}