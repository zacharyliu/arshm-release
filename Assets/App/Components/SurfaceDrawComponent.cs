using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace App.Components
{
    [RequireComponent(typeof(QuadComponent))]
    public class SurfaceDrawComponent : MonoBehaviour
    {
        public Transform CameraPosition;
        public Transform TargetPosition;
        public bool DrawOn;
        public event EventHandler<List<List<Vector3>>> OnNewLines;

        private bool _isDrawing;
        private LineRenderer _worldLineRenderer;
        private LineRenderer _surfaceLineRenderer;
        private QuadComponent _quad;

        private readonly List<List<Vector3>> _worldLines = new List<List<Vector3>>();
        private readonly List<List<Vector3>> _surfaceLines = new List<List<Vector3>>();

        private void Start()
        {
            _quad = GetComponent<QuadComponent>();
        }

        private void Update()
        {
            if (DrawOn && !_isDrawing)
            {
                _worldLines.Add(new List<Vector3>());
                _surfaceLines.Add(new List<Vector3>());

                _worldLineRenderer = new GameObject().AddComponent<LineRenderer>();
                _worldLineRenderer.startWidth = _worldLineRenderer.endWidth = 0.02f;
                _surfaceLineRenderer = new GameObject().AddComponent<LineRenderer>();
                _surfaceLineRenderer.startWidth = _surfaceLineRenderer.endWidth = 0.02f;
            }
            
            if (DrawOn)
            {
                if (!AddWorldPoint(TargetPosition.position))
                {
                    print("Unable to add new point");
                }
                else
                {
                    var world = _worldLines.Last();
                    _worldLineRenderer.positionCount = world.Count;
                    _worldLineRenderer.SetPositions(world.ToArray());

                    var surface = _surfaceLines.Last();
                    _surfaceLineRenderer.positionCount = surface.Count;
                    _surfaceLineRenderer.SetPositions(surface.ToArray());
                }
            }
            
            if (!DrawOn && _isDrawing)
            {
                _worldLineRenderer = null;
                _surfaceLineRenderer = null;

                OnNewLines?.Invoke(this, _worldLines);
            }
            
            _isDrawing = DrawOn;
        }

        private bool AddWorldPoint(Vector3 worldPoint)
        {
            Vector3 surfacePoint;
            if (!_quad.ProjectWorldToSurface(CameraPosition.position, worldPoint, out surfacePoint))
            {
                print("Could not raycast from camera to target");
                return false;
            }

            _worldLines.Last().Add(worldPoint);
            _surfaceLines.Last().Add(_quad.OffsetForDrawing(surfacePoint));
            return true;
        }
    }
}
