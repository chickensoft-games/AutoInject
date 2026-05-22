namespace Chickensoft.AutoInject.Tests;

using System.Threading.Tasks;
using Chickensoft.AutoInject.Tests.Subjects;
using Chickensoft.GoDotTest;
using Chickensoft.GodotTestDriver;
using Godot;
using Shouldly;

public class EnhancedLifecycleTest(Node testScene) : TestClass(testScene)
{
  [Test]
  public async Task HookOrderPreservedWhenSceneLoads()
  {
    // Children ready before ancestor provides dependencies.
    await TestHookOrder(isSceneLoad: true);
  }

  [Test]
  public async Task HookOrderPreservedWhenProvidersAlreadyInitialized()
  {
    // Ancestors provide dependencies before children ready.
    await TestHookOrder(isSceneLoad: false);
  }

  [Test]
  public async Task HookOrderPreservedWhenSceneLoadsInTesting()
  {
    // Children ready before ancestor provides dependencies in testing mode.
    await TestHookOrder(isSceneLoad: true, isTesting: true);
  }

  [Test]
  public async Task HookOrderPreservedInTestingWithProvidersAlreadyInitialized()
  {
    // Ancestors provide dependencies before children ready in testing mode.
    await TestHookOrder(isSceneLoad: false, isTesting: true);
  }

  private async Task TestHookOrder(bool isSceneLoad, bool isTesting = false)
  {
    var provider = new StringProvider() { Value = "Hi" };
    var fixture = new Fixture(TestScene.GetTree());
    var dependent = new OrderTrackingDependent();
    (dependent as IDependent)!.IsTesting = isTesting;

    if (isSceneLoad)
    {
      provider.AddChild(dependent);
      await fixture.AddToRoot(provider);
    }
    else
    {
      await fixture.AddToRoot(provider);
      provider.AddChild(dependent);
    }

    // Wait until Godot dispatches NotificationReady to the new child.
    await TestScene.GetTree().ToSignal(
      TestScene.GetTree(), SceneTree.SignalName.ProcessFrame
    );

    // Skip Initialize and Setup in testing mode
    string[] expectedCalls = isTesting
      ? ["OnReady", "OnResolved"]
      : ["Initialize", "OnReady", "Setup", "OnResolved"];
    dependent.Calls.ShouldBe(expectedCalls);

    await fixture.Cleanup();

    provider.RemoveChild(dependent);
    dependent.QueueFree();
    provider.QueueFree();
  }
}
