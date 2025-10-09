using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RootMotion;
using UnityEngine;
using UnityEngine.Animations;
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
using ConstraintSource = UnityEngine.Animations.ConstraintSource;
using RotationConstraint = UnityEngine.Animations.RotationConstraint;

namespace Warudo.Plugins.Scene.Assets
{
    [AssetType(
        Id = "mira-rotation-constraint",
        Title = "Rotation Constraint",
        Category = "Unity Constraints"
    )]
    public class UnityRotationConstraintAsset : Asset
    {
        public RotationConstraint Constraint;

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

        [DataInput(31)]
        [FloatSlider(0, 1)]
        public float Weight = 1.0f;

        [Trigger]
        [DisabledIf(nameof(DisableCreateConstraintTrigger))]
        [HiddenIf(nameof(HideCreateConstraintTrigger))]
        public void CreateConstraint()
        {
            // Assume Parent and Source are not null
            Transform parentTransform = Parent.GameObject.transform.Find(ParentTransformPath);
            Transform sourceTransform = Source.GameObject.transform.Find(SourceTransformPath);

            if (parentTransform == null)
            {
                throw new Exception("parent not found" + ParentTransformPath);
            }

            if (sourceTransform == null)
            {
                throw new Exception("source not found" + SourceTransformPath);
            }

            ConstraintSource constraintSource = new ConstraintSource();
            constraintSource.sourceTransform = sourceTransform;
            constraintSource.weight = 1.0f;

            parentTransform.gameObject.AddComponent(typeof(RotationConstraint));

            Constraint = parentTransform.GetComponent<RotationConstraint>();
            DebugToast("RotationConstraint component added.");
            Constraint.weight = Weight;
            Constraint.SetSources(new List<ConstraintSource> { constraintSource });
            Constraint.constraintActive = true;
            Constraint.enabled = true;
            Enabled = true;

            if (Constraint == null)
            {
                throw new Exception("failed to add constraint component");
            }
            DebugToast("Constraint created.");
        }

        [Trigger]
        [HiddenIf(nameof(HideDeleteConstraintTrigger))]
        public void DeleteConstraint()
        {
            DebugToast("Constraint deleted.");
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

            //return Transforms.AutoCompleteTransformChildren(
            //    (Parent is CharacterAsset characterAsset)
            //        ? characterAsset.MainTransform
            //        : Parent.GameObject.transform
            //);
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

            //return Transforms.AutoCompleteTransformChildren(
            //    (Source is CharacterAsset characterAsset)
            //        ? characterAsset.MainTransform
            //        : Source.GameObject.transform
            //);

            return Transforms.AutoCompleteTransformChildren(Source.GameObject.transform);
        }

        public void DebugToast(string msg)
        {
            Context.Service.Toast(Warudo.Core.Server.ToastSeverity.Info, "Debug", msg);
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            SetActive(true);
            Watch<bool>(nameof(Enabled), (oldValue, newValue) =>
            {
                if (Constraint != null)
                {
                    Constraint.constraintActive = newValue;
                }
            });
            Watch<GameObjectAsset>(nameof(Parent), OnParentChanged);
            Watch<GameObjectAsset>(nameof(Source), OnSourceChanged);
            Watch<String>(nameof(ParentTransformPath), (oldPath, newPath) =>
            {
                DebugToast("Parent Transform Path Changed.");
                if (Constraint != null)
                {
                    DeleteConstraint();
                }
            });
            Watch<String>(nameof(SourceTransformPath), (oldPath, newPath) =>
            {
                DebugToast("Source Transform Path Changed.");
                if (Constraint != null)
                {
                    DeleteConstraint();
                }
            });
        }

        protected void OnParentChanged(GameObjectAsset oldParent, GameObjectAsset newParent)
        {
            DebugToast("Parent Changed.");
            ParentTransformPath = null;
            if (Constraint != null)
            {
                DeleteConstraint();
            }
        }


        protected void OnSourceChanged(GameObjectAsset oldSource, GameObjectAsset newSource)
        {
            DebugToast("Source Changed.");
            SourceTransformPath = null;
            if (Constraint != null)
            {
                DeleteConstraint();
            }
        }
    }
}
