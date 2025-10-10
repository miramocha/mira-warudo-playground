using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Warudo.Core;
using Warudo.Core.Attributes;
using Warudo.Core.Data;
using Warudo.Core.Data.Models;
using Warudo.Core.Scenes;
using Warudo.Core.Utils;
using Warudo.Plugins.Core.Assets;
using Warudo.Plugins.Core.Assets.Character;
using Warudo.Plugins.Core.Assets.Mixins;
using Warudo.Plugins.Core.Utils;
using Axis = UnityEngine.Animations.Axis;
using Description = Warudo.Core.Attributes.DescriptionAttribute;
using IConstraint = UnityEngine.Animations.IConstraint;

namespace Warudo.Plugins.Scene.Assets
{
    public abstract class AUnityConstraintAsset : ADebuggableAsset
    {
        protected bool Activated = false;
        public IConstraint Constraint;

        [Section("Constraint Parent", UnityConstraintUIOrdering.PARENT_SECTION)]
        [DataInput(UnityConstraintUIOrdering.PARENT_INPUT)]
        [Description("The parent GameObject to apply the constraint to.")]
        public GameObjectAsset Parent;

        [DataInput(UnityConstraintUIOrdering.PARENT_TRANSFORM_PATH_INPUT)]
        [Label("Parent Transform Path")]
        [HiddenIf(nameof(Parent), Is.Null)]
        [AutoComplete("GetParentTransforms", true, "ROOT_TRANSFORM")]
        public string ParentTransformPath;
        public Transform ParentTransform => Parent?.GameObject?.transform.Find(ParentTransformPath);
        public Vector3 ParentRestLocalPosition;
        public Quaternion ParentRestLocalRotation;

        [Section("Constraint Source", UnityConstraintUIOrdering.SOURCE_SECTION)]
        [DataInput(UnityConstraintUIOrdering.SOURCE_INPUT)]
        [Description("The source GameObject to drive the constraint.")]
        public GameObjectAsset Source;

        [DataInput(UnityConstraintUIOrdering.SOURCE_TRANSFORM_PATH_INPUT)]
        [Label("Source Transform Path")]
        [HiddenIf(nameof(Source), Is.Null)]
        [AutoComplete("GetSourceTransforms", true, "ROOT_TRANSFORM")]
        public string SourceTransformPath;
        public Transform SourceTransform => Source?.GameObject?.transform.Find(SourceTransformPath);
        public Vector3 SourceRestLocalPosition;
        public Quaternion SourceRestLocalRotation;

        [Section("Constraint Settings", UnityConstraintUIOrdering.SETTINGS_SECTION)]
        [Trigger(UnityConstraintUIOrdering.CREATE_CONSTRAINT_TRIGGER)]
        [DisabledIf(nameof(DisableCreateConstraintTrigger))]
        [HiddenIf(nameof(HideCreateConstraintTrigger))]
        public void CreateConstraint()
        {
            if (ParentTransform == null)
            {
                throw new Exception("parent not found" + ParentTransformPath);
            }
            ParentRestLocalPosition = ParentTransform.localPosition;
            ParentRestLocalRotation = ParentTransform.localRotation;

            if (SourceTransform == null)
            {
                throw new Exception("source not found" + SourceTransformPath);
            }
            SourceRestLocalPosition = SourceTransform.localPosition;
            SourceRestLocalRotation = SourceTransform.localRotation;

            CreateSpecificConstraint();
            if (Locked)
            {
                ApplyLockedConstraintSettings();
            }
            else
            {
                UpdateConstraintDataInputs();
            }

            Activated = true;

            if (Constraint == null)
            {
                throw new Exception("failed to add constraint component");
            }
        }

        [DataInput(UnityConstraintUIOrdering.LOCK_TRIGGER)]
        [Label("Lock")]
        public bool Locked = false;

        protected virtual void CreateSpecificConstraint()
        {
            throw new NotImplementedException();
        }

        protected virtual void ApplyLockedConstraintSettings()
        {
            throw new NotImplementedException();
        }

        [Trigger(UnityConstraintUIOrdering.DELETE_CONSTRAINT_TRIGGER)]
        [HiddenIf(nameof(HideDeleteConstraintTrigger))]
        public virtual void DeleteConstraint()
        {
            if (Constraint != null)
            {
                UnityEngine.Object.Destroy((UnityEngine.Object)Constraint);
                DebugLog("Constraint deleted.");
                Constraint = null;

                // Reset parent and source to their rest positions
                ResetParent();
                ResetSource();
            }
        }

        [DataInput(UnityConstraintUIOrdering.WEIGHT_INPUT)]
        [Label("Weight")]
        [FloatSlider(0, 1)]
        [HiddenIf(nameof(HideWeight))]
        [DisabledIf(nameof(Locked), Is.True)]
        public float Weight = 1.0f;

        [DataInput(UnityConstraintUIOrdering.POSITION_AT_REST_INPUT)]
        [Label("Position At Rest")]
        [HiddenIf(nameof(HidePositionAtRest))]
        [DisabledIf(nameof(Locked), Is.True)]
        public Vector3 ConstraintPositionAtRest = Vector3.zero;

        [DataInput(UnityConstraintUIOrdering.ROTATION_AT_REST_INPUT)]
        [Label("Rotation At Rest")]
        [HiddenIf(nameof(HideRotationAtRest))]
        [DisabledIf(nameof(Locked), Is.True)]
        public Vector3 ConstraintRotationAtRest = Vector3.zero;

        [Section("Freeze Rotation Axes", UnityConstraintUIOrdering.FREEZE_ROTATION_SECTION)]
        [SectionHiddenIf(nameof(HideFreezeRotationAxes))]
        [DataInput(UnityConstraintUIOrdering.FREEZE_ROTATION_X_INPUT)]
        [Label("X Axis")]
        [DisabledIf(nameof(Locked), Is.True)]
        public bool FreezeRotationX = true;

        [DataInput(UnityConstraintUIOrdering.FREEZE_ROTATION_Y_INPUT)]
        [Label("Y Axis")]
        [DisabledIf(nameof(Locked), Is.True)]
        public bool FreezeRotationY = true;

        [DataInput(UnityConstraintUIOrdering.FREEZE_ROTATION_Z_INPUT)]
        [Label("Z Axis")]
        [DisabledIf(nameof(Locked), Is.True)]
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

        protected virtual bool HideFreezeRotationAxes()
        {
            return true;
        }

        [Section("Freeze Position Axes", UnityConstraintUIOrdering.FREEZE_POSITION_SECTION)]
        [SectionHiddenIf(nameof(HideFreezePositionAxes))]
        [DataInput(UnityConstraintUIOrdering.FREEZE_POSITION_X_INPUT)]
        [Label("X Axis")]
        [DisabledIf(nameof(Locked), Is.True)]
        public bool FreezePositionX = true;

        [DataInput(UnityConstraintUIOrdering.FREEZE_POSITION_Y_INPUT)]
        [Label("Y Axis")]
        [DisabledIf(nameof(Locked), Is.True)]
        public bool FreezePositionY = true;

        [DataInput(UnityConstraintUIOrdering.FREEZE_POSITION_Z_INPUT)]
        [Label("Z Axis")]
        [DisabledIf(nameof(Locked), Is.True)]
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

        protected virtual bool HideFreezePositionAxes()
        {
            return true;
        }

        protected virtual bool HideWeight()
        {
            return Constraint == null;
        }

        protected virtual bool HidePositionAtRest()
        {
            return true;
        }

        protected virtual bool HideRotationAtRest()
        {
            return true;
        }

        public void ResetParent()
        {
            if (ParentTransform != null)
            {
                DebugLog("Resetting parent to rest position.");
                ParentTransform.localPosition = ParentRestLocalPosition;
                ParentTransform.localRotation = ParentRestLocalRotation;
            }
        }

        public void ResetSource()
        {
            if (SourceTransform != null)
            {
                DebugLog("Resetting source to rest position.");
                SourceTransform.localPosition = SourceRestLocalPosition;
                SourceTransform.localRotation = SourceRestLocalRotation;
            }
        }

        public void ClearParent()
        {
            if (ParentTransform != null)
            {
                ParentTransform.localPosition = ParentRestLocalPosition;
                ParentTransform.localRotation = ParentRestLocalRotation;
            }
            SetDataInput(nameof(Parent), null, broadcast: true);
            SetDataInput(nameof(ParentTransformPath), null, broadcast: true);
            ParentRestLocalPosition = Vector3.zero;
            ParentRestLocalRotation = Quaternion.identity;
        }

        public void ClearSource()
        {
            if (SourceTransform != null)
            {
                SourceTransform.localPosition = SourceRestLocalPosition;
                SourceTransform.localRotation = SourceRestLocalRotation;
            }
            SetDataInput(nameof(Source), null, broadcast: true);
            SetDataInput(nameof(SourceTransformPath), null, broadcast: true);
            SourceRestLocalPosition = Vector3.zero;
            SourceRestLocalRotation = Quaternion.identity;
        }

        public bool DisableCreateConstraintTrigger()
        {
            return Parent == null || Source == null;
        }

        public bool HideCreateConstraintTrigger()
        {
            return Constraint != null;
        }

        public bool HideDeleteConstraintTrigger()
        {
            return Constraint == null;
        }

        public async UniTask<AutoCompleteList> GetParentTransforms()
        {
            if (Parent == null)
            {
                return AutoCompleteList.Message("PLEASE_SELECT_THE_PARENT_ASSET_FIRST");
            }

            if (!Parent.Active)
            {
                return AutoCompleteList.Message("SELECTED_PARENT_ASSET_IS_INACTIVE");
            }

            return Transforms.AutoCompleteTransformChildren(Parent.GameObject.transform);
        }

        public async UniTask<AutoCompleteList> GetSourceTransforms()
        {
            if (Source == null)
            {
                return AutoCompleteList.Message("PLEASE_SELECT_THE_PARENT_ASSET_FIRST");
            }

            if (!Source.Active)
            {
                return AutoCompleteList.Message("SELECTED_PARENT_ASSET_IS_INACTIVE");
            }

            return Transforms.AutoCompleteTransformChildren(Source.GameObject.transform);
        }

        protected virtual void ReloadConstraint()
        {
            DeleteConstraint();
            CreateConstraint();
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            SetActive(true);
            // ReloadConstraint();

            if (Parent == null)
                DebugLog("Parent is null");
            if (Source == null)
                DebugLog("Source is null");
            if (Constraint == null)
                DebugLog("Constraint is null");

            Watch<GameObjectAsset>(nameof(Parent), OnParentChanged);
            Watch<GameObjectAsset>(nameof(Source), OnSourceChanged);
            Watch<String>(nameof(ParentTransformPath), OnParentTransformPathChanged);
            Watch<String>(nameof(SourceTransformPath), OnSourceTransformPathChanged);
            Watch<float>(nameof(Weight), OnWeightChanged);
            Watch<Vector3>(nameof(ConstraintPositionAtRest), OnConstraintPositionAtRestChanged);
            Watch<Vector3>(nameof(ConstraintRotationAtRest), OnConstraintRotationAtRestChanged);
            WatchAll(
                new string[]
                {
                    nameof(FreezePositionX),
                    nameof(FreezePositionY),
                    nameof(FreezePositionZ),
                },
                OnConstraintFreezePositionAxesChanged
            );
            WatchAll(
                new string[]
                {
                    nameof(FreezeRotationX),
                    nameof(FreezeRotationY),
                    nameof(FreezeRotationZ),
                },
                OnConstraintFreezeRotationAxesChanged
            );
            WatchAdditionalConstraintInputs();
        }

        protected virtual void WatchAdditionalConstraintInputs() { }

        protected virtual void UpdateConstraintDataInputs() { }

        protected void OnParentTransformPathChanged(string oldPath, string newPath)
        {
            if (Constraint != null)
            {
                DeleteConstraint();
            }
        }

        protected void OnSourceTransformPathChanged(string oldPath, string newPath)
        {
            if (Constraint != null)
            {
                DeleteConstraint();
            }
        }

        protected void OnParentChanged(GameObjectAsset oldParent, GameObjectAsset newParent)
        {
            DebugLog($"Parent changed from {oldParent?.Name} to {newParent?.Name}");
            SetDataInput(nameof(ParentTransformPath), null, broadcast: true);
            if (Constraint != null)
            {
                DeleteConstraint();
            }
        }

        protected void OnSourceChanged(GameObjectAsset oldSource, GameObjectAsset newSource)
        {
            DebugLog($"Source changed from {oldSource?.Name} to {newSource?.Name}");
            SetDataInput(nameof(SourceTransformPath), null, broadcast: true);
            if (Constraint != null)
            {
                DeleteConstraint();
            }
        }

        protected void OnWeightChanged(float oldWeight, float newWeight)
        {
            Constraint.weight = newWeight;
        }

        protected virtual void OnConstraintFreezeRotationAxesChanged() { }

        protected virtual void OnConstraintFreezePositionAxesChanged() { }

        protected virtual void OnConstraintPositionAtRestChanged(Vector3 oldPos, Vector3 newPos) { }

        protected virtual void OnConstraintRotationAtRestChanged(
            Vector3 oldValue,
            Vector3 newValue
        ) { }

        protected const string DEFAULT_PARENT_TRANSFORM_INFO =
            "Parent Transform info will appear here.";
        protected const string DEFAULT_SOURCE_TRANSFORM_INFO =
            "Source Transform info will appear here.";
        protected const string DEFAULT_CONSTRAINT_INFO = "Constraint info will appear here.";

        [Markdown(order: 2503, primary: true)]
        public string ParentTransformHeader = "Parent Transform";

        [Markdown(2504)]
        public string ParentTransformInfo = DEFAULT_PARENT_TRANSFORM_INFO;

        [Markdown(order: 2505, primary: true)]
        public string SourceTransformHeader = "Source Transform";

        [Markdown(2506)]
        public string SourceTransformInfo = DEFAULT_SOURCE_TRANSFORM_INFO;

        [Markdown(order: 2507, primary: true)]
        public string ConstraintInfoHeader = "Constraint Info";

        [Markdown(2508)]
        public string ConstraintInfo = DEFAULT_CONSTRAINT_INFO;

        protected virtual void UpdateConstraintDebugInfo()
        {
            if (ParentTransform != null)
            {
                List<String> transformInfoList = new List<String>
                {
                    "Name: " + ParentTransform.name,
                    "Path: " + ParentTransformPath,
                    "World Position: " + ParentTransform.position.ToString("F3"),
                    "World Rotation: " + ParentTransform.rotation.eulerAngles.ToString("F3"),
                    "Local Position: " + ParentTransform.localPosition.ToString("F3"),
                    "Local Rotation: " + ParentTransform.localRotation.eulerAngles.ToString("F3"),
                };
                string newParentTransformInfo = String.Join("<br>", transformInfoList.ToArray());
                SetDataInput(nameof(ParentTransformInfo), newParentTransformInfo, broadcast: true);
            }
            else
            {
                SetDataInput(
                    nameof(ParentTransformInfo),
                    DEFAULT_PARENT_TRANSFORM_INFO,
                    broadcast: true
                );
            }

            if (SourceTransform != null)
            {
                List<String> transformInfoLines = new List<String>
                {
                    "Name: " + SourceTransform.name,
                    "Path: " + SourceTransformPath,
                    "World Position: " + SourceTransform.position.ToString("F3"),
                    "World Rotation: " + SourceTransform.rotation.eulerAngles.ToString("F3"),
                    "Local Position: " + SourceTransform.localPosition.ToString("F3"),
                    "Local Rotation: " + SourceTransform.localRotation.eulerAngles.ToString("F3"),
                };
                string newSourceTransformInfo = String.Join("<br>", transformInfoLines.ToArray());
                SetDataInput(nameof(SourceTransformInfo), newSourceTransformInfo, broadcast: true);
            }
            else
            {
                SetDataInput(
                    nameof(SourceTransformInfo),
                    DEFAULT_SOURCE_TRANSFORM_INFO,
                    broadcast: true
                );
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (DebugMode)
            {
                UpdateConstraintDebugInfo();
            }
        }

        protected override void OnDestroy()
        {
            DebugMode = false;
            DeleteConstraint();
            base.Destroy();
        }
    }
}
