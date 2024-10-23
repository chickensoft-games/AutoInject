
namespace Chickensoft.AutoInject;

using System;
using System.Collections.Generic;
using Godot;

#pragma warning disable IDE0005
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using System.Globalization;

/// <summary>
/// Actual implementation of the dependency resolver.
/// </summary>
public static class DependencyResolver {
  /// <summary>
  /// A type receiver for use with introspective node's reflection metadata.
  /// This is given a class at construction time and used to determine if the
  /// class can provide a value of a given type.
  /// </summary>
  private sealed class ProviderValidator : ITypeReceiver {
    /// <summary>Provider to validate.</summary>
    public IBaseProvider Provider { get; set; }

    /// <summary>
    /// Result of the validation. True if the node can provide the type.
    /// </summary>
    public bool Result { get; set; }

    public ProviderValidator() {
      Provider = default!;
    }

#nullable disable
    public void Receive<T>() => Result = Provider is IProvide<T>;
#nullable restore
  }

  /// <summary>
  /// Essentially a typedef for a Dictionary that maps types to providers.
  /// </summary>
  public class DependencyTable : Dictionary<Type, IBaseProvider> { }

  [ThreadStatic]
  private static readonly ProviderValidator _validator;

  static DependencyResolver() {
    _validator = new();
  }

  /// <summary>
  /// The provider validator. This receives the generic type of the provider
  /// and uses it to determine if the provider can provide the type of value
  /// requested by the dependent. Because we only have one validator and its
  /// state is mutated to avoid extra allocations, there is one validator per
  /// thread to guarantee safety.
  /// </summary>
  private static ProviderValidator Validator => _validator;

  /// <summary>
  /// Finds and returns the members of a script that are marked with the
  /// [Dependency] attribute.
  /// </summary>
  /// <param name="members">Script members.</param>
  /// <returns>Members that represent dependencies.</returns>
  private static IEnumerable<PropertyMetadata> GetDependenciesToResolve(
    IEnumerable<PropertyMetadata> properties
  ) {
    foreach (var property in properties) {
      if (property.Attributes.ContainsKey(typeof(DependencyAttribute))) {
        yield return property;
      }
    }
  }

  /// <summary>
  /// Called by the Dependent mixin on an introspective node to determine if
  /// dependencies are stale and need to be resolved. If so, this will
  /// automatically trigger the dependency resolution process.
  /// </summary>
  /// <param name="what">Godot node notification.</param>
  /// <param name="dependent">Dependent node.</param>
  /// <param name="allDependencies">All dependencies.</param>
  public static void OnDependent(
    int what,
    IDependent dependent,
    IEnumerable<PropertyMetadata> properties
  ) {
    var state = dependent.MixinState.Get<DependentState>();
    if (what == Node.NotificationExitTree) {
      dependent.MixinState.Get<DependentState>().ShouldResolveDependencies = true;
      foreach (var pending in state.Pending.Values) {
        pending.Unsubscribe();
      }
      state.Pending.Clear();
    }
    if (
        what == Node.NotificationReady &&
        state.ShouldResolveDependencies
      ) {
      Resolve(dependent, properties);
    }
  }

  /// <summary>
  /// Returns a dependency that was resolved from an ancestor provider node,
  /// or the provided fallback value returned from the given lambda.
  /// </summary>
  /// <typeparam name="TValue">The type of the value to resolve.</typeparam>
  /// <param name="dependent">Dependent node.</param>
  /// <param name="fallback">Function which returns a fallback value to use if
  /// a provider for this type wasn't found during dependency resolution.
  /// </param>
  /// <returns>
  /// The resolved dependency value, the fallback value, or throws an exception
  /// if the provider wasn't found during dependency resolution and a fallback
  /// value was not given
  /// </returns>
  /// <exception cref="ProviderNotFoundException">Thrown if the provider for
  /// the requested value could not be found and when no fallback value is
  /// specified.</exception>
  /// <exception cref="ProviderNotInitializedException">Thrown if a dependency
  /// is accessed before the provider has called Provide().</exception>
  public static TValue DependOn<TValue>(
    IDependent dependent, Func<TValue>? fallback = default
  ) where TValue : notnull {
    // First, check dependency fakes. Using a faked value takes priority over
    // all the other dependency resolution methods.
    var state = dependent.MixinState.Get<DependentState>();
    if (state.ProviderFakes.TryGetValue(typeof(TValue), out var fakeProvider)) {
      return fakeProvider.Value();
    }

    // Lookup dependency, per usual, respecting any fallback values if there
    // were no resolved providers for the requested type during dependency
    // resolution.
    if (state.Dependencies.TryGetValue(
        typeof(TValue), out var providerNode
      )
    ) {
      if (!providerNode.ProviderState.IsInitialized) {
        throw new ProviderNotInitializedException(typeof(TValue));
      }
      if (providerNode is IProvide<TValue> provider) {
        return provider.Value();
      }
      else if (providerNode is DefaultProvider defaultProvider) {
        return defaultProvider.Value();
      }
    }
    else if (fallback is not null) {
      // See if we were given a fallback.
      var provider = new DefaultProvider(fallback());
      state.Dependencies.Add(typeof(TValue), provider);
      return (TValue)provider.Value();
    }

    throw new ProviderNotFoundException(typeof(TValue));
  }

  /// <summary>
  /// Resolve dependencies. Used by the Dependent mixin to resolve
  /// dependencies for a given introspective node.
  /// </summary>
  /// <param name="dependent">Introspective node which wants to resolve
  /// dependencies.</param>
  /// <param name="dependenciesToResolve">Properties of the introspective node.
  /// </param>
  private static void Resolve(
    IDependent dependent,
    IEnumerable<PropertyMetadata> properties
  ) {
    var state = dependent.MixinState.Get<DependentState>();
    // Clear any previously resolved dependencies — if the ancestor tree hasn't
    // changed above us, we will just end up re-resolving them as they were.
    state.Dependencies.Clear();

    var shouldResolve = true;
    var remainingDependencies = new HashSet<PropertyMetadata>(
      GetDependenciesToResolve(properties)
    );

    var self = (Node)dependent;
    var node = self.GetParent();
    var foundDependencies = new HashSet<PropertyMetadata>();
    var providersInitializing = 0;

    void resolve() {
      if (self.IsNodeReady()) {
        // Godot node is already ready.
        if (!dependent.IsTesting) {
          dependent.Setup();
        }
        dependent.OnResolved();
        return;
      }

      // Godot node is not ready yet, so we will wait for OnReady before
      // calling Setup() and OnResolved().

      if (!dependent.IsTesting) {
        state.PleaseCallSetup = true;
      }
      state.PleaseCallOnResolved = true;
    }

    void onProviderInitialized(IBaseProvider provider) {
      providersInitializing--;

      lock (state.Pending) {
        state.Pending[provider].Unsubscribe();
        state.Pending.Remove(provider);
      }

      if (providersInitializing == 0) {
        resolve();
      }
    }

    while (node != null && shouldResolve) {
      foundDependencies.Clear();

      if (node is IBaseProvider provider) {
        // For each provider node ancestor, check each of our remaining
        // dependencies to see if the provider node is the type needed
        // to satisfy the dependency.
        foreach (var property in remainingDependencies) {
          Validator.Provider = provider;

          // Use the generated introspection metadata to determine if
          // we have found the correct provider for the dependency.
          property.TypeNode.GenericTypeGetter(Validator);
          var isCorrectProvider = Validator.Result;

          if (isCorrectProvider) {
            // Add the provider to our internal dependency table.
            state.Dependencies.Add(
              property.TypeNode.ClosedType, provider
            );

            // Mark this dependency to be removed from the list of dependencies
            // we're searching for.
            foundDependencies.Add(property);

            // If the provider is not yet initialized, subscribe to its
            // initialization event and add it to the list of pending
            // subscriptions.
            if (
              !provider.ProviderState.IsInitialized &&
              !state.Pending.ContainsKey(provider)
            ) {
              state.Pending[provider] =
                new PendingProvider(provider, onProviderInitialized);
              provider.ProviderState.OnInitialized += onProviderInitialized;
              providersInitializing++;
            }
          }
        }
      }

      // Remove the dependencies we've resolved.
      remainingDependencies.ExceptWith(foundDependencies);

      if (remainingDependencies.Count == 0) {
        // Found all dependencies, exit loop.
        shouldResolve = false;
      }
      else {
        // Still need to find dependencies — continue up the tree until
        // this returns null.
        node = node.GetParent();
      }
    }

    if (state.Pending.Count == 0) {
      // Inform dependent that dependencies have been resolved.
      resolve();
    }

    // We *could* check to see if a provider for every dependency was found
    // and throw an exception if any were missing, but this would break support
    // for fallback values.
  }

  public class DefaultProvider : IBaseProvider {
    private readonly dynamic _value;
    public ProviderState ProviderState { get; }

    public DefaultProvider(dynamic value) {
      _value = value;
      ProviderState = new() { IsInitialized = true };
    }

    public dynamic Value() => _value;
  }
}
