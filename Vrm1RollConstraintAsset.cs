using System;
using Cysharp.Threading.Tasks;
using RootMotion;
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
        public UniVRM10.Vrm10RollConstraint Constraint;

        // TODO: create abstract class for constraints

        [DataInput(0)]
        [Label("ENABLED")]
        public bool Enabled = true;

        [Section("Constraint Parent", 9)]
        [DataInput(10)]
        public GameObjectAsset Parent;

        [DataInput(11)]
        [Label("Parent Transform Path")]
        [AutoComplete("GetParentTransforms", true, "ROOT_TRANSFORM")]
        public string ParentTransformPath;

        [Section("Constraint Source", 19)]
        [DataInput(20)]
        public GameObjectAsset Source;

        [DataInput(21)]
        [Label("Source Transform Path")]
        [AutoComplete("GetSourceTransforms", true, "ROOT_TRANSFORM")]
        public string SourceTransformPath;

        [Section("Constraint Attributes", 29)]
        [DataInput(30)]
        public UniGLTF.Extensions.VRMC_node_constraint.RollAxis RollAxis = UniGLTF
            .Extensions
            .VRMC_node_constraint
            .RollAxis
            .Y;

        [DataInput(31)]
        [FloatSlider(0, 1)]
        public float Weight = 1.0f;

        [Trigger]
        [DisabledIf(nameof(DisableCreateConstraintTrigger))]
        [HiddenIf(nameof(HideCreateConstraintTrigger))]
        public void CreateConstraint()
        {
            // Assume Parent and Source are not null
            var parentTransform = Parent.GameObject.transform.Find(ParentTransformPath);
            var sourceTransform = Source.GameObject.transform.Find(SourceTransformPath);

            if (parentTransform == null)
            {
                throw new Exception("parent not found" + ParentTransformPath);
            }

            if (sourceTransform == null)
            {
                throw new Exception("source not found" + SourceTransformPath);
            }

            parentTransform.gameObject.AddComponent(typeof(Vrm10RollConstraint));
            Constraint = parentTransform.GetComponent<Vrm10RollConstraint>() as Vrm10RollConstraint;

            Context.Service.PromptMessage("Constraint Added", "Test");

            if (Constraint == null)
            {
                throw new Exception("failed to add constraint component");
            }

            Constraint.Source = sourceTransform;
            Constraint.RollAxis = RollAxis;
            Constraint.Weight = Weight;


            //      _constraint =
            // _constraint.Source = sourceTransform;
            // _constraint.RollAxis = RollAxis;
            // _constraint.Weight = Weight;
        }

        [Trigger]
        [HiddenIf(nameof(HideDeleteConstraintTrigger))]
        public void DeleteConstraint()
        {
            UnityEngine.Object.Destroy(Constraint);
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

            return Transforms.AutoCompleteTransformChildren(
                (Parent is CharacterAsset characterAsset)
                    ? characterAsset.MainTransform
                    : Parent.GameObject.transform
            );
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

            return Transforms.AutoCompleteTransformChildren(
                (Source is CharacterAsset characterAsset)
                    ? characterAsset.MainTransform
                    : Source.GameObject.transform
            );
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            SetActive(true);
        }
    }
}
