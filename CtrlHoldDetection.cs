
using UnityEngine;
using Warudo.Core.Attributes;
using Warudo.Core.Graphs;
using Warudo.Core.Scenes;
using Warudo.Plugins.Core.Assets;
using Warudo.Plugins.Core.Assets.Cinematography;
using System;

[NodeType(
    Id = "6828802d-d7ef-4aa1-803f-f04daa8e5fd4",
    Title = "Ctrl Detect",
    Category = "Custom")]
public class CtrlDetectNode : Node
{
    private bool leftCtrlDown = false;

    [DataOutput]
    [Label("Left Ctrl Down?")]
    public bool GetLeftCtrlDown()
    {
        return leftCtrlDown;
    }

    public override void OnUpdate()
    {
        leftCtrlDown = Input.GetKey(KeyCode.LeftControl);
    }
}