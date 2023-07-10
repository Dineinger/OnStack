using BenchmarkDotNet.Attributes;
using OnStackShared;
using System.Buffers;

namespace OnStackTests.Benchmarks;

[MemoryDiagnoser]
public class OnStackDefaultBenchmarks
{
    [Params(10, 100, 1000)]
    public int ObjectCount;

    private static readonly ArrayPool<MyHeapObject> heapObjectsPool = ArrayPool<MyHeapObject>.Shared;
    private static readonly ArrayPool<MyHeapObject.OnStack> stackObjectsPool = ArrayPool<MyHeapObject.OnStack>.Shared;

    [Benchmark]
    public void HeapVersion()
    {
        var array = heapObjectsPool.Rent(ObjectCount);

        for (int a = 0; a < ObjectCount; a++)
        {
            array[a] = new MyHeapObject()
            {
                Age = 0,
                Name = "Test"
            };
        }

        heapObjectsPool.Return(array);
    }

    [Benchmark]
    public void StackVersion()
    {
        var array = stackObjectsPool.Rent(ObjectCount);

        for (int a = 0; a < ObjectCount; a++)
        {
            array[a] = new MyHeapObject.OnStack()
            {
                Age = 0,
                Name = "Test"
            };
        }

        stackObjectsPool.Return(array);
    }

    [Benchmark]
    public MyHeapObject StackAndAllocateVersion()
    {
        var array = stackObjectsPool.Rent(ObjectCount);

        for (int a = 0; a < ObjectCount; a++)
        {
            array[a] = new MyHeapObject.OnStack()
            {
                Age = 0,
                Name = "Test"
            };
        }

        var x = array[Random.Shared.Next(0, ObjectCount)].Allocate();

        stackObjectsPool.Return(array);

        return x;
    }
}

[OnStack]
public partial class MyHeapObject
{
    public int Age;
    public string? Name;
}
