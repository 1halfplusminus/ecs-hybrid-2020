using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

public class Testing2 : MonoBehaviour {
    [SerializeField] private bool useJobs;
    [SerializeField] private Transform pfZombie;

    private List<Zombie> zombies;
    public class Zombie {
        public Transform transform;
        public float moveY;
    }

    private void Start () {
        zombies = new List<Zombie> ();
        for (int i = 0; i < 1000; i++) {
            Transform zombieTransform = Instantiate (pfZombie, new Vector3 (UnityEngine.Random.Range (-8f, 8f), UnityEngine.Random.Range (-5f, 5f)), Quaternion.identity);
            zombies.Add (new Zombie {
                transform = zombieTransform,
                    moveY = UnityEngine.Random.Range (1f, 2f)
            });
        }
    }
    // Update is called once per frame
    void Update () {
        float startTime = Time.realtimeSinceStartup;
        if (!useJobs) {
            foreach (var zombie in zombies) {
                zombie.transform.position += new Vector3 (0, zombie.moveY * Time.deltaTime);
                if (zombie.transform.position.y > 5f) {
                    zombie.moveY = -math.abs (zombie.moveY);
                }
                if (zombie.transform.position.y < -5f) {
                    zombie.moveY = +math.abs (zombie.moveY);
                }
                ReallyToughTask ();
            }
        } else {
            /*   NativeArray<float3> positionArray = new NativeArray<float3> (zombies.Count, Allocator.TempJob); */
            NativeArray<float> moveYs = new NativeArray<float> (zombies.Count, Allocator.TempJob);
            TransformAccessArray transformAccessArray = new TransformAccessArray (zombies.Count);
            for (int i = 0; i < zombies.Count; i++) {
                transformAccessArray.Add (zombies[i].transform);
                moveYs[i] = zombies[i].moveY;
            }
            ReallyToughParallelJobTransform reallyTough = new ReallyToughParallelJobTransform () {
                deltaTime = Time.deltaTime,
                moveY = moveYs,
            };

            JobHandle handle = reallyTough.Schedule (transformAccessArray);
            handle.Complete ();

            for (int i = 0; i < zombies.Count; i++) {
                zombies[i].moveY = moveYs[i];
            }

            /*    positionArray.Dispose (); */
            moveYs.Dispose ();
            transformAccessArray.Dispose ();
        }

        /*   if (!useJobs) {
              for (int i = 0; i < 10; i++) {
                  ReallyToughTask ();
              }

          } else {
              NativeList<JobHandle> jobHandles = new NativeList<JobHandle> (Allocator.Temp);
              for (int i = 0; i < 10; i++) {
                  JobHandle jobHandle = ReallyToughTaskJob ();
                  jobHandles.Add (jobHandle);
              }
              JobHandle.CompleteAll (jobHandles);
              jobHandles.Dispose ();
          } */
        Debug.Log (((Time.realtimeSinceStartup - startTime) * 1000f) + "ms");
    }

    void ReallyToughTask () {
        float value = 0f;
        for (int i = 0; i < 50000; i++) {
            value = math.exp10 (math.sqrt (value));
        }
    }

    private JobHandle ReallyToughTaskJob () {
        ReallyToughJob job = new ReallyToughJob ();
        return job.Schedule ();
    }
}

[BurstCompile]
public struct ReallyToughJob : IJob {
    public float something;
    public void Execute () {
        float value = 0f;
        for (int i = 0; i < 50000; i++) {
            value = math.exp10 (math.sqrt (value));
        }
    }
}

[BurstCompile]
public struct ReallyToughParallelJob : IJobParallelFor {
    public NativeArray<float> moveY;
    public NativeArray<float3> position;

    public float deltaTime;
    public void Execute (int index) {
        position[index] += new float3 (0, moveY[index] * deltaTime, 0);
        if (position[index].y > 5f) {
            moveY[index] = -math.abs (moveY[index]);
        }
        if (position[index].y < -5f) {
            moveY[index] = +math.abs (moveY[index]);
        }
        float value = 0f;
        for (int i = 0; i < 50000; i++) {
            value = math.exp10 (math.sqrt (value));
        }
    }
}

[BurstCompile]
public struct ReallyToughParallelJobTransform : IJobParallelForTransform {
    public NativeArray<float> moveY;

    public float deltaTime;

    public void Execute (int index, TransformAccess transform) {
        transform.position += new Vector3 (0, moveY[index] * deltaTime, 0);
        if (transform.position.y > 5f) {
            moveY[index] = -math.abs (moveY[index]);
        }
        if (transform.position.y < -5f) {
            moveY[index] = +math.abs (moveY[index]);
        }
        float value = 0f;
        for (int i = 0; i < 50000; i++) {
            value = math.exp10 (math.sqrt (value));
        }
    }
}