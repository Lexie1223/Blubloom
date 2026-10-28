using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("跟随目标：把Player物体拖进来")]
    public Transform target;

    [Header("背景 SpriteRenderer：把背景图拖进来，自动按背景算边界")]
    public SpriteRenderer background;

    [Header("自动按背景限制相机边界（推荐打开）")]
    public bool autoBounds = true;

    [Header("手动边界（autoBounds 关闭时生效，或作为兜底值）")]
    public float minX = -4.46f;
    public float maxX = 4.46f;
    public float minY = -0.02f;
    public float maxY = 0.02f;

    [Header("跟随平滑度")]
    public float smoothSpeed = 6f;

    private Vector3 _offset;
    private Camera _cam;

    void Start()
    {
        _cam = GetComponent<Camera>();
        if (target != null)
        {
            // 记录相机和玩家的初始距离
            _offset = transform.position - target.position;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        float targetX = target.position.x + _offset.x;
        float targetY = target.position.y + _offset.y;

        if (autoBounds && background != null && _cam != null)
        {
            // 相机视口半宽/半高，随屏幕比例变化
            float halfH = _cam.orthographicSize;
            float halfW = halfH * _cam.aspect;

            Bounds b = background.bounds;
            float minXc = b.min.x + halfW;
            float maxXc = b.max.x - halfW;
            float minYc = b.min.y + halfH;
            float maxYc = b.max.y - halfH;

            // 背景比相机视口还窄/矮的那一侧，直接锁中心
            targetX = minXc < maxXc ? Mathf.Clamp(targetX, minXc, maxXc) : b.center.x;
            targetY = minYc < maxYc ? Mathf.Clamp(targetY, minYc, maxYc) : b.center.y;
        }
        else
        {
            targetX = Mathf.Clamp(targetX, minX, maxX);
            targetY = Mathf.Clamp(targetY, minY, maxY);
        }

        Vector3 desiredPos = new Vector3(targetX, targetY, _offset.z);

        // 平滑移动
        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
    }
}
