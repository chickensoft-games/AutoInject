namespace Chickensoft.GodotGame;

using System.Threading.Tasks;
using Chickensoft.AutoInject.Tests.Fixtures;
using Chickensoft.GoDotTest;
using Godot;
using GodotTestDriver;
using GodotTestDriver.Util;
using Shouldly;

public class MultiResolutionTest(Node testScene) : TestClass(testScene) {
  private Fixture _fixture = default!;
  private MultiProvider _provider = default!;

  [Setup]
  public void Setup() {
    _fixture = new Fixture(TestScene.GetTree());
    _provider = _fixture.LoadScene<MultiProvider>(
      "res://test/fixtures/MultiProvider.tscn"
    );
  }

  [Cleanup]
  public void Cleanup() => _fixture.Cleanup();

  [Test]
  public async Task MultiDependentSubscribesToMultiProviderCorrectly() {
    await _fixture.AddToRoot(_provider);
    await _provider.WaitForEvents();
    _provider.Child.ReadyCalled.ShouldBeTrue();
    _provider.Child.OnResolvedCalled.ShouldBeTrue();
  }
}
