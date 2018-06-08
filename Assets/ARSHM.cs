using System.Collections.Generic;
using App;
using GoogleARCore;
using Newtonsoft.Json.Linq;
using UnityARInterface;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ARSHM : MonoBehaviour
{
	enum State
	{
		Camera,
		Map,
		Preview
	}

	public Button AnnotateButton;
	public Canvas Canvas;

	public GameObject ControlCanvas;
	public GameObject FocusDot;
	
	public GameObject NavbarCanvas;
	public Button NavbarBackButton;

	public GameObject PreviewImage;
	public GameObject PreviewCanvas;
	public GameObject LargePreviewImage;

	public GameObject Map;
	public GameObject MapCanvas;
	public GameObject LargeMap;

	private State _state = State.Camera;

	private float _deltaTimePose;
	private float _deltaTimePointCloud;
	private Pose _pose;
	private ARInterface.PointCloud _pointCloud;
	
	private List<TrackedAnnotation> _annotations = new List<TrackedAnnotation>();
	private TrackedAnnotation _currentAnnotation;
	
	private static Dictionary<GameObject, TrackedAnnotation> ObjToAnnotation = new Dictionary<GameObject, TrackedAnnotation>();

	// Use this for initialization
	void Start () {
		AnnotateButton.onClick.AddListener(AnnotateButtonClick);
		Map.GetComponent<ClickHandlerComponent>().OnClick += MapClickHandler;
		PreviewImage.GetComponent<ClickHandlerComponent>().OnClick += PreviewClickHandler;
		NavbarBackButton.onClick.AddListener(BackClick);

		Input.simulateMouseWithTouches = true;
	}

	private void BackClick()
	{
		if (_state == State.Map)
		{
			MapCanvas.SetActive(false);
			
			NavbarCanvas.SetActive(false);
			ControlCanvas.SetActive(true);
			_state = State.Camera;
		}
		else if (_state == State.Preview)
		{
			PreviewCanvas.SetActive(false);
			
			NavbarCanvas.SetActive(false);
			ControlCanvas.SetActive(true);
			_state = State.Camera;
		}
	}

	private void MapClickHandler(PointerEventData obj)
	{
		if (_state == State.Camera)
		{
			Debug.Log("map clicked");
			
			NavbarCanvas.SetActive(true);
			MapCanvas.SetActive(true);
			ControlCanvas.SetActive(false);
			
			_state = State.Map;
		}
	}

	private void PreviewClickHandler(PointerEventData obj)
	{
		if (_state == State.Camera)
		{
			Debug.Log("preview clicked");
			
			NavbarCanvas.SetActive(true);
			PreviewCanvas.SetActive(true);
			ControlCanvas.SetActive(false);
			
			_state = State.Preview;
		}
	}
	
	private void AnnotateButtonClick()
	{
		Debug.Log("attempting raycast");
		var castPosition = FocusDot.transform.position;
		TrackableHit hit;
//		if (ARInterface.GetInterface().TryRaycast(castPosition.x, castPosition.y, ref anchorPoint))
		if (Frame.Raycast(castPosition.x, castPosition.y, TrackableHitFlags.FeaturePoint, out hit))
		{
			Debug.Log("raycast success");
//			var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
//			obj.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
//			obj.transform.position = anchorPoint.position;
//			obj.transform.rotation = anchorPoint.rotation;
//			anchors.Add(anchorPoint.id, obj.transform);
			
			var png = ArUtils.GetCameraImage(ARInterface.GetInterface());
            
			if (png != null)
			{
				if (ARInterface.GetInterface().TryGetPose(ref _pose))
				{
					var anchor = hit.Trackable.CreateAnchor(hit.Pose);
					var annotation = new TrackedAnnotation(png, Camera.main.aspect, _pose, anchor);
					_annotations.Add(annotation);
					annotation.Show();
					
					ObjToAnnotation.Add(annotation._obj, annotation);
					
					setCurrentAnnotation(annotation);
					
//					SocketConnection.Instance.SendData("annotate", annotation.ToJson());
				}
			}
		}
		else
		{
			Debug.Log("raycast unsuccessful");
		}
	}

	private void setCurrentAnnotation(TrackedAnnotation a)
	{
		_currentAnnotation = a;
		var texture = new Texture2D(0, 0, TextureFormat.RGB24, false);
		texture.LoadImage(a.Image);
		PreviewImage.GetComponent<RawImage>().texture = texture;
		PreviewImage.SetActive(true);
		LargePreviewImage.GetComponent<RawImage>().texture = texture;
	}

	// Update is called once per frame
	void Update ()
	{
		var castPosition = FocusDot.transform.position;
		TrackableHit hit;
		var success = Frame.Raycast(castPosition.x, castPosition.y, TrackableHitFlags.FeaturePoint, out hit);
		FocusDot.GetComponent<Image>().color = success ? Color.green : Color.red;

		if (Input.GetMouseButtonDown(0))
		{
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit rayHit;
			if (Physics.Raycast(ray, out rayHit))
			{
				if (ObjToAnnotation.ContainsKey(rayHit.collider.gameObject))
				{
					setCurrentAnnotation(ObjToAnnotation[rayHit.collider.gameObject]);
				}
			}
		}

		if (Input.GetMouseButton(0))
		{
			var previewRect = LargePreviewImage.GetComponent<RectTransform>();
			var mapRect = LargeMap.GetComponent<RectTransform>();

			if (_state == State.Preview)
			{
				if (RectTransformUtility.RectangleContainsScreenPoint(previewRect, Input.mousePosition))
				{
					var point = ArUtils.GetRectMousePosition(previewRect);
				}
			}
			else if (_state == State.Map)
			{
				if (RectTransformUtility.RectangleContainsScreenPoint(mapRect, Input.mousePosition))
				{
					var point = ArUtils.GetRectMousePosition(mapRect);
					Debug.Log(point);
				}
			}
		}

		// Send pose update
//		_deltaTimePose += Time.unscaledDeltaTime;
//		if (_deltaTimePose >= 1.0/10)
//		{
//			if (ARInterface.GetInterface().TryGetPose(ref _pose))
//			{
//				SocketConnection.Instance.SendData("pose", _pose);
//
//				_deltaTimePose = 0;
//			}
//		}

		// Send point cloud update
//		_deltaTimePointCloud += Time.unscaledDeltaTime;
//		if (_deltaTimePointCloud >= 1.0/10)
//		{
//			if (ARInterface.GetInterface().TryGetPointCloud(ref _pointCloud))
//			{
//				var data = _pointCloud.points.ToArray();
//
//				SocketConnection.Instance.SendData("pointcloud", data);
//
//				_deltaTimePointCloud = 0;
//			}
//		}
	}
}

internal class TrackedAnnotation
{
	public byte[] Image;
	public float Aspect;
	public Pose Pose;
	public Anchor Anchor;
	
	public GameObject _obj;
	public GameObject _objMap;

	public TrackedAnnotation(byte[] image, float aspect, Pose pose, Anchor anchor)
	{
		Image = image;
		Aspect = aspect;
		Pose = pose;
		Anchor = anchor;
	}

	public void Show()
	{
		if (_obj != null) return;
		_obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		_obj.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
		_obj.transform.position = Anchor.transform.position;
		_obj.transform.rotation = Anchor.transform.rotation;
		_obj.transform.parent = Anchor.transform;
		_obj.GetComponent<MeshRenderer>().material.color = Color.blue;

		_objMap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		_objMap.layer = LayerMask.NameToLayer("Map");
		_objMap.transform.localScale = new Vector3(3f, 3f, 3f);
		_objMap.transform.position = Anchor.transform.position;
		_objMap.transform.rotation = Anchor.transform.rotation;
		_objMap.transform.parent = Anchor.transform;
		_objMap.GetComponent<MeshRenderer>().material.color = Color.blue;
	}

	public JObject ToJson()
	{
		return new JObject
		{
			["image"] = Image,
			["aspect"] = Aspect,
			["pose"] = SocketConnection.Convert(Pose),
			["anchor"] = SocketConnection.Convert(new Pose(Anchor.transform.position, Anchor.transform.rotation))
		};
	}
}
