namespace Chickensoft.AutoInject;

/// <summary>
/// Types that want to be informed of ready can implement this interface.
/// </summary>
public interface IReadyAware
{
  /// <summary>Called right before the node is ready.</summary>
  void OnBeforeReady();

  /// <summary>Called right after the node is readied.</summary>
  void OnAfterReady();
}
