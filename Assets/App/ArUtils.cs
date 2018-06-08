using System;
using UnityARInterface;
using UnityEngine;

namespace App
{
    public class ArUtils
    {
        // https://stackoverflow.com/a/6959465
        // https://www.fourcc.org/fccyvrgb.php
        public static Color YUVtoRGB(int y, int u, int v)
        {
            var r = Mathf.Clamp01((float) ((1.164 * (y - 16) + 1.596 * (v - 128)) / 255));
            var g = Mathf.Clamp01((float) ((1.164*(y - 16) - 0.813*(v - 128) - 0.391*(u - 128)) / 255));
            var b = Mathf.Clamp01((float) ((1.164*(y - 16) + 2.018*(u - 128)) / 255));

            return new Color(r, g, b);
        }

        public static byte[] GetCameraImage(ARInterface arInterface)
        {
            var cameraImage = new ARInterface.CameraImage();
            if (arInterface.TryGetCameraImage(ref cameraImage))
            {
                var displayTransform = arInterface.GetDisplayTransform();
                var textureSize = displayTransform.MultiplyVector(new Vector3(cameraImage.width, cameraImage.height));
                var textureWidth = Math.Abs((int) textureSize.x);
                var textureHeight = Math.Abs((int) textureSize.y);
                var inverse = displayTransform.inverse;
                var texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGB24, false);
                for (int j = 0; j < cameraImage.height; j++)
                {
                    for (var i = 0; i < cameraImage.width; i++)
                    {
                        var textureIdx = inverse.MultiplyVector(new Vector3(i, j));
                        var x = textureWidth - Math.Abs((int) textureIdx.x) - 1;
                        var y = textureHeight - Math.Abs((int) textureIdx.y) - 1;
                        var idxY = j * cameraImage.width + i;
                        var idxUV = j / 2 * cameraImage.width / 2 + i / 2;
                        texture.SetPixel(x, y, YUVtoRGB(cameraImage.y[idxY], cameraImage.uv[idxUV*2], cameraImage.uv[idxUV*2+1]));
                    }
                }
                texture.Apply();
                var png = texture.EncodeToPNG();
                return png;
            }

            return null;
        }

        public static Vector2 GetRectMousePosition(RectTransform rectTransform)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out localPoint);
            var normalized = new Vector2(0.5f + localPoint.x / rectTransform.rect.width, 0.5f + localPoint.y / rectTransform.rect.height);
            return normalized;
        }
    }
}
