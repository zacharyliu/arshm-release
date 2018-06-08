using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace App.Controllers
{
	[RequireComponent(typeof(Canvas))]
	public class MapCanvasController : MonoBehaviour
	{
		public Camera MapCamera;
		public GameObject MapMarkerPrefab;

		private Canvas _canvas;

		private Dictionary<AnnotationController, GameObject> _annotationControllers =
			new Dictionary<AnnotationController, GameObject>();

		// Use this for initialization
		void Start () {
			_canvas = GetComponent<Canvas>();
		}

		public void AddAnnotation(AnnotationController annotationController)
		{
			var marker = Instantiate(MapMarkerPrefab);
			marker.transform.SetParent(_canvas.transform, false);
			_annotationControllers.Add(annotationController, marker);
			marker.GetComponent<Button>().onClick.AddListener(annotationController.StartReadyMode);
		}

		void LateUpdate ()
		{
			// Must calculate in LateUpdate to ensure new map position is updated
			foreach (var entry in _annotationControllers)
			{
				var rectTransform = (RectTransform) entry.Value.transform;
				var screenPoint = MapCamera.WorldToScreenPoint(entry.Key.transform.position);
				Vector3 worldPoint;
				RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform) _canvas.transform, screenPoint, MapCamera, out worldPoint);
				rectTransform.position = worldPoint;
			}
		}
	}
}
