using UnityEngine;

public class FirstPersonCameraFollow : MonoBehaviour
{
    public Transform target;         // 玩家对象
    public Vector3 offset = new Vector3(0f, 1.6f, 0f); // 头部偏移
    public float positionSmoothTime = 0.05f;
    public float rotationSmoothTime = 10f;

    private Vector3 velocity = Vector3.zero;
    public Vector3 localRotation = new Vector3(10f, 0f, 0f);

    void LateUpdate()
    {
        if (!target) return;

        // 平滑位置跟随
        Vector3 desiredPosition = target.position + target.TransformVector(offset);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, positionSmoothTime);

        // 平滑旋转跟随
        Quaternion targetRot = Quaternion.LookRotation(target.forward);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * rotationSmoothTime);
        transform.localEulerAngles = localRotation;
    }
}
