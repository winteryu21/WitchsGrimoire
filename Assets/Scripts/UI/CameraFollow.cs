using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;       // 추적할 대상 (플레이어)
    public float smoothSpeed = 0.125f;  // 카메라 이동 속도
    public Vector3 offset;         // 위치 보정 값 (선택)

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, transform.position.z);
    }
}
