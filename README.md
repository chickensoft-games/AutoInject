# 💉 AutoInject

[![Chickensoft Badge][chickensoft-badge]][chickensoft-website] [![Discord][discord-badge]][discord] [![Read the docs][read-the-docs-badge]][docs] ![line coverage][line-coverage] ![branch coverage][branch-coverage]

Node-based dependency injection for C# Godot scripts at build-time.

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

## 📦 Installation

AutoInject is a source-only package that uses the [SuperNodes] source generator to generate the necessary dependency injection code at build-time. You'll need to include SuperNodes, the SuperNodes runtime types, and AutoInject in your project. All of the packages are extremely lightweight.

Simply add the following to your project's `.csproj` file. Be sure to check the latest versions for each package on [Nuget](https://www.nuget.org/packages?q=Chickensoft).

```xml
<ItemGroup>
    <PackageReference Include="Chickensoft.SuperNodes" Version="1.8.0" PrivateAssets="all" OutputItemType="analyzer" />
    <PackageReference Include="Chickensoft.SuperNodes.Types" Version="1.8.0" />
    <PackageReference Include="Chickensoft.AutoInject" Version="1.6.0" PrivateAssets="all" />
</ItemGroup>
```

## 🐔 Providers

To provide values to descendant nodes, add the `Provider` [PowerUp] to your node script and implement `IProvide<T>` for each value you'd like to make available.

Once providers have initialized the values they provide, they must call the `Provide` method to inform AutoInject that their provided values are now available.

The example below shows a node script that provides a `string` value to its descendants.

```csharp
namespace MyGameProject;

using Chickensoft.AutoInject;
using Godot;
using SuperNodes.Types;

[SuperNode(typeof(Provider))]
public partial class MyProvider : Node, IProvide<string> {
  public override partial void _Notification(int what);

  string IProvide<string>.Value() => "Value"

  // Call the Provide() method once your dependencies have been initialized.
  public void OnReady() => Provide();

  public void OnProvided() {
    // You can optionally implement this method. It gets called once you call
    // Provide() to inform AutoInject that the provided values are now 
    // available.
  }
}
```

## 🐣 Dependents

To use a provided value in a descendant node somewhere, add the `Dependent` PowerUp to your descendent node script and mark each dependency with the `[Dependency]` attribute. SuperNodes will automatically tell AutoInject when your node is ready and begin the dependency resolution process.

Once all of the dependencies in your dependent node are resolved, the `OnResolved` method of your dependent node will be called (if overridden).

```csharp
namespace MyGameProject;

using Godot;
using SuperNodes.Types;

[SuperNode(typeof(Dependent))]
public partial class StringDependent : Node {
  public override partial void _Notification(int what);

  [Dependency]
  public string MyDependency => DependOn<string>();

  public void OnResolved() {
    // All of my dependencies are now available! Do whatever you want with 
    // them here.
  }
}
```

The `OnResolved` method will be called after `_Ready/OnReady`, but before the first frame if (and only if) all the providers it depends on call `Provide()` before the first frame.

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
[SuperNode(typeof(Dependent))]
public partial class MyDependent : Node {
  public override partial void _Notification(int what);

  [Dependency]
  public MyValue Value => DependOn<MyValue>();

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
public string MyDependency => DependOn<string>(() => "fallback_value");
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

## How AutoInject Works

AutoInject uses a simple, specific algorithm to resolve dependencies.

- When the Dependent PowerUp is added to a SuperNode, the SuperNodes generator will copy the code from the Dependent PowerUp into the node it was applied to.
- A node script with the Dependent PowerUp observes its lifecycle. When it notices the `Node.NotificationReady` signal, it will begin the dependency resolution process without you having to write any code in your node script.
- The dependency process works as follows:
  - All properties of the node script are inspected using SuperNode's static reflection table generation. This allows the script to introspect itself without having to resort to C#'s runtime reflection calls. Properties with the `[Dependency]` attribute are collected into the set of required dependencies.
  - All required dependencies are added to the remaining dependencies set.
  - The dependent node begins searching its ancestors, beginning with itself, then its parent, and so on up the tree.
    - If the current search node implements `IProvide` for any of the remaining dependencies, the individual resolution process begins.
      - The dependency stores the provider in a dictionary property on your node script which was copied over from the Dependent PowerUp.
      - The dependency is added to the set of found dependencies.
      - If the provider search node has not already provided its dependencies, the dependent subscribes to the `OnInitialized` event of the provider.
      - Pending dependency provider callbacks track a counter for the dependent node that also remove that provider's dependency from the remaining dependencies set and initiate the OnResolved process if nothing is left.
      - Subscribing to an event on the provider node and tracking whether or not the provider is initialized is made possible by SuperNodes, which copies the code from the Provider PowerUp into the provider's node script.
    - After checking all the remaining dependencies, the set of found dependencies are removed from the remaining dependencies set and the found dependencies set is cleared for the next search node.
    - If all the dependencies are found, the dependent initiates the OnResolved process and finishes the search.
    - Otherwise, the search node's parent becomes the next parent to search.
  - Search concludes when providers for each dependency are found, or the top of the scene tree is reached.

There are some natural consequences to this algorithm, such as `OnResolved` not being invoked on a dependent until all providers have provided a value. This is intentional — providers are expected to synchronously initialize their provided values after `_Ready` has been invoked on them.

AutoInject primarily exists to to locate providers from dependents and subscribe to the providers just long enough for their own `_Ready` method to be invoked — waiting longer than that to call `Provide` from a provider can introduce dependency resolution deadlock or other undesirable circumstances that are indicative of anti-patterns.

By calling `Provide()` from `_Ready` in provider nodes, you ensure that the order of execution unfolds as follows, synchronously:

  1. Dependent node `_Ready` (descendant of the provider, deepest nodes ready-up first).
  2. Provider node `_Ready` (which calls `Provide`).
  3. Dependent `OnResolved`
  4. Frame 1 `_Process`
  5. Frame 2 `_Process`
  6. Etc.

By following the `Provide()` on `_Ready` convention, you guarantee all dependent nodes receive an `OnResolved` callback before the first process invocation occurs, guaranteeing that nodes are setup before frame processing begins ✨.

> If your provider is also a dependent, you can call `Provide` from `OnResolved` to allow it to provide dependencies to its subtree, which still guarantees that dependency resolution happens before frame processing begins. Just don't wait until processing has started to call `Provide` from your providers!
>
> In general, dependents should have access to their dependencies **before** frame processing callbacks are invoked on them.

---

🐣 Package generated from a 🐤 Chickensoft Template — <https://chickensoft.games>

[chickensoft-badge]: https://raw.githubusercontent.com/chickensoft-games/chickensoft_site/main/static/img/badges/chickensoft_badge.svg
[chickensoft-website]: https://chickensoft.games
[discord-badge]: https://raw.githubusercontent.com/chickensoft-games/chickensoft_site/main/static/img/badges/discord_badge.svg
[discord]: https://discord.gg/gSjaPgMmYW
[read-the-docs-badge]: https://raw.githubusercontent.com/chickensoft-games/chickensoft_site/main/static/img/badges/read_the_docs_badge.svg
[docs]: https://chickensoft.games/docsickensoft%20Discord-%237289DA.svg?style=flat&logo=discord&logoColor=white
[line-coverage]: Chickensoft.AutoInject.Tests/badges/line_coverage.svg
[branch-coverage]: Chickensoft.AutoInject.Tests/badges/branch_coverage.svg

[provider]: https://github.com/rrousselGit/provider
[tree-order]: https://kidscancode.org/godot_recipes/4.x/basics/tree_ready_order/
[SuperNodes]: https://github.com/chickensoft-games/SuperNodes
[PowerUp]: https://chickensoft.games/docs/super_nodes/#-powerups
