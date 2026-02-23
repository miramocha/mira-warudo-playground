using System.Numerics;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using Warudo.Core;
using Warudo.Core.Attributes;
using Warudo.Core.Data;
using Warudo.Core.Data.Models;
using Warudo.Core.Scenes;
using Warudo.Core.Utils;
using Warudo.Plugins.Core;
using Warudo.Plugins.Core.Assets;
using Warudo.Plugins.Core.Assets.Cinematography;
using WebSocketSharp;

// using WebSocketSharp.Websocket;

namespace Warudo.Plugins.Scene.Assets
{
    public class LunaSyncData
    {
        public Vector3 position;

        public Quaternion rotation;
    }

    [AssetType(Id = "ls-lunasync-retriever", Category = "LunaSync", Title = "LunaSync Retriever")]
    public class LS_LunaSyncRetrieverAsset : Asset
    {
        [DataInput]
        [Label("Camera Asset")]
        public CameraAsset CameraAsset;

        [DataInput]
        [Label("Update")]
        public bool UpdateAsset = false;

        [Markdown]
        public string LunaSyncData = "Data goes here";

        [DataInput]
        [Label("Pose Data WebSocket URL")]
        public string PoseDataWebSocketUrl = "ws://192.168.1.178:9090";

        protected override void OnCreate()
        {
            base.OnCreate();
            Connect();
            SetActive(true);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Disconnect();
        }

        private WebSocketSharp.WebSocket lunaSyncPoseDataWebSocket;

        private LunaSyncData lunaSyncData;

        [Trigger]
        public void Reconnect()
        {
            Disconnect();
            Connect();
        }

        [Trigger]
        public void Calibrate()
        {
            this.CameraAsset.Transform.Position = Vector3.zero;
        }

        private void Disconnect()
        {
            if (lunaSyncPoseDataWebSocket != null)
            {
                lunaSyncPoseDataWebSocket.Close();
                lunaSyncPoseDataWebSocket = null;
            }
        }

        private byte[] currentFrameBytes;

        private void Connect()
        {
            lunaSyncPoseDataWebSocket = new WebSocketSharp.WebSocket(PoseDataWebSocketUrl);

            lunaSyncPoseDataWebSocket.OnMessage += (sender, e) =>
            {
                this.lunaSyncData = JsonConvert.DeserializeObject<LunaSyncData>(e.Data);

                SetDataInput(nameof(LunaSyncData), e.Data, broadcast: true);
            };
            lunaSyncPoseDataWebSocket.Connect();
        }

        public override void OnFixedUpdate()
        {
            if (!this.UpdateAsset)
            {
                return;
            }

            if (this.CameraAsset == null)
            {
                return;
            }
            GameObject cameraGameObject = this.CameraAsset?.GameObject;
            if (cameraGameObject == null)
            {
                return;
            }

            this.CameraAsset.Transform.Position = this.lunaSyncData.position;
            this.CameraAsset.Transform.RotationQuaternion = this.lunaSyncData.rotation;
        }
    }
}
