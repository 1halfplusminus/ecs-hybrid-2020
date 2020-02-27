using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Collider = Unity.Physics.Collider;

[UpdateBefore (typeof (UnitControlSystem))]
public class SelectionAreaSystem : JobComponentSystem {
    public Entity prefab;
    public Entity instance = Entity.Null;
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBuffer;

    protected override void OnCreate () {
        RequireSingletonForUpdate<SelectionAreaData> ();
        endSimulationEntityCommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem> ();
        base.OnCreate ();
    }
    protected override JobHandle OnUpdate (JobHandle inputDependencies) {
        SelectionAreaData selectionAreaData = GetSingleton<SelectionAreaData> ();
        if (selectionAreaData.isSelecting) {
            CreateIfDontExist ();
            Entities.WithAll<SelectionAreaTag> ().ForEach ((Entity entity, ref Translation translation, ref NonUniformScale scale, ref PhysicsCollider collider) => {

                var center = math.lerp (selectionAreaData.upperRightPosition, selectionAreaData.lowerLeftPosition, 0.5f);
                var size = new float3 (selectionAreaData.startPosition.x - selectionAreaData.currentPosition.x,
                    selectionAreaData.startPosition.y - selectionAreaData.currentPosition.y,
                    1);
                BoxGeometry boxGeometry = new BoxGeometry () {
                    Center = new float3 (0, 0, 0) - new float3 (selectionAreaData.startPosition.x - selectionAreaData.currentPosition.x,
                    selectionAreaData.startPosition.y - selectionAreaData.currentPosition.y,
                    1) / 2,
                    Size = size * math.sign (size) + new float3 (1, 1, 1),
                    Orientation = quaternion.identity,
                    BevelRadius = 0,
                };
                BlobAssetReference<Unity.Physics.Collider> boxColliser = Unity.Physics.BoxCollider.Create (boxGeometry);
                collider.Value = boxColliser;

                translation.Value = selectionAreaData.startPosition;
                scale.Value = selectionAreaData.selectionAreaSize;
            }).Run ();
            return default;
        } else {
            var commandBuffer = endSimulationEntityCommandBuffer.CreateCommandBuffer ().ToConcurrent ();
            var jobHandle = Entities.WithAll<SelectionAreaTag> ().ForEach ((Entity entity) => {
                commandBuffer.DestroyEntity (0, entity);
            }).Schedule (inputDependencies);
            endSimulationEntityCommandBuffer.AddJobHandleForProducer (jobHandle);
            jobHandle.Complete ();
            return jobHandle;
        }

    }

    protected void CreateIfDontExist () {
        EntityQuery entityQuery = GetEntityQuery (typeof (SelectionAreaTag));
        if (entityQuery.CalculateEntityCount () == 0) {
            instance = EntityManager.Instantiate (prefab);
        }
    }
}