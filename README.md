# OnStack

OnStack is a project to generate a struct variant of a class.  
This can be used to create many of those and removing them soon again in places you want to **avoid heap allocations**

**Status: Unstable** (it works but was hardly tested for bugs).  
Copies fields over, generates a Allocate() method to create the class version.

## Example

**API may change as the project is still in development**!

```cs
// MyClass.cs ----------------------------------------------
namespace MyClasses;

using OnStackShared;

[OnStack]
public partial class MyClass
{
    public int A;
    public string? B;
}

// MyClass.g.cs --------------------------------------------

// Auto-generated code
namespace MyClasses;

public partial class MyClass
{
   public struct OnStack
   {
        public int A;
        public int B;

        public MyClass Allocate()
        {
            return new TestClass()
            {
                A = this.A,
                B = this.B,
            };
        }
    }
}


// Program.cs ----------------------------------------------

MyClass.OnStack myClassOnStack = new MyClass.OnStack();

myClassOnStack.A = 1;
myClassOnStack.B = "Hello Stack!";

MyClass myClassOnHeap = myClassOnStack.Allocate();
```

For classes you don't have control over (can't make them partial), I plan making a version that generates something like `MyClass_OnStack`

## Benchmarks

- Heap Version: Creates X amount of the object as class
- Stack Version: Create X amount of the object as struct
- Stack And Allocate Version: Creates X amount of object as struct and allocate one random item

Remember that execution times depend on the hardware.

|                  Method | ObjectCount |        Mean | Allocated |
|------------------------ |------------ |------------:|----------:|
|             HeapVersion |          10 |    96.40 ns |     320 B |
|            StackVersion |          10 |    48.38 ns |         - |
| StackAndAllocateVersion |          10 |    65.46 ns |      32 B |
|             HeapVersion |         100 |   744.71 ns |    3200 B |
|            StackVersion |         100 |   288.65 ns |         - |
| StackAndAllocateVersion |         100 |   345.58 ns |      32 B |
|             HeapVersion |        1000 | 8,555.95 ns |   32000 B |
|            StackVersion |        1000 | 2,684.40 ns |         - |
| StackAndAllocateVersion |        1000 | 2,680.60 ns |      32 B |

The code:

```cs
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
```
