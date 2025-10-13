using System.Collections.Generic;
using System.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Warudo.Core;
using Warudo.Core.Attributes;
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

        [DataOutput]
        public ParentConstraint Constraint() => constraint;

        protected override void OnCreate()
        {
            base.OnCreate();
            Watch<GameObjectAsset>(nameof(Asset), OnAssetChanged);
            Watch<string>(nameof(GameObjectPath), OnGameObjectPathChanged);
            Watch<ConstraintSourceData[]>(
                nameof(ConstraintSourceDataList),
                (oldValue, newValue) =>
                {
                    DebugToast("Constraint sources changed");
                    if (constraint != null)
                    {
                        constraint.SetSources(ConstraintSources());
                    }
                }
            );
        }

        protected override void OnAssetChanged(GameObjectAsset oldValue, GameObjectAsset newValue)
        {
            DestroyConstraint();
            if (FindTargetTransform() == null)
            {
                return;
            }

            CreateConstraint();
        }

        protected override void OnGameObjectPathChanged(string oldValue, string newValue)
        {
            DestroyConstraint();
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

            Transform().gameObject.AddComponent(typeof(ParentConstraint));
            constraint = Transform().GetComponent<ParentConstraint>();
            constraint.SetSources(ConstraintSources());
            constraint.enabled = true;
            constraint.constraintActive = true;
        }

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
                UnityEngine.Animations.ConstraintSource source =
                    new UnityEngine.Animations.ConstraintSource();
                source.sourceTransform = ConstraintSourceDataList[i].FindTargetTransform();
                source.weight = ConstraintSourceDataList[i].Weight;
                sources.Add(source);
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
