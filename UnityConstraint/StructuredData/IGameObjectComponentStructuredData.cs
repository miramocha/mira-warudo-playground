using Warudo.Plugins.Core.Assets;

namespace Warudo.Plugins.Scene.Assets;

public interface IGameObjectComponentStructuredData
{
    GameObjectAsset Asset { get; set; }
    string GameObjectPath { get; set; }
    string AssetGameObjectPathID { get; }
}
