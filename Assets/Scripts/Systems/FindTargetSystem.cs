using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

[DisableAutoCreation]
public class FindTargetSystem : ComponentSystem
{
    protected override void OnUpdate()
    {

        Entities.WithAll<UnitData>().ForEach((Entity entity, ref Translation unitTranslation) =>
        {
            Entity closestTargetEntity = Entity.Null;
            float3 unitPosition = unitTranslation.Value;
            float3 closestTargetPosition = new float3(0);
            Entities.WithAll<TargetData>().ForEach((Entity targetEntity, ref Translation targetTranslation) =>
             {
                 if (closestTargetEntity == Entity.Null)
                 {
                     closestTargetEntity = targetEntity;
                     closestTargetPosition = targetTranslation.Value;
                 }
                 else
                 {
                     if (math.distance(unitPosition, targetTranslation.Value) <= math.distance(unitPosition, closestTargetPosition))
                     {
                         closestTargetEntity = targetEntity;
                         closestTargetPosition = targetTranslation.Value;
                     }
                 }
             });
            if (closestTargetEntity != Entity.Null)
            {
                // PostUpdateCommands.AddComponent(entity, new HasTarget() { target = closestTargetEntity });
            }
        });
    }
}
