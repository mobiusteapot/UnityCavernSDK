using UnityEngine;

namespace Spelunx
{
    /*
     * This script assumes the CAVERN screen is a mirror, and the object holding this script will be mirrored.
     * A deadzone radius value can be assigned to prevent area near the center reflecting endlessly and spinning around.
     */
    public class CavernMirrorMotion : MonoBehaviour
    {
        [SerializeField] private Transform target = null; // The object to mirror (usually vive tracker)
        [SerializeField] private CavernRenderer cavernRenderer;
        [SerializeField] private float deadZoneRadius = 0.5f;

        void Start()
        {
            SetTarget(target);
        }

        void Update()
        {
            if (target == null) return;
            Vector3 newDirection = (new Vector3(target.position.x, 0, target.position.z) - new Vector3(cavernRenderer.transform.position.x, 0, cavernRenderer.transform.position.z)).normalized;
            float newRadius = (2 * cavernRenderer.GetCavernRadius() - (Vector3.Distance(target.position, cavernRenderer.transform.position)));
            if (newRadius > deadZoneRadius)
            {
                Vector3 newVector = newDirection * newRadius;
                transform.position = cavernRenderer.transform.position + new Vector3(newVector.x, target.position.y, newVector.z); // ignore original y value so it will be copy the target
            }
        }

        public void SetTarget(Transform target)
        {
            if (target == null)
            {
                Debug.Log(gameObject.name + ": CavernMirrorMotion has no target.");
                return;
            }
            this.target = target;
            transform.position = new Vector3(transform.position.x, target.position.y, transform.position.z);  // set height same as object to mirror
        }

        public Transform GetTarget()
        {
            return target;
        }
    }
}