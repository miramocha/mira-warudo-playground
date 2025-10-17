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
using ParentConstraint = UnityEngine.Animations.ParentConstraint;

namespace Warudo.Plugins.Scene.Assets;

public class ConstraintStructuredData
    : StructuredData<UnityParentConstraintsManagerAsset>,
        IGameObjectComponentStructuredData,
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

        DebugToast("Comfirming Deletion");
        if (confirmed == true)
        {
            DebugToast("Deletion Confirmed");
            Parent.DeleteConstraintStructuredData(this);
        }
    }

    [Trigger]
    [Label("Reset Rest Transform")]
    [HiddenIf(nameof(HideRestTransformInput))]
    public void ResetRestTransform()
    {
        if (!HideRestTransformInput())
        {
            ConstraintPositionAtRest = OriginalRestTransformData.Position;
            ConstraintRotationAtRest = OriginalRestTransformData.Rotation;
            Constraint.translationAtRest = ConstraintPositionAtRest;
            Constraint.rotationAtRest = ConstraintRotationAtRest;
            Broadcast();
        }
    }

    public void ResetTransform()
    {
        OriginalTransformData.ApplyAsLocalTransform(FindTargetTransform());
    }

    [DataInput]
    public string GroupName = "Default";

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
    [HiddenIf(nameof(HideRestTransformInput))]
    public Vector3 ConstraintPositionAtRest = Vector3.zero;

    [DataInput]
    [Label("Rotation At Rest")]
    [HiddenIf(nameof(HideRestTransformInput))]
    public Vector3 ConstraintRotationAtRest = Vector3.zero;

    // TO DO: Fix this
    protected bool HideRestTransformInput()
    {
        return true;
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
        get { return GameObjectComponentStructuredDataUtil.GetGameObjectComponentPathID(this); }
    }

    [Section("Constraint Sources", UnityConstraintUIOrdering.WEIGHT_INPUT + 1)]
    [DataInput]
    [Label("Sources")]
    public ConstraintSourceStructuredData[] ConstraintSourceStructuredDataList;

    public async UniTask<AutoCompleteList> AutoCompleteGameObjectPath()
    {
        return await GameObjectComponentStructuredDataUtil.AutoCompleteGameObjectPath(this);
    }

    public Transform FindTargetTransform()
    {
        return GameObjectComponentStructuredDataUtil.FindTargetTransform(this);
    }

    [Markdown]
    public string ConstraintInfo = "Constraint Info will appear here";

    protected override void OnCreate()
    {
        base.OnCreate();
        DebugToast("On create");
        WatchAll(
            new[] { nameof(FreezePositionX), nameof(FreezePositionY), nameof(FreezePositionZ) },
            delegate
            {
                Constraint.translationAxis = FreezePositionAxes;
            }
        );
        WatchAll(
            new[] { nameof(FreezeRotationX), nameof(FreezeRotationY), nameof(FreezeRotationZ) },
            delegate
            {
                Constraint.rotationAxis = FreezeRotationAxes;
            }
        );
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
        Watch<ConstraintSourceStructuredData[]>(
            nameof(ConstraintSourceStructuredDataList),
            delegate
            {
                DebugToast("Source changed");
                ApplyConstraintSources();
            }
        );

        if (!HideRestTransformInput())
        {
            Watch<Vector3>(
                nameof(ConstraintPositionAtRest),
                delegate(Vector3 oldValue, Vector3 newValue)
                {
                    Constraint.translationAtRest = ConstraintPositionAtRest;
                }
            );
            Watch<Vector3>(
                nameof(ConstraintRotationAtRest),
                delegate(Vector3 oldValue, Vector3 newValue)
                {
                    Constraint.rotationAtRest = ConstraintRotationAtRest;
                }
            );
        }
    }

    public void CreateConstraint()
    {
        if (FindTargetTransform() == null)
        {
            DebugToast("Transform is null");
        }
        OriginalTransformData.CopyFromLocalTransform(FindTargetTransform());

        if (FindTargetTransform() != null && Constraint == null)
        {
            DebugToast("Adding Constraint");

            FindTargetTransform().gameObject.AddComponent(typeof(ParentConstraint));
            Constraint.enabled = true;
            Constraint.constraintActive = true;
            OriginalRestTransformData.CopyFromLocalTransform(Constraint.gameObject.transform);

            ConstraintPositionAtRest = Constraint.translationAtRest;
            ConstraintRotationAtRest = Constraint.rotationAtRest;
            Broadcast();
            ApplyConstraintSources();
        }
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        updateConstraintDebugInfo();
    }

    protected override void OnDestroy()
    {
        if (Constraint != null)
        {
            DebugToast("Cleaning up...");
            ResetRestTransform();
            UnityEngine.Object.Destroy((UnityEngine.Object)Constraint);
            ResetTransform();
        }
        base.OnDestroy();
    }

    [DataInput]
    [Hidden]
    [Disabled]
    protected TransformData OriginalTransformData = StructuredData.Create<TransformData>();

    // [DataInput]
    // [Hidden]
    // [Disabled]
    protected TransformData OriginalRestTransformData = StructuredData.Create<TransformData>();

    public void ApplyConstraintSources()
    {
        List<UnityEngine.Animations.ConstraintSource> sources = new List<ConstraintSource>();

        if (ConstraintSourceStructuredDataList.Length == 0)
        {
            Constraint.SetSources(sources);
            ResetRestTransform();
            ResetTransform();
            return;
        }

        foreach (
            ConstraintSourceStructuredData sourceStructuredData in ConstraintSourceStructuredDataList
        )
        {
            Transform sourceTransform = sourceStructuredData.FindTargetTransform();

            if (sourceTransform != null)
            {
                UnityEngine.Animations.ConstraintSource source =
                    new UnityEngine.Animations.ConstraintSource();
                source.weight = sourceStructuredData.Weight;
                source.sourceTransform = sourceTransform;
                sources.Add(source);
            }
        }

        Constraint.SetSources(sources);
    }

    private void updateConstraintDebugInfo()
    {
        List<string> infoLines = new List<string>
        {
            "Entity Id: " + Id,
            "Asset Id: " + Asset?.IdString,
            "GameObject Id: " + FindTargetTransform()?.gameObject.GetInstanceID(),
            "Original Transform Data: " + OriginalTransformData,
            "Constraint: " + Constraint,
            "Constrant Source Count: " + (Constraint?.sourceCount ?? 0),
            "Parent ID: " + Parent?.IdString,
        };

        if (!HideRestTransformInput())
        {
            infoLines.AddRange(
                new List<string>
                {
                    "Rest Position: " + ConstraintPositionAtRest,
                    "Rest Rotation: " + ConstraintRotationAtRest,
                }
            );
        }
        string newInfo = string.Join("<br>", infoLines);
        SetDataInput(nameof(ConstraintInfo), newInfo, broadcast: true);
    }

    public void DebugToast(string msg)
    {
        Context.Service.Toast(Warudo.Core.Server.ToastSeverity.Info, "Debug", msg);
    }
}
