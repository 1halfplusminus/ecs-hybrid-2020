using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

class ConvertMarine : MonoBehaviour, IConvertGameObjectToEntity
{

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        conversionSystem.AddHybridComponent(GetComponent<SpriteRenderer>());
        conversionSystem.AddHybridComponent(GetComponent<Animator>());
        /** dstManager.AddComponentObject(entity, m_Graph);
        dstManager.AddComponentObject(entity, m_ScriptPlayable);/*  */
    }
}

[BurstCompile]
public struct AnimationJob : UnityEngine.Animations.IAnimationJob
{
    public void ProcessRootMotion(UnityEngine.Animations.AnimationStream stream)
    {

    }

    public void ProcessAnimation(UnityEngine.Animations.AnimationStream stream)
    {

    }
}
[BurstCompile]
public struct SimpleLoggerJob : IJob
{
    /*     public Animator animator; */
    public void Execute()
    {
        /*   playableGraph.Play(); */
        /*    Debug.Log("Test"); */
        float result = 0;
        for (int i = 0; i < 5000; i++)
        {
            result += 1f;
        }
    }
}