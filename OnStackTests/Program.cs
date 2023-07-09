using OnStackShared;

namespace OnStackTests;

class Program
{
    static void Main(string[] args)
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

[OnStack]
public partial class MyHeapObject
{
    public int Age;
    public string? Name;
}