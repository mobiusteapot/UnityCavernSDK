using UnityEngine;

namespace Spelunx.Vive
{
    public class LookAt : MonoBehaviour
    {
        [SerializeField, Tooltip("The target to follow. Usually a vive tracker.")] private Transform target = null;  // the target to follow (usually vive tracker)
        [SerializeField, Tooltip("Flip rotation. Enable this if the object is looking the opposite direction")] private bool flipRotation = false;
        void Update()
        {
            if (flipRotation)
            {
                transform.LookAt(2 * transform.position - target.position);
            }
            else
            {
                transform.LookAt(target);
            }
        }
    }
}
