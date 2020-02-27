using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
[GenerateAuthoringComponent]
public struct MoveTo : IComponentData {
    public bool move;
    public float3 position;
    public float3 lastMoveDir;
    public float moveSpeed;
}