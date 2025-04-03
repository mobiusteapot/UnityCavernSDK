using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Spelunx.Vive
{
    [DisallowMultipleComponent]
    public class ViveDebugKeys : MonoBehaviour, IDebugKeys
    {
        [Header("Input Actions")]
        [SerializeField, Tooltip("Calibrate the rotations of all vive trackers. Hold them upright in the center of the CAVERN and pointed towards the center of the screen.")]
        private InputAction calibrate = new("Calibrate", InputActionType.Value, "<Keyboard>/c");

        public List<(string Key, string Description)> KeyDescriptions()
        {
            return new(){
                (calibrate.GetBindingDisplayString(), "Calibrate the rotation of all vive trackers."),
            };
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
