namespace Chickensoft.AutoInject;

using Godot;

#pragma warning disable IDE0005
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using System.Globalization;
using System;
using System.Runtime.CompilerServices;


/// <summary>
/// Dependent mixin. Apply this to an introspective node to automatically
/// resolve dependencies marked with the [Dependency] attribute.
/// </summary>
[Mixin]
public interface IDependent : IMixin<IDependent>, IAutoInit, IReadyAware {
  DependentState DependentState {
    get {
      AddStateIfNeeded();
      return MixinState.Get<DependentState>();
    }
  }

  /// <summary>
  /// Called after dependencies are resolved, but before
  /// <see cref="OnResolved" /> is called if (and only if)
  /// <see cref="IsTesting" /> is false. This allows you to initialize
  /// properties that depend on dependencies separate from using those
  /// properties to facilitate easier testing.
  /// </summary>
  void Setup() { }

  /// <summary>
  /// Method that is invoked when all of the dependent node's dependencies are
  /// resolved (after _Ready() but before _Process()).
  /// </summary>
  void OnResolved() { }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  void IReadyAware.OnBeforeReady() { }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  void IReadyAware.OnAfterReady() {
    if (DependentState.PleaseCallSetup) {
      Setup();
      DependentState.PleaseCallSetup = false;
    }
    if (DependentState.PleaseCallOnResolved) {
      OnResolved();
      DependentState.PleaseCallOnResolved = false;
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void AddStateIfNeeded() {
    if (MixinState.Has<DependentState>()) { return; }

    MixinState.Overwrite(new DependentState());
  }

  void IMixin<IDependent>.Handler() { }

  // Specifying "new void" makes this hide the existing handler, which works
  // since the introspection generator calls us as ((IDependent)obj).Handler()
  // rather than ((IMixin<IDependent>)obj).Handler().
  public new void Handler() {
    if (this is not Node node) {
      return;
    }

    node.__SetupNotificationStateIfNeeded();
    AddStateIfNeeded();

    if (
      this is IIntrospective introspective &&
      !introspective.HasMixin(typeof(IAutoInit))
    ) {
      // Developer didn't give us the IAutoInit mixin, but all dependents are
      // required to also be IAutoInit. So we'll invoke it for them manually.
      (this as IAutoInit).Handler();
    }

    DependencyResolver.OnDependent(
      MixinState.Get<NotificationState>().Notification,
      this,
      Types.Graph.GetProperties(GetType())
    );
  }

  /// <summary>
  /// Add a fake value to the dependency table. Adding a fake value allows a
  /// unit test to override a dependency lookup with a fake value.
  /// </summary>
  /// <param name="value">Dependency value (probably a mock or a fake).</param>
  /// <typeparam name="T">Dependency type.</typeparam>
  public void FakeDependency<T>(T value) where T : notnull {
    AddStateIfNeeded();
    MixinState.Get<DependentState>().ProviderFakes[typeof(T)] =
      new DependencyResolver.DefaultProvider<T>(value);
  }
}
