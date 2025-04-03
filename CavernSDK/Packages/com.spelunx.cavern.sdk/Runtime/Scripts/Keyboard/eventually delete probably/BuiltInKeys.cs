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
        private const float IPD_CHANGE = 0.001f;
        public override void SetupInputActions(InputActionMap actionMap)
        {
            RegisterAction(actionMap, "Quit", "<Keyboard>/Escape");
            RegisterAction(actionMap, "Swap Eyes", "<Keyboard>/e");
            RegisterAction(actionMap, "Stereo/Mono Toggle", "<Keyboard>/t");
            RegisterAction(actionMap, "Mute Toggle", "<Keyboard>/m");
            RegisterAction(actionMap, "Mouse Move", "<Mouse>/delta");

            RegisterAction(actionMap, "Help", "<Keyboard>/h");

            RegisterAction(actionMap, "Increase IPD", "<Keyboard>/rightArrow");
            RegisterAction(actionMap, "Decrease IPD", "<Keyboard>/leftArrow");
        }

        public override void BindInputActions(DebugManager d, InputActionMap actionMap)
        {
            this.d = d;
            actionMap.FindAction("Quit").performed += QuitAction;
            actionMap.FindAction("Swap Eyes").performed += SwapEyesAction;
            actionMap.FindAction("Stereo/Mono Toggle").performed += MonoStereoAction;
            actionMap.FindAction("Mute Toggle").performed += MuteToggleAction;
            actionMap.FindAction("Mouse Move").performed += OnMouseMove;
            actionMap.FindAction("Help").performed += HelpAction;
            actionMap.FindAction("Increase IPD").performed += IncreaseIPDAction;
            actionMap.FindAction("Decrease IPD").performed += DecreaseIPDAction;
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

        void HelpAction(InputAction.CallbackContext ctx)
        {

        }

        void IncreaseIPDAction(InputAction.CallbackContext ctx)
        {
            CavernRenderer cavern = GameObject.FindFirstObjectByType<CavernRenderer>();
            cavern.IPD += IPD_CHANGE;
        }

        void DecreaseIPDAction(InputAction.CallbackContext ctx)
        {
            CavernRenderer cavern = GameObject.FindFirstObjectByType<CavernRenderer>();
            cavern.IPD -= IPD_CHANGE;
        }


        #region Cursor Hiding

        // We hide the cursor when it's not moving.
        // We use coroutines instead of an update loop because most of the time the mouse isn't going to be moving
        // And this saves on compute cost in that case (although it's slightly worse if the mouse is moving often)
        void StartHideMouse()
        {
            hideMouseCoroutine = HideMouse();
            d.StartCoroutine(hideMouseCoroutine); // This isn't a unity gameobject, so it can't run coroutines. Run them in the debugmanager object instead.
        }
        IEnumerator hideMouseCoroutine = null;
        IEnumerator HideMouse()
        {
            yield return new WaitForSeconds(3); // after three seconds, hide the mouse
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
