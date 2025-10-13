using Cysharp.Threading.Tasks;
using UnityEngine;
using Warudo.Core.Attributes;
using Warudo.Core.Data;
using Warudo.Core.Graphs;
using Warudo.Core.Utils;
using Warudo.Plugins.Core.Assets;
using Warudo.Plugins.Core.Assets.Character;
using Warudo.Plugins.Core.Assets.Environment;
using Warudo.Plugins.Core.Utils;
using ConstraintSource = UnityEngine.Animations.ConstraintSource;

namespace Warudo.Plugins.Core.Nodes;

public class ConstraintSourceData : StructuredData
{
    // private UnityEngine.Animations.ConstraintSource constraintSource =
    //     new UnityEngine.Animations.ConstraintSource();



    // [DataOutput]
    // public UnityEngine.Animations.ConstraintSource ConstraintSource() => constraintSource;

    public IConstraintSourceDataParent constraintSourceDataParent;
    public int index = 0;

    [DataInput(-1000)]
    [Label("ASSET")]
    public GameObjectAsset Asset;

    [DataInput(-999)]
    [AutoComplete("AutoCompleteGameObjectPath", false, "")]
    [Label("GAMEOBJECT_PATH")]
    [Description("LEAVE_EMPTY_TO_TARGET_THE_ROOT_GAMEOBJECT")]
    public string GameObjectPath;

    [DataInput(-998)]
    [FloatSlider(0, 1)]
    public float Weight = 1.0f;

    protected override void OnCreate()
    {
        base.OnCreate();
        WatchAll(new [] { nameof(Asset), nameof(GameObjectPath), nameof(Weight) }, OnDataChanged);
        // constraintSource.weight = Weight;
    }

    protected void OnDataChanged()
    {
        if(constraintSourceDataParent != null)
        {
            constraintSourceDataParent.UpdateConstriantSource(this);
        }
    }

    public async UniTask<AutoCompleteList> AutoCompleteGameObjectPath()
    {
        if (Asset == null || !Asset.Active)
        {
            return AutoCompleteList.Message("SELECTED_PARENT_ASSET_IS_INACTIVE");
        }

        if (Asset is EnvironmentAsset environmentAsset)
        {
            return environmentAsset.AutoCompleteGameObjectPath();
        }

        if (Asset is CharacterAsset characterAsset)
        {
            return Transforms.AutoCompleteTransformChildren(characterAsset.MainTransform);
        }

        return Transforms.AutoCompleteTransformChildren(Asset.GameObject.transform);
    }

    public Transform FindTargetTransform()
    {
        if (Asset.IsNullOrInactive())
        {
            return null;
        }

        string text = GameObjectPath ?? "";
        if (Asset is EnvironmentAsset environmentAsset)
        {
            return environmentAsset.GetSceneTransform(text);
        }

        if (Asset is CharacterAsset characterAsset)
        {
            return characterAsset.MainTransform.Find(text);
        }

        return Asset.GameObject.transform.Find(text);
    }
}
