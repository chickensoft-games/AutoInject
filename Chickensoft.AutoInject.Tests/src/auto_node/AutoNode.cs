namespace Chickensoft.AutoInject;

using Chickensoft.Introspection;

/// <summary>
/// <para>
/// Add this mixin to your introspective node to automatically connects nodes
/// declared with the [Node] attribute,
/// call an additional initialization lifecycle method, and allow you to
/// provide dependencies to descendant nodes or fetch them from ancestors via
/// the [Dependency] attribute.
/// </para>
/// <para>
/// This enables you to leverage all of the functionality of AutoInject with one
/// easy mixin.
/// </para>
/// </summary>
public interface IAutoNode : IMixin<IAutoNode>,
IAutoOn, IAutoConnect, IAutoInit, IProvider, IDependent
{
  void IMixin<IAutoNode>.Handler() { }

  new void Handler()
  {
    // IAutoOn isn't called since its handler does nothing.
    (this as IAutoConnect).Handler();
    // IDependent invokes IAutoInit, so we don't invoke it directly.
    (this as IProvider).Handler();
    (this as IDependent).Handler();
  }
}
