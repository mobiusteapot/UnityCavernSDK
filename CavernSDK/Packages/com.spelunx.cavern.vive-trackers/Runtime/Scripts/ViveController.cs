using System;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;
using static Spelunx.Vive.OVRT_TrackedObject;

namespace Spelunx.Vive
{
    public sealed class ViveController : OVRT_TrackedDevice
    {
        public enum Role
        {
            LeftHand,
            RightHand
        }

        public Role role;

        // Events for tracker buttons being pressed
        public UnityEvent<ViveControllerButton> onButtonPressed = new();
        public UnityEvent<ViveControllerButton> onButtonReleased = new();
        public UnityEvent<ViveControllerButton> onButtonHeld = new();
        public UnityEvent<ViveControllerButton> onButtonTouchPressed = new();
        public UnityEvent<ViveControllerButton> onButtonTouchReleased = new();
        public UnityEvent<ViveControllerButton> onButtonTouchHeld = new();
        // The previous controller state, used for determining press, hold, and release
        private VRControllerState_t previousControllerState = new();
        private VRControllerState_t currentControllerState = new();

        private UnityAction<TrackedDevicePose_t[]> _onNewPosesAction;
        private UnityAction<int, EVRButtonId, bool> _onButtonPressedAction;
        private UnityAction<int> _onTrackedDeviceRoleChangedAction;

        public void TriggerHapticPulse(ushort durationMicroSec = 500, EVRButtonId buttonId = EVRButtonId.k_EButton_SteamVR_Touchpad)
        {
            if (DeviceIndex == -1)
                return;

            if (OpenVR.System == null) return;

            var axisId = (uint)buttonId - (uint)EVRButtonId.k_EButton_Axis0;
            OpenVR.System.TriggerHapticPulse((uint)DeviceIndex, axisId, (char)durationMicroSec);
        }


        private void OnDeviceConnected(int index, bool connected)
        {
            if (OpenVR.System == null) return;

            var roleIndex = FindIndexForRole();

            if (roleIndex > -1)
            {
                IsConnected = connected;
                UpdateIndex();
            }
        }

        private void OnTrackedDeviceRoleChanged(int index)
        {
            UpdateIndex();
        }

        private int FindIndexForRole()
        {
            if (OpenVR.System == null) return -1;

            ETrackedControllerRole trackedRole = ETrackedControllerRole.Invalid;
            switch (role)
            {
                case Role.LeftHand:
                    trackedRole = ETrackedControllerRole.LeftHand; break;
                case Role.RightHand:
                    trackedRole = ETrackedControllerRole.RightHand; break;
            }

            if (trackedRole == ETrackedControllerRole.LeftHand || trackedRole == ETrackedControllerRole.RightHand)
            {
                return (int)OpenVR.System.GetTrackedDeviceIndexForControllerRole(trackedRole);
            }
            else
            {
                return -1;
            }
        }

        void Update()
        {
            if (DeviceIndex == -1 || OpenVR.System == null || !IsConnected)
                return;

            var deviceClass = OpenVR.System.GetTrackedDeviceClass((uint)DeviceIndex);
            if (deviceClass != ETrackedDeviceClass.Controller && deviceClass != ETrackedDeviceClass.GenericTracker) return;


            // Optimization to avoid allocations we want (previous, current) to become (current, next)
            // so write to previous and then swap
            if (OpenVR.System.GetControllerState((uint)DeviceIndex, ref previousControllerState, (uint)Marshal.SizeOf(typeof(VRControllerState_t))))
            {
                // Optimization to avoid allocations - move current state to previous, previous to current, and then overwrite
                (previousControllerState, currentControllerState) = (currentControllerState, previousControllerState);
                Debug.Log($"Device {DeviceIndex}: Buttons pressed = {currentControllerState.ulButtonPressed}, touched = {currentControllerState.ulButtonTouched}");
                foreach (ViveControllerButton buttonMask in Enum.GetValues(typeof(ViveControllerButton)))
                {
                    // press events
                    if ((currentControllerState.ulButtonPressed & (ulong)buttonMask) != 0)
                    {
                        // button is pressed
                        if ((previousControllerState.ulButtonPressed & (ulong)buttonMask) == 0)
                        {
                            // button wasn't pressed previous frame
                            onButtonPressed.Invoke(buttonMask);
                        }
                        else
                        {
                            // button was pressed previous frame, so now it's a hold
                            onButtonHeld.Invoke(buttonMask);
                        }
                    }
                    else if ((previousControllerState.ulButtonPressed & (ulong)buttonMask) != 0)
                    {
                        // Button was pressed last frame, and is no longer pressed
                        onButtonReleased.Invoke(buttonMask);
                    }

                    // touch events
                    if ((currentControllerState.ulButtonTouched & (ulong)buttonMask) != 0)
                    {
                        // button is touched
                        if ((previousControllerState.ulButtonTouched & (ulong)buttonMask) == 0)
                        {
                            // button wasn't touched previous frame
                            onButtonTouchPressed.Invoke(buttonMask);
                        }
                        else
                        {
                            // button was touched previous frame, so now it's a hold
                            onButtonTouchHeld.Invoke(buttonMask);
                        }
                    }
                    else if ((previousControllerState.ulButtonTouched & (ulong)buttonMask) != 0)
                    {
                        // Button was touched last frame, and is no longer touched
                        onButtonTouchReleased.Invoke(buttonMask);
                    }
                }
            }
        }

        // TODO: check that these axes are actually correct
        public Vector2 GetInputAxis0()
        {
            return currentControllerState.rAxis0.ToVector2();
        }

        public Vector2 GetTrackpadAxis()
        {
            return currentControllerState.rAxis1.ToVector2();
        }

        public Vector2 GetJoystickAxis()
        {
            return currentControllerState.rAxis2.ToVector2();
        }

        public Vector2 GetTriggerAxis()
        {
            return currentControllerState.rAxis3.ToVector2();
        }

        public Vector2 GetInputAxis4()
        {
            return currentControllerState.rAxis4.ToVector2();
        }


        private void OnNewPoses(TrackedDevicePose_t[] poses)
        {
            if (DeviceIndex == -1)
                return;

            var i = DeviceIndex;

            IsValid = false;

            if (i < 0 || poses.Length <= i)
                return;

            if (!poses[i].bDeviceIsConnected)
                return;

            if (!poses[i].bPoseIsValid)
                return;

            IsValid = true;

            var pose = new OVRT_Utils.RigidTransform(poses[i].mDeviceToAbsoluteTracking);

            if (origin != null)
            {
                transform.position = origin.transform.TransformPoint(pose.pos);
                transform.rotation = origin.rotation * pose.rot;
            }
            else
            {
                transform.localPosition = pose.pos;
                transform.localRotation = pose.rot;
            }
        }

        // This method doesn't work because of openvr events not properly giving button data
        private void OnButtonPressed(int index, EVRButtonId button, bool pressed)
        {
            if (index == DeviceIndex)
            {
                // onButtonPressed.Invoke(button, pressed);
            }
        }

        private void Awake()
        {
            _onNewPosesAction += OnNewPoses;
            _onButtonPressedAction += OnButtonPressed;
            _onDeviceConnectedAction += OnDeviceConnected;
            _onTrackedDeviceRoleChangedAction += OnTrackedDeviceRoleChanged;
        }

        private void Start()
        {
            UpdateIndex();
            SetOrigin(FindFirstObjectByType<Vive_Manager>().transform);
        }
        // t is treated as the (0,0,0) point
        public void SetOrigin(Transform t)
        {
            origin = t;
        }

        private void OnEnable()
        {
            UpdateIndex();

            OVRT_Events.NewPoses.AddListener(_onNewPosesAction);
            OVRT_Events.ButtonPressed.AddListener(_onButtonPressedAction);
            OVRT_Events.TrackedDeviceConnected.AddListener(_onDeviceConnectedAction);
            OVRT_Events.TrackedDeviceRoleChanged.AddListener(_onTrackedDeviceRoleChangedAction);
        }

        private void OnDisable()
        {
            OVRT_Events.NewPoses.RemoveListener(_onNewPosesAction);
            OVRT_Events.ButtonPressed.RemoveListener(_onButtonPressedAction);
            OVRT_Events.TrackedDeviceConnected.RemoveListener(_onDeviceConnectedAction);
            IsValid = false;
            IsConnected = false;
        }

        private void UpdateIndex()
        {
            DeviceIndex = FindIndexForRole();
            onDeviceIndexChanged.Invoke(DeviceIndex);
        }

        // Masks for the controller buttons
        public enum ViveControllerButton : ulong
        {
            System = 1ul << (int)EVRButtonId.k_EButton_System,
            ApplicationMenu = 1ul << (int)EVRButtonId.k_EButton_ApplicationMenu,
            Grip = 1ul << (int)EVRButtonId.k_EButton_Grip,
            Axis0 = 1ul << (int)EVRButtonId.k_EButton_Axis0,
            Axis1 = 1ul << (int)EVRButtonId.k_EButton_Axis1,
            Axis2 = 1ul << (int)EVRButtonId.k_EButton_Axis2,
            Axis3 = 1ul << (int)EVRButtonId.k_EButton_Axis3,
            Axis4 = 1ul << (int)EVRButtonId.k_EButton_Axis4,
            Touchpad = 1ul << (int)EVRButtonId.k_EButton_SteamVR_Touchpad,
            Trigger = 1ul << (int)EVRButtonId.k_EButton_SteamVR_Trigger
        }
    }
}
