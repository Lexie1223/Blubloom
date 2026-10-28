using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("移动速度，可以在Inspector里直接调")]
    public float moveSpeed = 5f;

    [Header("是否启用地图边界（物理墙 + 代码兜底）")]
    public bool useBounds = true;

    [Header("地图边界（基于碰撞体边缘，和场景里 Boundary 墙对齐）")]
    public float boundsMinX = -11.27f;
    public float boundsMaxX = 11.27f;
    public float boundsMinY = -4.12f;
    public float boundsMaxY = 4.1f;

    private Rigidbody2D rb;
    private Collider2D col;
    private Vector2 moveInput;

    void Start()
    {
        // 自动获取身上的刚体组件
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        float moveX = 0;
        float moveY = 0;

        // 左右：A/D + 左右方向键
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            moveX -= 1;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            moveX += 1;

        // 上下：W/S + 上下方向键
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            moveY += 1;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            moveY -= 1;

        // 归一化，斜着走速度不会变快
        moveInput = new Vector2(moveX, moveY).normalized;
    }

    void FixedUpdate()
    {
        // 用物理方式移动
        rb.linearVelocity = moveInput * moveSpeed;

        // 撞墙兜底：即使物理没拦住，也把碰撞体边缘夹回地图边界内
        if (useBounds) ClampToBounds();
    }

    private void ClampToBounds()
    {
        if (rb == null || col == null) return;

        Bounds b = col.bounds;
        Vector2 pos = rb.position;

        if (b.min.x < boundsMinX) pos.x += boundsMinX - b.min.x;
        else if (b.max.x > boundsMaxX) pos.x -= b.max.x - boundsMaxX;

        if (b.min.y < boundsMinY) pos.y += boundsMinY - b.min.y;
        else if (b.max.y > boundsMaxY) pos.y -= b.max.y - boundsMaxY;

        rb.position = pos;
    }
}
