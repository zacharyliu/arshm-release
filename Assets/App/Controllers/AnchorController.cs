using GoogleARCore;
using UnityEngine;

namespace App.Controllers
{
    public class AnchorController : MonoBehaviour
    {
        public Transform AnchorPosition;
        public bool HitSuccess { get; set; }
        public CrosshairsController Crosshairs;

        private void OnGUI()
        {
            if (Application.isEditor)
            {
                HitSuccess = GUILayout.Toggle(HitSuccess, "Hit Success");
            }
        }

        private bool DoRaycast(out TrackableHit hit)
        {
            var castPosition = Camera.main.WorldToScreenPoint(transform.position);
            return Frame.Raycast(castPosition.x, castPosition.y, TrackableHitFlags.FeaturePoint, out hit);
        }

        private void Update()
        {
            if (!Application.isEditor)
            {
                TrackableHit hit;
                HitSuccess = DoRaycast(out hit);
                AnchorPosition.position = hit.Pose.position;
            }

            Crosshairs.IsGreen = HitSuccess;
        }

        public bool GetAnchor(out Anchor anchor)
        {
            TrackableHit hit;
            if (!DoRaycast(out hit))
            {
                anchor = default(Anchor);
                return false;
            }

            anchor = hit.Trackable.CreateAnchor(hit.Pose);
            return true;
        }
    }
}
