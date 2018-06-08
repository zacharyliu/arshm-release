using System;
using System.IO;
using System.Linq;
using UnityARInterface;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;

public class Capture : MonoBehaviour
{
    private ARInterface _arInterface;

    [SerializeField]
    private ParticleSystem m_PointCloudParticlePrefab;

    private static int MAX_PARTICLES = 1000;
    
    private float m_ParticleSize = 0.02f;
    private ParticleSystem m_ParticleSystem;
    private ParticleSystem.Particle[] m_Particles = new ParticleSystem.Particle[MAX_PARTICLES];
    private int nextParticle;
    private ARInterface.PointCloud m_PointCloud;
    private float timeSincePointCloudUpdate = Single.PositiveInfinity;
    
    private static MatrixBuilder<float> FloatMatrix = Matrix<float>.Build;
    private Matrix<float> ParticleMatrix = FloatMatrix.Dense(4, MAX_PARTICLES);

    private Matrix<float> CameraMatrix = FloatMatrix.Dense(4, 4);
    private GameObject _surface;
    
    private int MeshSize = 19; // must be of form 2^a*3^b+1
    private Vector3[] _vertices;
    private int[,] _verticesIdx;
    private double[,] _verticesZ;
    private double[,] _verticesWeight;

    class CaptureInstance
    {
//        public float time;
//        public ARInterface.CameraImage image;
        public Pose pose;
        public Matrix4x4 displayTransform;
        public float aspect;

        public CaptureInstance(Pose pose, Matrix4x4 displayTransform, float aspect)
        {
            this.pose = pose;
            this.displayTransform = displayTransform;
            this.aspect = aspect;
        }

        public static CaptureInstance fromArInterface(ARInterface arInterface)
        {
            var pose = new Pose();
            arInterface.TryGetPose(ref pose);
            var displayTransform = arInterface.GetDisplayTransform();
            var aspect = Camera.main.aspect;

            return new CaptureInstance(pose, displayTransform, aspect);
        }
        
        // https://answers.unity.com/questions/402280/how-to-decompose-a-trs-matrix.html
        public Quaternion getRotation()
        {
            return pose.rotation * Quaternion.Inverse(Quaternion.LookRotation(displayTransform.GetColumn(2), displayTransform.GetColumn(1)));
        }
        
        // https://answers.unity.com/questions/402280/how-to-decompose-a-trs-matrix.html
        public Vector3 getScale()
        {
            return new Vector3(
                displayTransform.GetColumn(0).magnitude,
                displayTransform.GetColumn(1).magnitude,
                displayTransform.GetColumn(2).magnitude
            );
        }
    }

    private void Start()
    {
        _arInterface = ARInterface.GetInterface();
        m_ParticleSystem = Instantiate(m_PointCloudParticlePrefab, Camera.main.transform.parent);
        
        _surface = GameObject.CreatePrimitive(PrimitiveType.Quad);
        var mesh = _surface.GetComponent<MeshFilter>().mesh;
        MeshHelper.Subdivide(mesh, MeshSize - 1);
        _surface.GetComponent<MeshFilter>().mesh = mesh;
//        _surface.transform.parent = Camera.main.transform;
//        _surface.transform.localScale = new Vector3(Camera.main.aspect, 1, 1);
//        _surface.transform.position = new Vector3(0, 0, 1);

        _vertices = mesh.vertices;
        _verticesIdx = new int[MeshSize, MeshSize];
        _verticesZ = new double[MeshSize, MeshSize];
        _verticesWeight = new double[MeshSize, MeshSize];
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            var u = (int) Math.Round((mesh.vertices[i].x + 0.5) * (MeshSize - 1));
            var v = (int) Math.Round((mesh.vertices[i].y + 0.5) * (MeshSize - 1));
            _verticesIdx[u, v] = i;
        }
    }

    private void Update()
    {
        // Update camera matrix
        var matrixTrs = Matrix4x4.TRS(Camera.main.transform.position, Camera.main.transform.rotation, new Vector3(1, 1, 1));
        var matrixCombined = Camera.main.nonJitteredProjectionMatrix * matrixTrs.inverse;
        LoadMatrix(CameraMatrix, matrixCombined);

        // Update point cloud
        timeSincePointCloudUpdate += Time.deltaTime;
        if (timeSincePointCloudUpdate > 0)
        {
            if (ARInterface.GetInterface().TryGetPointCloud(ref m_PointCloud))
            {
                timeSincePointCloudUpdate = 0;
            
                // testing reset
                nextParticle = 0;
                
                for (int i = 0; i < m_PointCloud.points.Count; ++i)
                {
                    if (m_ParticleSystem != null)
                    {
                        m_Particles[nextParticle].position = m_PointCloud.points[i];
                        m_Particles[nextParticle].startColor = new Color(1.0f, 1.0f, 1.0f);
                        m_Particles[nextParticle].startSize = m_ParticleSize;
                    }

                    var p = m_PointCloud.points[i];
                    ParticleMatrix.SetColumn(nextParticle, new[] {p[0], p[1], p[2], 1});

                    nextParticle += 1;
                    
                    // testing reset
                    if (nextParticle == MAX_PARTICLES)
                    {
                        break;
                    }
                    
                    nextParticle %= MAX_PARTICLES;
                }

                if (m_ParticleSystem != null)
                {
                    m_ParticleSystem.SetParticles(m_Particles, m_Particles.Length);
                }
            }
        }
        
        // Reset buffers
        for (int i = 0; i < MeshSize; i++)
        {
            for (int j = 0; j < MeshSize; j++)
            {
                _verticesZ[i, j] = 0;
                _verticesWeight[i, j] = 0;
            }
        }

        // Compute z distances
//        var result = CameraMatrix * ParticleMatrix;
//        foreach (var vec in result.EnumerateColumns())
//        {
//            var zCam = -vec[3];
//            if (zCam < 0.1 || zCam > 100)
//            {
//                continue;
//            }
//
//            var x = vec[0] / vec[3];
//            var y = vec[1] / vec[3];
////            var z = vec[2] / vec[3];
//            
//            // if within range
//            if (x > -1 && x < 1 && y > -1 && y < 1)
//            {
//                for (int i = 0; i < MeshSize; i++)
//                {
//                    for (int j = 0; j < MeshSize; j++)
//                    {
//                        var u = (double) 2 * i / MeshSize - 1;
//                        var v = (double) 2 * j / MeshSize - 1;
//                        var distSq = Math.Pow(x - u, 2) + Math.Pow(y - v, 2);
//                        var sigmaSq = Math.Pow(0.1, 2);
//                        var weight = 1.0 / Math.Sqrt(2 * Math.PI * sigmaSq) * Math.Exp(-distSq / (2 * sigmaSq));
//
//                        _verticesZ[i, j] += zCam * weight;
//                        _verticesWeight[i, j] += weight;
//                    }
//                }
//            }
//        }
        
        for (var idx = 0; idx < nextParticle; idx++)
        {
            var vec = ParticleMatrix.Column(idx);
            var pos = Camera.main.WorldToScreenPoint(new Vector3(vec[0], vec[1], vec[2]));
            
            if (pos.z < 0.1 || pos.z > 100)
            {
                continue;
            }
            
            // if within range
            if (true)
            {
                for (int i = 0; i < MeshSize; i++)
                {
                    for (int j = 0; j < MeshSize; j++)
                    {
                        var u = (double) i / MeshSize * Camera.main.pixelWidth;
                        var v = (double) j / MeshSize * Camera.main.pixelHeight;
                        var distSq = Math.Pow(pos.x - u, 2) + Math.Pow(pos.y - v, 2);
                        var sigmaSq = Math.Pow(10, 2);
                        var weight = 1.0 / Math.Sqrt(2 * Math.PI * sigmaSq) * Math.Exp(-distSq / (2 * sigmaSq));

                        _verticesZ[i, j] += pos.z * weight;
                        _verticesWeight[i, j] += weight;
                    }
                }
            }
        }

        // Recalculate mesh points
        for (int i = 0; i < MeshSize; i++)
        {
            for (int j = 0; j < MeshSize; j++)
            {
//                var u = (double) 2 * i / MeshSize - 1;
//                var v = (double) 2 * j / MeshSize - 1;
//                var z = _verticesZ[i, j] / _verticesWeight[i, j];
//                var w = 1;
//                var vec = Vector<float>.Build.DenseOfArray(new[] {(float) (u * w), (float) (v * w), (float) (0 * w), (float) w});
//                vec = CameraMatrix.Inverse() * vec;
//                _vertices[_verticesIdx[i, j]] = new Vector3(vec[0] / vec[3], vec[1] / vec[3], vec[2] / vec[3]);
                
                var u = (double) i / MeshSize * Camera.main.pixelWidth;
                var v = (double) j / MeshSize * Camera.main.pixelHeight;
                var z = _verticesZ[i, j] / _verticesWeight[i, j];

                _vertices[_verticesIdx[i, j]] = Camera.main.ScreenToWorldPoint(new Vector3((float) u, (float) v, (float) z));
            }
        }
        
        // Set new mesh
        var mesh = _surface.GetComponent<MeshFilter>().mesh;
        mesh.SetVertices(_vertices.ToList());
//        mesh.triangles = mesh.triangles;
        mesh.RecalculateNormals();
    }

    private void LoadMatrix(Matrix<float> target, Matrix4x4 source)
    {
        for (var i = 0; i < 4; i++)
        {
            for (var j = 0; j < 4; j++)
            {
                target[i, j] = source[i, j];
            }
        }
    }

    // https://stackoverflow.com/a/6959465
    // https://www.fourcc.org/fccyvrgb.php
    public static Color YUVtoRGB(int y, int u, int v)
    {
        var r = Mathf.Clamp01((float) ((1.164 * (y - 16) + 1.596 * (v - 128)) / 255));
        var g = Mathf.Clamp01((float) ((1.164*(y - 16) - 0.813*(v - 128) - 0.391*(u - 128)) / 255));
        var b = Mathf.Clamp01((float) ((1.164*(y - 16) + 2.018*(u - 128)) / 255));

        return new Color(r, g, b);
    }
    
    private void OnGUI()
    {
        if (GUILayout.Button("Take Photo"))
        {
            var cameraImage = new ARInterface.CameraImage();
            if (_arInterface.TryGetCameraImage(ref cameraImage))
            {
                var displayTransform = _arInterface.GetDisplayTransform();
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
                File.WriteAllBytes(Application.persistentDataPath + "/output.png", png);

                File.WriteAllText(Application.persistentDataPath + "/output.json", JsonUtility.ToJson(CaptureInstance.fromArInterface(_arInterface)));
            }
        }

        if (GUILayout.Button("Open Photo"))
        {
            var plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
            
            var png = File.ReadAllBytes(Application.persistentDataPath + "/output.png");
            var texture = new Texture2D(0, 0, TextureFormat.RGB24, false);
            texture.LoadImage(png);
            plane.GetComponent<MeshRenderer>().material.mainTexture = texture;

            var json = File.ReadAllText(Application.persistentDataPath + "/output.json");
            var meta = JsonUtility.FromJson<CaptureInstance>(json);
            plane.transform.localRotation = meta.pose.rotation;
            plane.transform.localScale = new Vector3(meta.aspect, 1, 1) / 3;
            var offset = meta.pose.rotation * new Vector3(0, 0, 0.5f);
            plane.transform.position = meta.pose.position + offset;
        }
    }
}
