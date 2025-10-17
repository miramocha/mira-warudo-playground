using Warudo.Plugins.Core.Assets;

namespace Warudo.Plugins.Scene.Assets;

public interface IGameObjectComponentStructuredData
{
    GameObjectAsset GetAsset();
    string GetGameObjectPath();
    string GameObjectComponentPathID { get; }
}
