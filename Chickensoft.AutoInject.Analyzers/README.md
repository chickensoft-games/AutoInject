# AutoInject Analyzers

Roslyn analyzers providing diagnostics and code fixes for common issues using [Chickensoft.AutoInject](https://www.nuget.org/packages/Chickensoft.AutoInject) in Godot node scripts.

Current diagnostics and fixes:
* Missing override of `void _Notification(int what)`
* Missing call to `this.Notify()`
* Missing call to `this.Provide()` for nodes implementing `IProvider`
