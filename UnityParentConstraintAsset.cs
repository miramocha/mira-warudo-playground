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
        public override bool HideFreezeRotationAxes()
        {
            return Constraint == null;
        }

        public override bool HideFreezePositionAxes()
        {
            return Constraint == null;
        }

        protected override void CreateSpecificConstraint()
        {
            ConstraintSource constraintSource = new ConstraintSource();
            constraintSource.sourceTransform = SourceTransform;
            constraintSource.weight = Weight;

            ParentTransform.gameObject.AddComponent(typeof(ParentConstraint));

            Constraint = ParentTransform.GetComponent<ParentConstraint>();
            ParentConstraint parentConstraint = (ParentConstraint)Constraint;
            parentConstraint.weight = Weight;
            parentConstraint.SetSources(new List<ConstraintSource> { constraintSource });
            parentConstraint.constraintActive = true;
            parentConstraint.enabled = true;

            DebugLog("Rotation at rest:" + parentConstraint.rotationAtRest.ToString());
            DebugLog("Rotation at rest local:" + ParentRestLocalRotation.ToString());
        }

        public override void UpdateConstraintDebugInfo()
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
                    "Translation At Rest: " + parentConstraint.translationAtRest.ToString(),
                    "Rotation At Rest: " + parentConstraint.rotationAtRest.ToString(),
                    "Locked: " + parentConstraint.locked
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
