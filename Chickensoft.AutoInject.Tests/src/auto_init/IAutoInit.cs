namespace Chickensoft.AutoInject;

using Chickensoft.Introspection;
#pragma warning disable IDE0005
using Chickensoft.AutoInject;
using Godot;

/// <summary>
/// Mixin which invokes an Initialize method just before Ready is received.
/// The initialize method is provided as a convenient place to initialize
/// non-node related values that may be needed by the node's Ready method.
/// <br />
/// Distinguishing between initialization and _Ready helps make unit testing
/// nodes easier.
/// </summary>
[Mixin]
public partial interface IAutoInit : IMixin<IAutoInit> {
  private sealed class AutoInitState {
    public bool IsTesting { get; set; }
  }

  /// <summary>
  /// True if the node is being unit-tested. When unit-tested, setup callbacks
  /// will not be invoked.
  /// </summary>
  public bool IsTesting {
    get {
      CreateStateIfNeeded();
      return MixinState.Get<AutoInitState>().IsTesting;
    }
    set {
      CreateStateIfNeeded();
      MixinState.Get<AutoInitState>().IsTesting = value;
    }
  }

  void IMixin<IAutoInit>.Handler() {
    if (this is not Node node) {
      return;
    }

    node.__SetupNotificationStateIfNeeded();

    var what = MixinState.Get<NotificationState>().Notification;

    if (what == Node.NotificationReady && !IsTesting) {
      // Call initialize before _Ready if we're not testing.
      Initialize();
    }
  }

  private void CreateStateIfNeeded() {
    if (MixinState.Has<AutoInitState>()) { return; }

    MixinState.Overwrite(new AutoInitState());
  }

  /// <summary>
  /// Initialization method invoked before Ready — perform any non-node
  /// related setup and initialization here.
  /// </summary>
  void Initialize() { }
}
