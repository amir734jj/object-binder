# object-binder

## Description
Given a type X (maybe an interface or class) and type Y (has to be an interface), this library will generate a type Z where it implements interface Y and take an instance of X in the constructor where for all properties:

- getter will return value of bound property of X
- setter will apply value change back to Y

This library uses [Lamar Code Weaving](https://jasperfx.github.io/lamar/documentation/compilation/) for runtime IL generation, [InfoViaLinq](https://github.com/amir734jj/InfoViaLinq) to extract `PropertyInfo` from `MemberExpression` and [Guard](https://github.com/safakgur/guard) for validation.

## How to use:
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

Runtime generated class:
```csharp
public class ICommon_proxy_0 : ICommon {
    
    private readonly Entity _source = source;
    
    public ICommon_proxy_0(Entity source) { _source = source; }
    
    public string NameC { get { return _source.Name; } set { _source.Name = value; } }
    
    public bool AliveC { get { return _source.Alive; } set { _source.Alice = value; } }
}
```

## Note:
This library is useful when we are dealing with a class with many many properties and we want to map it to another class with smaller set of properties where changes to simplified object properties will be applied back to original object properties.

## Credit:
Special thanks to [@alexsniffin](https://github.com/alexsniffin) who came up with the original idea.

