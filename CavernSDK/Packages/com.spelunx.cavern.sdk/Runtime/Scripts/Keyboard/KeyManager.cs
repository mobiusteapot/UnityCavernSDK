using System;
using UnityEngine.InputSystem;

namespace Spelunx
{
    [Serializable]
    public abstract class KeyManager
    {
        abstract public string Action_Map_Name { get; }
        public abstract void SetupInputActions(InputActionMap actionMap);
        public abstract void BindInputActions(DebugManager d, InputActionMap actionMap);

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
