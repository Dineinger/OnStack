# OnStack

OnStack is a project to generate a struct variant of a class.  
This can be used to create many of those and removing them again in places you want to **avoid heap allocations**

Status: Generates a struct and copies fields over

## Example

**API may change as the project is still in development**!

```cs
// MyClass.cs

using OnStackShared;

[OnStack]
public partial class MyClass
{
    public int A;
    public string? B;
}

// Program.cs

MyClass.OnStack myClassOnStack = new MyClass.OnStack();

myClassOnStack.A = 1;
myClassOnStack.B = "Hello Stack!";
```

For classes you don't have control over (can't make them partial), I plan making a version that generates something like `MyClass_OnStack`