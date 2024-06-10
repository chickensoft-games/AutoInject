namespace Chickensoft.AutoInject.Tests;

using System;
using System.Collections.Generic;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.GoDotTest;
using Godot;
using Moq;
using Shouldly;

public class FakeNodeTreeTest : TestClass {
  public FakeNodeTreeTest(Node testScene) : base(testScene) {
    var a = new Mock<INode>();
    var b = new Mock<INode>();
    var c = new Mock<INode>();

    a.Setup(n => n.Name).Returns("A");
    c.Setup(n => n.Name).Returns("C");

    A = a.Object;
    B = b.Object;
    C = c.Object;
  }

  public INode A { get; }
  public INode B { get; }
  public INode C { get; }

  [Test]
  public void InitializesAndGetsChildrenAndShowsHasChildren() {
    var children = new Dictionary<string, INode>() { ["A"] = A, ["B"] = B };
    var tree = new FakeNodeTree(TestScene, children);

    tree.GetChildren().ShouldBe([A, B]);
    tree.HasNode("A").ShouldBeTrue();
    tree.HasNode("B").ShouldBeTrue();

    tree.GetChildCount().ShouldBe(2);

    tree.GetAllNodes().ShouldBe(new Dictionary<string, INode>() {
      ["A"] = A,
      ["B"] = B
    });
  }

  [Test]
  public void InitializesWithNothing() {
    var tree = new FakeNodeTree(TestScene);

    tree.GetChildren().ShouldBeEmpty();
  }

  [Test]
  public void AddChildWorks() {
    var children = new Dictionary<string, INode>() { ["A"] = A, ["B"] = B };
    var tree = new FakeNodeTree(TestScene, children);

    tree.AddChild(C);
    tree.GetChildren().ShouldBe([A, B, C]);
    tree.HasNode("A").ShouldBeTrue();
    tree.HasNode("B").ShouldBeTrue();
    tree.HasNode("C").ShouldBeTrue();

    tree.GetChildCount().ShouldBe(3);
  }

  [Test]
  public void AddChildGeneratesNameForNodeIfNeeded() {
    var tree = new FakeNodeTree(TestScene);
    tree.AddChild(B);
    tree.GetNode(B.GetType().Name + "@0").ShouldBe(B);
  }

  [Test]
  public void GetNodeReturnsNode() {
    var children = new Dictionary<string, INode>() { ["A"] = A, ["B"] = B };
    var tree = new FakeNodeTree(TestScene, children);

    tree.GetNode("A").ShouldBe(A);
    tree.GetNode<INode>("A").ShouldBe(A);
    tree.GetNode("B").ShouldBe(B);
    tree.GetNode("nonexistent").ShouldBeNull();
    tree.GetNode<INode2D>("nonexistent").ShouldBeNull();
  }

  [Test]
  public void FindChildReturnsMatchingNode() {
    var children = new Dictionary<string, INode>() { ["A"] = A, ["B"] = B, ["C"] = C };
    var tree = new FakeNodeTree(TestScene, children);

    var result = tree.FindChild("A");
    result.ShouldBe(A);
  }

  [Test]
  public void FindChildReturnsNullOnNoMatch() {
    var children = new Dictionary<string, INode>() { ["A"] = A, ["B"] = B, ["C"] = C };
    var tree = new FakeNodeTree(TestScene, children);

    var result = tree.FindChild("D");
    result.ShouldBeNull();
  }

  [Test]
  public void FindChildrenReturnsMatchingNodes() {
    var children = new Dictionary<string, INode>() { ["Apple"] = A, ["Banana"] = B, ["Cherry"] = C };
    var tree = new FakeNodeTree(TestScene, children);

    var results = tree.FindChildren("C*");
    results.ShouldBe([C]);
  }

  [Test]
  public void GetChildReturnsNodeByIndex() {
    var children = new Dictionary<string, INode>() { ["A"] = A, ["B"] = B, ["C"] = C };
    var tree = new FakeNodeTree(TestScene, children);

    var result = tree.GetChild<INode>(1); // Get the second child (B).
    var result2 = tree.GetChild(1);
    result.ShouldBe(B);
    result.ShouldBeSameAs(result2);
  }

  [Test]
  public void GetChildThrowsOnInvalidIndex() {
    var tree = new FakeNodeTree(TestScene);

    Should.Throw<ArgumentOutOfRangeException>(() => tree.GetChild<INode>(0));
  }

  [Test]
  public void GetChildUsesNegativeIndexToGetFromEnd() {
    var children = new Dictionary<string, INode>() { ["A"] = A, ["B"] = B, ["C"] = C };
    var tree = new FakeNodeTree(TestScene, children);

    var result = tree.GetChild<INode>(-1);
    result.ShouldBe(C);
  }

  [Test]
  public void RemoveChildRemovesNode() {
    var children = new Dictionary<string, INode>() { ["A"] = A, ["B"] = B, ["C"] = C };
    var tree = new FakeNodeTree(TestScene, children);

    tree.GetChildCount().ShouldBe(3);

    tree.RemoveChild(B); // Remove the "B" node.
    tree.HasNode("B").ShouldBeFalse();
    tree.GetChildren().ShouldBe([A, C]);

    tree.GetChildCount().ShouldBe(2);
  }
}
