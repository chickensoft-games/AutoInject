namespace Chickensoft.AutoInject;

using Godot;

public static class AutoInject
{
  internal const string FALLBACK_INSTANCE_META_KEY = "Chickensoft_AutoInject__FallbackInstance__";

  public static void SetGlobalFallback(Node fallbackProviderNode) =>
    Engine.GetMainLoop().SetMeta(FALLBACK_INSTANCE_META_KEY, fallbackProviderNode);

  public static Node? GetGlobalFallback()
  {
    if (!Engine.GetMainLoop().HasMeta(FALLBACK_INSTANCE_META_KEY))
    {
      return null;
    }
    return Engine.GetMainLoop().GetMeta(FALLBACK_INSTANCE_META_KEY).As<Node>();
  }
}
