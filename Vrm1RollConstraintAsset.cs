using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UniVRM10;
using Warudo.Core;
using Warudo.Core.Attributes;
using Warudo.Core.Data;
using Warudo.Core.Scenes;
using Warudo.Plugins.Core.Assets;
using Warudo.Plugins.Core.Assets.Character;
using Warudo.Plugins.Core.Assets.Mixins;
using Warudo.Plugins.Core.Utils;
using static RootMotion.Demos.Turret;

namespace Warudo.Plugins.Scene.Assets
{
    [AssetType(
        Id = "9ebe52e9-c968-4026-8902-0c9b61e98deb",
        Title = "VRM1 Roll Constraint",
        Category = "VRM1 Constraints"
    )]
    public class Vrm1RollConstraintAsset : Asset
    {
        [DataInput(0)]
        [Label("ENABLED")]
        public bool Enabled = true;

        [DataInput(1)]
        public UniGLTF.Extensions.VRMC_node_constraint.RollAxis RollAxis = UniGLTF
            .Extensions
            .VRMC_node_constraint
            .RollAxis
            .Y;

        [DataInput(10)]
        public GameObjectAsset Target;

        [DataInput(11)]
        [Label("Target Transform Path")]
        [AutoComplete("GetTargetTransforms", true, "ROOT_TRANSFORM")]
        public string TargetTransformPath;

        [DataInput(20)]
        public GameObjectAsset Source;

        [DataInput(21)]
        [Label("Source Transform Path")]
        [AutoComplete("GetSourceTransforms", true, "ROOT_TRANSFORM")]
        public string SourceTransformPath;


        [Trigger]
        [DisabledIf(nameof(ShowCreateConstraintTrigger))]
        public void CreateConstraint() { }

        protected override void OnCreate()
        {
            base.OnCreate();
            // Watch<GameObject>(Target, OnTargetChanged);
        }

        // protected void OnTargetChanged(GameObject from, GameObject to){
        //     Context.Service.PromptMessage(from.name, to.name);
        // }

        public bool ShowCreateConstraintTrigger() {
            return Target == null || Source == null;
        }

        public async UniTask<AutoCompleteList> GetTargetTransforms()
        {
            if (Target == null)
            {
                return AutoCompleteList.Message("PLEASE_SELECT_THE_PARENT_ASSET_FIRST");
            }

            if (!Target.Active)
            {
                return AutoCompleteList.Message("SELECTED_PARENT_ASSET_IS_INACTIVE");
            }

            return Transforms.AutoCompleteTransformChildren((Target is CharacterAsset characterAsset) ? characterAsset.MainTransform : Target.GameObject.transform);
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

            return Transforms.AutoCompleteTransformChildren((Source is CharacterAsset characterAsset) ? characterAsset.MainTransform : Source.GameObject.transform);
        }
    }
}
