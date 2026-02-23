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
        Id = "ml-shader-controller",
        Category = "Shader",
        Title = "ML Shader Controller",
        Singleton = true
    )]
    public class MLS_ShaderControllerAsset : Asset
    {
        [Section("Ambient Settings")]
        [DataInput]
        [Label("Ambient Color")]
        public Color AmbientColor = Color.white;

        [DataInput]
        [Label("Ambient Color Opacity")]
        [FloatSlider(0, 1)]
        public float AmbientColorOpacity = 1f;

        [DataInput]
        [Label("Ambient Color Intensity")]
        public float ColorIntensity = 1f;

        [Section("Global Lighting Settings")]
        [DataInput]
        [Label("Global Lighting Circle Offset")]
        public Vector2 GlobalLightingCircleOffset = Vector2.zero;

        [DataInput]
        [Label("Global Lighting Circle Edge Min/Max Offset")]
        public Vector2 GlobalLightingCircleEdgeMinMaxOffset = Vector2.zero;

        [DataInput]
        [Label("Global Lighting Circle Coordinates")]
        public Color GlobalLightingCircleCoordinates = Color.red;

        [Section("Rim Light Settings")]
        [DataInput]
        [Label("Global Rim Light Edge Min/Max Offset")]
        public Vector2 GlobalRimLightEdgeMinMaxOffset = Vector2.zero;

        [DataInput]
        [Label("Global Rim Light Opacity Offset")]
        [FloatSlider(-1, 1)]
        public float GlobalRimLightOpacityOffset = 0f;

        [DataInput]
        [Label("Global Rim Light Color Hue Offset")]
        [FloatSlider(-1, 1)]
        public float GlobalRimLightColorHueOffset = 0f;

        [DataInput]
        [Label("Enable Global Rim Light Color Cycling")]
        public bool EnableGlobalRimLightColorCycling = false;

        [DataInput]
        [Label("Global Rim Light Color Cycling Speed")]
        public float GlobalRimLightColorCyclingSpeed = 0f;

        [Section("Water Settings")]
        [DataInput]
        [Label("Global Water Opacity Offset")]
        [FloatSlider(-1, 1)]
        public float GlobalWaterOpacityOffset = 1f;

        protected override void OnCreate()
        {
            base.OnCreate();
            SetActive(true);
            WatchAll(
                new[] { nameof(GlobalWaterOpacityOffset) },
                () =>
                {
                    Shader.SetGlobalFloat(
                        "_Global_Water_Opacity_Offset",
                        this.GlobalWaterOpacityOffset
                    );
                }
            );
            WatchAll(
                new[] { nameof(AmbientColor), nameof(AmbientColorOpacity), nameof(ColorIntensity) },
                () =>
                {
                    Shader.SetGlobalColor(
                        "_Ambient_Color",
                        applyHDRIntensity(this.AmbientColor, this.ColorIntensity)
                    );
                    Shader.SetGlobalFloat("_Ambient_Color_Opacity", this.AmbientColorOpacity);
                }
            );
            WatchAll(
                new[]
                {
                    nameof(GlobalLightingCircleOffset),
                    nameof(GlobalLightingCircleEdgeMinMaxOffset),
                },
                () =>
                {
                    Shader.SetGlobalVector(
                        "_Global_Lighting_Circle_Offset",
                        this.GlobalLightingCircleOffset
                    );
                    Shader.SetGlobalVector(
                        "_Global_Lighting_Circle_Edge_Min_Max_Offset",
                        this.GlobalLightingCircleEdgeMinMaxOffset
                    );
                }
            );
            WatchAll(
                new[]
                {
                    nameof(GlobalRimLightEdgeMinMaxOffset),
                    nameof(GlobalRimLightOpacityOffset),
                    nameof(EnableGlobalRimLightColorCycling),
                    nameof(GlobalRimLightColorCyclingSpeed),
                    nameof(GlobalRimLightColorHueOffset),
                },
                () =>
                {
                    Shader.SetGlobalVector(
                        "_Global_Rim_Light_Edge_Min_Max_Offset",
                        this.GlobalRimLightEdgeMinMaxOffset
                    );
                    Shader.SetGlobalFloat(
                        "_Global_Rim_Light_Opacity_Offset",
                        this.GlobalRimLightOpacityOffset
                    );
                    Shader.SetGlobalFloat(
                        "_Enable_Global_Rim_Light_Color_Cycling",
                        this.EnableGlobalRimLightColorCycling ? 1f : 0f
                    );
                    Shader.SetGlobalFloat(
                        "_Global_Rim_Light_Color_Cycling_Speed",
                        this.GlobalRimLightColorCyclingSpeed
                    );
                    Shader.SetGlobalFloat(
                        "_Global_Rim_Light_Color_Hue_Offset",
                        this.GlobalRimLightColorHueOffset
                    );
                }
            );
            WatchAll(
                new[] { nameof(GlobalLightingCircleCoordinates) },
                () =>
                {
                    float h,
                        s,
                        v;
                    Color.RGBToHSV(GlobalLightingCircleCoordinates, out h, out s, out v);

                    // SetDataInput(
                    //     nameof(DebugVector2),
                    //     new Vector2(2 * (s - 0.5f), 2 * (v - 0.5f)).ToString(),
                    //     broadcast: true
                    // );

                    Shader.SetGlobalVector(
                        "_Global_Lighting_Circle_Offset",
                        new Vector2(2 * s - 0.5f, 2 * v - 0.5f)
                    );
                }
            );
        }

        public override void OnLateUpdate()
        {
            base.OnLateUpdate();
        }

        private static Color applyHDRIntensity(Color color, float intensity)
        {
            return new Color(
                color.r * Mathf.Pow(2, intensity),
                color.g * Mathf.Pow(2, intensity),
                color.b * Mathf.Pow(2, intensity),
                color.a
            );
        }
    }
}
