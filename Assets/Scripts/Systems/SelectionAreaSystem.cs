using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Collider = Unity.Physics.Collider;

[UpdateAfter(typeof(UnitControlSystem))]
public class SelectionAreaSystem : ComponentSystem
{
    public Entity prefab;

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<SelectionAreaData>();
        base.OnCreate();
    }
    protected override void OnUpdate()
    {
        SelectionAreaData selectionAreaData = GetSingleton<SelectionAreaData>();
        if (selectionAreaData.isSelecting)
        {
            CreateIfDontExist();
            Entities.WithAll<SelectionAreaTag>().ForEach((Entity entity, ref Translation translation, ref NonUniformScale scale, ref PhysicsCollider collider) =>
           {

               var center = math.lerp(selectionAreaData.upperRightPosition, selectionAreaData.lowerLeftPosition, 0.5f);
               var size = new float3(selectionAreaData.startPosition.x - selectionAreaData.currentPosition.x,
                   selectionAreaData.startPosition.y - selectionAreaData.currentPosition.y,
                   1);
               BoxGeometry boxGeometry = new BoxGeometry()
               {
                   Center = new float3(0, 0, 0) - new float3(selectionAreaData.startPosition.x - selectionAreaData.currentPosition.x,
                   selectionAreaData.startPosition.y - selectionAreaData.currentPosition.y,
                   1) / 2,
                   Size = size * math.sign(size) + new float3(1, 1, 1),
                   Orientation = quaternion.identity,
                   BevelRadius = 0,
               };
               BlobAssetReference<Unity.Physics.Collider> boxColliser = Unity.Physics.BoxCollider.Create(boxGeometry);
               collider.Value = boxColliser;

               translation.Value = selectionAreaData.startPosition;
               scale.Value = selectionAreaData.selectionAreaSize;
           });
        }
        else
        {
            Entities.WithAll<SelectionAreaTag>().ForEach((Entity entity) =>
            {
                EntityManager.DestroyEntity(entity);
            });
        }
    }

    protected void CreateIfDontExist()
    {
        EntityQuery entityQuery = GetEntityQuery(typeof(SelectionAreaTag));
        if (entityQuery.CalculateEntityCount() == 0)
        {
            EntityManager.Instantiate(prefab);
        }
    }
}