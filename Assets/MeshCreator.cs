using System;
using System.Collections;
using System.Collections.Generic;
using UnityARInterface;
using UnityEngine;

public class MeshCreator : MonoBehaviour
{
//	[SerializeField]
//	private ParticleSystem m_PointCloudParticlePrefab;
//
//	[SerializeField]
//	private int m_MaxPointsToShow = 300;
//
//	[SerializeField]
//	private float m_ParticleSize = 1.0f;

//	private ParticleSystem m_ParticleSystem;
//	private ParticleSystem.Particle [] m_Particles;
//	private ParticleSystem.Particle[] m_NoParticles;
	private ARInterface.PointCloud m_PointCloud;
	private Mesh m_Mesh;

	private float _lastUpdate;

	// Use this for initialization
	void Start()
	{
//		m_ParticleSystem = Instantiate(m_PointCloudParticlePrefab);
//		m_NoParticles = new ParticleSystem.Particle[1];
//		m_NoParticles[0].startSize = 0f;

		m_Mesh = GetComponent<MeshFilter>().mesh;
		m_Mesh.Clear();
	}

	// Update is called once per frame
	void Update()
	{
		if (Time.time > _lastUpdate + 0.1)
		{
			_lastUpdate = Time.time;
			
			if (ARInterface.GetInterface().TryGetPointCloud(ref m_PointCloud))
			{
				if (m_PointCloud.points.Count > 10)
				{
					var vertices = m_PointCloud.points.ToArray();
//					var indices = ConvexHull.Generate(vertices);
	//				Console.WriteLine("indices: " + indices.Length);
					m_Mesh.Clear();
					m_Mesh.vertices = vertices;
//					m_Mesh.triangles = indices;
				}

				//			var numParticles = Mathf.Min(m_PointCloud.points.Count, m_MaxPointsToShow);
				//			if (m_Particles == null || m_Particles.Length != numParticles)
				//				m_Particles = new ParticleSystem.Particle[numParticles];
				//
				//			for (int i = 0; i < numParticles; ++i)
				//			{
				//				m_Particles[i].position = m_PointCloud.points[i];
				//				m_Particles[i].startColor = new Color(1.0f, 1.0f, 1.0f);
				//				m_Particles[i].startSize = m_ParticleSize;
				//			}
				//
				//			m_ParticleSystem.SetParticles(m_Particles, numParticles);
			}
			else
			{
//				m_Mesh.Clear();
				
	//			m_ParticleSystem.SetParticles(m_NoParticles, 1);
			}
		}
	}
}
