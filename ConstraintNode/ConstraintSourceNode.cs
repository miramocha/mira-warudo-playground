using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Animations;
using UniVRM10;
using Warudo.Core;
using Warudo.Core.Attributes;
using Warudo.Core.Data;
using Warudo.Core.Graphs;
using Warudo.Core.Localization;
using Warudo.Core.Scenes;
using Warudo.Plugins.Core.Assets;
using Axis = UnityEngine.Animations.Axis;
using ConstraintSource = UnityEngine.Animations.ConstraintSource;

namespace Warudo.Plugins.Core.Nodes;

[NodeType(
    Id = "mira-constraint-source-node",
    Title = "Constraint Source",
    Category = "Unity Constraints",
    Width = 2f
)]
public class ConstraintSourceNode : AssetChildGameObjectNode
{
    protected UnityEngine.Animations.ConstraintSource constraintSource = new UnityEngine.Animations.ConstraintSource();

    [DataInput]
    [FloatSlider(0, 1)]
    public float Weight = 1.0f;

    [DataOutput]
    public UnityEngine.Animations.ConstraintSource Source()
    {

        return constraintSource;
    }

    protected override void OnCreate()
    {
        base.OnCreate();
        Watch<float>(nameof(Weight), OnWeightChanged);
    }

    protected virtual void OnWeightChanged(float oldValue, float newValue)
    {
        constraintSource.weight = newValue;
    }

    protected override void OnAssetChanged(GameObjectAsset oldValue, GameObjectAsset newValue)
    {
        base.OnAssetChanged(oldValue, newValue);
        constraintSource.sourceTransform = FindTargetTransform();
    }

    protected override void OnGameObjectPathChanged(string oldValue, string newValue)
    {
        base.OnGameObjectPathChanged(oldValue, newValue);
        constraintSource.sourceTransform = FindTargetTransform();
    }
}
