using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;
using Warudo.Core;
using Warudo.Core.Attributes;
using Warudo.Core.Data;
using Warudo.Core.Data.Models;
using Warudo.Core.Scenes;
using Warudo.Core.Server;
using Warudo.Core.Utils;
using Warudo.Plugins.Core;
using Warudo.Plugins.Core.Assets;
using Warudo.Plugins.Core.Utils;

namespace Warudo.Plugins.Scene.Assets;

public class ML_UnityConstraintSourceStructuredData
    : StructuredData<ML_UnityParentConstraintStructuredData>,
        ML_IGameObjectComponentStructuredData,
        ICollapsibleStructuredData
{
    public string GetHeader() => Asset?.Name + '/' + GameObjectPath;

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
    public string GameObjectPath;

    public string GetGameObjectPath()
    {
        return GameObjectPath;
    }

    [Section("Settings")]
    [DataInput]
    [Label("Weight")]
    [FloatSlider(0, 1)]
    public float Weight = 1.0f;

    public async UniTask<AutoCompleteList> AutoCompleteGameObjectPath()
    {
        return await ML_GameObjectComponentStructuredDataUtil.AutoCompleteGameObjectPath(this);
    }

    public Transform FindTargetTransform()
    {
        return ML_GameObjectComponentStructuredDataUtil.FindTargetTransform(this);
    }

    public string GameObjectComponentPathID
    {
        get { return ML_GameObjectComponentStructuredDataUtil.GetGameObjectComponentPathID(this); }
    }

    protected override void OnCreate()
    {
        base.OnCreate();
        WatchAll(
            new[] { nameof(Asset), nameof(GameObjectPath), nameof(Weight) },
            delegate
            {
                ML_DebugUtil.ToastDebug("Source attribute changed, Parent: " + Parent.IdString);
                Parent?.ApplyConstraintSources();
            }
        );
    }

    protected override void OnAssignedParent()
    {
        base.OnAssignedParent();
        ML_DebugUtil.ToastDebug("Source parent On assigned: " + Parent.IdString);
        // Parent.ApplyConstraintSources(); // -> Silent failure???
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        updateDebugInfo();
    }

    [Markdown]
    public string DebugInfo = "Constraint Info will appear here";

    private void updateDebugInfo()
    {
        List<string> infoLines = new List<string>
        {
            "Id: " + Id,
            "Asset Id: " + Asset?.IdString,
            "GameObject Id: " + FindTargetTransform()?.gameObject.GetInstanceID(),
            "Parent ID: " + Parent?.IdString,
        };
        string newInfo = string.Join("<br>", infoLines);
        SetDataInput(nameof(DebugInfo), newInfo, broadcast: true);
    }
}
