using UnityEngine;
using UnityEngine.UIElements;

namespace Spelunx
{
    /*
     * This object this script is attached to will move away from a target when within a distance while staying at the original height (y-axis).
     * The trigger distance, move away distance, and move away speed can be assigned in the editor.
     */
    public class EvasiveMotion : MonoBehaviour
    {
        [SerializeField] private Transform target = null; // the target that the object moves away from (uaully vive tracker)
        [SerializeField] private float triggerDistance = 2.0f; // the distance the object will be triggered to move away from
        [SerializeField] private float moveAwayDistance = 3.0f; // how far the object will move away
        [SerializeField] private float moveAwaySpeed = 3.0f; // the speed of the moving away movement

        private bool shouldMove = false;
        private Vector3 startPos, endPos;

        private void Start()
        {
            SetTarget(target);
        }

        void Update()
        {
            if (target == null) return;
            Vector3 directionAwayFromTarget = new Vector3(transform.position.x, 0, transform.position.z) - new Vector3(target.position.x, 0, target.position.z);
            
            // prefer to compare square magnitude because finding the distance requires a square root, which is more expensive than a multiplication
            if (!shouldMove && directionAwayFromTarget.sqrMagnitude < triggerDistance * triggerDistance)
            {
                shouldMove = true;
                startPos = transform.position;
                endPos = startPos + directionAwayFromTarget.normalized * moveAwayDistance;
            }

            if (shouldMove)
            {
                transform.position = Vector3.MoveTowards(transform.position, endPos, moveAwaySpeed * Time.deltaTime);
                shouldMove = transform.position != endPos;
            }
        }

        public void SetTarget(Transform target, float triggerDistance = 2.0f, float moveAwayDistance = 3.0f, float moveAwaySpeed = 3.0f)
        {
            if (target == null)
            {
                Debug.Log(gameObject.name + ": EvasiveMotion has no target.");
                return;
            }
            this.target = target;
            this.triggerDistance = triggerDistance;
            this.moveAwayDistance = moveAwayDistance;
            this.moveAwaySpeed = moveAwaySpeed;
        }

        public void SetTarget(Transform target)
        {
            SetTarget(target, triggerDistance, moveAwayDistance, moveAwaySpeed);
        }

        public Transform GetTarget()
        {
            return target;
        }
    }
}
