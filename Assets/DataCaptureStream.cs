using System;
using System.Collections.Generic;
using System.Linq;
using App;
using Mapbox.Unity.Location;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Quobject.SocketIoClientDotNet.Client;
using UnityARInterface;
using UnityEngine;

public class DataCaptureStream : MonoBehaviour {

    private ILocationProvider _locationProvider;
    private ARInterface.PointCloud _pointCloud;
    private float _deltaTimePointCloud;
    private float _deltaTimePose;
    private Pose _pose;
    private float _deltaTimeImage;
    private float _deltaTimeLocation;

    public string Url = "http://zach.princeton.edu:5000";
    private Socket _socket;
    private bool _sendImage;

//    private void OnGUI()
//    {
//        HostName = GUILayout.TextField(HostName);
//        int.TryParse(GUILayout.TextField(HostPort.ToString()), out HostPort);
//
//        if (GUILayout.Button("Connect"))
//        {
//            _client = new UdpClient(HostName, HostPort);
//        }
//    }

    private void OnGUI()
    {
        _sendImage = GUILayout.Toggle(_sendImage, "Send Image");
    }

    // Use this for initialization
    void Start()
    {
        _locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;
        _locationProvider.OnLocationUpdated += OnLocationUpdated;
        
        Debug.Log("connecting...");
        _socket = IO.Socket(Url);

        _socket.On(Socket.EVENT_CONNECT, () => { Debug.Log("connected"); });
    }

    // Update is called once per frame
    void Update()
    {
        // Send pose update
        _deltaTimePose += Time.unscaledDeltaTime;
        if (_deltaTimePose >= 1.0/5)
        {
            if (ARInterface.GetInterface().TryGetPose(ref _pose))
            {
                SendData("pose", _pose);

                _deltaTimePose = 0;
            }
        }

        // Send point cloud update
        _deltaTimePointCloud += Time.unscaledDeltaTime;
        if (_deltaTimePointCloud >= 1.0/5)
        {
            if (ARInterface.GetInterface().TryGetPointCloud(ref _pointCloud))
            {
                var data = _pointCloud.points.ToArray();

                SendData("pointcloud", data);

                _deltaTimePointCloud = 0;
            }
        }
        
        // Send image update
        _deltaTimeImage += Time.unscaledDeltaTime;
        if (_sendImage && _deltaTimeImage >= 1.0)
        {
            var png = ArUtils.GetCameraImage(ARInterface.GetInterface());
            
            if (png != null)
            {
                SendData("image", png);
                
                _deltaTimeImage = 0;
            }
        }
    }
    
    class WritablePropertiesOnlyResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> props = base.CreateProperties(type, memberSerialization);
            return props.Where(p => p.Writable).ToList();
        }
    }

    private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
    {
        ContractResolver = new WritablePropertiesOnlyResolver()
    };

    private void SendData(string label, object data)
    {
        var json = new JObject
        {
            ["label"] = label,
            ["time"] = Time.time,
            ["data"] = JToken.FromObject(data, JsonSerializer.CreateDefault(JsonSettings))
        };

        // Send data
        _socket.Emit("data", json);
    }

    private void OnLocationUpdated(Location location)
    {
        _deltaTimeLocation += Time.unscaledDeltaTime;
        if (_deltaTimeLocation >= 1.0/5)
        {
            SendData("location", location);

            _deltaTimeLocation = 0;
        }
    }
    
    private void OnApplicationQuit()
    {
        _socket.Disconnect();
    }
}
