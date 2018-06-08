using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace App.Models
{
    [Serializable]
    public class AnnotationModel : BaseModel
    {
        public Pose CameraPose;
        public float Aspect;
        public ImageModel Image;
        [Range(0, 120)] public float Fov = 60;
        public bool IsAnchored;
        public Vector3 AnchorPosition;
        public List<List<Vector2>> DrawLines;
        public List<List<Vector3>> SurfaceDrawLines;

        public AnnotationModel(JObject json)
        {
            Id = new Guid(json["id"].ToString());
            FromJson((JObject) json["data"]);
        }

        public AnnotationModel(Pose cameraPose, float aspect, ImageModel image)
        {
            Id = Guid.NewGuid();
            CameraPose = cameraPose;
            Aspect = aspect;
            Image = image;
            AddUpdateListener();
        }

        public override JObject ToJson()
        {
            return new JObject
            {
                ["CameraPose"] = ConvertToken(CameraPose),
                ["Aspect"] = Aspect,
                ["Image"] = Image.Id,
                ["Fov"] = Fov,
                ["IsAnchored"] = IsAnchored,
                ["AnchorPosition"] = ConvertToken(AnchorPosition),
                ["DrawLines"] = ConvertToken(DrawLines),
                ["SurfaceDrawLines"] = ConvertToken(SurfaceDrawLines)
            };
        }

        public sealed override void FromJson(JObject data)
        {
            CameraPose = data["CameraPose"].ToObject<Pose>();
            Aspect = data["Aspect"].ToObject<float>();

            var imageId = data["Image"].ToObject<string>();
            if (Image == null || Image.Id.ToString() != imageId)
            {
                Image = new ImageModel(new Guid(imageId));
            }

            Fov = data["Fov"].ToObject<float>();
            IsAnchored = data["IsAnchored"].ToObject<bool>();
            AnchorPosition = data["AnchorPosition"].ToObject<Vector3>();
            DrawLines = data["DrawLines"].ToObject<List<List<Vector2>>>();
            SurfaceDrawLines = data["SurfaceDrawLines"].ToObject<List<List<Vector3>>>();
        }

        public override void Save()
        {
            SaveJson("annotation");
        }
    }
}
