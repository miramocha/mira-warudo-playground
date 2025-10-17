using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Warudo.Core;
using Warudo.Core.Attributes;
using Warudo.Core.Data;
using Warudo.Core.Data.Models;
using Warudo.Core.Scenes;
using Warudo.Core.Server;
using Warudo.Plugins.Core.Assets;
using Warudo.Plugins.Core.Utils;

namespace Warudo.Plugins.Scene.Assets
{
    [AssetType(
        Id = "mira-unity-parent-constraint-manager",
        Category = "Unity Constraints",
        Title = "Unity Parent Constraints Manager",
        Singleton = true
    )]
    public class UnityParentConstraintsManagerAsset : ADebuggableAsset
    {
        private HashSet<string> constriantTransformIDSet
        {
            get
            {
                HashSet<string> idSet = new HashSet<string>();
                foreach (
                    ConstraintStructuredData structuredData in ConstraintStructuredDataArray.ToList<ConstraintStructuredData>()
                )
                {
                    idSet.Add(structuredData.GameObjectComponentPathID);
                }

                return idSet;
            }
        }

        [Section("Manage Constraint")]
        [Trigger]
        public async void CreateConstraint()
        {
            DebugLog("Launching prompt");
            CreateConstraintPromptStructuredData promptStructuredData =
                (CreateConstraintPromptStructuredData)(
                    await Context.Service.PromptStructuredDataInput<CreateConstraintPromptStructuredData>(
                        "Select Asset and Path to Add Constraint"
                    )
                );

            if (promptStructuredData == null)
            {
                DebugLog("Create Constraint Cancelled");
                return;
            }

            while (!validatePromptConstraintStructuredData(promptStructuredData))
            {
                promptStructuredData = (CreateConstraintPromptStructuredData)(
                    await Context.Service.PromptStructuredDataInput<CreateConstraintPromptStructuredData>(
                        "Select Asset and Path to Add Constraint"
                    )
                );

                if (promptStructuredData == null)
                {
                    DebugLog("Create Constraint Cancelled");
                    return;
                }
            }

            DebugLog("Structured Data Valid");
            addConstraintStructuredData(promptStructuredData);
        }

        // [Trigger]
        // public void RefreshAllConstraints()
        // {
        //     foreach (
        //         ConstraintStructuredData ConstraintStructuredData in ConstraintStructuredDataArray
        //     )
        //     {
        //         ConstraintStructuredData.RefreshConstraint();
        //     }
        // }

        [Section("🔗 Active Constraints")]
        [DataInput]
        [Label("Constraints")]
        [Disabled]
        public ConstraintStructuredData[] ConstraintStructuredDataArray;

        protected override void OnCreate()
        {
            base.OnCreate();
            SetActive(true);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        private void addConstraintStructuredData(
            CreateConstraintPromptStructuredData promptStructuredData
        )
        {
            ConstraintStructuredData structuredData =
                StructuredData.Create<ConstraintStructuredData>();
            structuredData.Asset = promptStructuredData.Asset;
            structuredData.GameObjectPath = promptStructuredData.GameObjectPath;
            structuredData.CreateConstraint();
            structuredData.Parent = this;

            List<ConstraintStructuredData> constraintStructuredDataList =
                ConstraintStructuredDataArray.ToList();
            constraintStructuredDataList.Add(structuredData);

            SetDataInput(
                nameof(ConstraintStructuredDataArray),
                constraintStructuredDataList.ToArray(),
                broadcast: true
            );
            structuredData.Broadcast();
        }

        private bool validatePromptConstraintStructuredData(
            CreateConstraintPromptStructuredData structuredData
        )
        {
            bool isValid = true;
            List<string> errorMessages = new List<string>();

            if (structuredData.FindTargetTransform() == null)
            {
                isValid = false;
                DebugLog("Transform not found");
                errorMessages.Add("Transform not found!");
            }

            if (constriantTransformIDSet.Contains(structuredData.GameObjectComponentPathID))
            {
                isValid = false;
                DebugLog("Constraint already exists!");
                errorMessages.Add("Constraint already exists!");
            }

            if (!isValid)
            {
                Context.Service.Toast(
                    ToastSeverity.Error,
                    "Invalid Input",
                    string.Join("<br>", errorMessages)
                );
            }

            return isValid;
        }

        protected override void UpdateDebugInfo()
        {
            List<string> debugInfoLines = new List<string>
            {
                "Manager ID: " + IdString,
                "transformIDSet: [" + string.Join("/", constriantTransformIDSet) + "]",
                "total constraint: " + ConstraintStructuredDataArray.Length,
            };
            string newDebugInfo = string.Join("<br>", debugInfoLines);
            SetDataInput(nameof(DebugInfo), newDebugInfo, broadcast: true);
        }

        public void DeleteConstraintStructuredData(
            ConstraintStructuredData constraintStructuredData
        )
        {
            DebugToast("Deleting: " + constraintStructuredData.GameObjectComponentPathID);
            List<ConstraintStructuredData> constraintStructureDataList =
                ConstraintStructuredDataArray.ToList();
            constraintStructureDataList.RemoveAll(current =>
                current.GameObjectComponentPathID
                == constraintStructuredData.GameObjectComponentPathID
            );

            SetDataInput(
                nameof(ConstraintStructuredDataArray),
                constraintStructureDataList.ToArray(),
                broadcast: true
            );
        }
    }
}
