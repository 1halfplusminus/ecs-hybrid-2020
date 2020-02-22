using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

[DisableAutoCreation]
[AlwaysSynchronizeSystem]
public class PlayerInputSystem : JobComponentSystem
{

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        Entities.ForEach((ref Translation translation, ref PaddleMovementData paddleMovementData, in PaddleInputData paddleInputData) =>
        {
            paddleMovementData.direction = 0;

            paddleMovementData.direction += Input.GetKey(paddleInputData.upKey) ? 1 : 0;
            paddleMovementData.direction -= Input.GetKey(paddleInputData.downKey) ? 1 : 0;
        }).Run();
        return default;
    }
}