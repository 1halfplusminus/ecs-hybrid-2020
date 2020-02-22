using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class GameHandlerX : MonoBehaviour
{
    private BlobAssetStore blobAssetStore;
    [SerializeField] GameObject targetPrefabs;
    [SerializeField] GameObject searchPrefabs;

    // Start is called before the first frame update
    void Start()
    {
        blobAssetStore = new BlobAssetStore();
        // Create entity prefab from the game object hierarchy once
        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, blobAssetStore);
        var convertedTargetPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(targetPrefabs, settings);
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        for (int i = 0; i < 20000; i++)
        {
            var entity = entityManager.Instantiate(convertedTargetPrefab);
            entityManager.SetComponentData(entity, new Translation() { Value = GetRandomPosition() });
        }
        var convertedSearchPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(searchPrefabs, settings);
        for (int i = 0; i < 1000; i++)
        {
            var entity = entityManager.Instantiate(convertedSearchPrefab);
            entityManager.SetComponentData(entity, new Translation() { Value = GetRandomPosition() });
        }
    }
    float3 GetRandomPosition()
    {
        return new float3(UnityEngine.Random.Range(-100, +100f), UnityEngine.Random.Range(-50, +50f), 0);
    }

    private void OnDestroy()
    {
        if (blobAssetStore != null) { blobAssetStore.Dispose(); }
    }
}
