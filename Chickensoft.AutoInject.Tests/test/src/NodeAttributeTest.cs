namespace Chickensoft.AutoInject.Tests;
using Chickensoft.GoDotTest;
using Godot;
using Shouldly;

public class NodeAttributeTest(Node testScene) : TestClass(testScene) {
  [Test]
  public void Initializes() {
    var attr = new NodeAttribute("path");
    attr.Path.ShouldBe("path");
  }
}
