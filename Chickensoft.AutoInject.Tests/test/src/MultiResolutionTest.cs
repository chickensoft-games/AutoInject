namespace Chickensoft.GodotGame;

using System.Threading.Tasks;
using Chickensoft.AutoInject.Tests.Fixtures;
using Chickensoft.GoDotTest;
using Godot;
using GodotTestDriver;
using GodotTestDriver.Util;
using Shouldly;

#pragma warning disable CA1001
public class MultiResolutionTest : TestClass {
  private Fixture _fixture = default!;
  private MultiProvider _provider = default!;

  public MultiResolutionTest(Node testScene) : base(testScene) { }

  [Setup]
  public async Task Setup() {
    _fixture = new Fixture(TestScene.GetTree());
    _provider = await _fixture.LoadAndAddScene<MultiProvider>(
      "res://test/fixtures/MultiProvider.tscn"
    );
  }

  [Cleanup]
  public void Cleanup() => _fixture.Cleanup();

  [Test]
  public async Task MultiDependentSubscribesToMultiProviderCorrectly() {
    _provider.Child.ReadyCalled.ShouldBeTrue();
    await TestScene.ProcessFrame(2);
    _provider.Child.OnResolvedCalled.ShouldBeTrue();
  }
}
#pragma warning restore CA1001
