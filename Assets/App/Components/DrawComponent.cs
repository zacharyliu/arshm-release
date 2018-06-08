using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace App.Components
{
    [RequireComponent(typeof(QuadComponent))]
    [RequireComponent(typeof(Collider))]
    public class DrawComponent : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public bool DrawEnabled;
        public bool LineMode;
        public event EventHandler<List<List<Vector2>>> OnNewLines;

        private QuadComponent _quad;
        private LineRenderer _lineRenderer;
        
        private readonly List<List<Vector3>> _surfaceLines = new List<List<Vector3>>();
        private readonly List<List<Vector2>> _localLines = new List<List<Vector2>>();

        private void Start()
        {
            _quad = GetComponent<QuadComponent>();
        }

        private bool AddSurfacePoint(Vector3 surfacePoint)
        {
            Vector2 localPoint;
            if (!_quad.ProjectSurfaceToLocal(surfacePoint, out localPoint))
            {
                print("out of bounds");
                return false;
            }
            _surfaceLines.Last().Add(_quad.OffsetForDrawing(surfacePoint));
            _localLines.Last().Add(localPoint);
            return true;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!DrawEnabled) return;

            _surfaceLines.Add(new List<Vector3>());
            _localLines.Add(new List<Vector2>());

            AddSurfacePoint(eventData.pointerCurrentRaycast.worldPosition);

            _lineRenderer = new GameObject().AddComponent<LineRenderer>();
            _lineRenderer.startWidth = _lineRenderer.endWidth = 0.02f;
            
            if (LineMode)
            {
                AddSurfacePoint(eventData.pointerCurrentRaycast.worldPosition);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!DrawEnabled) return;

            if (AddSurfacePoint(eventData.pointerCurrentRaycast.worldPosition))
            {
                if (LineMode)
                {
                    _surfaceLines.Last().RemoveAt(_surfaceLines.Last().Count - 2);
                    _localLines.Last().RemoveAt(_localLines.Last().Count - 2);
                }
                
                if (_lineRenderer != null)
                {
                    _lineRenderer.positionCount = _surfaceLines.Last().Count;
                    _lineRenderer.SetPositions(_surfaceLines.Last().ToArray());
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!DrawEnabled) return;

            _lineRenderer = null;

            OnNewLines?.Invoke(this, _localLines);
        }
    }
}
