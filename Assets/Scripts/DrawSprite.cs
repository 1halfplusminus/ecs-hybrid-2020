using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class DrawSprite : MonoBehaviour
{
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material zombieMaterial;

    [SerializeField] private Material kunaiMaterail;

    private EntityManager entityManager;
    // Start is called before the first frame update
    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        NativeArray<Entity> entityArray = new NativeArray<Entity>(20, Allocator.Temp);
        EntityArchetype entityArchetype = entityManager.CreateArchetype(
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(Translation),
            typeof(Rotation)
        /*  typeof(Scale), */
        /*     typeof(NonUniformScale) */
        );

        entityManager.CreateEntity(entityArchetype, entityArray);
        var zombieMesh = CreateMesh(1f, 1f);
        var kunaiMesh = CreateMesh(0.5f, 1f);
        for (int i = 0; i < entityArray.Length; i++)
        {
            var entity = entityArray[i];
            entityManager.SetComponentData(entity, new Translation()
            {
                Value = new float3 { x = UnityEngine.Random.Range(-8f, 8f), y = UnityEngine.Random.Range(-8f, 8f), z = 0 }
            });
            entityManager.SetSharedComponentData(entity, new RenderMesh()
            {
                mesh = (i < 10) ? zombieMesh : kunaiMesh,
                material = (i < 10) ? zombieMaterial : kunaiMaterail
            });
            /* entityManager.SetComponentData(entity, new NonUniformScale()
            {
                Value = new float3(1, 1, 1)
            }); */
        }
        entityArray.Dispose();
    }

    private Mesh CreateMesh(float width, float height)
    {
        Vector3[] vertices = new Vector3[4];
        Vector2[] uv = new Vector2[4];
        int[] triangles = new int[6];

        float halftWidth = width / 2f;
        float halftHeight = height / 2f;
        vertices[0] = new Vector3(-halftWidth, -halftHeight);
        vertices[1] = new Vector3(-halftWidth, +halftHeight);
        vertices[2] = new Vector3(halftWidth, halftHeight);
        vertices[3] = new Vector3(halftWidth, -halftHeight);

        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(0, 1);
        uv[2] = new Vector2(1, 1);
        uv[3] = new Vector2(1, 0);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 3;

        triangles[3] = 1;
        triangles[4] = 2;
        triangles[5] = 3;

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        return mesh;
    }
}

[DisableAutoCreation]
public class MoveSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        float time = Time.DeltaTime;
        float moveSpeed = 1f;
        Entities.ForEach((ref Translation translation) =>
        {
            translation.Value.y += time * moveSpeed;
        });
    }
}
[DisableAutoCreation]
public class RotatorSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        double time = Time.ElapsedTime;
        Entities.ForEach((ref Rotation rotation) =>
        {
            rotation.Value = quaternion.Euler(0, 0, math.PI * (float)time);
        });
    }
}
[DisableAutoCreation]
public class ScaleSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        float time = Time.DeltaTime;
        Entities.ForEach((ref Scale scale) =>
        {
            scale.Value = 1f * time;
        });
    }
}
[DisableAutoCreation]
public class ScaleNonUniformSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        float time = Time.DeltaTime;
        Entities.ForEach((ref NonUniformScale scale) =>
        {
            scale.Value += new float3(1f * time, 0.5f * time, 0.3f * time);
        });
    }
}