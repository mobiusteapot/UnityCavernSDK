using Microsoft.Azure.Kinect.BodyTracking;
using UnityEngine;

namespace Spelunx.Orbbec {
    public class BodyTrackerManager : MonoBehaviour {
        // Handler for SkeletalTracking thread.
        [SerializeField] private BodyTracker bodyTracker; // One for each skeleton on the screen. For now we only support 1.
        [SerializeField] private int trackerID = 0; // Tracker ids needed for when there are multiple trackers. For now we only support 1.

        [Header("Settings")]
        [SerializeField, Tooltip("What is the orientation of the sensor? How is it mounted? This needs to be set before playing, and cannot be changed on the fly.")] private SensorOrientation sensorOrientation;

        // Internal Variables
        private SkeletalFrameDataProvider skeletalFrameDataProvider = null; // One for each Femto Bolt. One Femto Bolt can support multiple (like 20?) skeletons.
        private FrameData frameData = new FrameData();

        private void Start() {
            skeletalFrameDataProvider = new SkeletalFrameDataProvider(trackerID, sensorOrientation);
        }

        private void Update() {
            if (!skeletalFrameDataProvider.IsRunning) { return; }
            if (!skeletalFrameDataProvider.ExtractData(ref frameData)) { return; }
            if (frameData.NumOfBodies == 0) { return; }
            bodyTracker.UpdateSkeleton(frameData, sensorOrientation);
        }

        private void OnDestroy() {
            if (skeletalFrameDataProvider != null) {
                skeletalFrameDataProvider.Dispose();
            }
        }

        public void SetSensorOrientation(SensorOrientation sensorOrientation) {
            this.sensorOrientation = sensorOrientation;
        }

        public SensorOrientation GetSensorOrientation() { return this.sensorOrientation; }
    }
}