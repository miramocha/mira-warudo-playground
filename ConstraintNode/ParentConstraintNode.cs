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
    public class ParentConstraintNode : AssetChildGameObjectNode, IConstraintSourceDataParent
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
            Watch<ConstraintSourceData[]>(
                nameof(ConstraintSourceDataList),
                OnConstraintSourceDataListChanged
            );
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

        protected virtual void OnConstraintSourceDataListChanged(
            ConstraintSourceData[] oldValue,
            ConstraintSourceData[] newValue
        )
        {
            DebugToast("old size: " + oldValue.Length + "new size: " + newValue.Length);
            UpdateConstraintSources();
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
            DebugToast("Creating new ParentConstraint on " + Transform().name);
            originalTransform.CopyFromLocalTransform(Transform());
            Transform().gameObject.AddComponent(typeof(ParentConstraint));
            constraint = Transform().GetComponent<ParentConstraint>();
            UpdateConstraintSources();
            constraint.weight = Weight;
            constraint.enabled = true;
            constraint.constraintActive = true;
        }

        protected void UpdateConstraintSources()
        {
            List<UnityEngine.Animations.ConstraintSource> sources = new List<ConstraintSource>();
            for (int i = 0; i < ConstraintSourceDataList.Length; i++)
            {
                ConstraintSourceData constraintSourceData = ConstraintSourceDataList[i];
                constraintSourceData.index = i;
                constraintSourceData.constraintSourceDataParent = this;

                UnityEngine.Animations.ConstraintSource source =
                    new UnityEngine.Animations.ConstraintSource();
                source.weight = constraintSourceData.Weight;
                source.sourceTransform = constraintSourceData.FindTargetTransform();
                sources.Add(source);
            }

            constraint.SetSources(sources);
        }

        [DataInput]
        [Label("Constraint Sources")]
        public ConstraintSourceData[] ConstraintSourceDataList;

        public void UpdateConstriantSource(ConstraintSourceData constraintSourceData)
        {
            if (constraint != null)
            {
                UnityEngine.Animations.ConstraintSource constraintSource;
                try
                {
                    constraintSource = constraint.GetSource(constraintSourceData.index);
                }
                catch (Exception e)
                {
                    return;
                }

                constraintSource.weight = constraintSourceData.Weight;
                constraintSource.sourceTransform = constraintSourceData.FindTargetTransform();
                // Since ConstraintSource is a struct, we have to put the whole thing back in

                constraint.SetSource(constraintSourceData.index, constraintSource);
            }
        }

        public void DebugToast(string msg)
        {
            // if (!DebugMode)
            //     return;

            Context.Service.Toast(Warudo.Core.Server.ToastSeverity.Info, "Debug", msg);
        }
    }
}
