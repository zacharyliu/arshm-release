using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace App.Models
{
    [Serializable]
    public class ImageModel : BaseModel
    {
        public byte[] Data;

        // TODO: cleanup callbacks
        public event EventHandler OnUpdate;

        public Texture Texture
        {
            get
            {
                if (Data == null)
                {
                    return null;
                }

                var texture = new Texture2D(0, 0, TextureFormat.RGB24, false);
                texture.LoadImage(Data);
                return texture;
            }
        }

        public ImageModel(byte[] data)
        {
            Id = Guid.NewGuid();
            Data = data;
            AddUpdateListener();
        }

        public ImageModel(Guid id)
        {
            Id = id;
            AddUpdateListener();
        }

        public override JObject ToJson()
        {
            return new JObject
            {
                ["Data"] = Data
            };
        }

        public override void FromJson(JObject data)
        {
            Data = data["Data"].ToObject<byte[]>();

            // TODO: cleanup callbacks
            OnUpdate?.Invoke(this, EventArgs.Empty);
        }

        public override void Save()
        {
            SaveJson("image");
        }
    }
}
