using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


public class ControlUnitsGiveOrders : MonoBehaviour
{
    private BlobAssetStore blobAssetStore;

    [SerializeField] GameObject unitPrefabs;
    // Start is called before the first frame update
    void Start()
    {
        blobAssetStore = new BlobAssetStore();
        // Create entity prefab from the game object hierarchy once
        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, blobAssetStore);
        var convertedTargetPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(unitPrefabs, settings);
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        for (int i = 0; i < 5000; i++)
        {
            var entity = entityManager.Instantiate(convertedTargetPrefab);
            entityManager.SetComponentData(entity, new Translation() { Value = GetRandomPosition(new float3(100f, 50f, 0f)) });

        }
    }
    float3 GetRandomPosition(float3 size)
    {
        return new float3(UnityEngine.Random.Range(-size.x, +size.x), UnityEngine.Random.Range(-size.y, +size.y), UnityEngine.Random.Range(-size.z, +size.z));
    }

    private void OnDestroy()
    {
        if (blobAssetStore != null) { blobAssetStore.Dispose(); }
    }

}
