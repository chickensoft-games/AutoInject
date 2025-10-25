namespace Chickensoft.AutoInject;

using System;

#pragma warning disable IDE0005
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using System.Globalization;

public class PendingProvider(
  IBaseProvider provider, Action<IBaseProvider> success
)
{
  public IBaseProvider Provider { get; } = provider;
  public Action<IBaseProvider> Success { get; } = success;
  public void Unsubscribe() => Provider.ProviderState.OnInitialized -= Success;
}
