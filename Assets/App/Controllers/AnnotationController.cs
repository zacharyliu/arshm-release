using System;
using App.Components;
using App.Models;
using UnityEngine;
using UnityEngine.EventSystems;

namespace App.Controllers
{
	public class AnnotationController : MonoBehaviour, IPointerClickHandler
	{
		public GameObject ImageQuad;
		public GameObject Pyramid;
		public MeshRenderer PyramidMesh;
		public QuadComponent ImageQuadComponent;
		public Transform TargetPosition;
		public bool Debug;

		public event EventHandler OnInactiveMode;
		public event EventHandler OnLeaveInactiveMode;
		public event EventHandler OnReadyMode;
		public event EventHandler OnDrawMode;
		public event EventHandler OnAnchorMode;
		public event EventHandler OnSurfaceDrawMode;

		[SerializeField]
		[Range(1, 100)]
		private float _unanchoredImageDistance = 0.5f;

		private float ImageDistance
		{
			get
			{
				if (!_model.IsAnchored)
				{
					return _unanchoredImageDistance;
				}
				var cameraVector = transform.rotation * Vector3.forward;
				var pointVector = _model.AnchorPosition - transform.position;
				return Vector3.Dot(cameraVector, pointVector);
			}
		}

		[SerializeField]
		private AnnotationModel _model;

		private enum State
		{
			Inactive,
			Ready,
			AnchorMode,
			DrawMode,
			SurfaceDrawMode
		}

		private State _state;
		private bool _guiCaptureButton;
		private DrawComponent _drawComponent;
		private SurfaceDrawComponent _surfaceDrawComponent;
		private bool _textureLoaded;

		private void OnGUI()
		{
			if (_state == State.Inactive || !Debug) return;

			GUILayout.Label("State: " + _state);
			
			if (GUILayout.Toggle(_guiCaptureButton, "Capture"))
			{
				if (!_guiCaptureButton)
				{
					OnCaptureButtonDown();
				}

				_guiCaptureButton = true;
			}
			else
			{
				if (_guiCaptureButton)
				{
					OnCaptureButtonUp();
				}
				
				_guiCaptureButton = false;
			}

			if (GUILayout.Button("Start Anchor Mode"))
			{
				StartAnchorMode();
			}

			if (GUILayout.Button("Start Draw Mode"))
			{
				StartDrawMode();
			}

			if (GUILayout.Button("Start Surface Draw Mode"))
			{
				StartSurfaceDrawMode();
			}

			if (GUILayout.Button("Exit Mode"))
			{
				StartReadyMode();
			}

			if (GUILayout.Button("Deselect"))
			{
				StartInactiveMode();
			}
		}

		public void OnCaptureButtonDown()
		{
			if (_state == State.SurfaceDrawMode)
			{
				_surfaceDrawComponent.DrawOn = true;
			}
		}

		public void OnCaptureButtonUp()
		{
			if (_state == State.SurfaceDrawMode)
			{
				_surfaceDrawComponent.DrawOn = false;
			}
		}

		public void PlaceAnchor()
		{
			if (_state == State.AnchorMode)
			{
				StartReadyMode();
			}
		}

		private void ResetUi()
		{
			_drawComponent.DrawEnabled = false;
			_surfaceDrawComponent.DrawOn = false;
		}

		public void ExitCurrentMode()
		{
			switch (_state)
			{
				case State.Inactive:
					break;
				case State.Ready:
					StartInactiveMode();
					break;
				default:
					StartReadyMode();
					break;
			}
		}

		public void StartInactiveMode()
		{
			ResetUi();
			_state = State.Inactive;
			OnInactiveMode?.Invoke(this, EventArgs.Empty);
		}

		public void StartReadyMode()
		{
			if (_state == State.Inactive)
			{
				OnLeaveInactiveMode?.Invoke(this, EventArgs.Empty);
			}
			else if (_state == State.Ready)
			{
				return;
			}

			_model.Save();
			ResetUi();
			_state = State.Ready;
			OnReadyMode?.Invoke(this, EventArgs.Empty);
		}

		public void StartAnchorMode()
		{
			if (_state != State.Ready)
			{
				return;
			}

			_state = State.AnchorMode;
			OnAnchorMode?.Invoke(this, EventArgs.Empty);
		}

		public void StartDrawMode()
		{
			if (_state != State.Ready)
			{
				return;
			}

			_drawComponent.DrawEnabled = true;
			_state = State.DrawMode;
			OnDrawMode?.Invoke(this, EventArgs.Empty);
		}
		
		public void StartSurfaceDrawMode()
		{
			if (_state != State.Ready)
			{
				return;
			}
			_state = State.SurfaceDrawMode;
			OnSurfaceDrawMode?.Invoke(this, EventArgs.Empty);
		}

		// Use this for initialization
		void Start () {
			// Initial setup
			_state = State.Inactive;
			transform.position = _model.CameraPose.position;
			transform.rotation = _model.CameraPose.rotation;
			UpdateFovDistance();

			// Prevent opacity flickering on initial load
			SetMaterialOpacity(ImageQuad.GetComponent<MeshRenderer>().material, 0);
			SetMaterialOpacity(PyramidMesh.material, 0);

			// Load image texture
			TryLoadTexture();
			_model.Image.OnUpdate += (sender, args) => TryLoadTexture();

			// Create components
			_drawComponent = ImageQuad.AddComponent<DrawComponent>();
			_surfaceDrawComponent = ImageQuad.AddComponent<SurfaceDrawComponent>();
			_surfaceDrawComponent.CameraPosition = transform;
			_surfaceDrawComponent.TargetPosition = TargetPosition;

			// Attach event listeners
			_drawComponent.OnNewLines += (sender, list) => _model.DrawLines = list;
			_surfaceDrawComponent.OnNewLines += (sender, list) => _model.SurfaceDrawLines = list;
		}

		private void TryLoadTexture()
		{
			var texture = _model.Image.Texture;
			if (texture != null)
			{
				_textureLoaded = true;
				ImageQuad.GetComponent<MeshRenderer>().material.mainTexture = texture;
			}
		}

		public void Setup(AnnotationModel model)
		{
			_model = model;
		}

		private void OnValidate()
		{
			UpdateFovDistance();
		}

		private void SetMaterialOpacity(Material material, float opacity)
		{
			var materialColor = material.color;
			materialColor.a = opacity;
			material.color = materialColor;
		}

		private void UpdateFovDistance()
		{
			var pyramidHeight = Mathf.Tan(_model.Fov / 2 * Mathf.Deg2Rad) * ImageDistance * 2 / Mathf.Sqrt(2);
			Pyramid.transform.localScale = new Vector3(pyramidHeight * _model.Aspect, pyramidHeight, ImageDistance);

			var imageHeight = Mathf.Tan(_model.Fov / 2 * Mathf.Deg2Rad) * ImageDistance * 2;
			ImageQuad.transform.localScale = new Vector3(imageHeight * _model.Aspect, imageHeight, 1);
			ImageQuad.transform.localPosition = new Vector3(0, 0, ImageDistance);
			ImageQuadComponent.UpdateMesh();
		}

		private bool PyramidContainsPoint(Vector3 point)
		{
			Vector3 result;
			return ImageQuadComponent.ProjectWorldToSurface(transform.position, point, out result);
		}

		private void UpdateOpacity()
		{
			float imageOpacity;
			float pyramidOpacity;

			switch (_state)
			{
				case State.Inactive:
					imageOpacity = 0.5f;
					pyramidOpacity = 0;
					break;
				case State.Ready:
					imageOpacity = 1;
					pyramidOpacity = 0;
					break;
				case State.AnchorMode:
					imageOpacity = 0.3f;
					pyramidOpacity = 0.3f;
					break;
				case State.DrawMode:
					imageOpacity = 1;
					pyramidOpacity = 0;
					break;
				case State.SurfaceDrawMode:
					imageOpacity = 0.1f;
					pyramidOpacity = 0;
					break;
				default:
					return;
			}

			SetMaterialOpacity(ImageQuad.GetComponent<MeshRenderer>().material, imageOpacity);
			SetMaterialOpacity(PyramidMesh.material, pyramidOpacity);
		}
		
		// Update is called once per frame
		void Update ()
		{
			UpdateOpacity();

			if (_state == State.AnchorMode)
			{
				if (PyramidContainsPoint(TargetPosition.position))
				{
					// If yes, update model
					_model.IsAnchored = true;
					_model.AnchorPosition = TargetPosition.position;

					// Move image plane
					UpdateFovDistance();
				}
			}
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (_state != State.Inactive) return;

			if (!_textureLoaded)
			{
				_model.Image.Fetch(result =>
				{
					// TODO: implement proper image fetch callback
					if (!result)
					{
						UnityEngine.Debug.LogError("couldn't fetch image");
						return;
					}
//					LoadTexture(_model.Image.Texture);
				});
			}
			StartReadyMode();
		}
	}
}
