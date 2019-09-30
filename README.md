# TypeMerger - Merge two objects into one

TypeMerger is a simple convention-based object merger for .NET Core. It allows two objects of any type to be merged into a new ``Type``. Object properties can be ignored and any collisions can be resolved using a fluent api. The object returned is a new ``Type`` that is dynamically generated and loaded using ``System.Reflection.Emit``.

## Dependencies
None
## How is it used?

### Simple usage
This will result in a new object that has All the properties from obj1 and obj2.
```
var result = TypeMerger.Merge(obj1, obj2);
```

### Ignore Certain Properties
This will result in a new object that has all of the properties from Obj1 and Obj2 Except for ``SomeProperty`` from obj1 and ``AnotherProperty`` from obj2.
```
var result = TypeMerger.Ignore(() => obj1.SomeProperty)
                       .Ignore(() => obj2.AnotherProperty)
                       .Merge(obj1, obj2); 

```

### What About Collisions? 
If both objects have the same property there is a fluent method that will tell the ``Merger`` which object to use for that property. You simply tell the ``Merger`` which property to ``Use``.

In this example given obj1 and obj2 that both have ``SomeProperty``, the value from obj2 will be used.
```
var result = TypeMerger.Use(() => obj2.SomeProperty)
                       .Merge(obj1, obj2);
```

#### *What about collisions which ``Use`` hasn't specified which object's property to use?*

If both objects have the same property, and you do not specify which one to ``Use``, then the property from the **first** object passed to ``Merge`` will be used. (Look at the ``Merge_Types_with_Name_Collision`` unit test for an example.)

### Mix & Match Your Merge
Combining the ``.Ignore`` and ``.Use`` fluent methods allows you to pick and choose what you want from your objects.

```
var obj1 = new {
    SomeProperty = "foo",
    SomeAmount = 20,
    AnotherProperty = "yo"
};

var obj2 = new {
    SomeProperty = "bar",
    SomePrivateStuff = "SECRET!!",
    SomeOtherProperty = "more stuff"
};

var result = TypeMerger.Ignore(() => obj1.AnotherProperty)
                       .Use(() => obj2.SomeProperty)
                       .Ignore(() => obj2.SomePrivateStuff)
                       .Merge(obj1, obj2);
```

The result object will have the following properties and values:

    SomeProperty: "bar"
    SomeAmount: 20
    SomeOtherProperty: "more stuff"




## History
The code is based on the original TypeMerger class written by [Mark Miller](http://www.developmentalmadness.com/). Updated, enhanced, and now maintained by [Kyle Finley](https://twitter.com/kfinley).

Original posting: [KyleFinley.net/typemerger](http://goo.gl/qJ9FqN)


