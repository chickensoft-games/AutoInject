#pragma warning disable
namespace Chickensoft.AutoInject;

using System;
using Godot;
using Chickensoft.Introspection;
using Chickensoft.AutoInject;

public static class ProviderExtensions {
  public static void Provide(this IProvider provider) {
    provider.Provide();
  }
}
