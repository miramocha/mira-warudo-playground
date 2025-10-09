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
        public override bool HideFreezeRotationAxes()
        {
            return Constraint == null;
        }

        public override void CreateConstraint()
        {
            base.CreateConstraint();
            ConstraintSource constraintSource = new ConstraintSource();
            constraintSource.sourceTransform = SourceTransform;
            constraintSource.weight = Weight;

            ParentTransform.gameObject.AddComponent(typeof(RotationConstraint));

            Constraint = ParentTransform.GetComponent<RotationConstraint>();
            RotationConstraint rotationConstraint = (RotationConstraint)Constraint;
            rotationConstraint.weight = Weight;
            rotationConstraint.SetSources(new List<ConstraintSource> { constraintSource });
            rotationConstraint.constraintActive = true;
            rotationConstraint.enabled = true;
            // Enabled = true;

            DebugLog("Rotation at rest:" + rotationConstraint.rotationAtRest.ToString());
            DebugLog("Rotation at rest local:" + ParentRestLocalRotation.ToString());

            if (Constraint == null)
            {
                throw new Exception("failed to add constraint component");
            }
            // DebugToast("Constraint created.");
        }
    }
}
