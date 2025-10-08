using System;
using Cysharp.Threading.Tasks;
using RootMotion;
using UnityEngine;
using UniVRM10;
using Warudo.Core;
using Warudo.Core.Attributes;
using Warudo.Core.Data;
using Warudo.Core.Scenes;
using Warudo.Plugins.Core.Assets;
using Warudo.Plugins.Core.Assets.Character;
using Warudo.Plugins.Core.Assets.Mixins;
using Warudo.Plugins.Core.Utils;
using static RootMotion.Demos.Turret;

namespace Warudo.Plugins.Scene.Assets
{
    [AssetType(
        Id = "1f84701b-0115-4a8a-b810-2c61b04f612f",
        Title = "VRM1 Rotation Constraint",
        Category = "VRM1 Constraints"
    )]
    public class Vrm1RotationConstraintAsset : Asset { }
}
