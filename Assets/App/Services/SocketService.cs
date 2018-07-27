using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
using Quobject.SocketIoClientDotNet.Client;
using UnityEngine;

// namespace tells Unity what directory of folders the script is located in.
namespace App.Services
{
    public class SocketService : MonoBehaviour
    {
        // Url is the server address. It is unclear whether you need to change this in code, or just in the Unity editor.
        // Either way, you need to change this based on what address the server is listening on.
        // How to set up a Python server is on Zach's GitHub readme.
        public string Url = "http://0.0.0.0:5000/";
        private Socket _socket;

        // TODO: refactor this to use delegates so there can be multiple listeners
        public Dictionary<Guid, Action<JObject>> OnUpdate = new Dictionary<Guid, Action<JObject>>();
        public Dictionary<string, Action<JObject>> OnCreate = new Dictionary<string, Action<JObject>>();

        // An instance of the SocketService.
        private static SocketService _instance;
        public static SocketService GetInstance()
        {
            return _instance;
        }

        // Set the instance to itself immediately when the app starts.
        private void Awake()
        {
            _instance = this;
        }

        private void Start()
        {
            Debug.Log("connecting...");
            _socket = IO.Socket(Url);
            On("model", OnModelData);
            _socket.On(Socket.EVENT_CONNECT, () => { Debug.Log("connected"); });
        }

        private void OnModelData(JObject obj)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                var id = (string) obj["id"];
                if (id == null)
                {
                    Debug.LogError("received an object with no GUID");
                    return;
                }

                var guid = new Guid(id);

                if (OnUpdate.ContainsKey(guid))
                {
                    OnUpdate[guid].Invoke((JObject) obj["data"]);
                    return;
                }

                OnCreate[obj["type"].ToString()].Invoke(obj);
            });
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

        public void SendData(string label, object data)
        {
            var json = new JObject
            {
                ["label"] = label,
                ["time"] = Time.time,
                ["data"] = Convert(data)
            };

            // Send data
            _socket.Emit("data", json);
        }

        public static JToken Convert(object data)
        {
            return JToken.FromObject(data, JsonSerializer.CreateDefault(JsonSettings));
        }

        public class Listener : IListener
        {
            private static int id_counter = 0;
            private int Id;
            private readonly Action<JObject> fn;

            public Listener(Action<JObject> fn)
            {
                this.fn = fn;
                this.Id = id_counter++;
            }

            public void Call(params object[] args)
            {
                if (fn != null)
                {
                    var arg1 = args.Length > 1 ? args[1] : args.Length > 0 ? args[0] : null;
                    fn((JObject) arg1);
                }
            }

            public int CompareTo(IListener other)
            {
                return this.GetId().CompareTo(other.GetId());
            }

            public int GetId()
            {
                return Id;
            }
        }

        private Emitter On(string eventString, Action<JObject> fn)
        {
            return _socket.On(eventString, new Listener(fn));
        }

        private void OnApplicationQuit()
        {
            _socket.Disconnect();
        }

        public void SaveJson(Guid id, string type, JObject json)
        {
            _socket.Emit("model", new JObject
            {
                ["id"] = id,
                ["type"] = type,
                ["data"] = json
            });
        }

        public void Fetch(Guid id, Action<JObject> action)
        {
            _socket.Emit("fetch", json =>
            {
                if (json == null)
                {
                    action(null);
                    return;
                }

                action((JObject) ((JObject) json)["data"]);
            }, id);
        }
    }
}
