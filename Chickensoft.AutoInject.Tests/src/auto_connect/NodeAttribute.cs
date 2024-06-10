namespace Chickensoft.AutoInject;

using System;
#pragma warning disable CS8019, IDE0005
using Chickensoft.AutoInject;

/// <summary>
/// Node attribute. Apply this to properties or fields that need to be
/// automatically connected to a corresponding node instance in the scene tree.
/// </summary>
/// <param name="path">Godot node path. If not provided, the name of the
/// property will be converted to PascalCase (with any leading
/// underscores removed) and used as a unique node identifier</param>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class NodeAttribute(string? path = null) : Attribute {
  /// <summary>
  /// Explicit node path or unique identifier that the tagged property or field
  /// should reference. If not provided (or null), the name of the property or
  /// field itself will be converted to PascalCase (with any leading
  /// underscores removed) and used as a unique node identifier. For example,
  /// the reference `Node2D _myNode` would be connected to `%MyNode`.
  /// </summary>
  public string? Path { get; } = path;
}
