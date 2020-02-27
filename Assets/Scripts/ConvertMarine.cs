using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

class ConvertMarine : MonoBehaviour, IConvertGameObjectToEntity {
    private EntityManager entityManager;
    private Entity entity;
    public void Start () {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }
    public void Convert (Entity convertedEntity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        entity = convertedEntity;
        conversionSystem.AddHybridComponent (GetComponent<SpriteRenderer> ());
        conversionSystem.AddHybridComponent (GetComponent<Animator> ());
        conversionSystem.AddHybridComponent (this);
        /* dstManager.AddComponentData(entity, new PhysicsDebugDisplayData()
        {
            DrawBroadphase = 1,
            DrawColliderAabbs = 1,
            DrawColliderEdges = 1,
            DrawColliders = 1,
            DrawCollisionEvents = 1,
            DrawContacts = 1,
            DrawJoints = 1,
            DrawMassProperties = 1,
            DrawTriggerEvents = 1
        }); */
        /** dstManager.AddComponentObject(entity, m_Graph);
        dstManager.AddComponentObject(entity, m_ScriptPlayable);/*  */
    }
    public void update () {
        UnitData unitData = entityManager.GetComponentData<UnitData> (entity);
        Animator animator = GetComponent<Animator> ();
        if (unitData.animation == UnitData.AnimationType.Walking) {
            animator.SetBool (UnitData.AnimationType.Walking.ToString (), true);
        }
        if (unitData.animation == UnitData.AnimationType.Idle) {
            animator.SetBool (UnitData.AnimationType.Walking.ToString (), false);
        }
    }
}

[BurstCompile]
public struct AnimationJob : UnityEngine.Animations.IAnimationJob {
    public void ProcessRootMotion (UnityEngine.Animations.AnimationStream stream) {

    }

    public void ProcessAnimation (UnityEngine.Animations.AnimationStream stream) {

    }
}

[BurstCompile]
public struct SimpleLoggerJob : IJob {
    /*     public Animator animator; */
    public void Execute () {
        /*   playableGraph.Play(); */
        /*    Debug.Log("Test"); */
        float result = 0;
        for (int i = 0; i < 5000; i++) {
            result += 1f;
        }
    }
}