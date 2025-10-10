using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using Warudo.Core;
using Warudo.Core.Attributes;
using Warudo.Core.Data;
using Warudo.Core.Scenes;
using Warudo.Plugins.Core.Assets;
using Warudo.Plugins.Core.Assets.Character;
using Warudo.Plugins.Core.Assets.Mixins;
using Warudo.Plugins.Core.Utils;
using Axis = UnityEngine.Animations.Axis;
using ConstraintSource = UnityEngine.Animations.ConstraintSource;
using RotationConstraint = UnityEngine.Animations.RotationConstraint;

namespace Warudo.Plugins.Scene.Assets
{
    [AssetType(
        Id = "mira-rotation-constraint",
        Title = "Rotation Constraint",
        Category = "Unity Constraints"
    )]
    public class UnityRotationConstraintAsset : AUnityConstraintAsset
    {
        [DataInput(1021)]
        [Label("Rotation Offset")]
        [HiddenIf(nameof(HideRotationOffset))]
        public Vector3 ConstraintRotationOffset = Vector3.zero;

        [Trigger(1022)]
        [Label("Reset Rotation Offset")]
        [HiddenIf(nameof(HideRotationOffset))]
        public void ResetConstraintRotationOffset()
        {
            Vector3 offset =
                ParentRestLocalRotation.eulerAngles - ParentTransform.localRotation.eulerAngles;
            DebugLog(
                "ParentTransform rotation: " + ParentTransform.localRotation.eulerAngles.ToString()
            );
            DebugLog("ParentRestLocalRotation: " + ParentRestLocalRotation.eulerAngles.ToString());
            DebugLog("Initial rotation offset set to: " + offset.ToString());
            SetDataInput(nameof(ConstraintRotationOffset), offset, broadcast: true);

            RotationConstraint rotationConstraint = (RotationConstraint)Constraint;
            rotationConstraint.rotationOffset = offset;
        }

        protected override bool HideFreezeRotationAxes()
        {
            return Constraint == null;
        }

        protected override bool HideRotationAtRest()
        {
            return Constraint == null;
        }

        protected bool HideRotationOffset()
        {
            return Constraint == null;
        }

        protected override void WatchAdditionalConstraintInputs()
        {
            Watch<Vector3>(nameof(ConstraintRotationOffset), OnConstraintRotationOffsetChanged);
        }

        protected override void OnConstraintRotationAtRestChanged(
            Vector3 oldValue,
            Vector3 newValue
        )
        {
            if (Constraint != null)
            {
                RotationConstraint rotationConstraint = (RotationConstraint)Constraint;
                rotationConstraint.rotationAtRest = newValue;
                // DebugLog("Set rotation at rest to: " + newValue.ToString());
            }
        }

        protected void OnConstraintRotationOffsetChanged(Vector3 oldValue, Vector3 newValue)
        {
            if (Constraint != null)
            {
                RotationConstraint rotationConstraint = (RotationConstraint)Constraint;
                rotationConstraint.rotationOffset = newValue;
                // DebugLog("Set rotation offset to: " + newValue.ToString());
            }
        }

        protected override void OnConstraintFreezeRotationAxesChanged()
        {
            RotationConstraint rotationConstraint = (RotationConstraint)Constraint;
            rotationConstraint.rotationAxis = FreezeRotationAxes;
            DebugLog("Set rotation axis to: " + FreezeRotationAxes.ToString());
        }

        protected override void CreateSpecificConstraint()
        {
            ConstraintSource constraintSource = new ConstraintSource();
            constraintSource.sourceTransform = SourceTransform;
            constraintSource.weight = 1f; // Full weight from source for now

            ParentTransform.gameObject.AddComponent(typeof(RotationConstraint));
            Constraint = ParentTransform.GetComponent<RotationConstraint>();
            RotationConstraint rotationConstraint = (RotationConstraint)Constraint;
            rotationConstraint.SetSources(new List<ConstraintSource> { constraintSource });

            rotationConstraint.enabled = true;
            rotationConstraint.constraintActive = true;

            DebugLog("Parent local rotation:" + ParentTransform.localRotation.ToString());
            DebugLog("Rotation at rest local:" + ParentRestLocalRotation.ToString());

            rotationConstraint.rotationOffset =
                ParentTransform.localRotation.eulerAngles - ParentRestLocalRotation.eulerAngles;
        }

        protected override void UpdateConstraintDataInputs()
        {
            RotationConstraint rotationConstraint = (RotationConstraint)Constraint;

            SetDataInput(nameof(Weight), rotationConstraint.weight, broadcast: true);
            SetDataInput(
                nameof(ConstraintRotationAtRest),
                rotationConstraint.rotationAtRest,
                broadcast: true
            );
            SetDataInput(
                nameof(ConstraintRotationOffset),
                rotationConstraint.rotationOffset,
                broadcast: true
            );

            Axis constraintFreezeRotationAxes = rotationConstraint.rotationAxis;
            SetDataInput(
                nameof(FreezeRotationX),
                constraintFreezeRotationAxes.HasFlag(Axis.X),
                broadcast: true
            );
            SetDataInput(
                nameof(FreezeRotationY),
                constraintFreezeRotationAxes.HasFlag(Axis.Y),
                broadcast: true
            );
            SetDataInput(
                nameof(FreezeRotationZ),
                constraintFreezeRotationAxes.HasFlag(Axis.Z),
                broadcast: true
            );
        }

        protected override void UpdateConstraintDebugInfo()
        {
            base.UpdateConstraintDebugInfo();

            if (Constraint != null)
            {
                RotationConstraint rotationConstraint = (RotationConstraint)Constraint;
                List<string> constraintInfoLines = new List<string>
                {
                    "Constraint Active: " + rotationConstraint.constraintActive,
                    "Enabled: " + rotationConstraint.enabled,
                    "Weight: " + rotationConstraint.weight,
                    "Rotation At Rest: " + rotationConstraint.rotationAtRest.ToString(),
                    "Locked: " + rotationConstraint.locked,
                    "Axes Frozen (Rotation): " + rotationConstraint.rotationAxis.ToString(),
                };
                string newConstraintInfo = String.Join("<br>", constraintInfoLines);
                SetDataInput(nameof(ConstraintInfo), newConstraintInfo, broadcast: true);
            }
            else
            {
                SetDataInput(nameof(ConstraintInfo), "No Constraint", broadcast: true);
            }
        }
    }
}
