using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using Unity.Collections;
public class TestingJobOutput1 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SimpleJob simpleJob = new SimpleJob
        {
            a = 1,
            b = 2,
            result = new NativeArray<int>(1, Allocator.TempJob)
        };
        JobHandle jobHandle = simpleJob.Schedule();
        jobHandle.Complete();

        Debug.Log(simpleJob.result[0]);

        simpleJob.result.Dispose();
    }

    // Update is called once per frame
    void Update()
    {

    }
}


public struct SimpleJob : IJob
{
    public int a;
    public int b;

    public NativeArray<int> result;
    public void Execute()
    {
        result[0] = a + b;
    }
}