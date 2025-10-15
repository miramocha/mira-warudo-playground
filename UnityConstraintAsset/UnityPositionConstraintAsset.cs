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
using PositionConstraint = UnityEngine.Animations.PositionConstraint;

namespace Warudo.Plugins.Scene.Assets
{
    // [AssetType(
    //     Id = "mira-position-constraint",
    //     Title = "Position Constraint",
    //     Category = "Unity Constraints"
    // )]
    public class UnityPositionConstraintAsset : Asset
    {
        [Markdown(1000, primary: true)]
        public string Title = "Parent Constraint";

        protected override void OnCreate()
        {
            base.OnCreate();
            SetActive(true);
            // throw new NotImplementedException();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            // throw new NotImplementedException();
            // base.OnDestroy();
        }
    }
}