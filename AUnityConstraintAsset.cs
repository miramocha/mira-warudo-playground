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
using Warudo.Plugins.Core.Assets;
using Warudo.Plugins.Core.Utils;
using IConstraint = UnityEngine.Animations.IConstraint;

namespace Warudo.Plugins.Scene.Assets
{
    // TODO: refactor to use generics
    public abstract class AUnityConstraintAsset : ADebuggableAsset
    {
        public IConstraint Constraint;

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
        public Transform ParentTransform => Parent?.GameObject?.transform.Find(ParentTransformPath);
        public Vector3 ParentRestLocalPosition;
        public Quaternion ParentRestLocalRotation;

        [Section("Constraint Source", 119)]
        [DataInput(20)]
        public GameObjectAsset Source;

        [DataInput(121)]
        [Label("Source Transform Path")]
        [AutoComplete("GetSourceTransforms", true, "ROOT_TRANSFORM")]
        public string SourceTransformPath;
        public Transform SourceTransform => Source?.GameObject?.transform.Find(SourceTransformPath);
        public Vector3 SourceRestLocalPosition;
        public Quaternion SourceRestLocalRotation;

        [DataInput(131)]
        [FloatSlider(0, 1)]
        public float Weight = 1.0f;

        [Section("Constraint Action", 1000)]
        [Trigger(1001)]
        [DisabledIf(nameof(DisableCreateConstraintTrigger))]
        [HiddenIf(nameof(HideCreateConstraintTrigger))]
        public virtual void CreateConstraint()
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
        }

        [Trigger(1002)]
        [HiddenIf(nameof(HideDeleteConstraintTrigger))]
        public virtual void DeleteConstraint()
        {
            if (Constraint != null)
            {
                UnityEngine.Object.Destroy((UnityEngine.Object)Constraint);
                DebugLog("Constraint deleted.");
                Constraint = null;
            }
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

        protected override void OnCreate()
        {
            base.OnCreate();
            SetActive(true);
            Watch<bool>(
                nameof(Enabled),
                (oldValue, newValue) =>
                {
                    if (Constraint != null)
                    {
                        Constraint.constraintActive = newValue;
                    }
                }
            );
            Watch<GameObjectAsset>(nameof(Parent), OnParentChanged);
            Watch<GameObjectAsset>(nameof(Source), OnSourceChanged);
            Watch<String>(nameof(ParentTransformPath), OnParentTransformPathChanged);
            Watch<String>(nameof(SourceTransformPath), OnSourceTransformPathChanged);
            Watch<float>(nameof(Weight), OnWeightChanged);
        }

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
            ParentTransformPath = null;
            if (Constraint != null)
            {
                DeleteConstraint();
            }
        }

        protected void OnSourceChanged(GameObjectAsset oldSource, GameObjectAsset newSource)
        {
            SourceTransformPath = null;
            if (Constraint != null)
            {
                DeleteConstraint();
            }
        }

        protected void OnWeightChanged(float oldWeight, float newWeight)
        {
            Constraint.weight = newWeight;
        }

        [Markdown(order: 2503, primary: true)]
        public string ParentTransformHeader = "Parent Transform";
        [Markdown(2504)]
        public string ParentTransformInfo = "Parent Transform info will appear here.";
    }
}
