using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Warudo.Core.Attributes;
using Warudo.Core.Data;
using Warudo.Core.Utils;
using Warudo.Plugins.Core.Assets;
using Warudo.Plugins.Core.Assets.Character;
using Warudo.Plugins.Core.Assets.Environment;
using Warudo.Plugins.Core.Utils;

namespace Warudo.Plugins.Scene.Assets;

public class CreateConstraintPromptStructuredData : StructuredData, IConstraintStructuredData
{
    [DataInput]
    [Label("ASSET")]
    public GameObjectAsset AssetInput;
    public GameObjectAsset Asset
    {
        get { return AssetInput; }
        set { SetDataInput(nameof(AssetInput), value, broadcast: true); }
    }

    [DataInput]
    [AutoComplete("AutoCompleteGameObjectPath", false, "")]
    [Label("GAMEOBJECT_PATH")]
    [Description("LEAVE_EMPTY_TO_TARGET_THE_ROOT_GAMEOBJECT")]
    public string GameObjectPathInput;
    public string GameObjectPath
    {
        get { return GameObjectPathInput; }
        set { SetDataInput(nameof(GameObjectPathInput), value, broadcast: true); }
    }

    public string ConstraintTransformID
    {
        get { return ConstraintStructuredDataUtil.GetConstraintTransformID(this); }
    }

    public async UniTask<AutoCompleteList> AutoCompleteGameObjectPath()
    {
        return await ConstraintStructuredDataUtil.AutoCompleteGameObjectPath(this);
    }

    public Transform FindTargetTransform()
    {
        return ConstraintStructuredDataUtil.FindTargetTransform(this);
    }
}
