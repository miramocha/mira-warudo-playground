using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Warudo.Core.Attributes;
using Warudo.Core.Data;
using Warudo.Core.Scenes;
using Warudo.Core.Utils;
using Warudo.Plugins.Core.Assets;
using Warudo.Plugins.Core.Assets.Character;
using Warudo.Plugins.Core.Assets.Environment;
using Warudo.Plugins.Core.Utils;

namespace Warudo.Plugins.Scene.Assets;

public static class ConstraintStructuredDataUtil
{
    public static async UniTask<AutoCompleteList> AutoCompleteGameObjectPath(
        IConstraintStructuredData structuredData
    )
    {
        GameObjectAsset asset = structuredData.Asset;

        if (asset == null || !asset.Active)
        {
            return AutoCompleteList.Message("SELECTED_PARENT_ASSET_IS_INACTIVE");
        }

        if (asset is EnvironmentAsset environmentAsset)
        {
            return environmentAsset.AutoCompleteGameObjectPath();
        }

        if (asset is CharacterAsset characterAsset)
        {
            return Transforms.AutoCompleteTransformChildren(characterAsset.MainTransform);
        }

        return Transforms.AutoCompleteTransformChildren(asset.GameObject.transform);
    }

    public static Transform FindTargetTransform(IConstraintStructuredData structuredData)
    {
        GameObjectAsset asset = structuredData.Asset;
        if (asset.IsNullOrInactive())
        {
            return null;
        }

        string text = structuredData.GameObjectPath ?? "";
        if (asset is EnvironmentAsset environmentAsset)
        {
            return environmentAsset.GetSceneTransform(text);
        }

        if (asset is CharacterAsset characterAsset)
        {
            return characterAsset.MainTransform.Find(text);
        }

        return asset.GameObject.transform.Find(text);
    }

    public static string GetConstraintTransformID(IConstraintStructuredData structuredData)
    {
        return structuredData.Asset?.IdString + "||" + structuredData.GameObjectPath;
    }
}
