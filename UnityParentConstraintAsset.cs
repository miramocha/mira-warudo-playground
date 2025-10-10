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
using ConstraintSource = UnityEngine.Animations.ConstraintSource;
using ParentConstraint = UnityEngine.Animations.ParentConstraint;

namespace Warudo.Plugins.Scene.Assets
{
    [AssetType(
        Id = "mira-parent-constraint",
        Title = "Parent Constraint",
        Category = "Unity Constraints"
    )]
    public class UnityParentConstraintAsset : AUnityConstraintAsset
    {
        protected override bool HideFreezeRotationAxes()
        {
            return Constraint == null;
        }

        protected override bool HideFreezePositionAxes()
        {
            return Constraint == null;
        }

        protected override bool HidePositionAtRest()
        {
            return Constraint == null;
        }

        protected override bool HideRotationAtRest()
        {
            return Constraint == null;
        }

        protected override void OnConstraintPositionAtRestChanged(
            Vector3 oldValue,
            Vector3 newValue
        )
        {
            if (Constraint != null)
            {
                ParentConstraint parentConstraint = (ParentConstraint)Constraint;
                parentConstraint.translationAtRest = newValue;
                // DebugLog("Set position at rest to: " + newValue.ToString());
            }
        }

        protected override void OnConstraintRotationAtRestChanged(
            Vector3 oldValue,
            Vector3 newValue
        )
        {
            if (Constraint != null)
            {
                ParentConstraint parentConstraint = (ParentConstraint)Constraint;
                parentConstraint.rotationAtRest = newValue;
                // DebugLog("Set rotation at rest to: " + newValue.ToString());
            }
        }

        protected override void CreateSpecificConstraint()
        {
            ConstraintSource constraintSource = new ConstraintSource();
            constraintSource.sourceTransform = SourceTransform;
            constraintSource.weight = 1f; // Full weight from source for now

            ParentTransform.gameObject.AddComponent(typeof(ParentConstraint));
            Constraint = ParentTransform.GetComponent<ParentConstraint>();
            ParentConstraint parentConstraint = (ParentConstraint)Constraint;
            parentConstraint.SetSources(new List<ConstraintSource> { constraintSource });
            parentConstraint.enabled = true;
            parentConstraint.constraintActive = true;

            DebugLog("Rotation at rest local:" + ParentRestLocalRotation.ToString());
            DebugLog("Position at rest local:" + ParentRestLocalPosition.ToString());
        }

        protected override void WatchAdditionalConstraintInputs() { }

        protected override void UpdateConstraintDataInputs()
        {
            ParentConstraint parentConstraint = (ParentConstraint)Constraint;

            SetDataInput(nameof(Weight), parentConstraint.weight, broadcast: true);
            SetDataInput(
                nameof(ConstraintPositionAtRest),
                parentConstraint.translationAtRest,
                broadcast: true
            );
            SetDataInput(
                nameof(ConstraintRotationAtRest),
                parentConstraint.rotationAtRest,
                broadcast: true
            );

            DebugLog("Position Axis: " + parentConstraint.ToString());
        }

        protected override void UpdateConstraintDebugInfo()
        {
            base.UpdateConstraintDebugInfo();

            if (Constraint != null)
            {
                ParentConstraint parentConstraint = (ParentConstraint)Constraint;
                List<string> constraintInfoLines = new List<string>
                {
                    "Constraint Active: " + parentConstraint.constraintActive,
                    "Enabled: " + parentConstraint.enabled,
                    "Weight: " + parentConstraint.weight,
                    "Position At Rest: " + parentConstraint.translationAtRest.ToString(),
                    "Rotation At Rest: " + parentConstraint.rotationAtRest.ToString(),
                    "Locked: " + parentConstraint.locked,
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
