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
