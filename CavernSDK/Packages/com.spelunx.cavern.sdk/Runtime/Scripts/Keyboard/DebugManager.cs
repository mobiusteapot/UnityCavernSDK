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

        [SerializeField]
        private InputAction quit;

        // void RegisterInput(InputAction act)
        // {
        //     inputs.Add(act);
        // }

        void Awake()
        {
            quit.performed += QuitAction;
            quit.Enable();
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
