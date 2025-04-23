using UnityEngine;

namespace Spelunx.Vive {
    public class LookAt : Interaction {
        [SerializeField, Tooltip("Flip rotation. Enable this if the object is looking the opposite direction")] private bool flipRotation = false;

        void Update() {
            if (flipRotation) {
                transform.LookAt(2 * transform.position - target.position);
            } else {
                transform.LookAt(target);
            }
        }
    }
}