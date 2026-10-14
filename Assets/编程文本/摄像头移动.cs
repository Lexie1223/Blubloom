using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("跟随目标：把Player物体拖进来")]
    public Transform target;

    [Header("相机移动边界，根据背景大小微调")]
    public float minX = -8f;
    public float maxX = 8f;
    public float minY = -3.2f;
    public float maxY = 3.2f;

    [Header("跟随平滑度")]
    public float smoothSpeed = 6f;

    private Vector3 _offset;

    void Start()
    {
        // 记录相机和玩家的初始距离
        _offset = transform.position - target.position;
    }

    void LateUpdate()
    {
        // 左右、上下都夹紧在边界内
        float targetX = Mathf.Clamp(target.position.x + _offset.x, minX, maxX);
        float targetY = Mathf.Clamp(target.position.y + _offset.y, minY, maxY);
        float targetZ = _offset.z;

        Vector3 desiredPos = new Vector3(targetX, targetY, targetZ);

        // 平滑移动
        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
    }
}
