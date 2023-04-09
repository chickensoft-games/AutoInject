namespace Chickensoft.AutoInject;

using System;
using Godot;
using SuperNodes.Types;
#pragma warning disable CS8019
using Chickensoft.AutoInject;
#pragma warning restore CS8019

/// <summary>
/// Represents a node that provides a value to its descendant nodes.
/// </summary>
public interface IProvider {
  /// <summary>
  /// Information about the provider — used internally to manage dependencies.
  /// </summary>
  ProviderState ProviderState { get; }

  /// <summary>
  /// When a provider has initialized all of the values it provides, this method
  /// is invoked on the provider itself (immediately after _Ready). When this
  /// method is called, the provider is guaranteed that all of its descendant
  /// nodes that depend this provider have resolved their dependencies.
  /// </summary>
  void OnProvided() { }
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

/// <summary>
/// Turns an ordinary node into a provider node.
/// </summary>
[PowerUp]
public abstract partial class Provider : Node, IProvider {
  /// <summary>
  /// Internal provider state used to manage dependencies.
  /// </summary>
  public ProviderState ProviderState { get; } = new();

  /// <summary>
  /// <para>
  /// Call this method once all your dependencies have been initialized. This
  /// will inform any dependent nodes that are waiting on this provider that
  /// the provider has finished initializing.
  /// </para>
  /// <para>
  /// Forgetting to call this method can prevent dependencies from resolving
  /// correctly throughout the scene tree.
  /// </para>
  /// </summary>
  public void Provide() => ProviderState.Provide(this);

  /// <summary>
  /// Provider lifecycle method automatically invoked by SuperNodes.
  /// </summary>
  /// <param name="what">Godot node notification.</param>
  public void OnProvider(int what) => ProviderState.OnProvider(what, this);
}

/// <summary>
/// Provider state used internally when resolving dependencies.
/// </summary>
public class ProviderState {
  /// <summary>Whether the provider has initialized all of its values.</summary>
  public bool IsInitialized { get; set; }

  /// <summary>
  /// Underlying event delegate used to inform dependent nodes that the
  /// provider has initialized all of the values it provides.
  /// </summary>
  public event Action<IProvider>? OnInitialized;

  /// <summary>
  /// Announces to descendent nodes that the values provided by this provider
  /// are initialized.
  /// </summary>
  /// <param name="provider">Provider node which has finished initializing
  /// the values it provides.</param>
  public void Announce(IProvider provider)
    => OnInitialized?.Invoke(provider);

  /// <summary>
  /// Internal implementation for the OnProvider lifecycle method. Resets the
  /// provider's initialized status when the provider leaves the scene tree.
  /// </summary>
  /// <param name="what">Godot node notification.</param>
  /// <param name="provider">Provider node.</param>
  public static void OnProvider(int what, IProvider provider) {
    if (what == Node.NotificationExitTree) {
      provider.ProviderState.IsInitialized = false;
    }
  }

  /// <summary>
  /// Internal implementation for the Provide method. This marks the Provider
  /// as having provided all of its values and then announces to dependent
  /// nodes that the provider has finished initializing.
  /// </summary>
  /// <param name="provider"></param>
  public static void Provide(IProvider provider) {
    provider.ProviderState.IsInitialized = true;
    provider.ProviderState.Announce(provider);
    provider.OnProvided();
  }
}
