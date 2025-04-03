using System;
using UnityEngine.InputSystem;

namespace Spelunx
{
    // KayManager handles registering the action to the action map and binding them at runtime
    [Serializable]
    public abstract class KeyManager
    {
        // an abstract string that can be overrided in child classes but still used in this one
        // This is mostly a remnant from a previous implementation, but I like it so here it remains
        abstract public string Action_Map_Name { get; }

        // The function to override that handles adding new input actions to the map in DebugManager.
        // This runs once
        public abstract void SetupInputActions(InputActionMap actionMap);

        // The function to override that handles binding callbacks to the action.performed at runtime
        public abstract void BindInputActions(DebugManager d, InputActionMap actionMap);

        // A helper function that adds inputs to the map only if they don't exist already.
        protected void RegisterAction(InputActionMap inputActions, string name, string binding)
        {
            InputAction action = inputActions.FindAction(name);
            if (action == null)
            {
                // add action to map
                action = inputActions.AddAction(name, InputActionType.Value, binding);
            }
        }
    }
}
