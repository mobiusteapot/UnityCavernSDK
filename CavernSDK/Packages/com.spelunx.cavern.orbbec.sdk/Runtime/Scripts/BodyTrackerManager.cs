using Microsoft.Azure.Kinect.BodyTracking;
using UnityEngine;

namespace Spelunx.Orbbec {
    public class BodyTrackerManager : MonoBehaviour {
        // Handler for SkeletalTracking thread.
        public BodyTracker bodyTracker; // One for each skeleton on the screen.

        [Header("Settings")]
        [SerializeField] private SensorOrientation sensorOrientation;

        // Internal Variables
        private SkeletalFrameDataProvider skeletalFrameDataProvider; // One for each Femto Bolt. One Femto Bolt can support multiple (like 20?) skeletons.
        private FrameData frameData = new FrameData();

        private void Start() {
            //tracker ids needed for when there are two trackers
            const int TRACKER_ID = 0;
            skeletalFrameDataProvider = new SkeletalFrameDataProvider(TRACKER_ID, sensorOrientation);
        }

        private void Update() {
            if (!skeletalFrameDataProvider.HasStarted) { return; }
            if (!skeletalFrameDataProvider.ExtractData(ref frameData)) { return; }
            if (frameData.NumOfBodies == 0) { return; }
            bodyTracker.UpdateSkeleton(frameData);
        }

        private void OnDestroy() {
            if (skeletalFrameDataProvider != null) {
                skeletalFrameDataProvider.Dispose();
            }
        }
    }
}