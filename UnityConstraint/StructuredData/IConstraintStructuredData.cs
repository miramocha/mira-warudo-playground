using Warudo.Plugins.Core.Assets;

namespace Warudo.Plugins.Scene.Assets;

public interface IConstraintStructuredData
{
    GameObjectAsset Asset { get; set; }
    string GameObjectPath { get; set; }
    string ConstraintTransformID { get; }
}
