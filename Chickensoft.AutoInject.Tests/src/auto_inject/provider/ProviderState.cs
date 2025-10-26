namespace Chickensoft.AutoInject;

using System;
using Godot;
#pragma warning disable IDE0005
using Chickensoft.Introspection;
using Chickensoft.AutoInject;


/// <summary>
/// Provider state used internally when resolving dependencies.
/// </summary>
public class ProviderState
{
  /// <summary>Whether the provider has initialized all of its values.</summary>
  public bool IsInitialized { get; set; }

  /// <summary>
  /// Underlying event delegate used to inform dependent nodes that the
  /// provider has initialized all of the values it provides.
  /// </summary>
  public event Action<IBaseProvider>? OnInitialized;

  /// <summary>
  /// Announces to descendent nodes that the values provided by this provider
  /// are initialized.
  /// </summary>
  /// <param name="provider">Provider node which has finished initializing
  /// the values it provides.</param>
  public void Announce(IBaseProvider provider)
    => OnInitialized?.Invoke(provider);

  /// <summary>
  /// Internal implementation for the OnProvider lifecycle method. Resets the
  /// provider's initialized status when the provider leaves the scene tree.
  /// </summary>
  /// <param name="what">Godot node notification.</param>
  /// <param name="provider">Provider node.</param>
  public static void OnProvider(int what, IProvider provider)
  {
    if (what == Node.NotificationExitTree)
    {
      provider.ProviderState.IsInitialized = false;
    }
  }

  /// <summary>
  /// Internal implementation for the Provide method. This marks the Provider
  /// as having provided all of its values and then announces to dependent
  /// nodes that the provider has finished initializing.
  /// </summary>
  /// <param name="provider"></param>
  public static void Provide(IProvider provider)
  {
    provider.ProviderState.IsInitialized = true;
    provider.ProviderState.Announce(provider);
    provider.OnProvided();
  }
}
