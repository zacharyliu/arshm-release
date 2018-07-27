using UnityEngine;

// namespace tells Unity what directory of folders the script is located in.
namespace App.Components
{
    // Requires the GameObject to have a Camera component.
	[RequireComponent(typeof(Camera))]
	public class CameraMovementComponent : MonoBehaviour
	{
		public float ScrollZoomSpeed = 0.05f;
		public float CameraMaxY = 200;
		public float CameraMinY = 10;
        // MapView is the RectTransform the map is displayed onto. It contains a render texture that accomplishes this.
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

        // Initialize the Camera with the Camera component, and set the simulation of mouse clicks with touches to true.
		private void Start()
		{
			_mapCamera = GetComponent<Camera>();
			Input.simulateMouseWithTouches = true;
		}

        // Called once per frame.
		private void Update()
		{
            // Set the aspect ratio of the Camera to fit the dimensions of the MapView.
			_mapCamera.aspect = MapView.rect.width / MapView.rect.height;

			if (!_isEnabled) return;

            // Check whether the input was a mouse click or a finger touch.
			if (Input.touchSupported && Input.touchCount > 0)
			{
				HandleTouch();
			}
			else
			{
				HandleMouse();
			}
		}

        // Zoom and pan according to mouse controls.
		void HandleMouse()
		{
			// zoom
			var scrollDelta = Input.GetAxis("Mouse ScrollWheel");
			ZoomMap(1 - scrollDelta * ScrollZoomSpeed);

			//pan mouse
			PanMap();
		}

        // Zoom and pan according to finger input touches.  
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

        // Change the zoom of the map by zoomFactor
		void ZoomMap(float zoomFactor)
		{
			var newPosition = _mapCamera.transform.position;
			newPosition.y = Mathf.Clamp(_mapCamera.transform.position.y * zoomFactor, CameraMinY, CameraMaxY);
			_mapCamera.transform.position = newPosition;
		}

        // Allows you to pan the map around
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
