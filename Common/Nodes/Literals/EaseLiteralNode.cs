using DG.Tweening;
using UnityEngine;
using Warudo.Core.Attributes;
using Warudo.Core.Graphs;

namespace Warudo.Plugins.Core.Nodes;

[NodeType(
    Id = "mira-ease-literal-node",
    Title = "TRANSITION_EASING",
    Category = "CATEGORY_LITERALS"
)]
public class LiteralVector3ListNode : Node
{
    [DataInput(9)]
    [Label("Value")]
    public Ease Value = Ease.OutCubic;

    [DataOutput(13)]
    [Label("OUTPUT")]
    public Ease Output()
    {
        return Value;
    }
}
