using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace App.Controllers
{
    public class ToolboxController : MonoBehaviour
    {
        public Button CloseButton;

        public GameObject ButtonLayout;
        public Button AnchorButton;
        public Button DrawButton;
        public Button SurfaceDrawButton;
        public Button TextButton;

        public PointerHandlerComponent HoldToDrawButton;
        public Button PlaceAnchorButton;

        public GameObject Crosshairs;

        private bool _isActionActive;
        private AnnotationController _currentAnnotationController;

        public void AttachAnnotation(AnnotationController annotationController)
        {
            // downwards actions
            CloseButton.onClick.AddListener(annotationController.ExitCurrentMode);
            AnchorButton.onClick.AddListener(annotationController.StartAnchorMode);
            DrawButton.onClick.AddListener(annotationController.StartDrawMode);
            SurfaceDrawButton.onClick.AddListener(annotationController.StartSurfaceDrawMode);
            PlaceAnchorButton.onClick.AddListener(annotationController.PlaceAnchor);
            HoldToDrawButton.OnPointerDownHandler += OnHoldToDrawButtonDown;
            HoldToDrawButton.OnPointerUpHandler += OnHoldToDrawButtonUp;

            // upward triggers
            annotationController.OnReadyMode += OnReadyMode;
            annotationController.OnAnchorMode += OnAnchorMode;
            annotationController.OnDrawMode += OnDrawMode;
            annotationController.OnSurfaceDrawMode += OnSurfaceDrawMode;

            _currentAnnotationController = annotationController;

            // init ui
            ShowModeUi();
            ResetActionUi();
        }

        private void HideModeUi()
        {
            ButtonLayout.SetActive(false);
        }

        private void ShowModeUi()
        {
            ButtonLayout.SetActive(true);
        }

        private void ResetActionUi()
        {
            Crosshairs.SetActive(false);
            HoldToDrawButton.gameObject.SetActive(false);
            PlaceAnchorButton.gameObject.SetActive(false);
        }

        private void OnAnchorMode(object sender, EventArgs e)
        {
            HideModeUi();
            ResetActionUi();
            PlaceAnchorButton.gameObject.SetActive(true);
            Crosshairs.SetActive(true);
        }

        private void OnDrawMode(object sender, EventArgs e)
        {
            HideModeUi();
            ResetActionUi();
        }

        private void OnSurfaceDrawMode(object sender, EventArgs e)
        {
            HideModeUi();
            ResetActionUi();
            HoldToDrawButton.gameObject.SetActive(true);
            Crosshairs.SetActive(true);
        }

        private void OnReadyMode(object sender, EventArgs e)
        {
            ShowModeUi();
            ResetActionUi();
        }

        private void OnHoldToDrawButtonDown(object sender, PointerEventData e)
        {
            _currentAnnotationController.OnCaptureButtonDown();
        }

        private void OnHoldToDrawButtonUp(object sender, PointerEventData e)
        {
            _currentAnnotationController.OnCaptureButtonUp();
        }

        public void DetachAnnotation()
        {
            // Clear event listeners
            CloseButton.onClick.RemoveAllListeners();
            AnchorButton.onClick.RemoveAllListeners();
            DrawButton.onClick.RemoveAllListeners();
            SurfaceDrawButton.onClick.RemoveAllListeners();
            TextButton.onClick.RemoveAllListeners();
            HoldToDrawButton.OnPointerDownHandler -= OnHoldToDrawButtonDown;
            HoldToDrawButton.OnPointerUpHandler -= OnHoldToDrawButtonUp;

            _currentAnnotationController.OnReadyMode -= OnReadyMode;
            _currentAnnotationController.OnAnchorMode -= OnAnchorMode;
            _currentAnnotationController.OnDrawMode -= OnDrawMode;
            _currentAnnotationController.OnSurfaceDrawMode -= OnSurfaceDrawMode;

            _currentAnnotationController = null;
        }
    }
}
