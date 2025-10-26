namespace Chickensoft.AutoInject.Tests;

using Chickensoft.GoDotTest;
using Chickensoft.Introspection;
using Godot;
using Shouldly;

public partial class AutoOnTest(Node testScene) : TestClass(testScene)
{
  [Meta(typeof(IAutoOn))]
  public partial class AutoOnTestNode : Node { }

  [Meta(typeof(IAutoOn))]
  public partial class NotAGodotNode { }

  public class NotAutoOn { }

  [Test]
  public void DoesNothingIfNotAGodotNode()
  {
    var node = new NotAGodotNode();

    Should.NotThrow(() => IAutoOn.InvokeNotificationMethods(node, 1));
  }

  [Test]
  public void DOesNothingIfNotAutoOn()
  {
    var node = new NotAutoOn();

    Should.NotThrow(() => IAutoOn.InvokeNotificationMethods(node, 1));
  }

  [Test]
  public void InvokesHandlerForNotification()
  {
    var node = new AutoOnTestNode();
    IAutoOn autoNode = node;

    Should.NotThrow(() =>
    {
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)GodotObject.NotificationPostinitialize
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)GodotObject.NotificationPredelete
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationEnterTree
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationWMWindowFocusIn
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationWMWindowFocusOut
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationWMCloseRequest
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationWMSizeChanged
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationWMDpiChange
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationVpMouseEnter
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationVpMouseExit
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationOsMemoryWarning
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationTranslationChanged
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationWMAbout
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationCrash
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationOsImeUpdate
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationApplicationResumed
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationApplicationPaused
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationApplicationFocusIn
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationApplicationFocusOut
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationTextServerChanged
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationWMMouseExit
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationWMMouseEnter
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationWMGoBackRequest
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationEditorPreSave
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationExitTree
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationChildOrderChanged
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationReady
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationEditorPostSave
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationUnpaused
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationPhysicsProcess
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationProcess
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationParented
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationUnparented
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationPaused
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationDragBegin
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationDragEnd
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationPathRenamed
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationInternalProcess
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationInternalPhysicsProcess
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationPostEnterTree
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationDisabled
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationEnabled
      );
      IAutoOn.InvokeNotificationMethods(
        autoNode, (int)Node.NotificationSceneInstantiated
      );
    });
  }
}
