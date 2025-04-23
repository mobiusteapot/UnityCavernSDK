using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

namespace Spelunx.Vive
{
    public sealed class ViveTracker : OVRT_TrackedDevice
    {
        // an enum representing the possible bindings a vive tracker can be assigned to
        // The tracker -> binding assignment should be done on the CAVERN computer
        // in SteamVR
        public enum SteamVRPoseBindings
        {
            [InspectorName("Disabled")] // note: This isn't actually disabled. It's just another binding
            TrackerRole_None,
            [InspectorName("Any Hand")]
            AnyHand,
            [InspectorName("Left Hand")]
            LeftHand,
            [InspectorName("Right Hand")]
            RightHand,
            [InspectorName("Left Foot")]
            TrackerRole_LeftFoot,
            [InspectorName("Right Foot")]
            TrackerRole_RightFoot,
            [InspectorName("Left Shoulder")]
            TrackerRole_LeftShoulder,
            [InspectorName("Right Shoulder")]
            TrackerRole_RightShoulder,
            [InspectorName("Left Elbow")]
            TrackerRole_LeftElbow,
            [InspectorName("Right Elbow")]
            TrackerRole_RightElbow,
            [InspectorName("Left Knee")]
            TrackerRole_LeftKnee,
            [InspectorName("Right Knee")]
            TrackerRole_RightKnee,
            [InspectorName("Left Wrist")]
            TrackerRole_LeftWrist,
            [InspectorName("Right Wrist")]
            TrackerRole_RightWrist,
            [InspectorName("Left Ankle")]
            TrackerRole_LeftAnkle,
            [InspectorName("Right Ankle")]
            TrackerRole_RightAnkle,
            [InspectorName("Waist")]
            TrackerRole_Waist,
            [InspectorName("Chest")]
            TrackerRole_Chest,
            [InspectorName("Camera")]
            TrackerRole_Camera,
            [InspectorName("Keyboard")]
            TrackerRole_Keyboard,
        }

        [Tooltip("Specify a binding from SteamVR for this tracker. Assign a tracker to this same binding in SteamVR.")]
        public SteamVRPoseBindings binding;

        [Tooltip("If not set, relative to parent")]

        private UnityAction<string, TrackedDevicePose_t, int> _onNewBoundPoseAction;
        private UnityAction _onTrackerRolesChanged;

        private Quaternion rotationAlignment = Quaternion.identity;

        // All bindings are referenced in steamvr by their name (like TrackerRole_Camera)
        // Except for the hands, which have binding names with commas in them
        // Hence the need for this function
        public static string TrackerRoleToBindingName(SteamVRPoseBindings binding)
        {
            switch (binding)
            {
                case SteamVRPoseBindings.AnyHand:
                    return "TrackerRole_Handed,TrackedControllerRole_Invalid";
                case SteamVRPoseBindings.LeftHand:
                    return "TrackerRole_Handed,TrackedControllerRole_LeftHand";
                case SteamVRPoseBindings.RightHand:
                    return "TrackerRole_Handed,TrackedControllerRole_RightHand";
                default:
                    return binding.ToString();
            }
        }

        // get the display name instead of the actual enum name of our bindings
        // thanks stackoverflow: https://stackoverflow.com/questions/1799370/getting-attributes-of-enums-value
        public static string GetReadableName(SteamVRPoseBindings binding)
        {
            try
            {
                var enumType = typeof(ViveTracker.SteamVRPoseBindings);

                var memberInfos = enumType
                    .GetMember(binding.ToString());

                var enumValueMemberInfo = memberInfos
                    .FirstOrDefault(m => m.DeclaringType == enumType);

                var valueAttributes = enumValueMemberInfo
                    .GetCustomAttributes(typeof(InspectorNameAttribute), false);

                var description = ((InspectorNameAttribute)valueAttributes[0])
                    .displayName;
                return description;
            }
            catch
            {
                return TrackerRoleToBindingName(binding);
            }
        }

        private void OnDeviceConnected(int index, bool connected)
        {
            if (DeviceIndex == index && !connected)
            {
                IsConnected = false;
            }
        }

        private bool doCalibration = false; // Whether to calibrate the rotation of the vive tracker on the next frame

        private void OnNewBoundPose(string binding, TrackedDevicePose_t pose, int deviceIndex)
        {
            if (TrackerRoleToBindingName(this.binding) != binding)
                return;

            IsValid = false;

            if (DeviceIndex != deviceIndex)
            {
                DeviceIndex = deviceIndex;
                onDeviceIndexChanged.Invoke(DeviceIndex);
            }
            IsConnected = pose.bDeviceIsConnected;

            if (!pose.bDeviceIsConnected)
                return;

            if (!pose.bPoseIsValid)
                return;

            IsValid = true;

            var rigidTransform = new OVRT_Utils.RigidTransform(pose.mDeviceToAbsoluteTracking);

            if (origin != null)
            {
                transform.position = origin.transform.TransformPoint(rigidTransform.pos);
                // Realign the rotation of the vive tracker. It's hard to know visually when they're pointing forwards,
                // So this offsets their rotation
                if (doCalibration)
                {
                    // calibrate
                    rotationAlignment = Quaternion.Inverse(origin.rotation * rigidTransform.rot);
                    doCalibration = false;
                }
                transform.rotation = origin.rotation * rigidTransform.rot * rotationAlignment;
            }
            else
            {
                transform.localPosition = rigidTransform.pos;
                transform.localRotation = rigidTransform.rot;
            }
        }

        // Calibrate vive trackers on the next pose frame
        public void Calibrate()
        {
            doCalibration = true;
        }

        private void OnTrackerRolesChanged()
        {
            IsValid = false;
            IsConnected = false;
        }

        // In theory this should be called when pin inputs are sent through the vive trackers
        // But it doesn't seem to happen. This might be an issue with SteamVR, or because the
        // tracker pose needs to be set to one of the hands or disabled
        private void OnButtonPressed(int deviceIndex, EVRButtonId button, bool pressed)
        {
            Debug.Log($"{deviceIndex}\t{button}\t{pressed}");
        }

        private void Awake()
        {
            _onNewBoundPoseAction += OnNewBoundPose;
            _onDeviceConnectedAction += OnDeviceConnected;
            _onTrackerRolesChanged += OnTrackerRolesChanged;
        }

        private void OnEnable()
        {
            OVRT_Events.NewBoundPose.AddListener(_onNewBoundPoseAction);
            OVRT_Events.TrackedDeviceConnected.AddListener(_onDeviceConnectedAction);
            OVRT_Events.TrackerRolesChanged.AddListener(_onTrackerRolesChanged);
            OVRT_Events.ButtonPressed.AddListener(OnButtonPressed);
        }

        private void OnDisable()
        {
            OVRT_Events.NewBoundPose.RemoveListener(_onNewBoundPoseAction);
            OVRT_Events.TrackedDeviceConnected.RemoveListener(_onDeviceConnectedAction);
            OVRT_Events.TrackerRolesChanged.RemoveListener(_onTrackerRolesChanged);
            OVRT_Events.ButtonPressed.RemoveListener(OnButtonPressed);
            IsValid = false;
            IsConnected = false;
        }

        // t is treated as the (0,0,0) point
        public void SetOrigin(Transform t)
        {
            origin = t;
        }

#if UNITY_EDITOR
        // A gizmo, which can be enabled or disabled through the gizmos menu
        // This shows the position, size, and rotation of the vive tracker.
        private void OnDrawGizmos()
        {
            Gizmos.DrawMesh(ViveDebugRenderer.trackerMesh, transform.position, transform.rotation);
        }
#endif
    }
}