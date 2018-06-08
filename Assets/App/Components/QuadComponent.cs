using UnityEngine;
using UnityEngine.Assertions;

namespace App.Components
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(Collider))]
    public class QuadComponent : MonoBehaviour
    {
        private Collider _collider;
        private Vector3 _origin;
        private Vector3 _uAxis;
        private Vector3 _vAxis;
        private Vector3 _normal;
        private Mesh _mesh;

        private void Start()
        {
            _collider = GetComponent<Collider>();
            _mesh = GetComponent<MeshFilter>().mesh;
            UpdateMesh();
        }

        public void UpdateMesh()
        {
            if (_mesh == null) return;

            // Check that this is a flat plane
            Assert.AreEqual(_mesh.vertices.Length, 4);
            Assert.AreEqual(_mesh.normals.Length, 4);
            Assert.AreEqual(_mesh.normals[0], _mesh.normals[1]);
            Assert.AreEqual(_mesh.normals[0], _mesh.normals[2]);
            Assert.AreEqual(_mesh.normals[0], _mesh.normals[3]);

            _normal = Vector3.Normalize(_mesh.normals[0]);

            _origin = transform.TransformPoint(_mesh.vertices[0]);
            _uAxis = transform.TransformPoint(_mesh.vertices[2]) - _origin;
            _vAxis = transform.TransformPoint(_mesh.vertices[3]) - _origin;
        }

        public Vector3 OffsetForDrawing(Vector3 surfacePoint)
        {
            return surfacePoint + 0.01f * _normal;
        }

        public bool ProjectSurfaceToLocal(Vector3 surfacePoint, out Vector2 result)
        {
            var vec = surfacePoint - _origin;
            var u = Vector3.Dot(vec, _uAxis) / _uAxis.sqrMagnitude;
            var v = Vector3.Dot(vec, _vAxis) / _vAxis.sqrMagnitude;
            if (u < 0 || u > 1 || v < 0 || v > 1)
            {
                result = new Vector2();
                return false;
            }
            result = new Vector2(u, v);
            return true;
        }

        public bool ProjectWorldToSurface(Vector3 cameraPoint, Vector3 targetPoint, out Vector3 result)
        {
            RaycastHit hitInfo;
            var ray = new Ray(cameraPoint, targetPoint - cameraPoint);
            var maxDistance = Mathf.Max((transform.position - cameraPoint).magnitude,
                (targetPoint - cameraPoint).magnitude) + 1; // add 1 to ensure it goes past the collider
            if (!_collider.Raycast(ray, out hitInfo, maxDistance))
            {
                result = new Vector3();
                return false;
            }

            result = hitInfo.point;
            return true;
        }
    }
}
