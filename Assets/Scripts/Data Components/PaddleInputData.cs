using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
[GenerateAuthoringComponent]
public struct PaddleInputData : IComponentData {
    public KeyCode upKey;
    public KeyCode downKey;
}