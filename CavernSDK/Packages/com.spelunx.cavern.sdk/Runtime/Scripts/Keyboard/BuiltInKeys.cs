using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Spelunx
{
    [Serializable]
    public class BuiltInKeys : KeyManager
    {
        public override string Action_Map_Name => "Cavern Renderer";
        private DebugManager d;

        public override void SetupInputActions(InputActionMap actionMap)
        {
            RegisterAction(actionMap, "Quit", "<Keyboard>/Escape");
            RegisterAction(actionMap, "SwapEyes", "<Keyboard>/e");
            RegisterAction(actionMap, "StereoMonoToggle", "<Keyboard>/t");
            RegisterAction(actionMap, "MuteToggle", "<Keyboard>/m");
            RegisterAction(actionMap, "MouseMove", "<Mouse>/delta");
        }

        public override void BindInputActions(DebugManager d, InputActionMap actionMap)
        {
            this.d = d;
            actionMap.FindAction("Quit").performed += QuitAction;
            actionMap.FindAction("SwapEyes").performed += SwapEyesAction;
            actionMap.FindAction("StereoMonoToggle").performed += MonoStereoAction;
            actionMap.FindAction("MuteToggle").performed += MuteToggleAction;
            actionMap.FindAction("MouseMove").performed += OnMouseMove;
            StartHideMouse();
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

        void SwapEyesAction(InputAction.CallbackContext ctx)
        {
            CavernRenderer cavern = GameObject.FindFirstObjectByType<CavernRenderer>();
            cavern.SwapEyes = !cavern.SwapEyes;
        }

        void MonoStereoAction(InputAction.CallbackContext ctx)
        {
            CavernRenderer cavern = GameObject.FindFirstObjectByType<CavernRenderer>();
            switch (cavern.GetStereoscopicMode())
            {
                case CavernRenderer.StereoscopicMode.Mono:
                    cavern.SetStereoscopicMode(CavernRenderer.StereoscopicMode.Stereo);
                    break;
                case CavernRenderer.StereoscopicMode.Stereo:
                    cavern.SetStereoscopicMode(CavernRenderer.StereoscopicMode.Mono);
                    break;
            }

        }

        //  void HeadtrackingToggleAction(InputAction.CallbackContext ctx){

        // }

        void MuteToggleAction(InputAction.CallbackContext ctx)
        {
            AudioListener l = GameObject.FindFirstObjectByType<AudioListener>();
            l.enabled = !l.enabled;
        }


        #region Cursor Hiding
        void StartHideMouse()
        {
            hideMouseCoroutine = HideMouse();
            d.StartCoroutine(hideMouseCoroutine);
        }
        IEnumerator hideMouseCoroutine = null;
        IEnumerator HideMouse()
        {
            yield return new WaitForSeconds(3);
            Cursor.visible = false;
        }

        void OnMouseMove(InputAction.CallbackContext context)
        {
            d.StopCoroutine(hideMouseCoroutine);

            Cursor.visible = true;
            hideMouseCoroutine = HideMouse();
            d.StartCoroutine(hideMouseCoroutine);
        }
        #endregion
    }
}
