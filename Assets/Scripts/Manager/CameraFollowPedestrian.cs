using UnityEngine;

public class CameraFollowPedestrian : MonoBehaviour
{
    public Transform pedestrian;
    public Vector3 offset;

    void LateUpdate()
    {
        if (pedestrian == null) return;

        transform.position = pedestrian.position + offset;
    }
}
