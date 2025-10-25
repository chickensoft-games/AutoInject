namespace Chickensoft.AutoInject;

using System.Collections.Generic;

/// <summary>
/// Dependent introspective nodes are all given a private dependency state which
/// stores the dependency table and a flag indicating if dependencies are
/// stale. This is the only pointer that is added to each dependent node to
/// avoid increasing the memory footprint of nodes.
/// </summary>
public class DependentState
{
  /// <summary>
  /// True if the node is being unit-tested. When unit-tested, setup callbacks
  /// will not be invoked.
  /// </summary>
  public bool IsTesting { get; set; }

  /// <summary>
  /// Resolved dependencies are stored in this table. Don't touch!
  /// </summary>
  public readonly DependencyResolver.DependencyTable Dependencies = [];

  /// <summary>
  /// Used by the dependency system to determine if dependencies are stale.
  /// Dependencies go stale whenever a node is removed from the tree and added
  /// back.
  /// </summary>
  public bool ShouldResolveDependencies { get; set; } = true;

  /// <summary>Set internally when Setup() should be called.</summary>
  public bool PleaseCallSetup { get; set; }
  /// <summary>Set internally when OnResolved() should be called.</summary>
  public bool PleaseCallOnResolved { get; set; }

  /// <summary>
  /// Dictionary of providers we are listening to that are still initializing
  /// their provided values. We use this in the rare event that we have to
  /// clean up subscriptions before providers ever finished initializing.
  /// </summary>
  public Dictionary<IBaseProvider, PendingProvider> Pending { get; }
    = [];

  /// <summary>
  /// Overrides for providers keyed by dependency type. Overriding providers
  /// allows nodes being unit-tested to provide fake providers during unit tests
  /// that return mock or faked values.
  /// </summary>
  public DependencyResolver.DependencyTable ProviderFakes
  {
    get;
  } = [];
}
