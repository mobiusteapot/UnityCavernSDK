using UnityEngine;
using UnityEngine.UI;
using Spelunx.Vive;

public class TrackerData : MonoBehaviour
{
    public ViveTracker tracker;
    public Text trackerName;
    public Text trackerPosition;
    public Text trackerRotation;
    public Text trackerDetected;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        trackerName.text = ViveTracker.GetReadableName(tracker.binding);
    }

    // Update is called once per frame
    void Update()
    {
        trackerPosition.text = tracker.transform.position.ToString();
        trackerRotation.text = tracker.transform.rotation.eulerAngles.ToString();
        if (tracker.IsConnected)
        {
            trackerDetected.text = "Yes";
            trackerDetected.color = Color.green;
        }
        else
        {
            trackerDetected.text = "No";
            trackerDetected.color = Color.red;
        }
    }
}