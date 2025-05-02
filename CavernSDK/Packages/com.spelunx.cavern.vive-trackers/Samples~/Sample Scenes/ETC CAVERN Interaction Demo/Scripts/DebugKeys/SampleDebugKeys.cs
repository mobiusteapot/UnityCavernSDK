using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Spelunx;
using Spelunx.Vive;

[DisallowMultipleComponent]
public class SampleDebugKeys : MonoBehaviour, IDebugKeys
{
    [Header("Input Actions")]
    [SerializeField, Tooltip("Toggle head tracking")]
    private InputAction headTracking = new("Toggle Head Tracking", InputActionType.Value, "<Keyboard>/o");

    private Vector3 cameraStartPos;
    private bool doHeadTracking = false;

    public List<(string Key, string Description)> KeyDescriptions()
    {
        return new(){
                (headTracking.GetBindingDisplayString(), "Toggle head tracking")
            };
    }

    // enable the input actions on play mode start
    void OnEnable()
    {
        headTracking.Enable();
    }


    // disable the input actions on play mode stop
    void OnDisable()
    {
        headTracking.Disable();
    }

    // bind the proper callbacks to each action.performed
    // using the saved key managers
    // This must happen in play mode, not in edit mode, or it won't work.
    void Awake()
    {
        headTracking.performed += ToggleHeadTrackAction;
    }

    void Start()
    {
        cameraStartPos = GetComponentInChildren<FollowMotion>().transform.position;
        GetComponentInChildren<FollowMotion>().enabled = false;
    }

    public void ToggleHeadTrackAction(InputAction.CallbackContext ctx)
    {
        doHeadTracking = !doHeadTracking;
        if (doHeadTracking)
        {
            GetComponentInChildren<FollowMotion>().enabled = true;
        }
        else
        {
            GetComponentInChildren<FollowMotion>().enabled = false;
            GetComponentInChildren<FollowMotion>().transform.position = cameraStartPos;

        }
    }
    public void DoExtraGUI()
    {
    }
}
