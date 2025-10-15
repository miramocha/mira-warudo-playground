using System;
using System.Collections.Generic;
using System.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Warudo.Core;
using Warudo.Core.Attributes;
using Warudo.Core.Data.Models;
using Warudo.Core.Graphs;
using Warudo.Plugins.Core.Assets;
using ConstraintSource = UnityEngine.Animations.ConstraintSource;
using ParentConstraint = UnityEngine.Animations.ParentConstraint;

namespace Warudo.Plugins.Core.Nodes
{
    [NodeType(
        Id = "mira-parent-constraint-node",
        Title = "Parent Constraint",
        Category = "Unity Constraints",
        Width = 2f
    )]
    public class ParentConstraintNode : AAssetChildGameObjectNode, IConstraintSourceDataParent
    {
        public bool isPrimary = false;

        [DataInput]
        [FloatSlider(0, 1)]
        [HiddenIf(nameof(HideInput))]
        public float Weight = 1.0f;

        [DataInput]
        [Label("Constraint Sources")]
        [HiddenIf(nameof(HideInput))]
        public ConstraintSourceData[] ConstraintSourceDataList;

        public bool HideInput() => !isPrimary;

        public override bool HideGameObjectOutput()
        {
            return false;
        }

        public override bool HideGameObjectTransformOutput()
        {
            return false;
        }

        [DataOutput]
        [HiddenIf(nameof(HideInput))]
        public GameObject Preview() => GameObject();

        protected override void OnCreate()
        {
            base.OnCreate();
            // Watch<float>(nameof(Weight), OnWeightChanged);
            // Watch<ConstraintSourceData[]>(
            //     nameof(ConstraintSourceDataList),W
            //     OnConstraintSourceDataListChanged
            // );
        }

        protected override void OnDestroy()
        {
            ParentConstraintNodeManager.DeregisterNode(this);
            base.OnDestroy();
        }

        protected virtual void OnWeightChanged(float oldValue, float newValue) { }

        protected virtual void OnConstraintSourceDataListChanged(
            ConstraintSourceData[] oldValue,
            ConstraintSourceData[] newValue
        ) { }

        protected override void OnAssetChanged()
        {
            base.OnAssetChanged();

            DebugToast("Asset Changed to " + Asset.Name);
            ParentConstraintNodeManager.RegisterNode(this);
        }

        protected override void OnGameObjectPathChanged(string oldValue, string newValue)
        {
            base.OnGameObjectPathChanged(oldValue, newValue);

            DebugToast("Path changed to " + newValue);
            ParentConstraintNodeManager.RegisterNode(this);
        }

        // protected void DestroyConstraint()
        // {
        //     if (constraint != null)
        //     {
        //         DebugToast("Destroying existing constraint");
        //         originalTransform.ApplyAsLocalTransform(constraint.gameObject.transform);
        //         UnityEngine.Object.Destroy((UnityEngine.Object)constraint);
        //         constraint = null;
        //     }
        // }

        // protected void CreateConstraint()
        // {
        //     DebugToast("Creating new ParentConstraint on " + Transform().name);
        //     originalTransform.CopyFromLocalTransform(Transform());
        //     Transform().gameObject.AddComponent(typeof(ParentConstraint));
        //     constraint = Transform().GetComponent<ParentConstraint>();
        //     UpdateConstraintSources();
        //     constraint.weight = Weight;
        //     constraint.enabled = true;
        //     constraint.constraintActive = true;
        // }

        protected void UpdateConstraintSources()
        {
            // List<UnityEngine.Animations.ConstraintSource> sources = new List<ConstraintSource>();
            // if (ConstraintSourceDataList.Length == 0)
            // {
            //     constraint.SetSources(sources);
            //     originalTransform.ApplyAsLocalTransform(constraint.gameObject.transform);
            //     return;
            // }

            // for (int i = 0; i < ConstraintSourceDataList.Length; i++)
            // {
            //     ConstraintSourceData constraintSourceData = ConstraintSourceDataList[i];
            //     constraintSourceData.index = i;
            //     constraintSourceData.constraintSourceDataParent = this;

            //     UnityEngine.Animations.ConstraintSource source =
            //         new UnityEngine.Animations.ConstraintSource();
            //     source.weight = constraintSourceData.Weight;
            //     source.sourceTransform = constraintSourceData.FindTargetTransform();
            //     sources.Add(source);
            // }

            // constraint.SetSources(sources);
        }

        public void UpdateConstriantSource(ConstraintSourceData constraintSourceData)
        {
            // if (constraint != null)
            // {
            //     UnityEngine.Animations.ConstraintSource constraintSource;
            //     try
            //     {
            //         constraintSource = constraint.GetSource(constraintSourceData.index);
            //     }
            //     catch (Exception e)
            //     {
            //         return;
            //     }

            //     constraintSource.weight = constraintSourceData.Weight;
            //     constraintSource.sourceTransform = constraintSourceData.FindTargetTransform();
            //     // Since ConstraintSource is a struct, we have to put the whole thing back in
            //     constraint.SetSource(constraintSourceData.index, constraintSource);
            // }
        }

        private bool DebugMode = true;

        private void DebugToast(string msg)
        {
            if (!DebugMode)
                return;

            Context.Service.Toast(Warudo.Core.Server.ToastSeverity.Info, "Debug", msg);
        }
    }
}
