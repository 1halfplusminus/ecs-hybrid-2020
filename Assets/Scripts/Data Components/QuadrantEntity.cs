using System;
using Unity.Entities;

[Serializable]
[GenerateAuthoringComponent]
public struct QuadrantEntity : IComponentData
{
    public TypeEnum typeEnum;

    public enum TypeEnum
    {
        UnitData,
        TargetData,
    }
}