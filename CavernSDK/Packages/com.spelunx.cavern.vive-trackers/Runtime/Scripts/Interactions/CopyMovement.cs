using UnityEngine;

namespace Spelunx.Vive
{
    /*
     * The object with this script attached will copy the movement of the source it is following
     */
    public class CopyMovement : MonoBehaviour
    {
        [SerializeField] private Transform target;  // the source to follow (usually vive tracker)

        void Update()
        {
            transform.position = target.position;
        }
    }
}

