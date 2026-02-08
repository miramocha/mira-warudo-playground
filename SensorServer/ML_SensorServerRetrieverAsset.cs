using System.Globalization;
using System.Net.WebSockets;
using System.Numerics;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Warudo.Core;
using Warudo.Core.Attributes;
using Warudo.Core.Data;
using Warudo.Core.Data.Models;
using Warudo.Core.Scenes;
using Warudo.Core.Utils;
using Warudo.Plugins.Core;
using Warudo.Plugins.Core.Assets;
using WebSocketSharp;

// using WebSocketSharp.Websocket;

namespace Warudo.Plugins.Scene.Assets
{

    public class SensorData
    {
        public float[] value;
        public long timestamp;
        public float accuracy;
    }

    [AssetType(
        Id = "ml-sensor-server-retriever",
        Category = "Tracking",
        Title = "ML Sensor Server Retriever"
    )]
    public class MLS_SensorServerRetrieverAsset : Asset
    {
        [DataInput]
        [Label("ASSET")]
        public GameObjectAsset Asset;

        [DataInput]
        [Label("Smooth Rotation")]
        public bool SmoothRotation = true;

        [Markdown]
        public string DebugInfo = "Sensor Info will appear here";

        protected override void OnCreate()
        {
            base.OnCreate();
            StartConnection();
            SetActive(true);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ws.Close();
        }

        private WebSocketSharp.WebSocket ws;
        private Vector3 trackerOrientation = Vector3.zero;

        private void StartConnection()
        {
            ws = new WebSocketSharp.WebSocket(
                "ws://Pixel-9a.attlocal.net:5555/sensor/connect?type=android.sensor.orientation"
            );
            ws.OnMessage += (sender, e) =>
            {
                SetDataInput(nameof(DebugInfo), "Received Sensor Data: " + e.Data, broadcast: true);
                SensorData sensorData = JsonConvert.DeserializeObject<SensorData>(e.Data);
                this.trackerOrientation = new Vector3(
                    sensorData.value[0],
                    sensorData.value[1],
                    sensorData.value[2]
                );
            };
            ws.Connect();
        }

        public override void OnLateUpdate()
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

            Vector3 currentRotation = this.Asset.Transform.Rotation;

            if (!this.SmoothRotation)
            {
                this.Asset.Transform.Rotation = this.trackerOrientation;
                return;
            }

            this.Asset.Transform.Rotation = Vector3.Slerp(
                this.Asset.Transform.Rotation,
                this.trackerOrientation,
                Time.deltaTime * 10f
            );
        }
    }
}
