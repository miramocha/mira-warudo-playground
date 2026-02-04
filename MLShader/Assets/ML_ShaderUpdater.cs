using Cysharp.Threading.Tasks;
using UnityEngine;
using Warudo.Core;
using Warudo.Core.Attributes;
using Warudo.Core.Data;
using Warudo.Core.Data.Models;
using Warudo.Core.Scenes;
using Warudo.Plugins.Core;
using Warudo.Plugins.Core.Assets;

namespace Warudo.Plugins.Scene.Assets
{
    [AssetType(Id = "ml-shader-updater", Category = "Shader", Title = "ML Shader Updater")]
    public class ML_ShaderUpdaterAsset : Asset
    {
        [DataInput]
        [Label("ASSET")]
        public GameObjectAsset Asset;

        protected override void OnCreate()
        {
            base.OnCreate();
            SetActive(true);
        }

        public override void OnUpdate()
        {
            if (this.Asset == null)
            {
                return;
            }

            GameObject gameObject = this.Asset?.GameObject;
            if (gameObject == null)
            {
                return;
            }


            Transform rootTransform = this.Asset.GameObject.transform;
            Renderer[] renderers = rootTransform?.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                foreach (Material mat in renderer.materials)
                {
                    if (mat.HasProperty("_Dissolve_Cutoff_Height_Start"))
                    {
                        mat.SetFloat("_Dissolve_Cutoff_Height_Start", rootTransform.position.y);
                    }
                }
            }
        }
    }
}
