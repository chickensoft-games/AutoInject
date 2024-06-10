#pragma warning disable
namespace Chickensoft.AutoInject;

using System;
using Godot;
using Chickensoft.Introspection;
using Chickensoft.AutoInject;

/// <summary>
/// Turns an ordinary node into a provider node.
/// </summary>
[Mixin]
public interface IProvider : IMixin<IProvider>, IBaseProvider {
  /// <inheritdoc />
  ProviderState IBaseProvider.ProviderState {
    get {
      AddStateIfNeeded();
      return MixinState.Get<ProviderState>();
    }
  }

  /// <summary>
  /// When a provider has initialized all of the values it provides, this method
  /// is invoked on the provider itself (immediately after _Ready). When this
  /// method is called, the provider is guaranteed that all of its descendant
  /// nodes that depend this provider have resolved their dependencies.
  /// </summary>
  void OnProvided() { }

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

  void IMixin<IProvider>.Handler() {
    ProviderState.OnProvider(
      MixinState.Get<NotificationState>().Notification, this
    );
  }

  private void AddStateIfNeeded() {
    if (MixinState.Has<ProviderState>()) { return; }
    MixinState.Set(new ProviderState());
  }
}
