using Unity.Entities;
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


[UpdateAfter(typeof(UnitControlSystem))]
public class SelectionAreaSystem : ComponentSystem
{
    public Entity prefab;

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<SelectionAreaData>();
        base.OnCreate();
    }
    protected override void OnUpdate()
    {
        SelectionAreaData selectionAreaData = GetSingleton<SelectionAreaData>();
        if (selectionAreaData.isSelecting)
        {
            CreateIfDontExist();
            Entities.WithAll<SelectionAreaTag>().ForEach((Entity entity, ref Translation translation, ref NonUniformScale scale) =>
           {
               translation.Value = selectionAreaData.startPosition;
               scale.Value = selectionAreaData.selectionAreaSize;
           });
        }
        else
        {
            Entities.WithAll<SelectionAreaTag>().ForEach((Entity entity) =>
            {
                EntityManager.DestroyEntity(entity);
            });
        }
    }

    protected void CreateIfDontExist()
    {
        EntityQuery entityQuery = GetEntityQuery(typeof(SelectionAreaTag));
        if (entityQuery.CalculateEntityCount() == 0)
        {
            EntityManager.Instantiate(prefab);
        }
    }
}