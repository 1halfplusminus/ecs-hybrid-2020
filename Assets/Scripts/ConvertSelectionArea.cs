using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public struct SelectionAreaTag : IComponentData
{

}
public class ConvertSelectionArea : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new NonUniformScale()
        {
            Value = float3.zero
        });
        dstManager.AddComponentData(entity, new SelectionAreaTag());
    }
}
