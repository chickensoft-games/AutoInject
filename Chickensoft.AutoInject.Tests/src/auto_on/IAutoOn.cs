namespace Chickensoft.AutoInject;

using Chickensoft.Introspection;
using Godot;

/// <summary>
/// Represents a node which automatically calls nicely named notification
/// methods, such as
/// <see cref="OnReady" />, <see cref="OnProcess(double)" />, etc.
/// </summary>
[Mixin]
public interface IAutoOn : IMixin<IAutoOn> {
  // Handler doesn't do anything, since
  // <see cref="NotificationExtensions.Notify(GodotObject, int)" />
  // automatically calls InvokeNotificationMethods after invoking mixins.
  // This ensures callbacks always run after mixins.
  void IMixin<IAutoOn>.Handler() { }

  public static void InvokeNotificationMethods(object? obj, int what) {
    if (obj is not IAutoOn autoNode || obj is not Node node) { return; }

    // Invoke Godot callbacks
    autoNode.OnNotification(what);

    switch (what) {
      case (int)GodotObject.NotificationPostinitialize:
        autoNode.OnPostinitialize();
        break;
      case (int)GodotObject.NotificationPredelete:
        autoNode.OnPredelete();
        break;
      case (int)Node.NotificationEnterTree:
        autoNode.OnEnterTree();
        break;
      case (int)Node.NotificationWMWindowFocusIn:
        autoNode.OnWMWindowFocusIn();
        break;
      case (int)Node.NotificationWMWindowFocusOut:
        autoNode.OnWMWindowFocusOut();
        break;
      case (int)Node.NotificationWMCloseRequest:
        autoNode.OnWMCloseRequest();
        break;
      case (int)Node.NotificationWMSizeChanged:
        autoNode.OnWMSizeChanged();
        break;
      case (int)Node.NotificationWMDpiChange:
        autoNode.OnWMDpiChange();
        break;
      case (int)Node.NotificationVpMouseEnter:
        autoNode.OnVpMouseEnter();
        break;
      case (int)Node.NotificationVpMouseExit:
        autoNode.OnVpMouseExit();
        break;
      case (int)Node.NotificationOsMemoryWarning:
        autoNode.OnOsMemoryWarning();
        break;
      case (int)Node.NotificationTranslationChanged:
        autoNode.OnTranslationChanged();
        break;
      case (int)Node.NotificationWMAbout:
        autoNode.OnWMAbout();
        break;
      case (int)Node.NotificationCrash:
        autoNode.OnCrash();
        break;
      case (int)Node.NotificationOsImeUpdate:
        autoNode.OnOsImeUpdate();
        break;
      case (int)Node.NotificationApplicationResumed:
        autoNode.OnApplicationResumed();
        break;
      case (int)Node.NotificationApplicationPaused:
        autoNode.OnApplicationPaused();
        break;
      case (int)Node.NotificationApplicationFocusIn:
        autoNode.OnApplicationFocusIn();
        break;
      case (int)Node.NotificationApplicationFocusOut:
        autoNode.OnApplicationFocusOut();
        break;
      case (int)Node.NotificationTextServerChanged:
        autoNode.OnTextServerChanged();
        break;
      case (int)Node.NotificationWMMouseExit:
        autoNode.OnWMMouseExit();
        break;
      case (int)Node.NotificationWMMouseEnter:
        autoNode.OnWMMouseEnter();
        break;
      case (int)Node.NotificationWMGoBackRequest:
        autoNode.OnWMGoBackRequest();
        break;
      case (int)Node.NotificationEditorPreSave:
        autoNode.OnEditorPreSave();
        break;
      case (int)Node.NotificationExitTree:
        autoNode.OnExitTree();
        break;
      case (int)Node.NotificationChildOrderChanged:
        autoNode.OnChildOrderChanged();
        break;
      case (int)Node.NotificationReady:
        if (node is IReadyAware readyAware) {
          readyAware.OnBeforeReady();
          autoNode.OnReady();
          readyAware.OnAfterReady();
          break;
        }
        autoNode.OnReady();
        break;
      case (int)Node.NotificationEditorPostSave:
        autoNode.OnEditorPostSave();
        break;
      case (int)Node.NotificationUnpaused:
        autoNode.OnUnpaused();
        break;
      case (int)Node.NotificationPhysicsProcess:
        autoNode.OnPhysicsProcess(node.GetPhysicsProcessDeltaTime());
        break;
      case (int)Node.NotificationProcess:
        autoNode.OnProcess(node.GetProcessDeltaTime());
        break;
      case (int)Node.NotificationParented:
        autoNode.OnParented();
        break;
      case (int)Node.NotificationUnparented:
        autoNode.OnUnparented();
        break;
      case (int)Node.NotificationPaused:
        autoNode.OnPaused();
        break;
      case (int)Node.NotificationDragBegin:
        autoNode.OnDragBegin();
        break;
      case (int)Node.NotificationDragEnd:
        autoNode.OnDragEnd();
        break;
      case (int)Node.NotificationPathRenamed:
        autoNode.OnPathRenamed();
        break;
      case (int)Node.NotificationInternalProcess:
        autoNode.OnInternalProcess();
        break;
      case (int)Node.NotificationInternalPhysicsProcess:
        autoNode.OnInternalPhysicsProcess();
        break;
      case (int)Node.NotificationPostEnterTree:
        autoNode.OnPostEnterTree();
        break;
      case (int)Node.NotificationDisabled:
        autoNode.OnDisabled();
        break;
      case (int)Node.NotificationEnabled:
        autoNode.OnEnabled();
        break;
      case (int)Node.NotificationSceneInstantiated:
        autoNode.OnSceneInstantiated();
        break;
      default:
        break;
    }
  }

  /// <summary>Notification received during object initialization.</summary>
  void OnPostinitialize() { }

  /// <summary>
  /// Notification received before an object is deleted by Godot (a destructor).
  /// </summary>
  void OnPredelete() { }

  /// <summary>
  /// Method invoked when a Godot notification is received.
  /// </summary>
  /// <param name="what">Notification.</param>
  void OnNotification(int what) { }

  /// <summary>
  /// Notification received when the node enters a SceneTree.
  /// </summary>
  void OnEnterTree() { }

  /// <summary>
  /// Notification received from the OS when the node's Window ancestor is
  /// focused. This may be a change of focus between two windows of the same
  /// engine instance, or from the OS desktop or a third-party application to
  /// a window of the game (in which case
  /// <see cref="Node.NotificationApplicationFocusIn" /> is
  /// also received).
  /// </summary>
  void OnWMWindowFocusIn() { }

  /// <summary>
  /// Notification received from the OS when the node's Window ancestor loses
  /// focus. This may be a change of focus between two windows of the same
  /// engine instance, or from a window of the game to the OS desktop or a
  /// third-party application (in which case
  /// <see cref="Node.NotificationApplicationFocusOut" /> is
  /// is also received).
  /// </summary>
  void OnWMWindowFocusOut() { }

  /// <summary>
  /// Notification received from the OS when a close request is sent (e.g.
  /// closing the window with a "Close" button or Alt + F4). Implemented on
  /// desktop platforms.
  /// </summary>
  void OnWMCloseRequest() { }

  /// <summary>
  /// Notification received when the window is resized. Note: Only the resized
  /// Window node receives this notification, and it's not propagated to the
  /// child nodes.
  /// </summary>
  void OnWMSizeChanged() { }

  /// <summary>
  /// Notification received from the OS when the screen's dots per inch (DPI)
  /// scale is changed. Only implemented on macOS.
  /// </summary>
  void OnWMDpiChange() { }

  /// <summary>
  /// Notification received when the mouse cursor enters the Viewport's
  /// visible area, that is not occluded behind other Controls or Windows,
  /// provided its <see cref="Viewport.GuiDisableInput" /> is false and
  /// regardless if it's currently focused or not.
  /// </summary>
  void OnVpMouseEnter() { }

  /// <summary>
  /// Notification received when the mouse cursor leaves the Viewport's visible
  /// area, that is not occluded behind other Controls or Windows, provided its
  /// <see cref="Viewport.GuiDisableInput" /> is false and regardless if
  /// it's currently focused or not.
  /// </summary>
  void OnVpMouseExit() { }

  /// <summary>
  /// Notification received from the OS when the application is exceeding its
  /// allocated memory. Implemented only on iOS.
  /// </summary>
  void OnOsMemoryWarning() { }

  /// <summary>
  /// Notification received when translations may have changed. Can be
  /// triggered by the user changing the locale, changing auto_translate_mode
  /// or when the node enters the scene tree. Can be used to respond to
  /// language changes, for example to change the UI strings on the fly. Useful
  /// when working with the built-in translation support, like Object.tr.
  /// <br />
  /// Note: This notification is received alongside
  /// <see cref="Node.NotificationReady" />,
  /// so if you are instantiating a scene, the child nodes will not be
  /// initialized yet. You can use it to setup translations for this node,
  /// child nodes created from script, or if you want to access child nodes
  /// added in the editor, make sure the node is ready using
  /// <see cref="Node.IsNodeReady" />.
  /// </summary>
  void OnTranslationChanged() { }

  /// <summary>
  /// Notification received from the OS when a request for "About" information
  /// is sent. Implemented only on macOS.
  /// </summary>
  void OnWMAbout() { }

  /// <summary>
  /// Notification received from Godot's crash handler when the engine is about
  /// to crash. Implemented on desktop platforms, if the crash handler is
  /// enabled.
  /// </summary>
  void OnCrash() { }

  /// <summary>
  /// Notification received from the OS when an update of the Input Method
  /// Engine occurs (e.g. change of IME cursor position or composition string).
  /// Implemented only on macOS.
  /// </summary>
  void OnOsImeUpdate() { }

  /// <summary>
  /// Notification received from the OS when the application is resumed.
  /// Specific to the Android and iOS platforms.
  /// </summary>
  void OnApplicationResumed() { }

  /// <summary>
  /// Notification received from the OS when the application is paused.
  /// Specific to the Android and iOS platforms.
  /// <br />
  ///  Note: On iOS, you only have approximately 5 seconds to finish a task
  /// started by this signal. If you go over this allotment, iOS will kill the
  /// app instead of pausing it.
  /// </summary>
  void OnApplicationPaused() { }

  /// <summary>
  /// Notification received from the OS when the application is focused, i.e.
  /// when changing the focus from the OS desktop or a third-party application
  /// to any open window of the Godot instance. Implemented on desktop and
  /// mobile platforms.
  /// </summary>
  void OnApplicationFocusIn() { }

  /// <summary>
  /// Notification received from the OS when the application has lost focus,
  /// i.e. when changing the focus from any open window of the Godot instance
  /// to the OS desktop or a third-party application. Implemented on desktop
  /// and mobile platforms.
  /// </summary>
  void OnApplicationFocusOut() { }

  /// <summary>
  /// Notification received when the TextServer is changed.
  /// </summary>
  void OnTextServerChanged() { }

  /// <summary>
  /// Notification received when the mouse leaves the window. Implemented for
  /// embedded windows and on desktop and web platforms.
  /// </summary>
  void OnWMMouseExit() { }

  /// <summary>
  /// Notification received when the mouse enters the window. Implemented for
  /// embedded windows and on desktop and web platforms.
  /// </summary>
  void OnWMMouseEnter() { }

  /// <summary>
  /// Notification received from the OS when a go back request is sent (e.g.
  /// pressing the "Back" button on Android). Implemented only on iOS.
  /// </summary>
  void OnWMGoBackRequest() { }

  /// <summary>
  /// Notification received right before the scene with the node is saved in
  /// the editor. This notification is only sent in the Godot editor and will
  /// not occur in exported projects.
  /// </summary>
  void OnEditorPreSave() { }

  /// <summary>
  /// Notification received when the node is about to exit a SceneTree.
  /// This notification is received after the related
  /// <see cref="Node.TreeExiting" /> signal.
  /// </summary>
  void OnExitTree() { }

  /// <summary>
  /// Notification received when the list of children is changed. This happens
  /// when child nodes are added, moved or removed.
  /// </summary>
  void OnChildOrderChanged() { }

  /// <summary>
  /// Notification received when the node is ready.
  /// </summary>
  void OnReady() { }

  /// <summary>
  /// Notification received right after the scene with the node is saved in
  /// the editor. This notification is only sent in the Godot editor and will
  /// not occur in exported projects.
  /// </summary>
  void OnEditorPostSave() { }

  /// <summary>
  /// Notification received when the node is unpaused. See
  /// <see cref="Node.ProcessMode" />
  /// </summary>
  void OnUnpaused() { }

  /// <summary>
  /// Notification received from the tree every physics frame when
  /// <see cref="Node.IsPhysicsProcessing" /> returns true.
  /// </summary>
  /// <param name="delta">Time since the last physics update, in seconds.
  /// </param>
  void OnPhysicsProcess(double delta) { }

  /// <summary>
  /// Notification received from the tree every rendered frame when
  /// <see cref="Node.IsPhysicsProcessing" /> returns true.
  /// </summary>
  /// <param name="delta">Time since the last process update, in seconds.
  /// </param>
  void OnProcess(double delta) { }

  /// <summary>
  /// Notification received when the node is set as a child of another node.
  /// <br />
  /// Note: This does not mean that the node entered the SceneTree.
  /// </summary>
  void OnParented() { }

  /// <summary>
  /// Notification received when the parent node calls
  /// <see cref="Node.RemoveChild(Node)" /> on this node.
  /// <br />
  ///  Note: This does not mean that the node exited the SceneTree.
  /// </summary>
  void OnUnparented() { }

  /// <summary>
  /// Notification received when the node is paused. See
  /// <see cref="Node.ProcessMode" />
  /// </summary>
  void OnPaused() { }

  /// <summary>
  /// Notification received when a drag operation begins. All nodes receive
  /// this notification, not only the dragged one.
  /// <br />
  /// Can be triggered either by dragging a Control that provides drag
  /// data (see <see cref="Control._GetDragData(Vector2))" />
  /// or by using
  /// <see cref="Control.ForceDrag(Variant, Control)" />.
  /// <br />
  /// Use see <see cref="Viewport.GuiGetDragData" /> to get the dragged
  /// data.
  /// </summary>
  void OnDragBegin() { }

  /// <summary>
  /// Notification received when a drag operation ends. Use
  /// <see cref="Viewport.GuiIsDragSuccessful" /> to check if the drag
  /// succeeded.
  /// </summary>
  void OnDragEnd() { }

  /// <summary>
  /// Notification received when the node's name or one of its ancestors'
  /// name is changed. This notification is not received when the node is
  /// removed from the SceneTree.
  /// </summary>
  void OnPathRenamed() { }

  /// <summary>
  /// Notification received from the tree every rendered frame when
  /// <see cref="Node.IsProcessingInternal" /> returns true.
  /// </summary>
  void OnInternalProcess() { }

  /// <summary>
  /// Notification received from the tree every physics frame when
  /// <see cref="Node.IsPhysicsProcessingInternal" /> returns true.
  /// </summary>
  void OnInternalPhysicsProcess() { }

  /// <summary>
  /// Notification received when the node enters the tree, just before
  /// <see cref="Node.NotificationReady" /> may be received. Unlike the
  /// latter, it is sent every time the node enters the tree, not just once.
  /// </summary>
  void OnPostEnterTree() { }

  /// <summary>
  /// Notification received when the node is disabled. See
  /// <see cref="Node.ProcessMode" />.
  /// </summary>
  void OnDisabled() { }

  /// <summary>
  /// Notification received when the node is enabled again after being disabled.
  /// See <see cref="Node.ProcessMode" />.
  /// </summary>
  void OnEnabled() { }

  /// <summary>
  /// Notification received only by the newly instantiated scene root node,
  /// when
  /// <see
  ///   cref="PackedScene.Instantiate(PackedScene.GenEditState)" />
  /// is completed.
  /// </summary>
  void OnSceneInstantiated() { }
}
