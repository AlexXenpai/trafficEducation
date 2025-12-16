using UnityEngine;

public class RigFollow : MonoBehaviour
{
    public Transform target;
    public float posLerp = 8f;
    public float rotLerp = 6f;

    void LateUpdate()
    {
        if (!target) return;
        transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * posLerp);
        transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, Time.deltaTime * rotLerp);
    }
}
