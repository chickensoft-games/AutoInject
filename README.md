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
- ✅ Resolution occurs in O(n), where `n` is the hight of the tree above the requesting dependent node (usually only a handful of nodes to search). For deep trees, "reflecting" dependencies by re-providing them further down the tree speeds things up further.
- ✅ Dependencies are resolved when the node enters the scene tree, allowing for O(1) access afterwards. Exiting and re-entering the scene tree triggers the dependency resolution process again.
- ✅ Scripts can be both dependents and providers.

## 📦 Installation

AutoInject is a source-only package that uses the [SuperNodes] source generator to generate the necessary dependency injection code at build-time. You'll need to include SuperNodes, the SuperNodes runtime types, and AutoInject in your project. All of the packages are extremely lightweight.

Simply add the following to your project's `.csproj` file. Be sure to check the latest versions for each package on [Nuget].

```xml
<ItemGroup>
    <PackageReference Include="Chickensoft.SuperNodes" Version="1.2.0" PrivateAssets="all" OutputItemType="analyzer" />
    <PackageReference Include="Chickensoft.SuperNodes.Types" Version="1.2.0" />
    <PackageReference Include="Chickensoft.AutoInject" Version="1.0.0" PrivateAssets="all" />
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

## ⚠️ Advice

### Simple Dependency Trees

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
