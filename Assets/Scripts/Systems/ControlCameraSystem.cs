using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;


public class ControlCameraSystem : JobComponentSystem
{
    float zoomAmount = 1f;
    float maxToClamp = 10f;
    float rotSpeed = 1f;
    float dragSpeed = 1f;
    float sensitivity = 0.5f;
    [BurstCompile]

    struct ControlCameraSystemJob : IJobForEach<CameraData, Translation>
    {
        public float axis;
        public float zoomAmount;
        public float maxToClamp;
        public float rotSpeed;

        public float sensitivity;
        public float axisX;
        public float axisY;

        public void Execute(ref CameraData cameraData, ref Translation translation)
        {

            zoomAmount = math.clamp(zoomAmount, -maxToClamp, maxToClamp);
            var translate = math.min(math.abs(axis), maxToClamp - math.abs(zoomAmount));
            var size = (translate * rotSpeed) * math.sign(axis);
            cameraData.size += size;
            translation.Value.x += (axisX * -1) * sensitivity;
            translation.Value.y += (axisY * -1) * sensitivity;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var axis = GetAxis();
        var job = new ControlCameraSystemJob()
        {
            axis = Input.GetAxis("Mouse ScrollWheel"),
            maxToClamp = maxToClamp,
            zoomAmount = zoomAmount,
            rotSpeed = rotSpeed,
            sensitivity = sensitivity,
            axisX = axis.Item1,
            axisY = axis.Item2
        };

        return job.Schedule(this, inputDependencies);
    }

    (float, float) GetAxis()
    {
        if (Input.GetMouseButton(2))
        {
            return (Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        }
        return (0, 0);
    }
}