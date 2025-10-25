namespace Chickensoft.AutoInject.Analyzers.Utils;

public static class Constants
{
  public const string META_ATTRIBUTE_NAME = "Meta";
  public const string PROVIDE_METHOD_NAME = "Provide";
  public const string NOTIFY_METHOD_NAME = "Notify";
  public const string NOTIFICATION_METHOD_NAME = "_Notification";
  public const string SETUP_METHOD_NAME = "Setup";
  public const string ONREADY_METHOD_NAME = "OnReady";
  public const string READY_METHOD_NAME = "_Ready";
  public const string WHAT_PARAMETER_NAME = "what";
  public const string PROVIDER_INTERFACE_NAME = "IProvide";
  public const string MESSAGE_PROVIDE_METHOD_NAME = "this.Provide()";
  public const string PROVIDE_NEW_METHOD_BODY = """
// Call the this.Provide() method once your dependencies have been initialized.
this.Provide();
""";
  public const string MESSAGE_NOTIFY_METHOD_NAME = "this.Notify(what)";

  /// <summary>
  /// Type names that we look for in Meta attributes to determine if a class needs a this.Notify(what) call.
  /// </summary>
  public static string[] AutoInjectMetaNames = [
    "IAutoNode",
    "IAutoOn",
    "IAutoConnect",
    "IAutoInit",
    "IProvider",
    "IDependent",
  ];

  /// <summary>
  /// Type names that we look for in Meta attributes to determine if a class needs a this.Provide() call.
  /// </summary>
  public static string[] ProviderMetaNames = [
    "IAutoNode",
    "IProvider",
  ];
}
