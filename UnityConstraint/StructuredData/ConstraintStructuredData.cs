// using System.Collections.Generic;
// using System.Runtime.Remoting.Contexts;
// using Cysharp.Threading.Tasks;
// using UnityEngine;
// using UnityEngine.Animations;
// using Warudo.Core.Attributes;
// using Warudo.Core.Data;
// using Warudo.Core.Utils;
// using Warudo.Plugins.Core.Assets;
// using Warudo.Plugins.Core.Assets.Character;
// using Warudo.Plugins.Core.Assets.Environment;
// using Warudo.Plugins.Core.Utils;

using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;
using Warudo.Core;
using Warudo.Core.Attributes;
using Warudo.Core.Data;
using Warudo.Core.Data.Models;
using Warudo.Core.Scenes;
using Warudo.Core.Server;
using Warudo.Core.Utils;
using Warudo.Plugins.Core.Assets;
using Warudo.Plugins.Core.Utils;

namespace Warudo.Plugins.Scene.Assets;

public class ConstraintStructuredData
    : StructuredData,
        IConstraintStructuredData,
        ICollapsibleStructuredData
{
    public string GetHeader() => Asset?.Name + '/' + GameObjectPath;

    [Trigger]
    [Label("Delete Constraint")]
    public async void ConfirmDelete()
    {
        bool confirmed = await Context.Service.PromptConfirmation(
            "Confirmation",
            "Deleting this constraint?"
        );

        if (confirmed == true)
        {
            Manager.DeleteConstraintStructuredData(this);
        }
    }

    [DataInput]
    [Label("ASSET")]
    [Disabled]
    public GameObjectAsset AssetInput;
    public GameObjectAsset Asset
    {
        get { return AssetInput; }
        set { SetDataInput(nameof(AssetInput), value, broadcast: true); }
    }

    [DataInput]
    [AutoComplete("AutoCompleteGameObjectPath", false, "")]
    [Label("GAMEOBJECT_PATH")]
    [Disabled]
    public string GameObjectPathInput;
    public string GameObjectPath
    {
        get { return GameObjectPathInput; }
        set { SetDataInput(nameof(GameObjectPathInput), value, broadcast: true); }
    }

    [Section("Settings", UnityConstraintUIOrdering.WEIGHT_INPUT - 1)]
    [DataInput(UnityConstraintUIOrdering.WEIGHT_INPUT)]
    [Label("Weight")]
    [FloatSlider(0, 1)]
    public float Weight = 1.0f;

    [DataInput(UnityConstraintUIOrdering.POSITION_AT_REST_INPUT)]
    [Label("Position At Rest")]
    public Vector3 ConstraintPositionAtRest = Vector3.zero;

    [DataInput(UnityConstraintUIOrdering.ROTATION_AT_REST_INPUT)]
    [Label("Rotation At Rest")]
    public Vector3 ConstraintRotationAtRest = Vector3.zero;

    [Section("Freeze Rotation Axes", UnityConstraintUIOrdering.FREEZE_ROTATION_SECTION)]
    [DataInput(UnityConstraintUIOrdering.FREEZE_ROTATION_X_INPUT)]
    [Label("X Axis")]
    public bool FreezeRotationX = true;

    [DataInput(UnityConstraintUIOrdering.FREEZE_ROTATION_Y_INPUT)]
    [Label("Y Axis")]
    public bool FreezeRotationY = true;

    [DataInput(UnityConstraintUIOrdering.FREEZE_ROTATION_Z_INPUT)]
    [Label("Z Axis")]
    public bool FreezeRotationZ = true;
    public Axis FreezeRotationAxes
    {
        get
        {
            Axis axes = Axis.None;
            if (FreezeRotationX)
                axes |= Axis.X;
            if (FreezeRotationY)
                axes |= Axis.Y;
            if (FreezeRotationZ)
                axes |= Axis.Z;
            return axes;
        }
    }

    public UnityParentConstraintsManagerAsset Manager { get; set; }

    public string ConstraintTransformID
    {
        get { return ConstraintStructuredDataUtil.GetConstraintTransformID(this); }
    }

    public async UniTask<AutoCompleteList> AutoCompleteGameObjectPath()
    {
        return await ConstraintStructuredDataUtil.AutoCompleteGameObjectPath(this);
    }

    public Transform FindTargetTransform()
    {
        return ConstraintStructuredDataUtil.FindTargetTransform(this);
    }

    [Markdown]
    public string ConstraintInfo = "Constraint Info will appear here";

    protected override void OnUpdate()
    {
        base.OnUpdate();
        updateConstraintInfo();
    }

    private void updateConstraintInfo()
    {
        List<string> infoLines = new List<string>
        {
            "Asset Id:" + Asset?.IdString,
            "GameObject Id: " + FindTargetTransform()?.gameObject.GetInstanceID() 
        };
        string newInfo = string.Join("<br>", infoLines);
        SetDataInput(nameof(ConstraintInfo), newInfo, broadcast: true);
    }
}
