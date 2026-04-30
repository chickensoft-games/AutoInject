namespace Chickensoft.AutoInject;

using Godot;

public static class AutoInject
{
  internal const string FALLBACK_INSTANCE_META_KEY = "Chickensoft_AutoInject__FallbackInstance__";

  /// <summary>
  /// Set global fallback dependency provider node. The node may implement
  /// IProvide<T> or IProvideAny.
  /// This node will be the last provider in the dependency resolution chain.
  /// </summary>
  /// <param name="fallbackProviderNode">
  /// The fallback dependency provider node or null to unset.
  /// </param>
  public static void SetGlobalFallback(Node? fallbackProviderNode) =>
    Engine.GetMainLoop().SetMeta(
      FALLBACK_INSTANCE_META_KEY,
      fallbackProviderNode ?? new Variant()
    );


  /// <summary>
  /// Set global fallback dependency provider node.
  /// This node will be the last provider in the dependency resolution chain.
  /// </summary>
  /// <returns>The fallback dependency provider node or null.</returns>
  public static Node? GetGlobalFallback()
  {
    if (!Engine.GetMainLoop().HasMeta(FALLBACK_INSTANCE_META_KEY))
    {
      return null;
    }
    return Engine.GetMainLoop().GetMeta(FALLBACK_INSTANCE_META_KEY).As<Node>();
  }
}
