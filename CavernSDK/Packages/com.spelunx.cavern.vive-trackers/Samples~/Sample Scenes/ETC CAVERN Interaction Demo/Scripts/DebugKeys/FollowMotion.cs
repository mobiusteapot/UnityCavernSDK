using System;
using UnityEngine;

namespace Spelunx.Vive
{
    /*
     * This script copies the movement of the target with an optional offset (x, y, z)
     * The default is no offset, moving at the same place as the target
     */
    public class FollowMotion : MonoBehaviour
    {
        [SerializeField, Tooltip("The target to follow. Usually a vive tracker.")] private Transform target = null;  // the target to follow (usually vive tracker)
        [SerializeField, Tooltip("The offset vector from the target position.")] private Vector3 offset = Vector3.zero;

        private void Start()
        {
            SetTarget(this.target);
        }

        void Update()
        {
            if (target == null) return;
            transform.position = target.position + offset;
        }

        public void SetTarget(Transform target, bool withOffset)
        {
            if (target == null)
            {
                Debug.Log(gameObject.name + ": FollowMotion has no target.");
                return;
            }
            this.target = target;
            offset = withOffset ? (transform.position - target.position) : Vector3.zero;
        }

        public void SetTarget(Transform target)
        {
            SetTarget(target, false);
        }

        public Transform GetTarget()
        {
            return target;
        }
    }
}
