using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEditor;
using UnityEngine.Events;
using UnityEditor.Events;

namespace Spelunx
{
    public class DebugManager : MonoBehaviour
    {
        // A map of the keybinds in this program. It gets populated by the KeyManagers in each package
        [SerializeField, Tooltip("A map of the existing keybinds used for debugging. It's better if these are consistent between projects, but they can be changed here if needed.")]
        private InputActionMap actions;


        // The keymanagers get added by each package when their objects are added to the scene (in edit mode).
        // They are serialized to persist them through edit mode and play mode
        // But shouldn't be touched besides that
        [SerializeField, SerializeReference, HideInInspector]
        private List<KeyManager> keyManagers = new();

        // A function called to add package keymanagers and populate the input action map.
        public void AddKeyManager(KeyManager man)
        {
            if (!keyManagers.Any(item => item.Action_Map_Name == man.Action_Map_Name))
            {
                keyManagers.Add(man);
                man.SetupInputActions(actions);
                EditorUtility.SetDirty(this);
            }
        }

        // enable the input actions on play mode start
        void OnEnable()
        {
            actions.Enable();
        }


        // disable the input actions on play mode stop
        void OnDisable()
        {
            actions.Disable();
        }

        // bind the proper callbacks to each action.performed
        // using the saved key managers
        // This must happen in play mode, not in edit mode, or it won't work.
        void Awake()
        {
            foreach (KeyManager manager in keyManagers)
            {
                manager.BindInputActions(this, actions);
            }
        }
    }
}
