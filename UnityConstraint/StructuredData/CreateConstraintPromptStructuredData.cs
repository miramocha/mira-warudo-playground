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

public class CreateConstraintPromptStructuredData
    : StructuredData,
        IGameObjectComponentStructuredData
{
    [DataInput]
    [Label("ASSET")]
    public GameObjectAsset Asset;

    public GameObjectAsset GetAsset()
    {
        return Asset;
    }

    [DataInput]
    [AutoComplete("AutoCompleteGameObjectPath", false, "")]
    [Label("GAMEOBJECT_PATH")]
    [Description("LEAVE_EMPTY_TO_TARGET_THE_ROOT_GAMEOBJECT")]
    public string GameObjectPath;

    public string GetGameObjectPath()
    {
        return GameObjectPath;
    }

    public async UniTask<AutoCompleteList> AutoCompleteGameObjectPath()
    {
        return await GameObjectComponentStructuredDataUtil.AutoCompleteGameObjectPath(this);
    }

    public Transform FindTargetTransform()
    {
        return GameObjectComponentStructuredDataUtil.FindTargetTransform(this);
    }

    public string GameObjectComponentPathID
    {
        get { return GameObjectComponentStructuredDataUtil.GetGameObjectComponentPathID(this); }
    }
}
