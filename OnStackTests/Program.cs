using OnStackShared;

namespace OnStackTests;

partial class Program
{
    static void Main(string[] args)
    {
        //TestClass.OnStack x = new TestClass.OnStack();
        HelloFrom("Generated Code");

        TestClass.OnStack x = new TestClass.OnStack();
        MyHeapObject.OnStack y = new MyHeapObject.OnStack();
    }

    static void X(MyHeapObject.OnStack testClass)
    {
        Console.WriteLine(testClass);
    }

    static partial void HelloFrom(string name);
}

[OnStack]
public partial class MyHeapObject
{

}