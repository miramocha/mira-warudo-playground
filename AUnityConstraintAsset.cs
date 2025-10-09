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
using Warudo.Plugins.Core.Assets.Character;
using Warudo.Plugins.Core.Utils;
using Axis = UnityEngine.Animations.Axis;
using IConstraint = UnityEngine.Animations.IConstraint;

namespace Warudo.Plugins.Scene.Assets
{
    // TODO: refactor to use generics
    public abstract class AUnityConstraintAsset : ADebuggableAsset
    {
        public IConstraint Constraint;

        [Section("Constraint Parent", 9)]
        [DataInput(10)]
        public GameObjectAsset Parent;

        [DataInput(11)]
        [Label("Parent Transform Path")]
        [HiddenIf(nameof(Parent), Is.Null)]
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
        [HiddenIf(nameof(Source), Is.Null)]
        [AutoComplete("GetSourceTransforms", true, "ROOT_TRANSFORM")]
        public string SourceTransformPath;
        public Transform SourceTransform => Source?.GameObject?.transform.Find(SourceTransformPath);
        public Vector3 SourceRestLocalPosition;
        public Quaternion SourceRestLocalRotation;

        [Section("Freeze Rotation Axes", 200)]
        [SectionHiddenIf(nameof(HideFreezeRotationAxes))]
        [DataInput(201)]
        [Label("X Axis")]
        public bool FreezeRotationX = true;

        [DataInput(202)]
        [Label("Y Axis")]
        public bool FreezeRotationY = true;

        [DataInput(203)]
        [Label("Z Axis")]
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
        public virtual bool HideFreezeRotationAxes()
        {
            return true;
        }

        [Section("Freeze Position Axes", 300)]
        [SectionHiddenIf(nameof(HideFreezePositionAxes))]
        [DataInput(301)]
        [Label("X Axis")]
        public bool FreezePositionX = true;

        [DataInput(302)]
        [Label("Y Axis")]
        public bool FreezePositionY = true;

        [DataInput(303)]
        [Label("Z Axis")]
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

        public virtual bool HideFreezePositionAxes()
        {
            return true;
        }

        [Section("Constraint Settings", 1000)]
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
                // SetDataInput(nameof(Enabled), false, broadcast: true);

                // Reset parent and source to their rest positions
                ResetParent();
                ResetSource();
            }
        }

        [DataInput(1003)]
        [Label("Weight")]
        [FloatSlider(0, 1)]
        [HiddenIf(nameof(HideWeight))]
        public float Weight = 1.0f;
        public virtual bool HideWeight()
        {
            return Constraint == null;
        }

        public void ResetParent()
        {
            if (ParentTransform != null)
            {
                ParentTransform.localPosition = ParentRestLocalPosition;
                ParentTransform.localRotation = ParentRestLocalRotation;
            }
        }

        public void ResetSource()
        {
            if (SourceTransform != null)
            {
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

        protected override void OnCreate()
        {
            base.OnCreate();
            SetActive(true);
            // Watch<bool>(
            //     nameof(Enabled),
            //     (oldValue, newValue) =>
            //     {
            //         if (Constraint != null)
            //         {
            //             Constraint.constraintActive = newValue;
            //         }
            //     }
            // );
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
            SetDataInput(nameof(ParentTransformPath), null, broadcast: true);
            if (Constraint != null)
            {
                DeleteConstraint();
            }
        }

        protected void OnSourceChanged(GameObjectAsset oldSource, GameObjectAsset newSource)
        {
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

        [Markdown(order: 2503, primary: true)]
        public string ParentTransformHeader = "Parent Transform";

        [Markdown(2504)]
        public string ParentTransformInfo = "Parent Transform info will appear here.";

        [Markdown(order: 2505, primary: true)]
        public string SourceTransformHeader = "Source Transform";

        [Markdown(2506)]
        public string SourceTransformInfo = "Source Transform info will appear here.";

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (ParentTransform != null)
            {
                List<String> transformInfoList = new List<String>
                {
                    "Name: " + ParentTransform.name,
                    "Path:" + ParentTransformPath,
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
                    "Parent Transform info will appear here.",
                    broadcast: true
                );
            }

            if (SourceTransform != null)
            {
                List<String> transformInfoList = new List<String>
                {
                    "Name: " + SourceTransform.name,
                    "Path:" + SourceTransformPath,
                    "World Position: " + SourceTransform.position.ToString("F3"),
                    "World Rotation: " + SourceTransform.rotation.eulerAngles.ToString("F3"),
                    "Local Position: " + SourceTransform.localPosition.ToString("F3"),
                    "Local Rotation: " + SourceTransform.localRotation.eulerAngles.ToString("F3"),
                };
                string newSourceTransformInfo = String.Join("<br>", transformInfoList.ToArray());
                SetDataInput(nameof(SourceTransformInfo), newSourceTransformInfo, broadcast: true);
            }
            else
            {
                SetDataInput(
                    nameof(SourceTransformInfo),
                    "Source Transform info will appear here.",
                    broadcast: true
                );
            }
        }
    }
}
