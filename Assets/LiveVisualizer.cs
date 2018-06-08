using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class LiveVisualizer : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem m_PointCloudParticlePrefab;

    private static int MAX_PARTICLES = 2000;
    
    private float m_ParticleSize = 0.02f;
    private ParticleSystem m_ParticleSystem;
    private ParticleSystem.Particle[] m_Particles = new ParticleSystem.Particle[MAX_PARTICLES];
    private int nextParticle;

    private float lastPointCloudUpdate;
    private bool _showPastPoints;

    private void OnGUI()
    {
        _showPastPoints = GUILayout.Toggle(_showPastPoints, "Show Past Points");
    }

    private void Start()
    {
        m_ParticleSystem = Instantiate(m_PointCloudParticlePrefab);

        var startTime = DateTime.Now;
        
        SocketConnection.Instance.On("data", json =>
        {
            switch (json["label"].ToObject<string>())
            {
                case "pointcloud":
                    var time = (float) DateTime.Now.Subtract(startTime).TotalMilliseconds / 1000;
                    if (time - lastPointCloudUpdate < 0.5) break;
                    lastPointCloudUpdate = time;

                    Task.Run(() =>
                    {
                        var points = (JArray) json["data"];
                        
                        Debug.Log("pointcloud1");
                        
                        var output = points.Select(p => new Vector3((float) p["x"], (float) p["y"], (float) p["z"])).ToList();
                        
                        Debug.Log("pointcloud2");

                        if (!_showPastPoints)
                        {
                            nextParticle = 0;
                        }

                        var numParticles = Math.Min(output.Count, m_Particles.Length);
                        for (int i = 0; i < numParticles; ++i)
                        {
                            m_Particles[nextParticle].position = output[i];
                            m_Particles[nextParticle].startColor = new Color(1.0f, 1.0f, 1.0f);
                            m_Particles[nextParticle].startSize = m_ParticleSize;

                            nextParticle = (nextParticle + 1) % m_Particles.Length;
                        }

                        UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        {
                            m_ParticleSystem.SetParticles(m_Particles,
                                _showPastPoints ? m_Particles.Length : numParticles);
                        });
                    });
                    
                    break;

                case "pose":
                    var position = ((JObject) json["data"])["position"];
                    var pos = new Vector3((float) position["x"], (float) position["y"], (float) position["z"]);

                    UnityMainThreadDispatcher.Instance().Enqueue(() => { transform.position = pos; });
                    break;

                case "annotate":
                    Task.Run(() =>
                    {
                        var image = json["data"]["image"].ToObject<byte[]>();
                        var aspect = json["data"]["aspect"].ToObject<float>();
                        var pose = json["data"]["pose"].ToObject<Pose>();
                        var anchor = json["data"]["anchor"].ToObject<Pose>();
            
                        UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        {
                            var texture = new Texture2D(0, 0, TextureFormat.RGB24, false);
                            texture.LoadImage(image);
                            
                            var plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
                            plane.GetComponent<MeshRenderer>().material.mainTexture = texture;
                            plane.transform.localRotation = pose.rotation;
                            plane.transform.localScale = new Vector3(aspect, 1, 1) / 3;
                            plane.transform.position = pose.position;

                            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            sphere.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
                            sphere.transform.position = anchor.position;
                            sphere.transform.rotation = anchor.rotation;

                            var line = plane.AddComponent<LineRenderer>();
                            line.startWidth = line.endWidth = 0.02f;
                            line.positionCount = 2;
                            line.SetPositions(new[]
                            {
                                pose.position,
                                anchor.position
                            });
                        });
                    });
                    break;
            }
        });
    }

    private void Update()
    {
    }
}
