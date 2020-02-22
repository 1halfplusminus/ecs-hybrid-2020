using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
[GenerateAuthoringComponent]
public struct PaddleMovementData : IComponentData {
    public int direction;
    public float speed;
}