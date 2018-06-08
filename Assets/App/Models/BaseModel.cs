using System;
using System.Collections.Generic;
using System.Linq;
using App.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace App.Models
{
    public abstract class BaseModel
    {
        public Guid Id;

        protected void SaveJson(string type)
        {
            SocketService.GetInstance().SaveJson(Id, type, ToJson());
        }

        protected void AddUpdateListener()
        {
            if (SocketService.GetInstance().OnUpdate.ContainsKey(Id))
            {
                Debug.LogError("id already registered as a listener");
            }
            SocketService.GetInstance().OnUpdate[Id] = FromJson;
        }

        public void Fetch(Action<bool> callback)
        {
            SocketService.GetInstance().Fetch(Id, data =>
            {
                if (data == null)
                {
                    callback(false);
                    return;
                }

                FromJson(data);
                callback(true);
            });
        }

        private class WritablePropertiesOnlyResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                var props = base.CreateProperties(type, memberSerialization);
                return props.Where(p => p.Writable).ToList();
            }
        }

        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new WritablePropertiesOnlyResolver(),
            NullValueHandling = NullValueHandling.Include
        };

        protected static JToken ConvertToken(object data)
        {
            if (data == null) return null;
            return JToken.FromObject(data, JsonSerializer.CreateDefault(JsonSettings));
        }

        public abstract JObject ToJson();

        public abstract void FromJson(JObject data);

        public abstract void Save();
    }
}
