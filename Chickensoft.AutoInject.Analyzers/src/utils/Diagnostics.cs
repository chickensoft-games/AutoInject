namespace Chickensoft.AutoInject.Analyzers.Utils;

using Microsoft.CodeAnalysis;

public static class Diagnostics {
  private const string ERR_PREFIX = "AUTO_INJECT";
  private const string ERR_CATEGORY = "Chickensoft.AutoInject.Analyzers";

  public static DiagnosticDescriptor MissingAutoInjectNotifyOverrideDescriptor { get; } = new(
    id: $"{ERR_PREFIX}001",
    title: $"Missing \"_Notification\" method override",
    messageFormat: $"Missing override of \"_Notification\" in AutoInject class implementation `{{0}}`",
    category: ERR_CATEGORY,
    defaultSeverity: DiagnosticSeverity.Error,
    isEnabledByDefault: true,
    description: "Overriding the _Notification method is required to pass the lifecycle of the Godot node to AutoInject. Without this, all AutoInject functionality will not work as expected."
  );

  public static Diagnostic MissingAutoInjectNotifyOverride(
    Location location, string name
  ) => Diagnostic.Create(MissingAutoInjectNotifyOverrideDescriptor, location, name);

  public static DiagnosticDescriptor MissingAutoInjectNotifyDescriptor { get; } = new(
    id: $"{ERR_PREFIX}002",
    title: $"Missing \"{Constants.NOTIFY_METHOD_NAME}\" method call",
    messageFormat: $"Missing \"{Constants.NOTIFY_METHOD_NAME}\" in AutoInject class implementation `{{0}}`",
    category: ERR_CATEGORY,
    defaultSeverity: DiagnosticSeverity.Error,
    isEnabledByDefault: true,
    description: "Calling this.Notify(what); within the _Notification method is required to pass the lifecycle of the Godot node to AutoInject. Without this, all AutoInject functionality will not work as expected."
  );

  public static Diagnostic MissingAutoInjectNotify(
    Location location, string name
  ) => Diagnostic.Create(MissingAutoInjectNotifyDescriptor, location, name);

  public static DiagnosticDescriptor MissingAutoInjectProvideDescriptor { get; } = new(
    id: $"{ERR_PREFIX}003",
    title: $"Missing \"{Constants.PROVIDE_METHOD_NAME}\" call in provider class",
    messageFormat: $"Missing \"{Constants.PROVIDE_METHOD_NAME}\" call in provider class implementation `{{0}}`",
    category: ERR_CATEGORY,
    defaultSeverity: DiagnosticSeverity.Error,
    isEnabledByDefault: true,
    description: "Calling the Provide method is required to provide dependencies to the AutoInject system. Without this, the provided dependencies will not be injected and dependent classes will not function as expected."
  );

  public static Diagnostic MissingAutoInjectProvide(
    Location location, string name
  ) => Diagnostic.Create(MissingAutoInjectProvideDescriptor, location, name);
}
