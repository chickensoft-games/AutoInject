#pragma warning disable
namespace Chickensoft.AutoInject;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Godot;
using SuperNodes.Types;
#pragma warning disable CS8019
using Chickensoft.AutoInject;
#pragma warning restore CS8019

/// <summary>
/// When a SuperNode applies the Dependent PowerUp, it inherits this interface
/// marking it as a dependent node.
/// </summary>
public interface IDependent : ISuperNode {
  /// <summary>Dependent state used to manage dependencies.</summary>
  DependencyState DependentState { get; }

  /// <summary>Event invoked when dependencies have been resolved.</summary>
  event Action? OnDependenciesResolved;

  /// <summary>
  /// Method that is invoked when all of the dependent node's dependencies are
  /// resolved (after _Ready() but before _Process()).
  /// </summary>
  void OnResolved() { }

  /// <summary>
  /// <para>
  /// Method used by the dependency resolution system to tell the dependent
  /// node to announce that all of its dependencies have been resolved.
  /// </para>
  /// <para>Don't call this method.</para>
  /// </summary>
  void _AnnounceDependenciesResolved() { }

  /// <summary>
  /// Add a fake value to the dependency table. Adding a fake value allows a
  /// unit test to override a dependency lookup with a fake value.
  /// </summary>
  /// <param name="value">Dependency value (probably a mock or a fake).</param>
  /// <typeparam name="T">Dependency type.</typeparam>
  void FakeDependency<T>(T value) where T : notnull;

  /// <summary>
  /// Returns a dependency that was resolved from an ancestor provider node.
  /// </summary>
  /// <typeparam name="TValue">The type of the value to resolve.</typeparam>
  /// <param name="fallback">Fallback value to use if a provider for this type
  /// wasn't found during dependency resolution.</param>
  /// <returns>
  /// The resolved dependency value, the fallback value, or throws an exception
  /// if the provider wasn't found during dependency resolution and a fallback
  /// value was not given
  /// </returns>
  /// <exception cref="ProviderNotFoundException">Thrown if the provider for
  /// the requested value could not be found and when no fallback value is
  /// specified.</exception>
  TValue DependOn<TValue>(Func<TValue>? fallback = default)
    where TValue : notnull;
}

/// <summary>
/// Dependent PowerUp. Apply this to SuperNodes to automatically resolve
/// dependencies marked with the [Dependency] attribute without using
/// reflection.
/// </summary>
[PowerUp]
public abstract partial class Dependent : Node, IDependent {
  #region SuperNodesStaticReflectionStubs
  // These static stubs don't need to be copied over because we'll be copied
  // into a SuperNode that declares these.

  [PowerUpIgnore]
  public static ImmutableDictionary<string, ScriptPropertyOrField>
    ScriptPropertiesAndFields { get; } =
      new Dictionary<string, ScriptPropertyOrField>().ToImmutableDictionary();

  [PowerUpIgnore]
  public static TResult ReceiveScriptPropertyOrFieldType<TResult>(
    string scriptProperty, ITypeReceiver<TResult> receiver
  ) => default!;

  #endregion SuperNodesStaticReflectionStubs

  #region ISuperNode
  // These don't need to be copied over since we will be copied into an
  // ISuperNode.

  [PowerUpIgnore]
  public ImmutableDictionary<string, ScriptPropertyOrField> PropertiesAndFields
      => throw new NotImplementedException();
  [PowerUpIgnore]
  public TResult GetScriptPropertyOrFieldType<TResult>(
      string scriptProperty, ITypeReceiver<TResult> receiver
    ) => throw new NotImplementedException();
  [PowerUpIgnore]
  public dynamic GetScriptPropertyOrField(string scriptProperty) =>
      throw new NotImplementedException();
  [PowerUpIgnore]
  public void SetScriptPropertyOrField(string scriptProperty, dynamic value) =>
      throw new NotImplementedException();

  #endregion ISuperNode

  #region AddedInstanceState

  public event Action? OnDependenciesResolved;

  /// <summary>
  /// Dependent SuperNodes are all given a private dependency state which
  /// stores the dependency table and a flag indicating if dependencies are
  /// stale. This is the only pointer that is added to each dependent node to
  /// avoid increasing the memory footprint of nodes.
  /// </summary>
  public DependencyState DependentState { get; } = new();

  #endregion AddedInstanceState

  /// <summary>
  /// Dictionary of script members that were marked with the dependency
  /// attribute, keyed by member name. This is computed statically to avoid
  /// needing to compute it for each node.
  /// </summary>
  private static readonly Lazy<
    ImmutableDictionary<string, ScriptPropertyOrField>
  > _allDependencies = new(
    () => DependencyResolver.GetDependenciesToResolve(
      ScriptPropertiesAndFields
    !)
  );

  /// <summary>
  /// Called by SuperNodes on behalf of your node any time your node receives an
  /// event. This is what allows the Dependent PowerUp to automatically manage
  /// dependencies on behalf of your node script.
  /// </summary>
  /// <param name="what">Godot notification.</param>
  public void OnDependent(int what) =>
    DependencyResolver.OnDependent(
      what,
      this,
      _allDependencies.Value
    );

  public void _AnnounceDependenciesResolved() =>
    OnDependenciesResolved?.Invoke();

  /// <summary>
  /// Returns a dependency that was resolved from an ancestor provider node.
  /// </summary>
  /// <typeparam name="TValue">The type of the value to resolve.</typeparam>
  /// <param name="fallback">Fallback value to use if a provider for this type
  /// wasn't found during dependency resolution.</param>
  /// <returns>
  /// The resolved dependency value, the fallback value, or throws an exception
  /// if the provider wasn't found during dependency resolution and a fallback
  /// value was not given
  /// </returns>
  /// <exception cref="ProviderNotFoundException">Thrown if the provider for
  /// the requested value could not be found and when no fallback value is
  /// specified.</exception>
  public TValue DependOn<TValue>(Func<TValue>? fallback = default)
    where TValue : notnull => DependencyResolver.DependOn(this, fallback);


  /// <summary>
  /// Add a fake value to the dependency table. Adding a fake value allows a
  /// unit test to override a dependency lookup with a fake value.
  /// </summary>
  /// <param name="value">Dependency value (probably a mock or a fake).</param>
  /// <typeparam name="T">Dependency type.</typeparam>
  public void FakeDependency<T>(T value) where T : notnull {
    DependentState.ProviderFakes[typeof(T)] =
      new DependencyResolver.DefaultProvider(value);
  }
}

/// <summary>
/// Data added to each Dependent SuperNode.
/// </summary>
public class DependencyState {
  /// <summary>
  /// Resolved dependencies are stored in this table. Don't touch!
  /// </summary>
  public readonly DependencyResolver.DependencyTable Dependencies = new();

  /// <summary>
  /// Used by the dependency system to determine if dependencies are stale.
  /// Dependencies go stale whenever a node is removed from the tree and added
  /// back.
  /// </summary>
  public bool ShouldResolveDependencies { get; set; } = true;

  /// <summary>
  /// Dictionary of providers we are listening to that are still initializing
  /// their provided values. We use this in the rare event that we have to
  /// clean up subscriptions before providers ever finished initializing.
  /// </summary>
  public Dictionary<IProvider, PendingProvider> Pending { get; }
    = new();

  /// <summary>
  /// Overrides for providers keyed by dependency type. Overriding providers
  /// allows nodes being unit-tested to provide fake providers during unit tests
  /// that return mock or faked values.
  /// </summary>
  public Dictionary<Type, DependencyResolver.DefaultProvider> ProviderFakes {
    get;
  } = new();
}

public record PendingProvider(
  IProvider Provider,
  Action<IProvider> Success
) {
  public void Unsubscribe() {
    Provider.ProviderState.OnInitialized -= Success;
  }
}

/// <summary>
/// Actual implementation of the dependency resolver. Implementation is stored
/// here to prevent copying too much duplicate code into every SuperNode that
/// uses the Dependent PowerUp.
/// </summary>
public static class DependencyResolver {
  /// <summary>
  /// A type receiver for use with SuperNode's static reflection. This is given
  /// a class at construction time and used to determine if the class can provide
  /// a value of a given type.
  /// </summary>
  public class ProviderValidator : ITypeReceiver<bool> {
    /// <summary>Provider to validate.</summary>
    public IProvider Provider { get; set; }

    public ProviderValidator() {
      Provider = default!;
    }

#nullable disable
    public bool Receive<T>() => Provider is IProvide<T>;
#nullable restore
  }

  /// <summary>
  /// Essentially a typedef for a Dictionary that maps types to providers.
  /// </summary>
  public class DependencyTable : Dictionary<Type, IProvider> { }

  [ThreadStatic]
  private static readonly ProviderValidator _validator = new();

  /// <summary>
  /// The provider validator. This receives the generic type of the provider
  /// and uses it to determine if the provider can provide the type of value
  /// requested by the dependent. Because we only have one validator and its
  /// state is mutated to avoid extra allocations, there is one validator per
  /// thread to guarantee safety.
  /// </summary>
  public static ProviderValidator Validator => _validator;

  /// <summary>
  /// Finds and returns the members of a script that are marked with the
  /// [Dependency] attribute.
  /// </summary>
  /// <param name="members">Script members.</param>
  /// <returns>Members that represent dependencies.</returns>
  public static ImmutableDictionary<string, ScriptPropertyOrField>
    GetDependenciesToResolve(
      ImmutableDictionary<string, ScriptPropertyOrField> members
    ) {
    var dependencies = ImmutableDictionary
      .CreateBuilder<string, ScriptPropertyOrField>();
    foreach (var member in members.Values) {
      if (member.Attributes.ContainsKey(
        "global::Chickensoft.AutoInject.DependencyAttribute"
      )) {
        dependencies.Add(member.Name, member);
      }
    }
    return dependencies.ToImmutable();
  }

  /// <summary>
  /// Called by the Dependent PowerUp applied to SuperNodes to determine if
  /// dependencies are stale and need to be resolved. If so, this will
  /// automatically trigger the dependency resolution process.
  /// </summary>
  /// <param name="what">Godot node notification.</param>
  /// <param name="dependent">Dependent node.</param>
  /// <param name="allDependencies">All dependencies.</param>
  public static void OnDependent(
    int what,
    IDependent dependent,
    ImmutableDictionary<string, ScriptPropertyOrField> allDependencies
  ) {
    if (what == Node.NotificationExitTree) {
      dependent.DependentState.ShouldResolveDependencies = true;
      foreach (var pending in dependent.DependentState.Pending.Values) {
        pending.Unsubscribe();
      }
      dependent.DependentState.Pending.Clear();
    }
    if (
        what == Node.NotificationReady &&
        dependent.DependentState.ShouldResolveDependencies
      ) {
      Resolve(dependent, allDependencies);
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
    if (dependent.DependentState.ProviderFakes.TryGetValue(
        typeof(TValue), out var fakeProvider
      )
    ) {
      return fakeProvider.Value();
    }

    // Lookup dependency, per usual, respecting any fallback values if there
    // were no resolved providers for the requested type during dependency
    // resolution.
    if (dependent.DependentState.Dependencies.TryGetValue(
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
      dependent.DependentState.Dependencies.Add(typeof(TValue), provider);
      return (TValue)provider.Value();
    }

    throw new ProviderNotFoundException(typeof(TValue));
  }

  /// <summary>
  /// Resolve dependencies. Used by the Dependent PowerUp to resolve
  /// dependencies for a given SuperNode.
  /// </summary>
  /// <param name="dependent">SuperNode who wishes to resolve dependencies.
  /// </param>
  /// <param name="dependenciesToResolve">Members of the SuperNode which
  /// represent dependencies.</param>
  private static void Resolve(
    IDependent dependent,
    ImmutableDictionary<string, ScriptPropertyOrField> dependenciesToResolve
  ) {
    var state = dependent.DependentState;
    // Clear any previously resolved dependencies — if the ancestor tree hasn't
    // changed above us, we will just end up re-resolving them as they were.
    state.Dependencies.Clear();

    var shouldResolve = true;
    var remainingDependencies =
      new HashSet<ScriptPropertyOrField>(dependenciesToResolve.Values);
    var node = (Node)dependent;
    var foundDependencies = new HashSet<ScriptPropertyOrField>();
    var providersInitializing = 0;

    void resolve() {
      dependent.OnResolved();
      dependent._AnnounceDependenciesResolved();
    }

    void onProviderInitialized(IProvider provider) {
      providersInitializing--;

      lock (dependent.DependentState.Pending) {
        dependent.DependentState.Pending[provider].Unsubscribe();
        dependent.DependentState.Pending.Remove(provider);
      }

      if (providersInitializing == 0) {
        resolve();
      }
    }

    while (node != null && shouldResolve) {
      foundDependencies.Clear();

      if (node is IProvider provider) {
        // For each provider node ancestor, check each of our remaining
        // dependencies to see if the provider node is the type needed
        // to satisfy the dependency.
        foreach (var dependency in remainingDependencies) {
          Validator.Provider = provider;

          // Use SuperNode's static reflection capabilities to determine if
          // we have found the correct provider for the dependency.
          var isCorrectProvider = dependent.GetScriptPropertyOrFieldType(
            dependency.Name, Validator
          );

          if (isCorrectProvider) {
            // Add the provider to our internal dependency table.
            dependent.DependentState.Dependencies.Add(
              dependency.Type, provider
            );

            // Mark this dependency to be removed from the list of dependencies
            // we're searching for.
            foundDependencies.Add(dependency);

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

  public class DefaultProvider : IProvider {
    private readonly dynamic _value;
    public ProviderState ProviderState { get; }

    public DefaultProvider(dynamic value) {
      _value = value;
      ProviderState = new() { IsInitialized = true };
    }

    public dynamic Value() => _value;
  }
}
#pragma warning restore
