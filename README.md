# 💉 AutoInject

[![Chickensoft Badge][chickensoft-badge]][chickensoft-website] [![Discord][discord-badge]][discord] ![line coverage][line-coverage] ![branch coverage][branch-coverage]

Node-based dependency injection for C# Godot scripts at build-time, including utilities for automatic node-binding, additional lifecycle hooks, and .net-inspired notification callbacks.

---

<p align="center">
<img alt="Chickensoft.AutoInject" src="Chickensoft.AutoInject/icon.png" width="200">
</p>

## 📘 Background

Game scripts quickly become difficult to maintain when strongly coupled to each other. Various approaches to dependency injection are often used to facilitate weak coupling. For C# scripts in Godot games, AutoInject is provided to allow nodes higher in the scene tree to provide dependencies to their descendant nodes lower in the tree.

AutoInject borrows the concept of a `Provider` and a `Dependent` from [other tree-based dependency provisioning systems][provider]. A `Provider` node provides values to its descendant nodes. A `Dependent` node requests values from its ancestor nodes.

Because `_Ready/OnReady` is called on node scripts further down the tree first in Godot (see [Understanding Tree Order][tree-order] for more), nodes lower in the tree often cannot access the values they need since they do not exist until their ancestors have a chance to create them in their own `_Ready/OnReady` methods. AutoInject solves this problem by temporarily subscribing to each `Provider` it finds that is still initializing from each `Dependent` until it knows the dependencies have been resolved.

Providing nodes "top-down" over sections of the game's scene tree has a few advantages:

- ✅ Dependent nodes can find the nearest ancestor that provides the value they need, allowing provided values to be overridden easily (when desired).
- ✅ Nodes can be moved around the scene tree without needing to update their dependencies.
- ✅ Nodes that end up under a different provider will automatically use that new provider's value.
- ✅ Scripts don't have to know about each other.
- ✅ The natural flow-of-data mimics the other patterns used throughout the Godot engine.
- ✅ Dependent scripts can still be run in isolated scenes by providing default fallback values.
- ✅ Scoping dependencies to the scene tree prevents the existence of values that are invalid above the provider node.
- ✅ Resolution occurs in O(n), where `n` is the height of the tree above the requesting dependent node (usually only a handful of nodes to search). For deep trees, "reflecting" dependencies by re-providing them further down the tree speeds things up further.
- ✅ Dependencies are resolved when the node enters the scene tree, allowing for O(1) access afterwards. Exiting and re-entering the scene tree triggers the dependency resolution process again.
- ✅ Scripts can be both dependents and providers.

## 📼 About Mixins

The [Introspection] generator that AutoInject uses allows you to add [mixins] to an existing C# class. Mixins are similar to interfaces, but they allow a node to gain additional instance state, as well as allow the node to know which mixins are applied to it and invoke mixin handler methods — all without reflection.

In addition, AutoInject provides a few extra utilities to make working with node scripts even easier:

- 🎮 `IAutoOn`: allow node scripts to implement .NET-style handler methods for Godot notifications: i.e., `OnReady`, `OnProcess`, etc.
- 🪢 `IAutoConnect`: automatically bind properties marked with `[Node]` to a node in the scene tree — also provides access to nodes via their interfaces using [GodotNodeInterfaces].
- 🛠 `IAutoInit`: adds an additional lifecycle method that is called before `_Ready` if (and only if) the node's `IsTesting` property is set to false. The additional lifecycle method for production code enables you to more easily unit test code by separating initialization logic from the engine lifecycle.
- 🎁 `IProvider`: a node that provides one or more dependencies to its descendants. Providers must implement `IProvide<T>` for each type of value they provide.
- 🔗 `IDependent`: a node that depends on one or more dependencies from its ancestors. Dependent nodes must mark their dependencies with the `[Dependency]` attribute and call `this.DependOn<T>()` to retrieve the value.
- 🐤 `IAutoNode`: a mixin that applies all of the above mixins to a node script at once.

Want all the functionality that AutoInject provides? Simply add this to your Godot node:

```csharp
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;

// Apply all of the AutoInject mixins at once:
[Meta(typeof(IAutoNode))]
public partial class MyNode : Node {
  public override void _Notification(int what) => this.Notify(what);
}
```

Alternatively, you can use just the mixins you need from this project.

```csharp
[Meta(
  typeof(IAutoOn),
  typeof(IAutoConnect),
  typeof(IAutoInit),
  typeof(IProvider),
  typeof(IDependent)
)]
public partial class MyNode : Node {
  public override void _Notification(int what) => this.Notify(what);
}
```

> [!IMPORTANT]
> For the mixins to work, you must override `_Notification` in your node script and call `this.Notify(what)` from it. This is necessary for the mixins to know when to invoke their handler methods. Unfortunately, there is no way around this since Godot must see the `_Notification` method in your script to generate handlers for it.
>
> ```csharp
> public override void _Notification(int what) => this.Notify(what);
> ```

## 📦 Installation

AutoInject is a source-only package that uses the [Introspection] source generator. AutoInject provides two mixins: `IDependent` and `IProvider` that must be applied with the Introspection generator's `[Meta]`.

You'll need to include `Chickensoft.Introspection`, `Chickensoft.Introspection.Generator`, and `Chickensoft.AutoInject` in your project. All of the packages are extremely lightweight.

Simply add the following to your project's `.csproj` file. Be sure to specify the appropriate versions for each package by checking on [Nuget](https://www.nuget.org/packages?q=Chickensoft).

```xml
<ItemGroup>
    <PackageReference Include="Chickensoft.GodotNodeInterfaces" Version="..." />
    <PackageReference Include="Chickensoft.Introspection" Version="..." />
    <PackageReference Include="Chickensoft.Introspection.Generator" Version="..." PrivateAssets="all" OutputItemType="analyzer" />
    <PackageReference Include="Chickensoft.AutoInject" Version="..." PrivateAssets="all" />
</ItemGroup>
```

You can also add `Chickensoft.AutoInject.Analyzers` to your project to get additional checks and code fixes for AutoInject, such as ensuring that you override `_Notification` and call `this.Provide()` from your provider nodes.

```xml
<ItemGroup>
    <PackageReference Include="Chickensoft.AutoInject.Analyzers" Version="..." PrivateAssets="all" OutputItemType="analyzer" />
</ItemGroup>
```

> [!WARNING]
> We strongly recommend treating warning `CS9057` as an error to catch possible compiler-mismatch issues with the Introspection generator. (See the [Introspection] README for more details.) To do so, add a `WarningsAsErrors` line to your `.csproj` file's `PropertyGroup`:
>
> ```xml
> <PropertyGroup>
>   <TargetFramework>net8.0</TargetFramework>
>   ...
>   <!-- Catch compiler-mismatch issues with the Introspection generator -->
>   <WarningsAsErrors>CS9057</WarningsAsErrors>
>   ...
> </PropertyGroup>
> ```

> [!TIP]
> Want to see AutoInject in action? Check out the Chickensoft [Game Demo].

## 🎁 Providers

To provide values to descendant nodes, add the `IProvider` mixin to your node script and implement `IProvide<T>` for each value you'd like to make available.

Once providers have initialized the values they provide, they must call the `this.Provide()` extension method to inform AutoInject that the provided values are now available.

The example below shows a node script that provides a `string` value to its descendants. Values are always provided by their type.

```csharp
namespace MyGameProject;

using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;

[Meta(typeof(IAutoNode))]
public partial class MyProvider : Node, IProvide<string> {
  public override void _Notification(int what) => this.Notify(what);

  string IProvide<string>.Value() => "Value"

  // Call the this.Provide() method once your dependencies have been initialized.
  public void OnReady() => this.Provide();

  public void OnProvided() {
    // You can optionally implement this method. It gets called once you call
    // this.Provide() to inform AutoInject that the provided values are now
    // available.
  }
}
```

## 🐣 Dependents

To use a provided value in a descendant node somewhere, add the `IDependent` mixin to your descendent node script and mark each dependency with the `[Dependency]` attribute. The notification method override is used to automatically tell the mixins when your node is ready and begin the dependency resolution process.

Once all of the dependencies in your dependent node are resolved, the `OnResolved()` method of your dependent node will be called (if overridden).

```csharp
namespace MyGameProject;

using Chickensoft.Introspection;
using Godot;

[Meta(typeof(IAutoNode))]
public partial class StringDependent : Node {
  public override void _Notification(int what) => this.Notify(what);

  [Dependency]
  public string MyDependency => this.DependOn<string>();

  public void OnResolved() {
    // All of my dependencies are now available! Do whatever you want with
    // them here.
  }
}
```

The `OnResolved` method will be called after `_Ready/OnReady`, but before the first frame if (and only if) all the providers it depends on call `this.Provide()` before the first frame.

Essentially, `OnResolved` is called when the slowest provider has finished
providing dependencies. For the best experience, do not wait until processing occurs to call `Provide` from your providers.

If you have a node script which is both a `Dependent` and a `Provider`, you can safely call `Provide` from the `OnResolved` method to allow it to provide dependencies.

The general rule of thumb for any `Provider` node is as follows: **call `Provide` as soon as you possibly can: either from `_Ready/OnReady` or from `OnResolved`.** If all providers in your project follow this rule, dependency provision will complete before processing occurs for nodes that are already in the tree. Dependent nodes added later will begin the dependency resolution process once the node receives the `Node.NotificationReady` notification.

## 🙏 Tips

### Keep Dependency Trees Simple

For best results, keep dependency trees simple and free from asynchronous initialization. If you try to get too fancy, you can introduce dependency resolution deadlock. Avoiding complex dependency hierarchies can often be done with a little extra experimentation as you design your game.

### Listen to Dependencies

Instead of subscribing to a parent node's events, consider subscribing to events emitted by the dependency values themselves.

```csharp
[Meta(typeof(IAutoNode))]
public partial class MyDependent : Node {
  public override void _Notification(int what) => this.Notify(what);

  [Dependency]
  public MyValue Value => this.DependOn<MyValue>();

  public void OnResolved() {
    // Setup subscriptions once dependencies are valid.
    MyValue.OnSomeEvent += ValueUpdated
  }

  public void OnTreeExit() {
    // Clean up subscriptions here!
    MyValue.OnSomeEvent -= ValueUpdated
  }

  public void ValueUpdated() {
    // Do something in response to the value we depend on changing.
  }
}
```

### Fallback Values

You can provide fallback values to use when a provider can't be found. This can make it easier to run a scene by itself from the editor without having to worry about setting up production dependencies. Naturally, the fallback value will only be used if a provider can't be found for that type above the dependent node.

```csharp
[Dependency]
public string MyDependency => this.DependOn<string>(() => "fallback_value");
```

### Faking Dependencies

Sometimes, when testing, you may wish to "fake" the value of a dependency. Faked dependencies take precedence over any providers that may exist above the dependent node, as well as any provided fallback value.

```csharp
  [Test]
  public void FakesDependency() {
    // Some dependent
    var dependent = new MyNode();

    var fakeValue = "I'm fake!";
    dependent.FakeDependency(fakeValue);

    TestScene.AddChild(dependent);

    dependent._Notification((int)Node.NotificationReady);

    dependent.OnResolvedCalled.ShouldBeTrue();
    dependent.MyDependency.ShouldBe(fakeValue);

    TestScene.RemoveChild(dependent);
  }
```

## ❓ How AutoInject Works

AutoInject uses a simple, specific algorithm to resolve dependencies.

- When the Dependent mixin is added to an introspective node, the Introspection generator will generate metadata about the type which allows AutoInject to determine what properties the type has, as well as see their attributes.
- A node script with the Dependent mixin observes its lifecycle. When it notices the `Node.NotificationReady` signal, it will begin the dependency resolution process without you having to write any code in your node script.
- The dependency process works as follows:
  - All properties of the node script are inspected using the metadata generated by the Introspection generator. This allows the script to introspect itself without having to resort to C#'s runtime reflection calls. Properties with the `[Dependency]` attribute are collected into the set of required dependencies.
  - All required dependencies are added to the remaining dependencies set.
  - The dependent node begins searching its ancestors, beginning with itself, then its parent, and so on up the tree.
    - If the current search node implements `IProvide` for any of the remaining dependencies, the individual resolution process begins.
      - The dependency stores the provider in a dictionary property in the node script.
      - The dependency is added to the set of found dependencies.
      - If the provider search node has not already provided its dependencies, the dependent subscribes to the `OnInitialized` event of the provider.
      - Pending dependency provider callbacks track a counter for the dependent node that also remove that provider's dependency from the remaining dependencies set and initiate the OnResolved process if nothing is left.
    - After checking all the remaining dependencies, the set of found dependencies are removed from the remaining dependencies set and the found dependencies set is cleared for the next search node.
    - If all the dependencies are found, the dependent initiates the OnResolved process and finishes the search.
    - Otherwise, the search node's parent becomes the next parent to search.
  - Search concludes when providers for each dependency are found, or the top of the scene tree is reached.

There are some natural consequences to this algorithm, such as `OnResolved` not being invoked on a dependent until all providers have provided a value. This is intentional — providers are expected to synchronously initialize their provided values after `_Ready` has been invoked on them.

AutoInject primarily exists to to locate providers from dependents and subscribe to the providers just long enough for their own `_Ready` method to be invoked — waiting longer than that to call `Provide` from a provider can introduce dependency resolution deadlock or other undesirable circumstances that are indicative of an anti-pattern.

By calling `this.Provide()` from `_Ready` in provider nodes, you ensure that the order of execution unfolds as follows, synchronously:

  1. Dependent node `_Ready` (descendant of the provider, deepest nodes ready-up first).
  2. Provider node `_Ready` (which calls `Provide`).
  3. Dependent `OnResolved`
  4. Frame 1 `_Process`
  5. Frame 2 `_Process`
  6. Etc.

By following the `this.Provide()` on `_Ready` convention, you guarantee all dependent nodes receive an `OnResolved` callback before the first process invocation occurs, guaranteeing that nodes are setup before frame processing begins ✨.

> [!TIP]
> If your provider is also a dependent, you can call `this.Provide()` from `OnResolved()` to allow it to provide dependencies to its subtree, which still guarantees that dependency resolution happens before the next frame is processed.
>
> In general, dependents should have access to their dependencies **before** frame processing callbacks are invoked on them.

## 🪢 IAutoConnect

The `IAutoConnect` mixin automatically connects properties in your script to a declared node path or unique node name in the scene tree whenever the scene is instantiated, without reflection. It can also be used to connect nodes as interfaces.

Simply apply the `[Node]` attribute to any field or property in your script that you want to automatically connect to a node in your scene.

If you don't specify a node path in the `[Node]` attribute, the name of the field or property will be converted to a [unique node identifier][unique-nodes] name in PascalCase. For example, the field name below `_my_unique_node` is converted to the unique node path name `%MyUniqueNode` by converting the property name to PascalCase and prefixing the percent sign indicator. Likewise, the property name `MyUniqueNode` is converted to `%MyUniqueNode`, which isn't much of a conversion since the property name is already in PascalCase.

For best results, use PascalCase for your node names in the scene tree (which Godot tends to do by default, anyways).

In the example below, we're using [GodotNodeInterfaces] to reference nodes as their interfaces instead of their concrete Godot types. This allows us to write a unit test where we fake the nodes in the scene tree by substituting mock nodes, allowing us to test a single node script at a time without polluting our test coverage.

```csharp
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;

[Meta(typeof(IAutoConnect))]
public partial class MyNode : Node2D {
  public override void _Notification(int what) => this.Notify(what);

  [Node("Path/To/SomeNode")]
  public INode2D SomeNode { get; set; } = default!;

  [Node] // Connects to "%MyUniqueNode" since no path was specified.
  public INode2D MyUniqueNode { get; set; } = default!;

  [Node("%OtherUniqueName")]
  public INode2D DifferentName { get; set; } = default!;
}
```

> [!TIP]
> `IAutoConnect` can only bind properties to nodes, not fields.

### 🧪 Testing

AutoConnect integrates seamlessly with [GodotNodeInterfaces] to facilitate unit testing Godot node scripts by allowing you to fake the node tree during testing.

We can easily write a test for the example above by substituting mock nodes:

```csharp
namespace Chickensoft.AutoInject.Tests;

using System.Threading.Tasks;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.GoDotTest;
using Chickensoft.AutoInject.Tests.Fixtures;
using Godot;
using GodotTestDriver;
using Moq;
using Shouldly;

#pragma warning disable CA1001
public class MyNodeTest(Node testScene) : TestClass(testScene) {
  private Fixture _fixture = default!;
  private MyNode _scene = default!;

  private Mock<INode2D> _someNode = default!;
  private Mock<INode2D> _myUniqueNode = default!;
  private Mock<INode2D> _otherUniqueNode = default!;

  [Setup]
  public async Task Setup() {
    _fixture = new(TestScene.GetTree());

    _someNode = new();
    _myUniqueNode = new();
    _otherUniqueNode = new();

    _scene = new MyNode();
    _scene.FakeNodeTree(new() {
      ["Path/To/SomeNode"] = _someNode.Object,
      ["%MyUniqueNode"] = _myUniqueNode.Object,
      ["%OtherUniqueName"] = _otherUniqueNode.Object,
    });

    await _fixture.AddToRoot(_scene);
  }

  [Cleanup]
  public async Task Cleanup() => await _fixture.Cleanup();

  [Test]
  public void UsesFakeNodeTree() {
    // Making a new instance of a node without instantiating a scene doesn't
    // trigger NotificationSceneInstantiated, so if we want to make sure our
    // AutoNodes get hooked up and use the FakeNodeTree, we need to do it manually.
    _scene._Notification((int)Node.NotificationSceneInstantiated);

    _scene.SomeNode.ShouldBe(_someNode.Object);
    _scene.MyUniqueNode.ShouldBe(_myUniqueNode.Object);
    _scene.DifferentName.ShouldBe(_otherUniqueNode.Object);
    _scene._my_unique_node.ShouldBe(_myUniqueNode.Object);
  }
}
```

## 🛠 IAutoInit

The `IAutoInit` will conditionally call the `void Initialize()` method your node script has from `_Ready` if (and only if) the `IsTesting` field that it adds to your node is false. Conditionally calling the `Initialize()` method allows you to split your node's late member initialization into two-phases, allowing nodes to be more easily unit tested.

When writing tests for your node, simply initialize any members that would need to be mocked in a test in your `Initialize()` method.

```csharp
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;

[Meta(typeof(IAutoInit), typeof(IAutoOn))]
public partial class MyNode : Node2D {
  public override void _Notification(int what) => this.Notify(what);

  public IMyObject Obj { get; set; } = default!;

  public void Initialize() {
    // Initialize is called from the Ready notification if our IsTesting
    // property (added by IAutoInit) is false.

    // Initialize values which would be mocked in a unit testing method.
    Obj = new MyObject();
  }

  public void OnReady() {
    // Guaranteed to be called after Initialize()

    // Use object we setup in Initialize() method (or, if we're running in a
    // unit test, this will use whatever the test supplied)
    Obj.DoSomething();
  }
}
```

Likewise, when creating a node during a unit test, you can set the `IsTesting` property to `true` to prevent the `Initialize()` method from being called.

```csharp
var myNode = new MyNode() {
  Obj = mock.Object
};

(myNode as IAutoInit).IsTesting = true;
```

For example tests, please see the [Game Demo] project.

## 🌱 Enhanced Lifecycle

AutoInject enhances the typical Godot node lifecycle by adding additional hooks that allow you to handle dependencies and initialization in a more controlled manner (primarily for making testing easier).

This is the lifecycle of a dependent node in the game environment:

```text
Initialize() -> OnReady() -> Setup() -> OnResolved()
```

Note that this lifecycle is preserved regardless of how the node is added to the scene tree.

And this is the lifecycle of a dependent node in a test environment:

```text
OnReady() -> OnResolved()
```

## 🔋 IAutoOn

The `IAutoOn` mixin allows node scripts to implement .NET-style handler methods for Godot notifications, prefixed with `On`.

```csharp
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;

[Meta(typeof(IAutoOn))]
public partial class MyNode : Node2D {
  public override void _Notification(int what) => this.Notify(what);

  public void OnReady() {
    // Called when the node enters the scene tree.
  }

  public void OnProcess(double delta) {
    // Called every frame.
  }
}
```

## 🦾 IAutoNode

The `IAutoNode` mixin simply applies all of the mixins provided by AutoInject to a node script at once.

```csharp
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;

[Meta(typeof(IAutoNode))]
public partial class MyNode : Node { }
```

---

🐣 Package generated from a 🐤 Chickensoft Template — <https://chickensoft.games>

[chickensoft-badge]: https://chickensoft.games/img/badges/chickensoft_badge.svg
[chickensoft-website]: https://chickensoft.games
[discord]: https://discord.gg/gSjaPgMmYW
[discord-badge]: https://chickensoft.games/img/badges/discord_badge.svg
[line-coverage]: Chickensoft.AutoInject.Tests/badges/line_coverage.svg
[branch-coverage]: Chickensoft.AutoInject.Tests/badges/branch_coverage.svg

[provider]: https://github.com/rrousselGit/provider
[tree-order]: https://kidscancode.org/godot_recipes/4.x/basics/tree_ready_order/
[Introspection]: https://github.com/chickensoft-games/Introspection
[mixins]: https://github.com/chickensoft-games/Introspection?tab=readme-ov-file#%EF%B8%8F-mixins
[GodotNodeInterfaces]: https://github.com/chickensoft-games/GodotNodeInterfaces
[Game Demo]: https://github.com/chickensoft-games/GameDemo
[unique-nodes]: https://docs.godotengine.org/en/stable/tutorials/scripting/scene_unique_nodes.html
