using System;
using UnityEngine;
// using UnityEngine.InputSystem;

namespace Spelunx
{
    public class KeyboardInputManager : MonoBehaviour
    {

        // public InputAction quit;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            // InputSystem.RegisterInteraction
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.Escape))
            {
#if UNITY_EDITOR
                // UnityEditor.EditorApplication.isPlaying = false;
                UnityEditor.EditorApplication.ExitPlaymode();
#else
                Application.Quit();
#endif
            }
        }

        public void RegisterInput()
        {

        }
    }
}
