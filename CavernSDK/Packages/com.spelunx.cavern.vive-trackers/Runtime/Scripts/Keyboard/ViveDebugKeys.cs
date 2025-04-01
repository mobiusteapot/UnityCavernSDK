using UnityEngine;
using System;
using UnityEngine.InputSystem;
namespace Spelunx.Vive
{
    [Serializable]
    public class ViveDebugKeys : KeyManager
    {
        public override string Action_Map_Name => "Vive Trackers";

        public override void SetupInputActions(InputActionMap actionMap)
        {
            RegisterAction(actionMap, "CalibrateViveRotation", "<Keyboard>/c");
        }

        public override void BindInputActions(DebugManager d, InputActionMap actionMap)
        {
            actionMap.FindAction("CalibrateViveRotation").performed += CalibrateAction;
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
