namespace Chickensoft.AutoInject;

using System.Runtime.CompilerServices;
using Chickensoft.GodotNodeInterfaces;
#pragma warning disable CS8019, IDE0005
using Chickensoft.AutoInject;
using Godot;
using Chickensoft.Introspection;
using System.Collections.Generic;

/// <summary>
/// Apply this mixin to your introspective node to automatically connect
/// declared node references to their corresponding instances in the scene tree.
/// </summary>
[Mixin]
public interface IAutoConnect : IMixin<IAutoConnect>, IFakeNodeTreeEnabled {

  FakeNodeTree? IFakeNodeTreeEnabled.FakeNodes {
    get {
      _AddStateIfNeeded();
      return MixinState.Get<FakeNodeTree>();
    }
    set {
      if (value is { } tree) {
        MixinState.Overwrite(value);
      }
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  void IMixin<IAutoConnect>.Handler() {
    var what = MixinState.Get<NotificationState>().Notification;

    if (what == Node.NotificationSceneInstantiated) {
      AutoConnector.ConnectNodes(Types.Graph.GetProperties(GetType()), this);
    }
  }

#pragma warning disable IDE1006 // Naming Styles
  public void _AddStateIfNeeded(Dictionary<string, INode>? nodes = null) {
    if (this is not Node node) { return; }
    if (!MixinState.Has<FakeNodeTree>()) {
      MixinState.Overwrite(new FakeNodeTree(node, nodes));
    }
  }
}

