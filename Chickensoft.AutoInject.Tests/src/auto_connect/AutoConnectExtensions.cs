namespace Chickensoft.AutoInject;

using System;
using Chickensoft.GodotNodeInterfaces;
#pragma warning disable CS8019, IDE0005
using Chickensoft.AutoInject;
using Godot;
using System.Collections.Generic;

public static class AutoConnectExtensions {
  /// <summary>
  /// Initialize the fake node tree for unit testing.
  /// </summary>
  /// <param name="node">Godot node.</param>
  /// <param name="nodes">Map of node paths to mock nodes.</param>
  /// <exception cref="InvalidOperationException" />
  public static void FakeNodeTree(
    this Node node, Dictionary<string, INode>? nodes
  ) {
    if (node is not IAutoConnect autoConnect) {
      throw new InvalidOperationException(
        "Cannot create a fake node tree on a node without the AutoConnect " +
        "mixin."
      );
    }

    autoConnect.FakeNodes = new(node, nodes);
  }
}
