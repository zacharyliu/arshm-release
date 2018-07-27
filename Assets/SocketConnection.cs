using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
using Quobject.SocketIoClientDotNet.Client;
using UnityEngine;

public class SocketConnection : MonoBehaviour
{
    public static SocketConnection Instance;
    
    public string Url = "http://0.0.0.0:5000/";
    private Socket _socket;

    private void Awake()
    {
        Instance = this;
        
        Debug.Log("connecting...");
        _socket = IO.Socket(Url);

        _socket.On(Socket.EVENT_CONNECT, () => { Debug.Log("connected"); });
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

    public Emitter On(string eventString, Action<JObject> fn)
    {
        return _socket.On(eventString, new Listener(fn));
    }
    
    private void OnApplicationQuit()
    {
        _socket.Disconnect();
    }
}
