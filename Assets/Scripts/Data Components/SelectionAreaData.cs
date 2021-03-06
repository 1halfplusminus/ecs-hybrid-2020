using Unity.Entities;
using Unity.Mathematics;

public struct SelectionAreaData : IComponentData
{
    public float3 lowerLeftPosition;
    public float3 upperRightPosition;
    public float3 startPosition;
    public float3 endPosition;
    public float3 selectionAreaSize;
    public float3 currentPosition;
    public bool isSelecting;
}