using UnityEngine;

public class XROriginFollowTarget : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        transform.position = target.position + offset;
    }
}
