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
    public static class ParentConstraintNodeManager
    {
        private static Dictionary<string, ParentConstraintModel> gameObjectIdToModel =
            new Dictionary<string, ParentConstraintModel>();
        private static Dictionary<string, string> nodeIdToGameObjectId =
            new Dictionary<string, string>();

        // gets called when create/asset selection
        public static void RegisterNode(ParentConstraintNode node)
        {
            GameObject gameObject = node.FindTargetTransform()?.gameObject;
            string gameObjectId = gameObject?.GetInstanceID().ToString();
            string previousGameObjectId;

            // If node is already registered before, we have to first remove it from a model
            if (nodeIdToGameObjectId.TryGetValue(node.IdString, out previousGameObjectId))
            {
                // If registered node has changed gameobject, then we remove it from model
                if (gameObjectId != previousGameObjectId)
                {
                    // Assume that if first map has the node id then model already exists for previousGameObjectId
                    ParentConstraintModel previousModel;
                    gameObjectIdToModel.TryGetValue(gameObjectId, out previousModel);

                    if (previousModel != null)
                    {
                        previousModel.RemoveNode(node.IdString);
                    }
                }
                // If registered node has the same gameobject, then we do nothing
                else
                {
                    return;
                }
            }

            nodeIdToGameObjectId[node.IdString] = gameObjectId;
            ParentConstraintModel model;
            gameObjectIdToModel.TryGetValue(gameObjectId, out model);

            if (model == null)
            {
                model = new ParentConstraintModel(gameObject);
                model.ConstraintNodes.Add(node);
            }
        }

        // gets called when deleted
        public static void DeregisterNode(ParentConstraintNode parentConstraintNode) { }

        public class ParentConstraintModel
        {
            public GameObject GameObject { get; set; }
            public List<ParentConstraintNode> ConstraintNodes { get; set; }

            public ParentConstraintModel(GameObject gameObject)
            {
                GameObject = gameObject;
                ConstraintNodes = new List<ParentConstraintNode>();
            }

            public void RemoveNode(string nodeId)
            {
                ConstraintNodes.RemoveAll(constraintNode => constraintNode.IdString == nodeId);
            }
        }
    }
}
