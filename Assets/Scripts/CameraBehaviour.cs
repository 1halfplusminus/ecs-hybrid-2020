using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour, IConvertGameObjectToEntity
{
    Entity entity;

    EntityManager entityManager;
    public void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        this.entity = entity;
        var camera = GetComponent<Camera>();
        dstManager.AddComponentObject(entity, gameObject);
        dstManager.AddComponentData(entity, new CameraData { size = camera.orthographicSize });
        dstManager.AddComponentData(entity, new CopyTransformToGameObject());
    }
    void Update()
    {
        var camera = GetComponent<Camera>();
        var cameraData = entityManager.GetComponentData<CameraData>(entity);
        camera.orthographicSize = cameraData.size;
    }
}
