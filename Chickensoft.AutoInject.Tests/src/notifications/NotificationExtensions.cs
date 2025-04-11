namespace Chickensoft.AutoInject;

using System.Runtime.CompilerServices;
using Chickensoft.Introspection;
using Godot;

public static class NotificationExtensions {
  /// <summary>
  /// Notify mixins applied to a Godot object that a notification has been
  /// received.
  /// </summary>
  /// <param name="obj">Godot object.</param>
  /// <param name="what">Godot object notification.</param>
  public static void Notify(this GodotObject obj, int what) {
    obj.__SetupNotificationStateIfNeeded();

    if (obj is not IIntrospectiveRef introspective) {
      return;
    }

    // Share the notification that just occurred with the mixins we're
    // about to invoke.
    introspective.MixinState.Get<NotificationState>().Notification = what;

    // Invoke each mixin's handler method.
    introspective.InvokeMixins();

    // If we're an IAutoOn, invoke the notification methods like OnReady,
    // OnProcess, etc. We specifically do this last.
    if (obj is IAutoOn autoOn) {
      IAutoOn.InvokeNotificationMethods(introspective, what);
    }
  }

#pragma warning disable IDE1006
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void __SetupNotificationStateIfNeeded(this GodotObject obj) {
    if (obj is not IIntrospectiveRef introspective) {
      return;
    }

    if (!introspective.MixinState.Has<NotificationState>()) {
      introspective.MixinState.Overwrite(new NotificationState());
    }
  }
}
