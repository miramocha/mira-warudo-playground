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
    [AssetType(
        Id = "mira-quality-settings",
        Category = "Configuration",
        Title = "Extended Quality Settings",
        Singleton = true
    )]
    public class ML_QualitySettingsAsset : Asset
    {
        [DataInput]
        [FloatSlider(0, 1000)]
        public float ShadowDistance;

        protected override void OnCreate()
        {
            ShadowDistance = QualitySettings.shadowDistance;
            Broadcast();
            SetActive(true);
            Watch<float>(
                nameof(ShadowDistance),
                delegate
                {
                    QualitySettings.shadowDistance = ShadowDistance;
                }
            );
        }
    }
}
