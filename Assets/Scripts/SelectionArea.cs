using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

class SelectionArea : MonoBehaviour
{
    private BlobAssetStore blobAssetStore;
    UnitControlSystem unitControlSystem;

    SelectionAreaSystem selectionAreaSystem;
    [SerializeField] GameObject selectionAreaPrefab;
    [SerializeField] GameObject selectCirclePrefab;
    void Start()
    {
        blobAssetStore = new BlobAssetStore();
        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, blobAssetStore);
        var convertedSelectionAreaPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(selectionAreaPrefab, settings);
        var convertedSelectCirclePrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(selectCirclePrefab, settings);
        var selectionAreaSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<SelectionAreaSystem>();
        selectionAreaSystem.prefab = convertedSelectionAreaPrefab;

        var unitSelectRenderer = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<UnitSelectRenderer>();
        unitSelectRenderer.circleEntity = convertedSelectCirclePrefab;

    }
    private void OnDestroy()
    {
        if (blobAssetStore != null) { blobAssetStore.Dispose(); }
    }
}