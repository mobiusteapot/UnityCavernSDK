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
        // [SerializeField]
        // private InputActionAsset actions;

        [SerializeField]
        private InputActionMap actions;

        [SerializeField, SerializeReference, HideInInspector]
        private List<KeyManager> keyManagers = new();

        public void AddKeyManager(KeyManager man)
        {
            if (!keyManagers.Any(item => item.Action_Map_Name == man.Action_Map_Name))
            {
                // SerializedObject so = new SerializedObject(this);
                // SerializedProperty p = so.FindProperty("keyManagers");
                // p;
                keyManagers.Add(man);
                man.SetupInputActions(actions);
                // so.Update();
                // so.ApplyModifiedPropertiesWithoutUndo();
                // PrefabUtility.RecordPrefabInstancePropertyModifications(keyManagers);
                EditorUtility.SetDirty(this);
            }
        }

        void OnEnable()
        {
            actions.Enable();
        }

        void OnDisable()
        {
            actions.Disable();
        }

        void Awake()
        {
            foreach (KeyManager manager in keyManagers)
            {
                manager.BindInputActions(this, actions);
            }
        }
    }
}
