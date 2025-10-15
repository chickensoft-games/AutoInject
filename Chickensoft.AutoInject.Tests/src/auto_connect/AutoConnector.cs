namespace Chickensoft.AutoInject;

using System;
using System.Runtime.CompilerServices;
using Chickensoft.GodotNodeInterfaces;
#pragma warning disable CS8019, IDE0005
using Chickensoft.AutoInject;
using Godot;
using Chickensoft.Introspection;
using System.Collections.Generic;

public static class AutoConnector {
  public class TypeChecker : ITypeReceiver {
    public object Value { get; set; } = default!;

    public bool Result { get; private set; }

    public void Receive<T>() => Result = Value is T;
  }

  private static readonly TypeChecker _checker = new();

  public static void ConnectNodes(
      IEnumerable<PropertyMetadata> properties,
      IAutoConnect autoConnect
    ) {
    var node = (Node)autoConnect;
    foreach (var property in properties) {
      if (
        !property.Attributes.TryGetValue(
          typeof(NodeAttribute), out var nodeAttributes
        ) ||
        property.Getter is not { } getter ||
        getter.Invoke(node) is not null
      ) {
        continue;
      }
      var nodeAttribute = (NodeAttribute)nodeAttributes[0];

      var path = nodeAttribute.Path ?? AsciiToPascalCase(property.Name);

      Exception? e;

      // First, check to see if the node has been faked for testing.
      // Faked nodes take precedence over real nodes.
      //
      // FakeNodes will never be null on an AutoConnect node, actually.
      if (autoConnect.FakeNodes!.GetNode(path) is { } fakeNode) {
        // We found a faked node for this path. Make sure it's the expected
        // type.
        _checker.Value = fakeNode;

        property.TypeNode.GenericTypeGetter(_checker);

        var satisfiesFakeType = _checker.Result;

        if (!satisfiesFakeType) {
          e = new InvalidOperationException(
            $"Found a faked node at '{path}' of type " +
            $"'{fakeNode.GetType().Name}' that is not the expected type " +
            $"'{property.TypeNode.ClosedType}' for member " +
            $"'{property.Name}' on '{node.Name}'."
          );
          GD.PushError(e.Message);
          throw e;
        }
        // Faked node satisfies the expected type :)
        if (property.Setter is { } setter) {
          setter(node, fakeNode);
        }

        continue;
      }

      // We're dealing with what should be an actual node in the tree.
      var potentialChild = node.GetNodeOrNull(path);

      if (potentialChild is not Node child) {
        e = new InvalidOperationException(
          $"AutoConnect: Node at '{path}' does not exist in either the real " +
          $"or fake subtree for '{node.Name}' member '{property.Name}' of " +
          $"type '{property.TypeNode.ClosedType}'."
        );
        GD.PushError(e.Message);
        throw e;
      }

      // see if the unchecked node satisfies the expected type of node from the
      // property type
      _checker.Value = child;
      property.TypeNode.GenericTypeGetter(_checker);
      var originalNodeSatisfiesType = _checker.Result;

      if (originalNodeSatisfiesType) {
        // Property expected a vanilla Godot node type and it matched, so we
        // set it and leave.
        if (property.Setter is { } setter) {
          setter(node, child);
        }
        continue;
      }

      // Plain Godot node type wasn't expected, so we need to check if the
      // property was expecting a Godot node interface type.
      //
      // Check to see if the node needs to be adapted to satisfy an
      // expected interface type.
      var adaptedChild = GodotInterfaces.AdaptNode(child);
      _checker.Value = adaptedChild;

      property.TypeNode.GenericTypeGetter(_checker);
      var adaptedChildSatisfiesType = _checker.Result;

      if (adaptedChildSatisfiesType) {
        if (property.Setter is { } setter) {
          setter(node, adaptedChild);
        }
        continue;
      }

      // Tell user we can't connect the node to the property.
      e = new InvalidOperationException(
        $"Node at '{path}' of type '{child.GetType().Name}' does not " +
        $"satisfy the expected type '{property.TypeNode.ClosedType}' for " +
        $"member '{property.Name}' on '{node.Name}'."
      );
      GD.PushError(e.Message);
      throw e;
    }
  }

  /// <summary>
  /// <para>
  /// Converts an ASCII string to PascalCase. This looks insane, but it is the
  /// fastest out of all the benchmarks I did.
  /// </para>
  /// <para>
  /// Since messing with strings can be slow and looking up nodes is a common
  /// operation, this is a good place to optimize. No heap allocations!
  /// </para>
  /// <para>
  /// Removes underscores, always capitalizes the first letter, and capitalizes
  /// the first letter after an underscore.
  /// </para>
  /// </summary>
  /// <param name="input">Input string.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string AsciiToPascalCase(string input) {
    var span = input.AsSpan();
    Span<char> output = stackalloc char[span.Length + 1];
    var outputIndex = 1;

    output[0] = '%';

    for (var i = 1; i < span.Length + 1; i++) {
      var c = span[i - 1];

      if (c == '_') { continue; }

      output[outputIndex++] = i == 1 || span[i - 2] == '_'
        ? (char)(c & 0xDF)
        : c;
    }

    return new string(output[..outputIndex]);
  }
}
