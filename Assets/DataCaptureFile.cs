using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mapbox.Unity.Location;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UnityARInterface;
using UnityEngine;

public class DataCaptureFile : MonoBehaviour {

    private ILocationProvider _locationProvider;
    private ARInterface.PointCloud _pointCloud;
    private float _deltaTimePointCloud;
    private float _deltaTimePose;
    private Pose _pose;
    private float _deltaTimeLocation;
    private string _filename;
    private StreamWriter _writer;

    // Use this for initialization
    void Start()
    {
        _locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;
        _locationProvider.OnLocationUpdated += OnLocationUpdated;
    }

    private void OnGUI()
    {
        var buttonRect = new Rect((Screen.width / 2) - 200, (Screen.height / 2) - 200, 400, 100);

        if (_writer == null)
        {
            if (GUI.Button(buttonRect, "Start Recording"))
                StartRecording();
        }
        else
        {
            var labelRect = new Rect(Screen.width / 2 - 200, Screen.height / 2, 400, 100);
            GUI.Label(labelRect, "Saving to: " + _filename);
            if (GUI.Button(buttonRect, "Stop"))
                StopRecording();
        }
    }

    public void StartRecording()
    {
        StopRecording();
        _filename = Path.Combine(
            Application.persistentDataPath,
            DateTime.UtcNow.ToString("s",
                System.Globalization.CultureInfo.InvariantCulture) + ".txt");
        _writer = new StreamWriter(_filename);
    }

    public void StopRecording()
    {
        _writer?.Close();
        _writer = null;
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
                SaveData("pose", _pose);

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

                SaveData("pointcloud", data);

                _deltaTimePointCloud = 0;
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

    private void SaveData(string label, object data)
    {
        if (_writer == null) return;

        var json = new JObject
        {
            ["label"] = label,
            ["time"] = Time.time,
            ["data"] = JToken.FromObject(data, JsonSerializer.CreateDefault(JsonSettings))
        };

        _writer.WriteLine(json.ToString(Formatting.None));
    }

    private void OnLocationUpdated(Location location)
    {
        _deltaTimeLocation += Time.unscaledDeltaTime;
        if (_deltaTimeLocation >= 1.0/5)
        {
            SaveData("location", location);

            _deltaTimeLocation = 0;
        }
    }

    private void OnApplicationQuit()
    {
        StopRecording();
    }
}
