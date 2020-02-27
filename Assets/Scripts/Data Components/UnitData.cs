﻿using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
[GenerateAuthoringComponent]
public struct UnitData : IComponentData {
    public enum AnimationType { Idle, Walking }
    public AnimationType animation;
}