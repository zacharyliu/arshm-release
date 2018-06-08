using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
using Quobject.SocketIoClientDotNet.Client;

using UnityEngine;

public class Visualizer : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem m_PointCloudParticlePrefab;

    private static int MAX_PARTICLES = 1000;
    
    private float m_ParticleSize = 0.02f;
    private ParticleSystem m_ParticleSystem;
    private ParticleSystem.Particle[] m_Particles;
    
    private LineRenderer _lineRenderer;

    private void Start()
    {
        m_ParticleSystem = Instantiate(m_PointCloudParticlePrefab);
        
        _lineRenderer = GetComponent<LineRenderer>();

        SocketConnection.Instance.On("pointcloud", json =>
        {
            var points = (JArray) json["data"];
            var output = points.Select(p => new Vector3((float) p[0], (float) p[1], (float) p[2])).ToList();
            Debug.Log(output);

            var numParticles = output.Count;
            if (m_Particles == null || m_Particles.Length != numParticles)
                m_Particles = new ParticleSystem.Particle[numParticles];

            for (int i = 0; i < numParticles; ++i)
            {
                m_Particles[i].position = output[i];
                m_Particles[i].startColor = new Color(1.0f, 1.0f, 1.0f);
                m_Particles[i].startSize = m_ParticleSize;
            }

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                m_ParticleSystem.SetParticles(m_Particles, m_Particles.Length);
            });
        });

        SocketConnection.Instance.On("poses", json =>
        {
            var poseData = (JArray) json["data"];
            var positions = poseData
                .Select(p => p["position"])
                .Select(p => new Vector3((float) p["x"], (float) p["y"], (float) p["z"]))
                .ToArray();

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                _lineRenderer.positionCount = positions.Length;
                _lineRenderer.SetPositions(positions);
            });
        });
    }

    private void Update()
    {
    }
}
