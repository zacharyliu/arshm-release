using System;
using App.Components;
using App.Models;
using App.Services;
using UnityARInterface;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// namespace tells Unity what directory of folders the script is located in.
namespace App.Controllers
{
    public class MainController : MonoBehaviour
    {
        // enumeration to see what state the app is in (whether it's using the Camera, viewing the Map, or viewing the Preview.
        // Currently, there does not appear to be a Preview GameObject. Unclear whether Zach intentionally did not have one,
        // or if the connection to the MainController has not saved over.
        private enum State
        {
            Camera,
            Map,
            Preview
        }

        public GameObject ControlCanvas;
        public GameObject PreviewImage;
        public GameObject MapPreview;
        public Button AnnotateButton;

        public GameObject NavbarCanvas;
        public Button NavbarBackButton;

        public GameObject PreviewCanvas;
        public GameObject LargePreviewImage;

        public GameObject MapCanvas;
        public GameObject MapFull;
        public MapCanvasController MapCanvasController;
        public CameraMovementComponent MapCameraMovementComponent;

        public ToolboxController ToolboxController;

        public AnnotationController AnnotationPrefab;

        public Texture2D DebugImage;

        public Transform AnchorPosition;

        private State _state = State.Camera;

        private float _deltaTimePose;
        private float _deltaTimePointCloud;
        private Pose _pose;
        private ARInterface.PointCloud _pointCloud;

        private AnnotationController _currentAnnotation;

        void Awake()
        {
            SocketService.GetInstance().OnCreate["annotation"] = json =>
            {
                var annotationModel = new AnnotationModel(json);
                AddAnnotation(annotationModel);
            };
        }

        // Use this for initialization
        void Start()
        {
            AnnotateButton.onClick.AddListener(AnnotateButtonClick);
            MapPreview.GetComponent<ClickHandlerComponent>().OnClick += MapClickHandler;
            PreviewImage.GetComponent<ClickHandlerComponent>().OnClick += PreviewClickHandler;
            NavbarBackButton.onClick.AddListener(BackClick);
            ToolboxController.gameObject.SetActive(false);
            AnnotateButton.gameObject.SetActive(true);
        }

        // This method accounts for when the user Backclicks from the map or preview state. It sets the canvases appropriately.
        private void BackClick()
        {
            if (_state == State.Map)
            {
                MapCanvas.SetActive(false);

                NavbarCanvas.SetActive(false);
                ControlCanvas.SetActive(true);

                MapCameraMovementComponent.MapView = MapPreview.GetComponent<RectTransform>();
                MapCameraMovementComponent.IsEnabled = false;

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

        // This method gets called when the user clicks on the map. 
        private void MapClickHandler(PointerEventData obj)
        {
            if (_state == State.Camera)
            {
                Debug.Log("map clicked");

                NavbarCanvas.SetActive(true);
                MapCanvas.SetActive(true);
                ControlCanvas.SetActive(false);

                MapCameraMovementComponent.MapView = MapFull.GetComponent<RectTransform>();
                MapCameraMovementComponent.IsEnabled = true;

                _state = State.Map;
            }
        }

        // This method is called when the user clicks on the preview
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

        // This method is called when the user clicks the annotate button.
        private void AnnotateButtonClick()
        {
            // TODO: check if tracking

            byte[] png;
            if (DebugImage != null)
            {
                png = DebugImage.EncodeToPNG();
            }
            else
            {
                png = ArUtils.GetCameraImage(ARInterface.GetInterface());
                if (png == null) return;
            }

            var imageModel = new ImageModel(png);
            imageModel.Save();
            var annotationModel =
                new AnnotationModel(
                    new Pose(Camera.main.transform.position, Camera.main.transform.rotation),
                    Camera.main.aspect,
                    imageModel);
            annotationModel.Save();
            AddAnnotation(annotationModel);
        }

        // Adds an annotation to the Map Canvas. Passed into a script attached to Map Canvas.
        private void AddAnnotation(AnnotationModel annotationModel)
        {
            var annotationController = Instantiate(AnnotationPrefab);
            annotationController.Setup(annotationModel);
            annotationController.TargetPosition = AnchorPosition.transform;
            annotationController.OnLeaveInactiveMode += OnAnnotationControllerSelect;
            annotationController.OnInactiveMode += OnAnnotationControllerDeselect;
            MapCanvasController.AddAnnotation(annotationController);
        }

        // I believe the next four methods set the current annotation and remove annotations. Unclear how it works.
        private void OnAnnotationControllerSelect(object sender, EventArgs eventArgs)
        {
            SetCurrentAnnotation((AnnotationController) sender);
        }

        
        private void OnAnnotationControllerDeselect(object sender, EventArgs eventArgs)
        {
            ClearCurrentAnnotation();
        }

        private void ClearCurrentAnnotation()
        {
            if (_currentAnnotation == null) return;

            ToolboxController.DetachAnnotation();
            ToolboxController.gameObject.SetActive(false);
            _currentAnnotation = null;
            AnnotateButton.gameObject.SetActive(true);
        }

        private void SetCurrentAnnotation(AnnotationController a)
        {
            ClearCurrentAnnotation();
            _currentAnnotation = a;
            ToolboxController.gameObject.SetActive(true);
            ToolboxController.AttachAnnotation(a);
            AnnotateButton.gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            // Checks if the input was a left mouse click.
            if (Input.GetMouseButton(0))
            {
                // Unclear what this code does.
                var previewRect = LargePreviewImage.GetComponent<RectTransform>();
                var mapRect = MapFull.GetComponent<RectTransform>();

                if (_state == State.Preview)
                {
                    if (RectTransformUtility.RectangleContainsScreenPoint(previewRect, Input.mousePosition))
                    {
                        var point = GetRectPosition(previewRect);
                    }
                }
                else if (_state == State.Map)
                {
                    if (RectTransformUtility.RectangleContainsScreenPoint(mapRect, Input.mousePosition))
                    {
                        var point = GetRectPosition(mapRect);
                        Debug.Log(point);
                    }
                }
            }
        }

        // Get the Rectangle Position of a Rect Transform
        private static Vector2 GetRectPosition(RectTransform rectTransform)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null,
                out localPoint);
            var normalized = new Vector2(0.5f + localPoint.x / rectTransform.rect.width,
                0.5f + localPoint.y / rectTransform.rect.height);
            return normalized;
        }
    }
}
