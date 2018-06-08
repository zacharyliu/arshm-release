using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Mapbox.Unity.Location;
using UnityARInterface;
using UnityEngine;

public class DataCapture : MonoBehaviour
{
    private ILocationProvider _locationProvider;
    private ARInterface.PointCloud _pointCloud;
    private UdpClient _client;
    private float _deltaTimePointCloud;
    private float _deltaTimePose;
    private Pose _pose;
    
    public string HostName = "zach.princeton.edu";
    public int HostPort = 10000;

    private void OnGUI()
    {
        HostName = GUILayout.TextField(HostName);
        int.TryParse(GUILayout.TextField(HostPort.ToString()), out HostPort);

        if (GUILayout.Button("Connect"))
        {
            _client = new UdpClient(HostName, HostPort);            
        }
    }

    // Use this for initialization
    void Start()
    {
        _locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;
        _locationProvider.OnLocationUpdated += OnLocationUpdated;
    }

    // Update is called once per frame
    void Update()
    {
        // Send pose update
        _deltaTimePose += Time.unscaledDeltaTime;
        if (_deltaTimePose >= 1.0/30)
        {
            if (ARInterface.GetInterface().TryGetPose(ref _pose))
            {
                var data = JsonUtility.ToJson(_pose);
                SendData("pose", data);

                _deltaTimePose = 0;
            }
        }

        // Send point cloud update
        _deltaTimePointCloud += Time.unscaledDeltaTime;
        if (_deltaTimePointCloud >= 1.0/10)
        {
            if (ARInterface.GetInterface().TryGetPointCloud(ref _pointCloud))
            {
                var data = string.Join(";", _pointCloud.points
                    .Select(x => new SerializableVector3(x.x, x.y, x.z))
                    .Select(JsonUtility.ToJson)
                    .ToArray());
                SendData("pointcloud", data);

                _deltaTimePointCloud = 0;
            }
        }
    }

    private void SendData(string label, string data)
    {
        if (_client == null)
        {
            return;
        }
        
        var formattedData = $"{label}|{Time.time}|{data}";
        
        // Convert the string to bytes
        var byteData = Encoding.ASCII.GetBytes(formattedData);
        
        // Send data
        _client.BeginSend(byteData, byteData.Length, SendCallback, _client);
    }

    private void OnLocationUpdated(Location location)
    {
        var data = JsonUtility.ToJson(location);
        SendData("location", data);
    }

    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.  
            var client = (Socket) ar.AsyncState;

            // Complete sending the data to the remote device.  
            client.EndSend(ar);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
}