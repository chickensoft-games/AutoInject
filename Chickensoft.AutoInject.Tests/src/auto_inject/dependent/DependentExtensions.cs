namespace Chickensoft.AutoInject;

using System;
#pragma warning disable IDE0005
using Chickensoft.AutoInject;

public static class DependentExtensions
{
  /// <inheritdoc
  ///   cref="DependencyResolver.DependOn{TValue}(IDependent, Func{TValue}?)" />
  public static TValue DependOn<TValue>(
    this IDependent dependent,
    Func<TValue>? fallback = default
  ) where TValue : notnull => DependencyResolver.DependOn(dependent, fallback);

  /// <inheritdoc cref="IDependent.FakeDependency{T}(T)" />
  public static void FakeDependency<T>(
    this IDependent dependent, T value
  ) where T : notnull => dependent.FakeDependency(value);
}
