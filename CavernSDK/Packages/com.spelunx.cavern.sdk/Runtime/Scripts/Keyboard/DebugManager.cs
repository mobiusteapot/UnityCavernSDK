using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Spelunx
{
    public class DebugManager : MonoBehaviour
    {


        // [SerializeField]
        // private List<InputAction> inputs;
        // private InputActionMap actions;

        // InputActionMap a;

        [SerializeField]
        private InputAction quit;

        // public void RegisterInput(string name, InputAction act)
        // {
        //     // Don't add the action if the map already has it
        //     if(actions.Contains(act)){
        //         return;
        //     }
        //     actions.AddAction(name, act);
        // }

        void Awake()
        {
            quit.performed += QuitAction;
            quit.Enable();
            // a.AddAction();
            // a.
            // foreach (InputAction act in inputs)
            // {
            //     act.Enable();
            // }
        }
        public void QuitAction(InputAction.CallbackContext ctx)
        {
#if UNITY_EDITOR
            // UnityEditor.EditorApplication.isPlaying = false;
            UnityEditor.EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }
    }
}
