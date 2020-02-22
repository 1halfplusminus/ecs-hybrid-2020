using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
[GenerateAuthoringComponent]
public struct SpeedIncreaseOverTimeData : IComponentData {
    public float increasePerSeconds;
}