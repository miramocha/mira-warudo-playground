using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
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
using Warudo.Plugins.Core;
using Warudo.Plugins.Core.Assets;
using Warudo.Plugins.Core.Utils;
using ParentConstraint = UnityEngine.Animations.ParentConstraint;

namespace Warudo.Plugins.Scene.Assets;

public class ML_UnityParentConstraintStructuredData
    : StructuredData<ML_UnityParentConstraintsManagerAsset>,
        ML_IGameObjectComponentStructuredData,
        ICollapsibleStructuredData
{
    public string GetHeader() => Asset?.Name + '/' + GameObjectPath;

    public ParentConstraint Constraint
    {
        get { return (ParentConstraint)FindTargetTransform()?.GetComponent<ParentConstraint>(); }
    }

    [Trigger]
    [Label("Delete")]
    public async void ConfirmDelete()
    {
        bool confirmed = await Context.Service.PromptConfirmation(
            "Confirmation",
            "Deleting this constraint?"
        );

        ML_DebugUtil.ToastDebug("Comfirming Deletion");
        if (confirmed == true)
        {
            ML_DebugUtil.ToastDebug("Deletion Confirmed");
            Parent.DeleteConstraintStructuredData(this);
        }
    }

    [Trigger]
    [Label("Refresh")]
    public void RefreshConstraint()
    {
        CreateConstraint();
    }

    [DataInput]
    [Label("ASSET")]
    [Disabled]
    public GameObjectAsset Asset;

    public GameObjectAsset GetAsset()
    {
        return Asset;
    }

    [DataInput]
    [AutoComplete("AutoCompleteGameObjectPath", false, "")]
    [Label("GAMEOBJECT_PATH")]
    [Disabled]
    public string GameObjectPath;

    public string GetGameObjectPath()
    {
        return GameObjectPath;
    }

    [Section("Settings")]
    [DataInput]
    [Label("Weight")]
    [FloatSlider(0, 1)]
    public float Weight = 1.0f;

    [DataInput]
    [Label("Position At Rest")]
    [HiddenIf(nameof(HideRestTransformEditing))]
    public Vector3 ConstraintPositionAtRest = Vector3.zero;

    [DataInput]
    [Label("Rotation At Rest")]
    [HiddenIf(nameof(HideRestTransformEditing))]
    public Vector3 ConstraintRotationAtRest = Vector3.zero;

    protected bool HideRestTransformEditing()
    {
        return !ML_FeatureFlags.PARENT_CONSTRAINT_REST_TRANSFORM_EDITING;
    }

    [Section("Freeze Rotation Axes")]
    [DataInput]
    [Label("X Axis")]
    public bool FreezeRotationX = true;

    [DataInput]
    [Label("Y Axis")]
    public bool FreezeRotationY = true;

    [DataInput]
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

    [Section("Freeze Position Axes")]
    [DataInput]
    [Label("X Axis")]
    public bool FreezePositionX = true;

    [DataInput]
    [Label("Y Axis")]
    public bool FreezePositionY = true;

    [DataInput]
    [Label("Z Axis")]
    public bool FreezePositionZ = true;
    public Axis FreezePositionAxes
    {
        get
        {
            Axis axes = Axis.None;
            if (FreezePositionX)
                axes |= Axis.X;
            if (FreezePositionY)
                axes |= Axis.Y;
            if (FreezePositionZ)
                axes |= Axis.Z;
            return axes;
        }
    }

    public string GameObjectComponentPathID
    {
        get { return ML_GameObjectComponentStructuredDataUtil.GetGameObjectComponentPathID(this); }
    }

    [Section("Constraint Sources")]
    [DataInput]
    [Label("Sources")]
    public ML_UnityConstraintSourceStructuredData[] UnityConstraintSourceStructuredDataList;

    public async UniTask<AutoCompleteList> AutoCompleteGameObjectPath()
    {
        return await ML_GameObjectComponentStructuredDataUtil.AutoCompleteGameObjectPath(this);
    }

    public Transform FindTargetTransform()
    {
        return ML_GameObjectComponentStructuredDataUtil.FindTargetTransform(this);
    }

    protected override void OnCreate()
    {
        base.OnCreate();
        ML_DebugUtil.ToastDebug("On create");
        WatchAsset(
            nameof(Asset),
            delegate
            {
                CreateConstraint();
            }
        );
        Watch<GameObjectAsset>(
            nameof(Asset),
            delegate
            {
                CreateConstraint();
            }
        );
        Watch<string>(
            nameof(GameObjectPath),
            delegate
            {
                CreateConstraint();
            }
        );
        Watch<float>(
            nameof(Weight),
            delegate(float oldValue, float newValue)
            {
                Constraint.weight = newValue;
            }
        );
        Watch<ML_UnityConstraintSourceStructuredData[]>(
            nameof(UnityConstraintSourceStructuredDataList),
            delegate
            {
                ML_DebugUtil.ToastDebug("Parent source change received.");
                ApplyConstraintSources();
            }
        );
        WatchAll(
            new[] { nameof(FreezePositionX), nameof(FreezePositionY), nameof(FreezePositionZ) },
            delegate
            {
                if (Constraint != null)
                {
                    Constraint.translationAxis = FreezePositionAxes;
                }
            }
        );
        WatchAll(
            new[] { nameof(FreezeRotationX), nameof(FreezeRotationY), nameof(FreezeRotationZ) },
            delegate
            {
                if (Constraint != null)
                {
                    Constraint.rotationAxis = FreezeRotationAxes;
                }
            }
        );
    }

    public void CreateConstraint()
    {
        if (FindTargetTransform() == null)
        {
            ML_DebugUtil.ToastDebug("Transform is null");
            return;
        }

        if (FindTargetTransform() != null && Constraint == null)
        {
            ML_DebugUtil.ToastDebug("Adding Constraint");
            FindTargetTransform().gameObject.AddComponent(typeof(ParentConstraint));
            Constraint.enabled = true;
            Constraint.constraintActive = true;
            originalTransformData.CopyFromLocalTransform(FindTargetTransform());
        }

        ApplyConstraintSources();
    }

    protected override void OnDestroy()
    {
        if (Constraint != null)
        {
            ML_DebugUtil.ToastDebug("Cleaning up...");
            originalTransformData.ApplyAsLocalTransform(FindTargetTransform());
            UnityEngine.Object.Destroy((UnityEngine.Object)Constraint);
        }
        base.OnDestroy();
    }

    private TransformData originalTransformData = StructuredData.Create<TransformData>();

    public void ApplyConstraintSources()
    {
        List<UnityEngine.Animations.ConstraintSource> sources = new List<ConstraintSource>();

        if (UnityConstraintSourceStructuredDataList.Length == 0)
        {
            ML_DebugUtil.ToastDebug("Source list is empty");
            Constraint?.SetSources(sources);
            originalTransformData.ApplyAsLocalTransform(Constraint.gameObject.transform);
            return;
        }

        ML_DebugUtil.ToastDebug("Source list is NOT empty");

        foreach (
            ML_UnityConstraintSourceStructuredData sourceStructuredData in UnityConstraintSourceStructuredDataList
        )
        {
            Transform sourceTransform = sourceStructuredData.FindTargetTransform();

            if (sourceTransform != null)
            {
                ML_DebugUtil.ToastDebug("Adding source to list");
                UnityEngine.Animations.ConstraintSource source =
                    new UnityEngine.Animations.ConstraintSource();
                source.weight = sourceStructuredData.Weight;
                source.sourceTransform = sourceTransform;
                sources.Add(source);
            }
            else
            {
                ML_DebugUtil.ToastDebug(
                    "Source transform not found " + sourceStructuredData.Asset.Name
                );
            }
        }

        Constraint.SetSources(sources);
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        updateDebugInfo();
    }

    [Markdown]
    public string DebugInfo = "Debug Info will appear here";

    private void updateDebugInfo()
    {
        List<string> infoLines = new List<string>
        {
            "Id: " + Id,
            "Asset Id: " + Asset?.IdString,
            "GameObject Id: " + FindTargetTransform()?.gameObject.GetInstanceID(),
            "Original Transform Data: " + originalTransformData,
            "Constraint: " + Constraint,
            "Constrant Source Count: " + (Constraint?.sourceCount ?? 0),
            "Constraint Source SD: " + UnityConstraintSourceStructuredDataList.Length,
            "Parent ID: " + Parent?.IdString,
        };
        string newInfo = string.Join("<br>", infoLines);
        SetDataInput(nameof(DebugInfo), newInfo, broadcast: true);
    }
}
