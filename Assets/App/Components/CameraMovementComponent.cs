using UnityEngine;

namespace App.Components
{
	[RequireComponent(typeof(Camera))]
	public class CameraMovementComponent : MonoBehaviour
	{
		public float ScrollZoomSpeed = 0.05f;
		public float CameraMaxY = 200;
		public float CameraMinY = 10;
		public RectTransform MapView;

		private bool _isEnabled;
		public bool IsEnabled
		{
			get { return _isEnabled; }
			set
			{
				if (!value)
				{
					_isDragging = false;
				}

				_isEnabled = value;
			}
		}

		private Camera _mapCamera;
		private bool _isDragging;
		private Vector3 _originMousePosition;

		private void Start()
		{
			_mapCamera = GetComponent<Camera>();
			Input.simulateMouseWithTouches = true;
		}

		private void Update()
		{
			_mapCamera.aspect = MapView.rect.width / MapView.rect.height;

			if (!_isEnabled) return;

			if (Input.touchSupported && Input.touchCount > 0)
			{
				HandleTouch();
			}
			else
			{
				HandleMouse();
			}
		}

		void HandleMouse()
		{
			// zoom
			var scrollDelta = Input.GetAxis("Mouse ScrollWheel");
			ZoomMap(1 - scrollDelta * ScrollZoomSpeed);

			//pan mouse
			PanMap();
		}

		void HandleTouch()
		{
			//pinch to zoom.
			switch (Input.touchCount)
			{
				case 1:
					PanMap();
					break;
				case 2:
					if (!RectTransformUtility.RectangleContainsScreenPoint(MapView, Input.mousePosition, null))
					{
						break;
					}

					// Store both touches.
					var touchZero = Input.GetTouch(0);
					var touchOne = Input.GetTouch(1);

					// Find the position in the previous frame of each touch.
					var touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
					var touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

					// Find the magnitude of the vector (the distance) between the touches in each frame.
					var prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
					var touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

					// Find the difference in the distances between each frame.
					var zoomFactor = prevTouchDeltaMag / touchDeltaMag;
					ZoomMap(zoomFactor);
					break;
			}
		}

		void ZoomMap(float zoomFactor)
		{
			var newPosition = _mapCamera.transform.position;
			newPosition.y = Mathf.Clamp(_mapCamera.transform.position.y * zoomFactor, CameraMinY, CameraMaxY);
			_mapCamera.transform.position = newPosition;
		}


		void PanMap()
		{
			if (!Input.GetMouseButton(0))
			{
				_isDragging = false;
				return;
			}

			if (!_isDragging
			    && !RectTransformUtility.RectangleContainsScreenPoint(MapView, Input.mousePosition, null))
			{
				return;
			}

			var mousePosScreen = (Vector3) ArUtils.GetRectMousePosition(MapView);
			mousePosScreen.z = _mapCamera.transform.position.y;
			var mousePosition = _mapCamera.ViewportToWorldPoint(mousePosScreen);

			if (!_isDragging)
			{
				_isDragging = true;
				_originMousePosition = mousePosition;
				return;
			}

			var delta = mousePosition - _originMousePosition;
			_mapCamera.transform.position -= delta;
			_originMousePosition = mousePosition - delta;

			// inertia effect
			_mapCamera.GetComponent<Rigidbody>().velocity = -delta / Time.deltaTime;
		}
	}
}
