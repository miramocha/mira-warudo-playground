using Cysharp.Threading.Tasks;
using UnityEngine;
using Warudo.Core;
using Warudo.Core.Attributes;
using Warudo.Core.Graphs;
using Warudo.Plugins.Core.Assets;

namespace Warudo.Plugins.Core.Nodes;

[NodeType(
    Id = "mira-asset-child-gameobject",
    Title = "Asset Child GameObject",
    Category = "Utilities",
    Width = 2f
)]
public class AssetChildGameObjectNode : AssetGameObjectNode
{
    [DataOutput]
    public GameObject GameObject() => base.FindTargetTransform()?.gameObject;

    [DataOutput]
    public Transform Transform() => GameObject()?.gameObject?.transform;

    protected override void OnCreate()
    {
        base.OnCreate();
        Watch<GameObjectAsset>(nameof(Asset), OnAssetChanged);
        Watch<string>(nameof(GameObjectPath), OnGameObjectPathChanged);
    }

    protected virtual void OnAssetChanged(GameObjectAsset oldValue, GameObjectAsset newValue)
    {
        // Context.Service.Toast(Warudo.Core.Server.ToastSeverity.Info, "Debug", "GameObject changed");
    }

    protected virtual void OnGameObjectPathChanged(string oldValue, string newValue)
    {
        // Context.Service.Toast(Warudo.Core.Server.ToastSeverity.Info, "Debug", "GameObject path changed");
    }
}
