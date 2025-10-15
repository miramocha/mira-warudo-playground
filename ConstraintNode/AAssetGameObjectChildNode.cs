using Cysharp.Threading.Tasks;
using UnityEngine;
using Warudo.Core.Attributes;
using Warudo.Core.Graphs;
using Warudo.Plugins.Core.Assets;

namespace Warudo.Plugins.Core.Nodes;

public abstract class AAssetChildGameObjectNode : AssetGameObjectNode
{
    [DataOutput]
    [HiddenIf(nameof(HideGameObjectOutput))]
    public GameObject GameObject() => base.FindTargetTransform()?.gameObject;

    [DataOutput]
    [HiddenIf(nameof(HideGameObjectTransformOutput))]
    public Transform Transform() => GameObject()?.gameObject?.transform;

    public virtual bool HideGameObjectOutput()
    {
        return true;
    }

    public virtual bool HideGameObjectTransformOutput()
    {
        return true;
    }

    protected override void OnCreate()
    {
        base.OnCreate();
        WatchAsset(nameof(Asset), OnAssetChanged);
        Watch<string>(nameof(GameObjectPath), OnGameObjectPathChanged);
    }

    protected virtual void OnAssetChanged() { }

    protected virtual void OnGameObjectPathChanged(string oldValue, string newValue) { }
}
