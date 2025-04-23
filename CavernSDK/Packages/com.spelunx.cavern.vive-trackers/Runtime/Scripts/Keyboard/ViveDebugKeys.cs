using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Spelunx.Vive
{
    /// <summary>
    /// Helpful debug information (both keyboard shortcuts and GUI) for Vive Trackers.
    /// This also adds the ability to calibrate the rotation of all active Vive Trackers.
    /// </summary>
    [DisallowMultipleComponent]
    public class ViveDebugKeys : MonoBehaviour, IDebugKeys
    {
        [Header("Input Actions")]
        [SerializeField, Tooltip("Calibrate the rotations of all vive trackers. Hold them upright in the center of the CAVERN and pointed towards the center of the screen.")]
        private InputAction calibrate = new("Calibrate", InputActionType.Value, "<Keyboard>/c");

        // display the number of vive trackers in the debug GUI
        private int numViveTrackers = 0;
        private readonly List<string> trackerRoles = new();
        public List<(string Key, string Description)> KeyDescriptions()
        {
            return new(){
                (calibrate.GetBindingDisplayString(), "Calibrate the rotation of all vive trackers."),
            };
        }

        // Render information about the currently bound Vive Trackers in the Debug UI
        public void DoExtraGUI()
        {
            GUILayout.Label($"Vive Trackers: {numViveTrackers}");
            GUILayout.Label($"Tracker roles: {string.Join(", ", trackerRoles)}");
        }

        // enable the input actions on play mode start
        void OnEnable()
        {
            calibrate.Enable();
        }


        // disable the input actions on play mode stop
        void OnDisable()
        {
            calibrate.Disable();
        }

        // bind the proper callbacks to each action.performed
        // using the saved key managers
        // This must happen in play mode, not in edit mode, or it won't work.
        void Awake()
        {
            calibrate.performed += CalibrateAction;

            // add the vive tracker info to the GUI
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("ViveTracker"))
            {
                numViveTrackers++;
                ViveTracker.SteamVRPoseBindings binding = go.GetComponent<ViveTracker>().binding;
                trackerRoles.Add(ViveTracker.GetReadableName(binding));
            }
        }

        void CalibrateAction(InputAction.CallbackContext ctx)
        {
            foreach (GameObject tracker in GameObject.FindGameObjectsWithTag("ViveTracker"))
            {
                tracker.GetComponent<ViveTracker>().Calibrate();
            }
        }

    }
}
