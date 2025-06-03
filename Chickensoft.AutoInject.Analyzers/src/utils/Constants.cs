namespace Chickensoft.AutoInject.Analyzers.Utils;

public static class Constants {
  public const string META_ATTRIBUTE_NAME = "Meta";

  /// <summary>
  /// Type names that we look for in Meta attributes to determine if a class needs a this.Provide() call.
  /// </summary>
  public static readonly string[] ProviderMetaNames = [
    "IAutoNode",
    "IProvider",
  ];

  public const string PROVIDER_INTERFACE_NAME = "IProvide";

  /// <summary>
  /// Type names that we look for in Meta attributes to determine if a class needs a this.Notify(what) call.
  /// </summary>
  public static readonly string[] AutoInjectTypeNames = [
    "IAutoNode",
    "IAutoOn",
    "IAutoConnect",
    "IAutoInit",
    "IProvider",
    "IDependent",
  ];

  public const string PROVIDE_METHOD_NAME = "this.Provide()";
  public const string PROVIDE_NEW_METHOD_BODY = """
// Call the this.Provide() method once your dependencies have been initialized.
this.Provide();
""";
  public const string NOTIFY_METHOD_NAME = "this.Notify(what)";
}
