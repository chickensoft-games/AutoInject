namespace Chickensoft.AutoInject;

using System;

/// <summary>
/// Represents a dependency on a value provided by a provider node higher in
/// the current scene tree. This attribute should be applied to a property of
/// a dependent node.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class DependencyAttribute : Attribute { }
