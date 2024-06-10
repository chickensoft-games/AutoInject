#pragma warning disable
namespace Chickensoft.AutoInject;

using System;
using Godot;
using Chickensoft.Introspection;
using Chickensoft.AutoInject;

/// <summary>Base provider interface used internally by AutoInject.</summary>
public interface IBaseProvider {
  /// <summary>Provider state.</summary>
  ProviderState ProviderState { get; }
}

/// <summary>
/// A provider of a value of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of value provided. To prevent pain, providers
/// should not provide a value that could ever be null.</typeparam>
public interface IProvide<T> : IProvider where T : notnull {
  /// <summary>Value that is provided by the provider.</summary>
  T Value();
}
