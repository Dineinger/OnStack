using BenchmarkDotNet.Running;
using OnStackShared;
using OnStackTests.Benchmarks;

namespace OnStackTests;

class Program
{
    static void Main(string[] args)
    {
        BenchmarkRunner.Run<OnStackDefaultBenchmarks>();
    }

    static void APITests()
    {
        MyHeapObject.OnStack x = new MyHeapObject.OnStack();
        x.Name = "Test";
        x.Age = 0;
        X(x);
    }

    static void X(MyHeapObject.OnStack testClass)
    {
        Console.WriteLine($"Age: {testClass.Age}");
        Console.WriteLine(testClass.Name ?? "Was Null");
    }
}

