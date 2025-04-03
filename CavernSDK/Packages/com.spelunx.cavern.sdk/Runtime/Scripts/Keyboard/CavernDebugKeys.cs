using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Spelunx
{
    [DisallowMultipleComponent]
    public class CavernDebugKeys : MonoBehaviour, IDebugKeys
    {
        [Header("Input Actions")]
        [SerializeField, Tooltip("Quits the game or play mode")]
        private InputAction quit = new("Quit", InputActionType.Value, "<Keyboard>/q");
        [SerializeField, Tooltip("Opens the help debug window")]
        private InputAction help = new("Help", InputActionType.Value, "<Keyboard>/h");
        [SerializeField, Tooltip("Swaps the eyes on the stereoscopic glasses")]
        private InputAction swapEyes = new("Swap Eyes", InputActionType.Value, "<Keyboard>/e");
        [SerializeField, Tooltip("Toggles rendering between stereo and mono")]
        private InputAction stereoMonoToggle = new("Stereo/Mono Toggle", InputActionType.Value, "<Keyboard>/t");
        [SerializeField, Tooltip("Toggles muting all sounds")]
        private InputAction muteToggle = new("Mute Toggle", InputActionType.Value, "<Keyboard>/m");
        [SerializeField, Tooltip("Hides the mouse when it doesn't move for a few seconds")]
        private InputAction mouseMove = new("Mouse Move", InputActionType.Value, "<Mouse>/delta");
        [SerializeField, Tooltip("Increase the interpupillary distance")]
        private InputAction increaseIPD = new("Increase IPD", InputActionType.Value, "<Keyboard>/rightArrow");
        [SerializeField, Tooltip("Decreases the interpupillary distance")]
        private InputAction decreaseIPD = new("Decrease IPD", InputActionType.Value, "<Keyboard>/leftArrow");

        [Header("Settings")]
        [SerializeField, Range(0, 0.01f), Tooltip("Amount to adjust interpupillary distance for stereo rendering")]
        private float IPD_CHANGE = 0.001f;

        // used to render the help debug window
        private List<string> helpKeys = new();
        private List<string> helpDescriptions = new();
        private UnityAction extraGUICalls;
        private bool showHelp = false;

        public List<(string Key, string Description)> KeyDescriptions()
        {
            return new(){
                (quit.GetBindingDisplayString(), "Quit the game or exit play mode"),
                (help.GetBindingDisplayString(), "Open this help window"),
                (swapEyes.GetBindingDisplayString(), "Swap the eyes on the stereoscopic glasses"),
                (stereoMonoToggle.GetBindingDisplayString(), "Toggle rendering between stereo and mono"),
                (muteToggle.GetBindingDisplayString(), "Mute all sounds"),
                (increaseIPD.GetBindingDisplayString(), "Increase IPD"),
                (decreaseIPD.GetBindingDisplayString(), "Decrease IPD")
            };
        }

        public void DoExtraGUI()
        {
            GUILayout.Label($"Framerate: {1 / Time.deltaTime} fps");
        }

        // enable the input actions on play mode start
        void OnEnable()
        {
            quit.Enable();
            help.Enable();
            swapEyes.Enable();
            stereoMonoToggle.Enable();
            muteToggle.Enable();
            mouseMove.Enable();
            increaseIPD.Enable();
            decreaseIPD.Enable();
        }


        // disable the input actions on play mode stop
        void OnDisable()
        {
            quit.Disable();
            help.Disable();
            swapEyes.Disable();
            stereoMonoToggle.Disable();
            muteToggle.Disable();
            mouseMove.Disable();
            increaseIPD.Disable();
            decreaseIPD.Disable();
        }

        // bind the proper callbacks to each action.performed
        // using the saved key managers
        // This must happen in play mode, not in edit mode, or it won't work.
        void Awake()
        {
            quit.performed += QuitAction;
            help.performed += HelpAction;
            swapEyes.performed += SwapEyesAction;
            stereoMonoToggle.performed += MonoStereoAction;
            muteToggle.performed += MuteToggleAction;
            mouseMove.performed += OnMouseMove;
            increaseIPD.performed += IncreaseIPDAction;
            decreaseIPD.performed += DecreaseIPDAction;
        }

        void Start()
        {
            // Start the coroutine for hiding the mouse if it doesn't move
            hideMouseCoroutine = HideMouse();
            StartCoroutine(hideMouseCoroutine);
            // find all help descriptions and add them to list
            foreach (IDebugKeys manager in GetComponents<IDebugKeys>())
            {
                foreach ((string Key, string Description) d in manager.KeyDescriptions())
                {
                    helpKeys.Add(d.Key);
                    helpDescriptions.Add(d.Description);
                }
                extraGUICalls += manager.DoExtraGUI;
            }
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
            CavernRenderer cavern = GetComponentInChildren<CavernRenderer>();
            cavern.SwapEyes = !cavern.SwapEyes;
        }

        void MonoStereoAction(InputAction.CallbackContext ctx)
        {
            CavernRenderer cavern = GetComponentInChildren<CavernRenderer>();
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
            AudioListener l = GetComponentInChildren<AudioListener>();
            l.enabled = !l.enabled;
        }

        void HelpAction(InputAction.CallbackContext ctx)
        {
            showHelp = !showHelp;
        }

        void IncreaseIPDAction(InputAction.CallbackContext ctx)
        {
            CavernRenderer cavern = GetComponentInChildren<CavernRenderer>();
            cavern.IPD += IPD_CHANGE;
        }

        void DecreaseIPDAction(InputAction.CallbackContext ctx)
        {
            CavernRenderer cavern = GetComponentInChildren<CavernRenderer>();
            cavern.IPD -= IPD_CHANGE;
        }


        #region Cursor Hiding

        // We hide the cursor when it's not moving.
        // We use coroutines instead of an update loop because most of the time the mouse isn't going to be moving
        // And this saves on compute cost in that case (although it's slightly worse if the mouse is moving often)
        IEnumerator hideMouseCoroutine = null;
        IEnumerator HideMouse()
        {
            yield return new WaitForSeconds(3); // after three seconds, hide the mouse
            Cursor.visible = false;
        }

        void OnMouseMove(InputAction.CallbackContext context)
        {
            StopCoroutine(hideMouseCoroutine);

            Cursor.visible = true;
            hideMouseCoroutine = HideMouse();
            StartCoroutine(hideMouseCoroutine);
        }
        #endregion

        #region Debug GUI
        void OnGUI()
        {
            if (!showHelp) return;
            GUILayout.BeginArea(new Rect(40, 40, 500, 500), GUI.skin.box);
            // GUILayout.Box("Debug Info");
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            foreach (string key in helpKeys)
            {
                GUILayout.Label(key);
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            foreach (string description in helpDescriptions)
            {
                GUILayout.Label(description);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            extraGUICalls.Invoke();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        #endregion
    }
}
