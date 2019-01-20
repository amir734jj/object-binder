# object-binder

Given a type X (maybe an interface or class) and type Y (has to be an interface), this library will generate a type Z where it implements interface Y and take an instance of X in the constructor where for all properties:

- getter will return value of bound property of X
- setter will apply value change back to Y

Given class:
```csharp
public class Entity
{
    public string Name { get; set; }
    
    public int Age { get; set; }
    
    public bool Alive { get; set; }
    
    public Guid Id { get; set; }
}
```
And interface:
```csharp
public interface ICommon
{
    string NameC { get; set; }

    bool AliveC { get; set; }
}
```

We can generate a bound type:
```csharp
var boundType = ObjectBinderBuilder.New<Entity, ICommon>()
    .Bind(x => x.Name, x => x.NameC)
    .Bind(x => x.Alive, x => x.AliveC)
    .Build()
    .BoundType;
    
var sourceObj = new Entity { ... };
var proxy = (ICommon) Activator.CreateInstance(boundType, sourceObj);

proxy.NameC = "Something else...";

// Assert property change in `sourceObj`
Assert.Equals(sourceObj.Name, proxy.NameC);
```

## Note:
This library is useful when we are dealing with a class with many many properties and we want to map it to another class with smaller set of fields where changes to simplified object will be applied back to original object. 
