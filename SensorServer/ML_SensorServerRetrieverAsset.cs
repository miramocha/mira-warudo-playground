using System.Globalization;
using System.Net.WebSockets;
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
using WebSocketSharp;

// using WebSocketSharp.Websocket;

namespace Warudo.Plugins.Scene.Assets
{
    public class SensorData
    {
        public float[] values;
        public long timestamp;
        public float accuracy;

        public Vector3 GetDataAsVector3()
        {
            if (values != null && values.Length >= 3)
            {
                return new Vector3(values[0], values[1], values[2]);
            }
            return Vector3.zero;
        }

        public Quaternion GetDataAsQuaternion()
        {
            if (values != null && values.Length >= 4)
            {
                return new Quaternion(values[0], values[1], values[2], values[3]);
            }
            return Quaternion.identity;
        }
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
        public bool SmoothRotation = false;

        [DataInput]
        [Label("Use Game Rotation Vector")]
        public bool UseGameRotationVector = false;

        [DataInput]
        [Label("Calculate Position")]
        public bool CalculatePosition = false;

        public string BaseWebSocketURL = "ws://Pixel-9a.attlocal.net:5555/sensor/connect?type=";

        [Markdown]
        [Label("Orentation Info")]
        public string OrientationInfo = "Orientation Info will appear here";

        [Markdown]
        [Label("Game Rotation Vector Info")]
        public string GameRotationVectorInfo = "Game Rotation Vector Info will appear here";

        [Markdown]
        [Label("Linear Acceleration Info")]
        public string LinearAccelerationInfo = "Linear Acceleration Info will appear here";

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

        private WebSocketSharp.WebSocket orientationWebSocket;
        private WebSocketSharp.WebSocket gameRotationVectorWebSocket;
        private WebSocketSharp.WebSocket linearAccelerationWebSocket;

        private SensorData oritenationData = new SensorData();
        private SensorData gameRotationVectorData = new SensorData();
        private SensorData linearAccelerationData = new SensorData();
        private Vector3 velocity = Vector3.zero;

        [Trigger]
        public void Reconnect()
        {
            Disconnect();
            Connect();
        }

        private void Disconnect()
        {
            orientationWebSocket.Close();
            gameRotationVectorWebSocket.Close();
            linearAccelerationWebSocket.Close();
        }

        private void Connect()
        {
            this.velocity = Vector3.zero;
            orientationWebSocket = new WebSocketSharp.WebSocket(
                BaseWebSocketURL + "android.sensor.orientation"
            );
            orientationWebSocket.OnMessage += (sender, e) =>
            {
                this.oritenationData = JsonConvert.DeserializeObject<SensorData>(e.Data);
                SetDataInput(nameof(OrientationInfo), e.Data, broadcast: true);
            };
            orientationWebSocket.Connect();

            gameRotationVectorWebSocket = new WebSocketSharp.WebSocket(
                BaseWebSocketURL + "android.sensor.game_rotation_vector"
            );
            gameRotationVectorWebSocket.OnMessage += (sender, e) =>
            {
                this.gameRotationVectorData = JsonConvert.DeserializeObject<SensorData>(e.Data);
                SetDataInput(nameof(GameRotationVectorInfo), e.Data, broadcast: true);
            };
            gameRotationVectorWebSocket.Connect();

            linearAccelerationWebSocket = new WebSocketSharp.WebSocket(
                BaseWebSocketURL + "android.sensor.linear_acceleration"
            );
            linearAccelerationWebSocket.OnMessage += (sender, e) =>
            {
                this.linearAccelerationData = JsonConvert.DeserializeObject<SensorData>(e.Data);
                SetDataInput(nameof(LinearAccelerationInfo), e.Data, broadcast: true);
            };
            linearAccelerationWebSocket.Connect();
        }

        public override void OnFixedUpdate()
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

            if (this.UseGameRotationVector)
            {
                this.updateWithGameRotationVector();
            }
            else
            {
                this.updateWithOrientation();
            }

            if (this.CalculatePosition)
            {
                this.velocity =
                    this.velocity
                    + this.linearAccelerationData.GetDataAsVector3() * Time.fixedDeltaTime / 100f;
                this.Asset.Transform.Position =
                    this.Asset.Transform.Position + this.velocity * Time.fixedDeltaTime;
            }
        }

        private void updateWithOrientation()
        {
            Vector3 currentRotation = this.Asset.Transform.Rotation;

            if (!this.SmoothRotation)
            {
                this.Asset.Transform.Rotation = this.oritenationData.GetDataAsVector3();
                return;
            }

            this.Asset.Transform.Rotation = Vector3.Slerp(
                this.Asset.Transform.Rotation,
                this.oritenationData.GetDataAsVector3(),
                Time.fixedDeltaTime * 10f
            );
        }

        private void updateWithGameRotationVector()
        {
            Quaternion currentRotation = this.Asset.Transform.RotationQuaternion;

            if (!this.SmoothRotation)
            {
                this.Asset.Transform.RotationQuaternion =
                    this.gameRotationVectorData.GetDataAsQuaternion();
                return;
            }

            this.Asset.Transform.RotationQuaternion = Quaternion.Slerp(
                this.Asset.Transform.RotationQuaternion,
                this.gameRotationVectorData.GetDataAsQuaternion(),
                Time.fixedDeltaTime * 10f
            );
        }
    }
}
