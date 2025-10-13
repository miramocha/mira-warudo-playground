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
    public class ParentConstraintNode : AssetChildGameObjectNode
    {
        private ParentConstraint constraint;
        private TransformData originalTransform = new TransformData();

        [DataOutput]
        public int SourceCount() => constraint?.sourceCount ?? 0;

        [DataOutput]
        public TransformData OriginalTransform() => originalTransform;

        [DataOutput]
        public ParentConstraint Constraint() => constraint;

        [DataInput(-998)]
        [FloatSlider(0, 1)]
        public float Weight = 1.0f;

        protected override void OnCreate()
        {
            base.OnCreate();
            Watch<float>(nameof(Weight), OnWeightChanged);
            Watch<ConstraintSourceData[]>(nameof(ConstraintSourceDataList), OnConstraintSourceDataListChanged);
        }

        protected override void OnDestroy()
        {
            DestroyConstraint();
            base.OnDestroy();
        }

        protected virtual void OnWeightChanged(float oldValue, float newValue)
        {
            if (constraint != null)
            {
                constraint.weight = newValue;
            }
        }

        protected virtual void OnConstraintSourceDataListChanged(ConstraintSourceData[] oldValue, ConstraintSourceData[] newValue)
        {
            DebugToast("old size: " + oldValue.Length + "new size: " + newValue.Length);
            RefreshSources();
        }

        protected override void OnAssetChanged(GameObjectAsset oldValue, GameObjectAsset newValue)
        {
            base.OnAssetChanged(oldValue, newValue);
            DebugToast("Asset changed to " + newValue?.Name);
            DestroyConstraint();
            if (FindTargetTransform() == null)
            {
                return;
            }

            CreateConstraint();
        }

        protected override void OnGameObjectPathChanged(string oldValue, string newValue)
        {
            base.OnGameObjectPathChanged(oldValue, newValue);
            DestroyConstraint();
            DebugToast("Path changed to " + newValue);
            if (FindTargetTransform() == null)
            {
                return;
            }

            CreateConstraint();
        }

        protected void DestroyConstraint()
        {
            if (constraint != null)
            {
                DebugToast("Destroying existing constraint");
                originalTransform.ApplyAsLocalTransform(constraint.gameObject.transform);
                UnityEngine.Object.Destroy((UnityEngine.Object)constraint);
                constraint = null;

                // Reset parent and source to their rest positions
                // if (resetParent)
                //     ResetParent();
            }
        }

        protected void CreateConstraint()
        {
            // ConstraintSource constraintSource = new ConstraintSource();
            // constraintSource.sourceTransform = SourceTransform;
            // constraintSource.weight = 1f; // Full weight from source for now
            DebugToast("Creating new ParentConstraint on " + Transform().name);
            originalTransform.CopyFromLocalTransform(Transform());
            Transform().gameObject.AddComponent(typeof(ParentConstraint));
            constraint = Transform().GetComponent<ParentConstraint>();
            constraint.SetSources(ConstraintSources());
            constraint.weight = Weight;
            constraint.enabled = true;
            constraint.constraintActive = true;
        }

        [FlowInput]
        public Continuation RefreshSources()
        {
            constraint.SetSources(ConstraintSources());
            return null;
        }

        // [FlowOutput]
        // public Continuation  SourceRefreshed()
        // {
        //     return null;
        // }

        [DataInput]
        [Label("Constraint Sources")]
        public ConstraintSourceData[] ConstraintSourceDataList;

        public List<UnityEngine.Animations.ConstraintSource> ConstraintSources()
        {
            DebugToast("Getting constraint sources");
            List<UnityEngine.Animations.ConstraintSource> sources =
                new List<UnityEngine.Animations.ConstraintSource>();
            for (int i = 0; i < ConstraintSourceDataList.Length; i++)
            {
                if (ConstraintSourceDataList[i].ConstraintSource().sourceTransform != null)
                {
                    sources.Add(ConstraintSourceDataList[i].ConstraintSource());
                }
            }

            return sources;
        }

        public void DebugToast(string msg)
        {
            // if (!DebugMode)
            //     return;

            Context.Service.Toast(Warudo.Core.Server.ToastSeverity.Info, "Debug", msg);
        }
    }
}
