namespace Chickensoft.AutoInject;

using System;

/// <summary>
/// Exception thrown when a provider node cannot be found
/// in any of the dependent node's ancestors while resolving dependencies.
/// </summary>
public class ProviderNotFoundException : InvalidOperationException {
  /// <summary>Creates a new provider not found exception.</summary>
  /// <param name="providerType"></param>
  public ProviderNotFoundException(Type providerType) : base(
    $"No provider found for the following type: {providerType}" + ". " +
    "Consider specifying a fallback value in `DependOn<T>(T fallback)`."
  ) { }
}
