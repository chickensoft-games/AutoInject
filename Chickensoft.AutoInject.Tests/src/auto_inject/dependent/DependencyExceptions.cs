#pragma warning disable
namespace Chickensoft.AutoInject;

using System;

/// <summary>
/// Exception thrown when a provider node cannot be found
/// in any of the dependent node's ancestors while resolving dependencies.
/// </summary>
public class ProviderNotFoundException : InvalidOperationException {
  /// <summary>Creates a new provider not found exception.</summary>
  /// <param name="providerType">Provider type.</param>
  public ProviderNotFoundException(Type providerType) : base(
    $"No provider found for the following type: {providerType}" + ". " +
    "Consider specifying a fallback value in `DependOn<T>(T fallback)`."
  ) { }
}

/// <summary>
/// Exception thrown when a dependency is accessed before the provider has
/// called <see cref="IProvide{T}.Provide(T)"/>.
/// </summary>
public class ProviderNotInitializedException : InvalidOperationException {
  /// <summary>Creates a new provider has not provided exception.</summary>
  /// <param name="providerType">Provider type.</param>
  public ProviderNotInitializedException(Type providerType) : base(
    "The provider for the following type has not called Provide() yet: " +
    $"{providerType}" + ". Please call Provide() from the provider " +
    "once all of its dependencies have been initialized."
  ) { }
}
#pragma warning restore
