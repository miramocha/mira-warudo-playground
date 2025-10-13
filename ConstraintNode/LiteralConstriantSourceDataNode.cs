using System.Data;
using Warudo.Core.Attributes;
using Warudo.Core.Graphs;
using Warudo.Plugins.Core.Assets;

namespace Warudo.Plugins.Core.Nodes;

[NodeType(
    Id = "mira-literal-constraint-source-data-node",
    Title = "Constraint Source",
    Category = "Unity Constraint"
)]
public class LiteralConstriantSourceDataNode : AAssetChildGameObjectNode
{
    ConstraintSourceData constraintSourceData = new ConstraintSourceData();

    [DataInput(-998)]
    [FloatSlider(0, 1)]
    public float Weight = 1.0f;

    [DataOutput]
    public ConstraintSourceData ConstraintSourceData() => constraintSourceData;

    protected override void OnCreate()
    {
        base.OnCreate();
        Watch<GameObjectAsset>(nameof(Asset), OnAssetChanged);
        Watch<string>(nameof(GameObjectPath), OnGameObjectPathChanged);
        Watch<float>(nameof(Weight), OnWeightChanged);
    }

    protected override void OnAssetChanged(GameObjectAsset oldValue, GameObjectAsset newValue)
    {
        constraintSourceData.Asset = newValue;
    }

    protected override void OnGameObjectPathChanged(string oldValue, string newValue)
    {
        constraintSourceData.GameObjectPath = newValue;
    }

    protected virtual void OnWeightChanged(float oldValue, float newValue)
    {
        constraintSourceData.Weight = newValue;
    }
}
